using MeetingApp.Models;

namespace MeetingApp.Services
{
    // Services/FileService.cs
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        // İzin verilen dosya tipleri
        private readonly string[] _allowedTypes =
            { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<Document> UploadDocumentAsync(DocumentUploadDto dto, int meetingId)
        {
            return await Task.Run(() =>
            {
                var ext = Path.GetExtension(dto.FileName).ToLower();

                if (!_allowedTypes.Contains(ext))
                    throw new Exception($"Desteklenmeyen dosya tipi: {ext}");

                var base64 = dto.Base64Content.Contains(",")
                    ? dto.Base64Content.Split(',')[1]
                    : dto.Base64Content;

                byte[] fileBytes = Convert.FromBase64String(base64);

                if (fileBytes.Length > 10 * 1024 * 1024)
                    throw new Exception("Dosya boyutu 10MB'dan büyük olamaz");

                return new Document
                {
                    FileName = dto.FileName,
                    FileType = ext,
                    FileContent = fileBytes,
                    FileSize = fileBytes.Length,
                    MeetingId = meetingId,
                    UploadedAt = DateTime.UtcNow
                };
            });
        }

        public async Task<string> UploadProfileImageAsync(string base64, string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            var allowedImages = new[] { ".jpg", ".jpeg", ".png" };

            if (!allowedImages.Contains(ext))
                throw new Exception("Profil resmi sadece .jpg, .jpeg, .png olabilir");

            var cleanBase64 = base64.Contains(",") ? base64.Split(',')[1] : base64;
            byte[] fileBytes = Convert.FromBase64String(cleanBase64);

            // Max 2MB kontrolü
            if (fileBytes.Length > 2 * 1024 * 1024)
                throw new Exception("Profil resmi 2MB'dan büyük olamaz");

            var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "profiles");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var newFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadPath, newFileName);

            await File.WriteAllBytesAsync(filePath, fileBytes);

            return $"/uploads/profiles/{newFileName}";
        }

        public void Delete(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            var fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
