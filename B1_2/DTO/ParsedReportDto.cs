
using B1_2.DB;

namespace B1_2.DTO
{
    //dto для парсинга всего отчета (агрегирует все dto и другую информацию полученную из файла)
    public class ParsedReportDto
    {
        public string BankName { get; set; } = "";
        public string FileName { get; set; } = "";
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public List<ParsedAccountClassDto> AccountClasses { get; set; } = new();
        public List<ParsedAccountDto> Accounts { get; set; } = new();
        public List<ParsedAccountBalanceDto> AccountBalances { get; set; } = new();
    }


}
