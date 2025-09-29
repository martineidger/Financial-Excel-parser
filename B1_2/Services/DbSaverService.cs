using B1_2.DB;
using B1_2.DTO;
using B1_2.Services.Abstractions;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Z.EntityFramework.Extensions;

namespace B1_2.Services
{
    public class DbSaverService : IDbSaverService
    {
        private readonly BalanceSheetsDbContext _context;

        public DbSaverService(BalanceSheetsDbContext context)
        {
            _context = context;
        }

        public async Task SaveAsync(ParsedReportDto parsedReport, CancellationToken cancellationToken)
        {
            if (parsedReport == null)
                throw new ArgumentNullException(nameof(parsedReport));

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var newBanks = new List<Bank>();
            var newPeriods = new List<DB.Period>();
            var newClasses = new List<AccountClass>();
            var newAccounts = new List<Account>();
            var newBalances = new List<AccountBalance>();

            var existingBanks = await _context.Banks
                .ToDictionaryAsync(b => b.Name, b => b, cancellationToken);

            if (!existingBanks.TryGetValue(parsedReport.BankName, out var bank))
            {
                bank = new Bank
                {
                    Id = Guid.NewGuid(),
                    Name = parsedReport.BankName
                };
                newBanks.Add(bank);
                existingBanks[bank.Name] = bank;
            }

            var existingPeriods = await _context.Periods
                .ToDictionaryAsync(p => (p.DateStart, p.DateEnd), p => p, cancellationToken);

            var key = (parsedReport.PeriodStart, parsedReport.PeriodEnd);
            var newPeriod = new DB.Period();
            if (!existingPeriods.ContainsKey(key))
            {

                newPeriod.Id = Guid.NewGuid();
                newPeriod.DateStart = parsedReport.PeriodStart;
                newPeriod.DateEnd = parsedReport.PeriodEnd;
                newPeriods.Add(newPeriod);
                existingPeriods[key] = newPeriod;
            }
            else
            {
                newPeriod.Id = existingPeriods[key].Id;
            }

            var existingClasses = await _context.AccountClasses
                .ToDictionaryAsync(c => c.Code, c => c, cancellationToken);

          
            foreach (var accClassDto in parsedReport.AccountClasses)
            {
                if (!existingClasses.TryGetValue(accClassDto.Code, out var dbClass))
                {
                    dbClass = new AccountClass
                    {
                        Id = Guid.NewGuid(),
                        Code = accClassDto.Code,
                        Name = accClassDto.Name
                    };
                    newClasses.Add(dbClass);
                    existingClasses[dbClass.Code] = dbClass;
                }
            }


            var existingAccounts = await _context.Accounts
                .Where(a => a.BankId == bank.Id)
                .ToDictionaryAsync(a => a.AccountNumber, a => a, cancellationToken);

            foreach (var accDto in parsedReport.Accounts)
            {
                if (!existingAccounts.TryGetValue(accDto.AccountNumber, out var dbAcc))
                {
                    dbAcc = new Account
                    {
                        Id = Guid.NewGuid(),
                        BankId = bank.Id,
                        AccountNumber = accDto.AccountNumber,
                        AccountClassId = existingClasses[accDto.AccountClassCode].Id
                    };
                    newAccounts.Add(dbAcc);
                    existingAccounts[dbAcc.AccountNumber] = dbAcc;
                }
            }


            var uploadedFile = new UploadedFile
            {
                Id = Guid.NewGuid(),
                FileName = parsedReport.FileName,
                UploadDate = DateTime.Now
            };
            await _context.UploadedFiles.AddAsync(uploadedFile, cancellationToken);

            await _context.Banks.AddRangeAsync(newBanks, cancellationToken);
            await _context.Periods.AddRangeAsync(newPeriods, cancellationToken);
            await _context.AccountClasses.AddRangeAsync(newClasses, cancellationToken);
            await _context.Accounts.AddRangeAsync(newAccounts, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var accountBalances = new List<AccountBalance>();

            foreach (var balanceDto in parsedReport.AccountBalances)
            {
                if (!existingAccounts.TryGetValue(balanceDto.AccountNumber, out var account))
                    throw new InvalidOperationException($"Account с номером {balanceDto.AccountNumber} не найден");

                var periodKey = (balanceDto.PeriodStart, balanceDto.PeriodEnd);
                if (!existingPeriods.TryGetValue(periodKey, out var period))
                    throw new InvalidOperationException($"Period {balanceDto.PeriodStart} - {balanceDto.PeriodEnd} не найден");

                var balance = new AccountBalance
                {
                    Id = Guid.NewGuid(),
                    AccountId = account.Id,
                    PeriodId = period.Id,
                    UploadedFileId = uploadedFile.Id,
                    InpBalanceActive = balanceDto.InpBalanceActive,
                    InpBalancePassive = balanceDto.InpBalancePassive,
                    TurnoverDebit = balanceDto.TurnoverDebit,
                    TurniverCredit = balanceDto.TurnoverCredit,
                    OutpBalanceActive = balanceDto.OutpBalanceActive,
                    OutpBalancePassive = balanceDto.OutpBalancePassive
                };

                accountBalances.Add(balance);
            }

            await SaveAccountBalancesWithCopy(accountBalances, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        private async Task SaveAccountBalancesWithCopy(List<AccountBalance> balances, CancellationToken cancellationToken)
        {
            var conn = (NpgsqlConnection)_context.Database.GetDbConnection();

            using var writer = conn.BeginBinaryImport(@"
                COPY account_balances (
                    id,
                    account_id,
                    period_id,
                    uploaded_file_id,
                    inp_balance_active,
                    inp_balance_passive,
                    turnover_debit,
                    turniver_credit,
                    outp_balance_active,
                    outp_balance_passive
                ) FROM STDIN (FORMAT BINARY)");

            foreach (var b in balances)
            {
                writer.StartRow();
                writer.Write(b.Id, NpgsqlDbType.Uuid);
                writer.Write(b.AccountId, NpgsqlDbType.Uuid);
                writer.Write(b.PeriodId, NpgsqlDbType.Uuid);
                writer.Write(b.UploadedFileId, NpgsqlDbType.Uuid);
                writer.Write(b.InpBalanceActive, NpgsqlDbType.Numeric);
                writer.Write(b.InpBalancePassive, NpgsqlDbType.Numeric);
                writer.Write(b.TurnoverDebit, NpgsqlDbType.Numeric);
                writer.Write(b.TurniverCredit, NpgsqlDbType.Numeric);
                writer.Write(b.OutpBalanceActive, NpgsqlDbType.Numeric);
                writer.Write(b.OutpBalancePassive, NpgsqlDbType.Numeric);
            }

            await writer.CompleteAsync(cancellationToken);
        }
    }
}
