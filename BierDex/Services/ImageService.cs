namespace BierDex.Services
{
    public class ImageService 
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _permittedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Bestand is leeg.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_permittedExtensions.Contains(extension))
                throw new InvalidOperationException("Ongeldig bestandstype. Alleen JPG, PNG en WebP zijn toegestaan.");

            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("Bestand is te groot (max 5MB).");

            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", folderName);

            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{folderName}/{fileName}";
        }
    }
}
