using B1_2.DTO;

namespace B1_2.Services.Abstractions
{
    public interface IExcelParserService
    {
        ParsedReportDto Parse(Stream excelStream);
    }
}
