using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TechMove.API.Data;
using TechMove.API.Models;

namespace TechMove.API.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public string TestAuthToken { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add In-Memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid().ToString());
                });

                // Configure test authentication
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.ASCII.GetBytes("TestSecretKeyForIntegrationTests12345678901234567890")),
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ClockSkew = TimeSpan.Zero
                        };
                    });

                // Build service provider
                var sp = services.BuildServiceProvider();

                // Create scope and seed data
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    // Seed test data
                    SeedTestData(db);
                }
            });
        }

        private void SeedTestData(ApplicationDbContext db)
        {
            // Add test clients
            if (!db.Clients.Any())
            {
                db.Clients.AddRange(
                    new Client
                    {
                        Name = "Test Client 1",
                        Email = "test1@example.com",
                        Phone = "123456789",
                        Address = "123 Test St",
                        Region = "Africa",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Client
                    {
                        Name = "Test Client 2",
                        Email = "test2@example.com",
                        Phone = "987654321",
                        Address = "456 Test Ave",
                        Region = "Europe",
                        CreatedAt = DateTime.UtcNow
                    }
                );
                db.SaveChanges();
            }

            // Add test contracts
            if (!db.Contracts.Any())
            {
                var client = db.Clients.First();
                db.Contracts.AddRange(
                    new Contract
                    {
                        ClientId = client.ClientId,
                        ContractNumber = "CON-001",
                        ServiceLead = "John Doe",
                        StartDate = DateTime.UtcNow.AddDays(-30),
                        EndDate = DateTime.UtcNow.AddDays(30),
                        Status = ContractStatus.Active,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Contract
                    {
                        ClientId = client.ClientId,
                        ContractNumber = "CON-002",
                        ServiceLead = "Jane Smith",
                        StartDate = DateTime.UtcNow.AddDays(-60),
                        EndDate = DateTime.UtcNow.AddDays(-10),
                        Status = ContractStatus.Expired,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Contract
                    {
                        ClientId = client.ClientId,
                        ContractNumber = "CON-003",
                        ServiceLead = "Bob Johnson",
                        StartDate = DateTime.UtcNow.AddDays(-15),
                        EndDate = DateTime.UtcNow.AddDays(45),
                        Status = ContractStatus.OnHold,
                        CreatedAt = DateTime.UtcNow
                    }
                );
                db.SaveChanges();
            }

            // Add test service requests
            if (!db.ServiceRequests.Any())
            {
                var contract = db.Contracts.First(c => c.Status == ContractStatus.Active);
                db.ServiceRequests.AddRange(
                    new ServiceRequest
                    {
                        ContractId = contract.ContractId,
                        RequestTitle = "Test Request 1",
                        Description = "This is a test service request",
                        AmountUSD = 1000,
                        AmountZAR = 18500,
                        ExchangeRateUsed = 18.50m,
                        Status = RequestStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ServiceRequest
                    {
                        ContractId = contract.ContractId,
                        RequestTitle = "Test Request 2",
                        Description = "Another test service request",
                        AmountUSD = 2500,
                        AmountZAR = 46250,
                        ExchangeRateUsed = 18.50m,
                        Status = RequestStatus.InProgress,
                        CreatedAt = DateTime.UtcNow
                    }
                );
                db.SaveChanges();
            }
        }
    }
}