using B1_2.DB.Repositories.Abstractions;
using B1_2.DTO;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using NPOI.HPSF;

namespace B1_2.DB.Repositories
{
    public class BalanceSheetRepository : IBalanceSheetRepository
    {
        private readonly BalanceSheetsDbContext _db;
        public BalanceSheetRepository(BalanceSheetsDbContext db)
        {
            _db = db;
        }

        public async Task DeleteFileReportAsync(string filename, CancellationToken cancellationToken)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            await _db.AccountBalances
                .Where(b => b.UploadedFile.FileName == filename)
                .ExecuteDeleteAsync(cancellationToken);

            await _db.UploadedFiles
                .Where(f => f.FileName == filename)
                .ExecuteDeleteAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        public async Task<List<UploadedFile>> GetFilesAsync(CancellationToken cancellationToken)
        {
            var files = await _db.UploadedFiles.ToListAsync(cancellationToken);
            return files;
        }

        public async Task<UploadedReportDto> GetReportByFileAsync(string file, CancellationToken cancellationToken)
        {
            var balances = await _db.AccountBalances
                .Include(b => b.UploadedFile)
                .Include(b => b.Period)
                .Include(b => b.Account).ThenInclude(a => a.AccountClass)
                .Include(b => b.Account).ThenInclude(a => a.Bank)
                .Where(b => b.UploadedFile.FileName == file)
                .ToListAsync(cancellationToken);

            var period = balances.First().Period;

            var dtoBalances = balances.Select(b => new AccountBalanceDto
            {
                InpBalanceActive = b.InpBalanceActive,
                InpBalancePassive = b.InpBalancePassive,
                TurnoverDebit = b.TurnoverDebit,
                TurniverCredit = b.TurniverCredit,
                OutpBalanceActive = b.OutpBalanceActive,
                OutpBalancePassive = b.OutpBalancePassive,
                AccountNumber = b.Account?.AccountNumber.ToString(),
                AccountClassName = b.Account?.AccountClass?.Name
            }).ToList();

            var report = new UploadedReportDto
            {
                FileName = file,
                BankName = balances.First()?.Account?.Bank?.Name,
                PeriodStart = period.DateStart,
                PeriodEnd = period.DateEnd,
                AccountBalances = dtoBalances
            };

            return report;

        }
    }
}
