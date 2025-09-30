using B1_2.DTO;

namespace B1_2.DB.Repositories.Abstractions
{
    //интерфейс репозитория для di
    public interface IBalanceSheetRepository
    {
        //список загруженных файлов
        Task<List<UploadedFile>> GetFilesAsync(CancellationToken cancellationToken);
        //данные с определенного файла
        Task<UploadedReportDto> GetReportByFileAsync(string file, CancellationToken cancellationToken);
        //удаляем все данные файла и сам файл
        Task DeleteFileReportAsync(string filename,  CancellationToken cancellationToken);
    }
}
