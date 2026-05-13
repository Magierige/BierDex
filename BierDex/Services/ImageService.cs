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
            // 1. Check of het bestand leeg is
            if (file == null || file.Length == 0)
                throw new ArgumentException("Bestand is leeg.");

            // 2. Check de extensie
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_permittedExtensions.Contains(extension))
                throw new InvalidOperationException("Ongeldig bestandstype. Alleen JPG, PNG en WebP zijn toegestaan.");

            // 3. Optioneel: Check bestandsgrootte (bijv. max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("Bestand is te groot (max 5MB).");

            // 4. Bepaal het pad (wwwroot/uploads/folderName)
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", folderName);

            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // 5. Genereer een unieke bestandsnaam om conflicten te voorkomen
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // 6. Opslaan naar schijf
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 7. Return het relatieve pad voor opslag in de database
            return $"/uploads/{folderName}/{fileName}";
        }
    }
}
