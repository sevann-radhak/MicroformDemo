using MicroformAzure.Functions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace MicroformAzure.Functions
{
    public class MicroformAzureContext : DbContext
    {
        public MicroformAzureContext(DbContextOptions<MicroformAzureContext> options)
            : base(options)
        { }

        public DbSet<ApplicationPayerInfoEntity> ApplicationPayerInfo { get; set; }
        public DbSet<ApplicationPayerTokenEntity> ApplicationPayerToken { get; set; }
        public DbSet<ApplicationRequestEntity> ApplicationRequest { get; set; }
        public DbSet<ApplicationSetupEntity> ApplicationSetup { get; set; }
        public DbSet<CybersourceConfigurationEntity> CybersourceConfiguration { get; set; }
        public DbSet<PaymentRequestEntity> PaymentRequest { get; set; }
        public DbSet<PaymentResultEntity> PaymentResult { get; set; }
        public DbSet<ApplicationLogsEntity> ApplicationLogs { get; set; }
        public DbSet<ScheduledPaymentsEntity> ScheduledPayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationPayerInfoEntity>()
                .HasIndex(n => n.PayerId)
                .IsUnique();

            modelBuilder.Entity<ApplicationSetupEntity>()
                .HasIndex(n => n.ApplicationName)
                .IsUnique();

            modelBuilder.Entity<CybersourceConfigurationEntity>()
                .HasIndex(n => n.MerchantID)
                .IsUnique();
        }
    }

    public class FunctionContextFactory : IDesignTimeDbContextFactory<MicroformAzureContext>
    {
        public MicroformAzureContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<MicroformAzureContext> optionsBuilder = new DbContextOptionsBuilder<MicroformAzureContext>();
            optionsBuilder.UseSqlServer(
                Environment.GetEnvironmentVariable("SqlConnectionString", EnvironmentVariableTarget.Process));

            return new MicroformAzureContext(optionsBuilder.Options);
        }
    }
}
