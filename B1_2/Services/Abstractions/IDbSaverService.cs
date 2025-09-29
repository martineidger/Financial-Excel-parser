using B1_2.DTO;

namespace B1_2.Services.Abstractions
{
    public interface IDbSaverService
    {
        Task SaveAsync(ParsedReportDto parsedReport, CancellationToken cancellationToken);
    }
}
