using Microsoft.EntityFrameworkCore;
using TechMove.Models;

namespace TechMove.Data
{
    public class ApplicationDbContext : DbContext

    {
        //line used to connect the DB context to the real time database, withou it it wont work
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        //add what data needs to be created and store for the database
        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }


        //methods tells the database what data is required and what we expect the database to create
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //client configuration, no need to call the IDs, the system automatically identifies the IDs
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Name);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Region).IsRequired().HasMaxLength(50);
            });

            //contract configuration, no need to call the IDs, the system automatically identifies the IDs
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasIndex(e => e.ContractNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.StartDate, e.EndDate });

                entity.Property(e => e.ContractNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ServiceLead).IsRequired().HasMaxLength(500);


                //configure relationship with client, because the contract model calls the client id (one-to-many)
                entity.HasOne(e => e.Client)
                        .WithMany(c => c.Contracts)
                        .HasForeignKey(e => e.ClientId)
                        .OnDelete(DeleteBehavior.Restrict);


                //configure contract status conversion 
                entity.Property(e => e.Status)
                        .HasConversion<string>()
                        .HasMaxLength(20);
            });

            //serviceRequest configuration, no need to call the IDs, the system automatically identifies the IDs
            modelBuilder.Entity<ServiceRequest>(entity =>
            {
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);

                entity.Property(e => e.AmountUSD).HasPrecision(18, 2);
                entity.Property(e => e.AmountZAR).HasPrecision(18, 2);
                entity.Property(e => e.ExchangeRateUsed).HasPrecision(18, 2);

                //configure relationship with contract
                entity.HasOne(e => e.Contract)
                        .WithMany(c => c.ServiceRequests)
                        .HasForeignKey(e => e.ContractId)
                        .OnDelete(DeleteBehavior.Restrict);

                //configure servicerequest status conversion
                entity.Property(e => e.Status)
                        .HasConversion<string>()
                        .HasMaxLength(20);

            });





        }



    }
}
