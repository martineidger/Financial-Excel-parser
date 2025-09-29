
using B1_2.DB;
using B1_2.DTO;
using B1_2.Services.Abstractions;
using NPOI.HSSF.UserModel; 
using NPOI.SS.UserModel;
using System.Text.RegularExpressions;

namespace B1_2.Services
{
    public class ExcelParserService : IExcelParserService
    {
        public ParsedReportDto Parse(Stream excelStream)
        {
            var workbook = new HSSFWorkbook(excelStream);
            var sheet = workbook.GetSheetAt(0);

            var report = new ParsedReportDto();

            report.BankName = sheet.GetRow(0)?.GetCell(0)?.ToString();

            IRow? periodRow = sheet.FirstOrDefault(r =>
                r.Cells().Any(c => c?.ToString()?.Contains("за период") == true));

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

            IRow headerRow = sheet.First(r =>
                r.Cells().Any(c => c?.ToString()?.StartsWith("Б/сч") == true));

            int startRow = headerRow.RowNum + 1;

            ParsedAccountClassDto? currentClass = null;

            for (int i = startRow; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;

                var firstCell = row.GetCell(0)?.ToString();

                if (string.IsNullOrWhiteSpace(firstCell))
                    continue;

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

                if (int.TryParse(firstCell, out int accountNumber))
                {
                    if (currentClass == null)
                        throw new InvalidOperationException($"Счёт {accountNumber} встретился без класса!");

                    var acc = new ParsedAccountDto
                    {
                        AccountNumber = accountNumber,
                        AccountClassCode = currentClass.Code,
                    };
                    report.Accounts.Add(acc);

                
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

    public static class NpoiExtensions
    {
        public static IEnumerable<IRow> Rows(this ISheet sheet)
        {
            for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
            {
                yield return sheet.GetRow(i);
            }
        }

        public static IEnumerable<ICell> Cells(this IRow row)
        {
            for (int i = row.FirstCellNum; i < row.LastCellNum; i++)
            {
                yield return row.GetCell(i);
            }
        }

        public static IRow? FirstOrDefault(this ISheet sheet, Func<IRow, bool> predicate)
        {
            foreach (var row in sheet.Rows())
            {
                if (row != null && predicate(row))
                    return row;
            }
            return null;
        }

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
