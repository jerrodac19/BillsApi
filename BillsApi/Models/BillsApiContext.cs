using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BillsApi.Models;

public partial class BillsApiContext : DbContext
{
    public BillsApiContext()
    {
    }

    public BillsApiContext(DbContextOptions<BillsApiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccountBalance> AccountBalances { get; set; }

    public virtual DbSet<BalanceMonitor> BalanceMonitors { get; set; }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<BillConfiguration> BillConfigurations { get; set; }

    public virtual DbSet<DailyAllowance> DailyAllowances { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Income> Incomes { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AccountB__3214EC07092CD002");

            entity.ToTable("AccountBalance");

            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.GroupId)
                .HasDefaultValue(1)
                .HasColumnName("GroupID");
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.Group).WithMany(p => p.AccountBalances)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccountBalance_Groups");
        });

        modelBuilder.Entity<BalanceMonitor>(entity =>
        {
            entity.ToTable("BalanceMonitor");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.Updated).HasColumnType("datetime");
        });

        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bills__3214EC07F83B15FB");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Amount).HasColumnType("smallmoney");
            entity.Property(e => e.CalendarEventId).HasMaxLength(50);
            entity.Property(e => e.ConfigurationId).HasColumnName("ConfigurationID");
            entity.Property(e => e.Payed).HasDefaultValue(false);
            entity.Property(e => e.ReminderId).HasMaxLength(50);
            entity.Property(e => e.TaskId).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(50);
            entity.Property(e => e.Updated).HasColumnType("datetime");

            entity.HasOne(d => d.Configuration).WithMany(p => p.Bills)
                .HasForeignKey(d => d.ConfigurationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bills_BillConfigurations");
        });

        modelBuilder.Entity<BillConfiguration>(entity =>
        {
            entity.Property(e => e.DefaultAmount).HasColumnType("smallmoney");
            entity.Property(e => e.DefaultTitle).HasMaxLength(50);
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.TransactionRegex).HasMaxLength(50);
            entity.Property(e => e.Website).HasMaxLength(50);

            entity.HasOne(d => d.Group).WithMany(p => p.BillConfigurations)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_BillConfigurations_Groups");
        });

        modelBuilder.Entity<DailyAllowance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DailyAll__3214EC078F068AC8");

            entity.ToTable("DailyAllowance");

            entity.Property(e => e.Allowance).HasColumnType("money");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.GroupId).HasColumnName("GroupID");

            entity.HasOne(d => d.Group).WithMany(p => p.DailyAllowances)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DailyAllowance_Groups");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Income>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Income__3214EC07ED3D60D2");

            entity.ToTable("Income");

            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Incomes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Income_Users");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(e => e.AccountName).HasMaxLength(25);
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Deposit).HasColumnType("money");
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Withdrawal).HasColumnType("money");

            entity.HasOne(d => d.Group).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transactions_Groups");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC0782CF78EA");

            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Group).WithMany(p => p.Users)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("GroupId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
