using B1_2.DB;

namespace B1_2.DTO
{
    //dto для вывода данных отчета (из одного загруженного файла)
    public class UploadedReportDto
    {
        public string FileName { get; set; }
        public string BankName { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<AccountBalanceDto> AccountBalances { get; set; }
    }
}
