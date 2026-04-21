using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace CasaDeLasTortas.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string filePath);
        string GetFileUrl(string filePath);
        bool FileExists(string filePath);
        Task<string> UploadFileAsync(IFormFile file, string folder);
        
        //  Método para guardar archivos desde Base64 (móvil/cámara)
        Task<string> SaveBase64FileAsync(string base64Data, string folder, string fileName);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("El archivo está vacío");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(extension))
                    throw new InvalidOperationException($"Extensión no permitida: {extension}");

                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (file.Length > maxFileSize)
                    throw new InvalidOperationException("El archivo excede 5MB");

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return $"/uploads/{folder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar archivo");
                throw;
            }
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("El archivo está vacío");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(extension))
                    throw new InvalidOperationException($"Extensión no permitida: {extension}");

                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (file.Length > maxFileSize)
                    throw new InvalidOperationException("El archivo excede 5MB");

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return $"/uploads/{folder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar archivo");
                throw;
            }
        }

        /// <summary>
        ///  Guarda un archivo desde una cadena Base64
        /// Útil para uploads desde móvil/cámara
        /// </summary>
        public async Task<string> SaveBase64FileAsync(string base64Data, string folder, string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(base64Data))
                    throw new ArgumentException("Los datos Base64 están vacíos");

                // Remover el prefijo data:image/xxx;base64, si existe
                var base64Clean = base64Data;
                if (base64Data.Contains(","))
                {
                    base64Clean = base64Data.Split(',')[1];
                }

                // Decodificar Base64 a bytes
                byte[] fileBytes;
                try
                {
                    fileBytes = Convert.FromBase64String(base64Clean);
                }
                catch (FormatException)
                {
                    throw new ArgumentException("El formato Base64 es inválido");
                }

                // Validar tamaño (5MB máximo)
                const long maxFileSize = 5 * 1024 * 1024;
                if (fileBytes.Length > maxFileSize)
                    throw new InvalidOperationException("El archivo excede 5MB");

                // Determinar extensión
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension))
                {
                    // Intentar detectar por el header del Base64
                    extension = DetectExtensionFromBase64(base64Data);
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".webp" };
                if (!allowedExtensions.Contains(extension))
                    throw new InvalidOperationException($"Extensión no permitida: {extension}");

                // Crear directorio si no existe
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generar nombre único
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Guardar archivo
                await File.WriteAllBytesAsync(filePath, fileBytes);

                _logger.LogInformation("Archivo Base64 guardado: {FilePath}", filePath);

                return $"/uploads/{folder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar archivo Base64");
                throw;
            }
        }

        /// <summary>
        /// Detecta la extensión del archivo basándose en el prefijo del Base64
        /// </summary>
        private string DetectExtensionFromBase64(string base64Data)
        {
            if (base64Data.StartsWith("data:image/jpeg") || base64Data.StartsWith("data:image/jpg"))
                return ".jpg";
            if (base64Data.StartsWith("data:image/png"))
                return ".png";
            if (base64Data.StartsWith("data:image/gif"))
                return ".gif";
            if (base64Data.StartsWith("data:image/webp"))
                return ".webp";
            if (base64Data.StartsWith("data:application/pdf"))
                return ".pdf";
            
            // Por defecto asumimos jpg
            return ".jpg";
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return false;

                var physicalPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

                if (File.Exists(physicalPath))
                {
                    await Task.Run(() => File.Delete(physicalPath));
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar: {filePath}");
                return false;
            }
        }

        public string GetFileUrl(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return "/images/default-placeholder.png";

            return filePath.StartsWith("/") ? filePath : $"/{filePath}";
        }

        public bool FileExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            var physicalPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            return File.Exists(physicalPath);
        }
    }
}