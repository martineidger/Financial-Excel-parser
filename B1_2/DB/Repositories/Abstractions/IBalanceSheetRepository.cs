using B1_2.DTO;

namespace B1_2.DB.Repositories.Abstractions
{
    public interface IBalanceSheetRepository
    {
        Task<List<UploadedFile>> GetFilesAsync(CancellationToken cancellationToken);
        Task<UploadedReportDto> GetReportByFileAsync(string file, CancellationToken cancellationToken);
        Task DeleteFileReportAsync(string filename,  CancellationToken cancellationToken);
    }
}
