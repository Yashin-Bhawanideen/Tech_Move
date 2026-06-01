using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Moq;

using TechMove.Services;

namespace TechMove.Tests.Services
{
    [TestFixture]
    public class FileServiceTests
    {
        private Mock<IWebHostEnvironment> _environmentMock;
        private Mock<Microsoft.Extensions.Logging.ILogger<FileService>> _loggerMock;
        private string _testUploadPath;

        [SetUp]
        public void Setup()
        {
            _environmentMock = new Mock<IWebHostEnvironment>();
            _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<FileService>>();
            _testUploadPath = Path.GetTempPath();

            _environmentMock.Setup(e => e.WebRootPath).Returns(_testUploadPath);
        }

        [Test]
        public void ValidateFile_ValidPdfFile_ReturnsTrue()
        {
            // Arrange
            var service = new FileService(_environmentMock.Object, _loggerMock.Object);
            var file = CreateMockFormFile("test.pdf", "application/pdf", CreateSamplePdfContent());

            // Act
            var result = service.ValidateFile(file);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateFile_InvalidFileExtension_ReturnsFalse()
        {
            // Arrange
            var service = new FileService(_environmentMock.Object, _loggerMock.Object);
            var file = CreateMockFormFile("test.exe", "application/x-msdownload", new byte[] { 0x4D, 0x5A });

            // Act
            var result = service.ValidateFile(file);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateFile_InvalidMimeType_ReturnsFalse()
        {
            // Arrange
            var service = new FileService(_environmentMock.Object, _loggerMock.Object);
            var file = CreateMockFormFile("test.pdf", "image/jpeg", CreateSamplePdfContent());

            // Act
            var result = service.ValidateFile(file);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateFile_NullFile_ReturnsFalse()
        {
            // Arrange
            var service = new FileService(_environmentMock.Object, _loggerMock.Object);

            // Act
            var result = service.ValidateFile(null);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateFile_EmptyFile_ReturnsFalse()
        {
            // Arrange
            var service = new FileService(_environmentMock.Object, _loggerMock.Object);
            var file = CreateMockFormFile("test.pdf", "application/pdf", new byte[0]);

            // Act
            var result = service.ValidateFile(file);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateFile_FakePdfHeader_ReturnsFalse()
        {
            // Arrange
            var service = new FileService(_environmentMock.Object, _loggerMock.Object);
            var fakePdfContent = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // Not PDF header
            var file = CreateMockFormFile("test.pdf", "application/pdf", fakePdfContent);

            // Act
            var result = service.ValidateFile(file);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task SaveSignedAgreementAsync_ValidFile_SavesToDisk()
        {
            // Arrange
            var service = new FileService(_environmentMock.Object, _loggerMock.Object);
            var file = CreateMockFormFile("agreement.pdf", "application/pdf", CreateSamplePdfContent());

            // Act
            var result = await service.SaveSignedAgreementAsync(file, 1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain("/uploads/contracts/"));
            Assert.That(result, Does.EndWith(".pdf"));
        }

        [Test]
        public void SaveSignedAgreementAsync_InvalidFile_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = new FileService(_environmentMock.Object, _loggerMock.Object);
            var file = CreateMockFormFile("test.exe", "application/x-msdownload", new byte[] { 0x4D, 0x5A });

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.SaveSignedAgreementAsync(file, 1));

            Assert.That(exception.Message, Does.Contain("Invalid file"));
        }

        [Test]
        public void ValidateFile_PdfFileWithCorrectHeader_ReturnsTrue()
        {
            // Arrange
            var service = new FileService(_environmentMock.Object, _loggerMock.Object);
            var validPdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
            var file = CreateMockFormFile("test.pdf", "application/pdf", validPdfHeader);

            // Act
            var result = service.ValidateFile(file);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateFile_FileTooLarge_ReturnsFalse()
        {
            // Arrange
            var service = new FileService(_environmentMock.Object, _loggerMock.Object);
            var largeFile = new byte[11 * 1024 * 1024]; // 11MB (exceeds 10MB limit)
            var file = CreateMockFormFile("large.pdf", "application/pdf", largeFile);

            // Act
            var result = service.ValidateFile(file);

            // Assert
            Assert.That(result, Is.False);
        }

        private IFormFile CreateMockFormFile(string fileName, string contentType, byte[] content)
        {
            var stream = new MemoryStream(content);
            var file = new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
            return file;
        }

        private byte[] CreateSamplePdfContent()
        {
          
            byte[] pdfHeader = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
            return pdfHeader;
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