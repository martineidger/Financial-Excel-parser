using B1_2.DTO;

namespace B1_2.Services.Abstractions
{
    //абстракция сервиса для сохранения данных в бд
    public interface IDbSaverService
    {
        Task SaveAsync(ParsedReportDto parsedReport, CancellationToken cancellationToken);
    }
}
