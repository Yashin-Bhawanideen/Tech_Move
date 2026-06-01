namespace TechMove.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;
        private readonly string _uploadFolder;

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
            _uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", "contracts");

            //ensure upload directory exists
            if(!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }
        }

        public bool ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;


            //check file extenstion
            var allowedExtensions = new[] { ".pdf" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                return false;


            
            if(file.ContentType != "application/pdf")
                return false;

            //check file size
            const long maxFileSize = 10 *1024 *1024;
            if(file.Length > maxFileSize)
                return false;

            
            using var reader = new BinaryReader(file.OpenReadStream());
            var header = new byte[4];
            reader.Read(header, 0, 4);
           

            //pdf file start with "%pdf
            if (header[0] != 0x25 || header[1] != 0x50 || header[2] != 0x44 || header[3] != 0x46)
                return false;

            return true;    
        
        }

        public async Task<string> SaveSignedAgreementAsync(IFormFile file, int contractId)
        {
            if (!ValidateFile(file))
                throw new InvalidOperationException("Invalid file. Only PDF files are allowed.");
            try
            {
                //generate unique filename
                var uniqueFileName = $"{contractId}_{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

                var filePath = Path.Combine(_uploadFolder, uniqueFileName);

                //save file 
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("File saved successfully: {FilePath}", filePath);


                //return relative path for database storage
                return $"/uploads/contracts/{uniqueFileName}";

            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error saving file for contract {ContractId}", contractId);
                throw;

            }
        }

        public async Task<byte[]> GetFileAsync(string filePath)
        {
            try
            {
                //convert relative URL to physical path
                var physicalPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

                if (!File.Exists(physicalPath))
                    throw new FileNotFoundException($"File not found: {filePath}");

                return await File.ReadAllBytesAsync(physicalPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file: {FilePath}", filePath);
                throw;

            }
        }

        public void DeleteFile(string filePath)
        {
            try
            {
                var physicalPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    _logger.LogInformation("File deleted: {FilePath}", filePath);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            }
        }

        
    }
}
