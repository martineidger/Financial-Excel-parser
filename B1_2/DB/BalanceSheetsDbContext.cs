using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace B1_2.DB;

//сгенерированный контекст бд (на основе готовых таблиц в бд)
public partial class BalanceSheetsDbContext : DbContext
{
    public BalanceSheetsDbContext()
    {
    }

    public BalanceSheetsDbContext(DbContextOptions<BalanceSheetsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AccountBalance> AccountBalances { get; set; }

    public virtual DbSet<AccountClass> AccountClasses { get; set; }

    public virtual DbSet<Bank> Banks { get; set; }

    public virtual DbSet<Period> Periods { get; set; }

    public virtual DbSet<UploadedFile> UploadedFiles { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder.UseNpgsql("Name=BalanceSheetsDb");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("accounts_pkey");

            entity.ToTable("accounts");

            entity.HasIndex(e => e.AccountNumber, "accounts_account_number_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AccountClassId).HasColumnName("account_class_id");
            entity.Property(e => e.AccountNumber).HasColumnName("account_number");
            entity.Property(e => e.BankId).HasColumnName("bank_id");

            entity.HasOne(d => d.AccountClass).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.AccountClassId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("accounts_account_class_id_fkey");

            entity.HasOne(d => d.Bank).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.BankId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("accounts_bank_id_fkey");
        });

        modelBuilder.Entity<AccountBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("account_balances_pkey");

            entity.ToTable("account_balances");

            entity.HasIndex(e => new { e.AccountId, e.PeriodId }, "account_balances_account_id_period_id_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.InpBalanceActive)
                .HasPrecision(18, 2)
                .HasColumnName("inp_balance_active");
            entity.Property(e => e.InpBalancePassive)
                .HasPrecision(18, 2)
                .HasColumnName("inp_balance_passive");
            entity.Property(e => e.OutpBalanceActive)
                .HasPrecision(18, 2)
                .HasColumnName("outp_balance_active");
            entity.Property(e => e.OutpBalancePassive)
                .HasPrecision(18, 2)
                .HasColumnName("outp_balance_passive");
            entity.Property(e => e.PeriodId).HasColumnName("period_id");
            entity.Property(e => e.TurniverCredit)
                .HasPrecision(18, 2)
                .HasColumnName("turniver_credit");
            entity.Property(e => e.TurnoverDebit)
                .HasPrecision(18, 2)
                .HasColumnName("turnover_debit");
            entity.Property(e => e.UploadedFileId).HasColumnName("uploaded_file_id");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountBalances)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("account_balances_account_id_fkey");

            entity.HasOne(d => d.Period).WithMany(p => p.AccountBalances)
                .HasForeignKey(d => d.PeriodId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("account_balances_period_id_fkey");

            entity.HasOne(d => d.UploadedFile).WithMany(p => p.AccountBalances)
                .HasForeignKey(d => d.UploadedFileId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("account_balances_uploaded_file_id_fkey");
        });

        modelBuilder.Entity<AccountClass>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("account_classes_pkey");

            entity.ToTable("account_classes");

            entity.HasIndex(e => e.Code, "account_classes_code_key").IsUnique();

            entity.HasIndex(e => e.Name, "account_classes_name_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(5)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Bank>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("banks_pkey");

            entity.ToTable("banks");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Period>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("periods_pkey");

            entity.ToTable("periods");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DateStart).HasColumnType("timestamp without time zone").HasColumnName("date_start");
            entity.Property(e => e.DateEnd).HasColumnType("timestamp without time zone").HasColumnName("date_end");
            
        });

        modelBuilder.Entity<UploadedFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("uploaded_files_pkey");

            entity.ToTable("uploaded_files");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.UploadDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("upload_date");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
