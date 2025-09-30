using B1_2.DTO;

namespace B1_2.Services.Abstractions
{
    //абстракция сервиса для парсинга данных их excel в обьекты
    public interface IExcelParserService
    {
        ParsedReportDto Parse(Stream excelStream);
    }
}
