
using B1_2.DB;
using B1_2.DTO;
using B1_2.Services.Abstractions;
using NPOI.HSSF.UserModel; 
using NPOI.SS.UserModel;
using System.Text.RegularExpressions;

namespace B1_2.Services
{
    //реализация сервиса для парсинга данных из excel
    public class ExcelParserService : IExcelParserService
    {
        public ParsedReportDto Parse(Stream excelStream)
        {
            //обьект файла из потока
            var workbook = new HSSFWorkbook(excelStream);

            //берем первую страницу
            var sheet = workbook.GetSheetAt(0);

            //обьект отчета, куда будут заполняться данные из файла 
            var report = new ParsedReportDto();

            //имя банка. положение четко известно - 1 ячейка по строке и столбцу
            report.BankName = sheet.GetRow(0)?.GetCell(0)?.ToString();

            //получем ячейку с периодом
            IRow? periodRow = sheet.FirstOrDefault(r =>
                r.Cells().Any(c => c?.ToString()?.Contains("за период") == true));

            //проверки + парсинг даты в переменные в отчет
            if (periodRow != null)
            {
                string periodText = periodRow.GetCell(0)?.ToString() ?? "";
                var parts = periodText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 6 &&
                    DateTime.TryParse(parts[3], out var startDate) &&
                    DateTime.TryParse(parts[5], out var endDate))
                {
                    report.PeriodStart = startDate;
                    report.PeriodEnd = endDate;

                }
            }

            //находим стартовую строку с номерами счетов (и другими заголовками)
            IRow headerRow = sheet.First(r =>
                r.Cells().Any(c => c?.ToString()?.StartsWith("Б/сч") == true));

            int startRow = headerRow.RowNum + 1;

            ParsedAccountClassDto? currentClass = null;

            //обработка счетов и балансов счетов
            for (int i = startRow; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;

                var firstCell = row.GetCell(0)?.ToString();

                if (string.IsNullOrWhiteSpace(firstCell))
                    continue;

                //если ячейка для класса -> добавляем в данные о классе
                if (firstCell.StartsWith("КЛАСС"))
                {
                    var parts = firstCell.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    currentClass = new ParsedAccountClassDto
                    {
                        Code = parts.Length > 1 ? parts[1] : "",
                        Name = parts.Length > 2 ? string.Join(' ', parts.Skip(2)) : ""
                    };
                    report.AccountClasses.Add(currentClass);
                    continue;
                }

                //если обычный номер -> добавляем данные о счете и его балансе
                if (int.TryParse(firstCell, out int accountNumber))
                {
                    if (currentClass == null)
                        throw new InvalidOperationException($"Счёт {accountNumber} встретился без класса!");

                    //данные о счете
                    var acc = new ParsedAccountDto
                    {
                        AccountNumber = accountNumber,
                        AccountClassCode = currentClass.Code,
                    };
                    report.Accounts.Add(acc);

                
                    //данные о балансе
                    report.AccountBalances.Add(new ParsedAccountBalanceDto
                    {
                        AccountNumber = accountNumber,
                        PeriodStart = report.PeriodStart,
                        PeriodEnd = report.PeriodEnd,
                        InpBalanceActive = ParseDecimal(row.GetCell(1)),
                        InpBalancePassive = ParseDecimal(row.GetCell(2)),
                        TurnoverDebit = ParseDecimal(row.GetCell(3)),
                        TurnoverCredit = ParseDecimal(row.GetCell(4)),
                        OutpBalanceActive = ParseDecimal(row.GetCell(5)),
                        OutpBalancePassive = ParseDecimal(row.GetCell(6))
                    });

                }
            }

            return report;
        }
        //метод для парсинга и проверки дробного числа из ячейки
        private decimal ParseDecimal(ICell? cell)
        {
            if (cell == null) return 0;

            if (cell.CellType == CellType.Numeric)
                return (decimal)cell.NumericCellValue;

            var raw = cell.ToString()?.Replace(" ", "") ?? "0";
            if (decimal.TryParse(raw, out var d))
                return d;

            return 0;
        }
    }

    // вспомогательны функции для удобного парсинга
    public static class ExcelExtensions
    {
        //преобразуем страницу excel в набор строк
        public static IEnumerable<IRow> Rows(this ISheet sheet)
        {
            for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
            {
                //позволяет не загружать память, а отдавать строки по мере запроса
                yield return sheet.GetRow(i);
            }
        }

        //преобразуем строку в набор ячеек
        public static IEnumerable<ICell> Cells(this IRow row)
        {
            for (int i = row.FirstCellNum; i < row.LastCellNum; i++)
            {
                yield return row.GetCell(i);
            }
        }

        //реализация метода FirstOrDefault из Linq, но для работы со страницами и строками
        //находит строку по условию или возвращает null
        public static IRow? FirstOrDefault(this ISheet sheet, Func<IRow, bool> predicate)
        {
            foreach (var row in sheet.Rows())
            {
                if (row != null && predicate(row))
                    return row;
            }
            return null;
        }

        //реализация метода First из Linq, но для работы со страницами и строками
        //находит строку по условию или выбрасывает исключение
        public static IRow First(this ISheet sheet, Func<IRow, bool> predicate)
        {
            foreach (var row in sheet.Rows())
            {
                if (row != null && predicate(row))
                    return row;
            }
            throw new InvalidOperationException("Не найдена строка");
        }
    }
}
