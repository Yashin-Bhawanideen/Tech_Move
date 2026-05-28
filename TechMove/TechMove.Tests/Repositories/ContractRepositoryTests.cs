using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TechMove.Data;
using TechMove.Models;
using TechMove.Repositories;
using Contract = TechMove.Models.Contract;

namespace TechMove.Tests.Repositories
{
    [TestFixture]
    public class ContractRepositoryTests
    {
        private ApplicationDbContext _context;
        private ContractRepository _repository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new ContractRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task IsContractActiveForServiceAsync_ActiveContract_ReturnsTrue()
        {
            // Arrange
            var client = CreateTestClient();
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            var contract = new Contract
            {
                ClientId = client.ClientId,
                ContractNumber = "CON-001",
                ServiceLead = "Test Lead",
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(10),
                Status = ContractStatus.Active
            };
            await _context.Contracts.AddAsync(contract);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsContractActiveForServiceAsync(contract.ContractId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsContractActiveForServiceAsync_ExpiredContract_ReturnsFalse()
        {
            // Arrange
            var client = CreateTestClient();
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            var contract = new Contract
            {
                ClientId = client.ClientId,
                ContractNumber = "CON-002",
                ServiceLead = "Test Lead",
                StartDate = DateTime.UtcNow.AddDays(-20),
                EndDate = DateTime.UtcNow.AddDays(-10),
                Status = ContractStatus.Expired
            };
            await _context.Contracts.AddAsync(contract);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsContractActiveForServiceAsync(contract.ContractId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsContractActiveForServiceAsync_OnHoldContract_ReturnsFalse()
        {
            // Arrange
            var client = CreateTestClient();
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            var contract = new Contract
            {
                ClientId = client.ClientId,
                ContractNumber = "CON-003",
                ServiceLead = "Test Lead",
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(10),
                Status = ContractStatus.OnHold
            };
            await _context.Contracts.AddAsync(contract);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsContractActiveForServiceAsync(contract.ContractId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsContractActiveForServiceAsync_DraftContract_ReturnsFalse()
        {
            // Arrange
            var client = CreateTestClient();
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            var contract = new Contract
            {
                ClientId = client.ClientId,
                ContractNumber = "CON-004",
                ServiceLead = "Test Lead",
                StartDate = DateTime.UtcNow.AddDays(-10),
                EndDate = DateTime.UtcNow.AddDays(10),
                Status = ContractStatus.Draft
            };
            await _context.Contracts.AddAsync(contract);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsContractActiveForServiceAsync(contract.ContractId);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task SearchContractsAsync_ByStatus_ReturnsCorrectContracts()
        {
            // Arrange
            var client = CreateTestClient();
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            var activeContract = new Contract
            {
                ClientId = client.ClientId,
                ContractNumber = "CON-ACTIVE",
                ServiceLead = "Lead 1",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = ContractStatus.Active
            };

            var expiredContract = new Contract
            {
                ClientId = client.ClientId,
                ContractNumber = "CON-EXPIRED",
                ServiceLead = "Lead 2",
                StartDate = DateTime.UtcNow.AddDays(-60),
                EndDate = DateTime.UtcNow.AddDays(-30),
                Status = ContractStatus.Expired
            };

            await _context.Contracts.AddRangeAsync(activeContract, expiredContract);
            await _context.SaveChangesAsync();

            // Act
            var activeResults = await _repository.SearchContractsAsync(null, null, ContractStatus.Active);
            var expiredResults = await _repository.SearchContractsAsync(null, null, ContractStatus.Expired);

            // Assert
            Assert.That(activeResults.Count(), Is.EqualTo(1));
            Assert.That(activeResults.First().ContractNumber, Is.EqualTo("CON-ACTIVE"));
            Assert.That(expiredResults.Count(), Is.EqualTo(1));
            Assert.That(expiredResults.First().ContractNumber, Is.EqualTo("CON-EXPIRED"));
        }

        [Test]
        public async Task SearchContractsAsync_ByDateRange_ReturnsCorrectContracts()
        {
            // Arrange
            var client = CreateTestClient();
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            var contract1 = new Contract
            {
                ClientId = client.ClientId,
                ContractNumber = "CON-001",
                ServiceLead = "Lead 1",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                Status = ContractStatus.Active
            };

            var contract2 = new Contract
            {
                ClientId = client.ClientId,
                ContractNumber = "CON-002",
                ServiceLead = "Lead 2",
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 12, 31),
                Status = ContractStatus.Active
            };

            await _context.Contracts.AddRangeAsync(contract1, contract2);
            await _context.SaveChangesAsync();

            // Act
            var startDate = new DateTime(2024, 6, 1);
            var results = await _repository.SearchContractsAsync(startDate, null, null);

            // Assert
            Assert.That(results.Count(), Is.EqualTo(1));
            Assert.That(results.First().ContractNumber, Is.EqualTo("CON-002"));
        }

        [Test]
        public async Task GetActiveContractsAsync_ReturnsOnlyActiveContracts()
        {
            // Arrange
            var client = CreateTestClient();
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            var activeContract = new Contract
            {
                ClientId = client.ClientId,
                ContractNumber = "CON-ACTIVE",
                ServiceLead = "Lead 1",
                StartDate = DateTime.UtcNow.AddDays(-5),
                EndDate = DateTime.UtcNow.AddDays(5),
                Status = ContractStatus.Active
            };

            var expiredContract = new Contract
            {
                ClientId = client.ClientId,
                ContractNumber = "CON-EXPIRED",
                ServiceLead = "Lead 2",
                StartDate = DateTime.UtcNow.AddDays(-15),
                EndDate = DateTime.UtcNow.AddDays(-5),
                Status = ContractStatus.Expired
            };

            await _context.Contracts.AddRangeAsync(activeContract, expiredContract);
            await _context.SaveChangesAsync();

            // Act
            var activeResults = await _repository.GetActiveContractsAsync();

            // Assert
            Assert.That(activeResults.Count(), Is.EqualTo(1));
            Assert.That(activeResults.First().ContractNumber, Is.EqualTo("CON-ACTIVE"));
        }

        [Test]
        public async Task GetContractWithDetailsAsync_IncludesClientAndServiceRequests()
        {
            // Arrange
            var client = CreateTestClient();
            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            var contract = new Contract
            {
                ClientId = client.ClientId,
                ContractNumber = "CON-001",
                ServiceLead = "Test Lead",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30),
                Status = ContractStatus.Active
            };
            await _context.Contracts.AddAsync(contract);
            await _context.SaveChangesAsync();

            var serviceRequest = new ServiceRequest
            {
                ContractId = contract.ContractId,
                RequestTitle = "Test Request",
                Description = "Test Description",
                AmountUSD = 1000,
                AmountZAR = 18500,
                ExchangeRateUsed = 18.5m,
                Status = RequestStatus.Pending
            };
            await _context.ServiceRequests.AddAsync(serviceRequest);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetContractWithDetailsAsync(contract.ContractId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Client, Is.Not.Null);
            Assert.That(result.Client.Name, Is.EqualTo("Test Client"));
            Assert.That(result.ServiceRequests.Count, Is.EqualTo(1));
            Assert.That(result.ServiceRequests.First().RequestTitle, Is.EqualTo("Test Request"));
        }

        private Client CreateTestClient()
        {
            return new Client
            {
                Name = "Test Client",
                Email = "test@test.com",
                Phone = "123456789",
                Address = "Test Address",
                Region = "Test Region"
            };
        }
    }
}
//References
//Kayal, S., 2026. Fundamentals of Unit Testing: Unit Testing of MVC Application. [Online]
//Available at: https://www.c-sharpcorner.com/UploadFile/dacca2/fundamentals-of-unit-testing-unit-testing-of-mvc-applicatio/
//microsoft, 2026.Creating Unit Tests for ASP.NET MVC Applications (C#). [Online] 
//Available at: https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions-1/unit-testing/creating-unit-tests-for-asp-net-mvc-applications-cs
//Reily, J., 2013.Unit testing MVC controllers / Mocking UrlHelper. [Online]
//Available at: https://johnnyreilly.com/unit-testing-mvc-controllers-mocking
//StrangeWill, 2011.Nunit Testing MVC Site. [Online]
//Available at: https://stackoverflow.com/questions/7476041/nunit-testing-mvc-site

