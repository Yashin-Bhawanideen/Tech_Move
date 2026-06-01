using Microsoft.EntityFrameworkCore;
using TechMove.API.Models;

namespace TechMove.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Client Configuration
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.ClientId);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Name);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Region).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Contract Configuration
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(e => e.ContractId);
                entity.HasIndex(e => e.ContractNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.StartDate, e.EndDate });

                entity.Property(e => e.ContractNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ServiceLead).IsRequired().HasMaxLength(500);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.TermsAndConditions).HasMaxLength(1000);
                entity.Property(e => e.SignedAgreementPath).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Configure relationship with Client
                entity.HasOne(e => e.Client)
                      .WithMany(c => c.Contracts)
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Configure status conversion (store enum as string)
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);
            });

            // ServiceRequest Configuration
            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasKey(e => e.ServiceRequestId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);

                entity.Property(e => e.RequestTitle).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.AmountUSD).HasPrecision(18, 2);
                entity.Property(e => e.AmountZAR).HasPrecision(18, 2);
                entity.Property(e => e.ExchangeRateUsed).HasPrecision(10, 4);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Configure relationship with Contract
                entity.HasOne(e => e.Contract)
                      .WithMany(c => c.ServiceRequests)
                      .HasForeignKey(e => e.ContractId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Configure status conversion (store enum as string)
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);
            });
        }
    }
}