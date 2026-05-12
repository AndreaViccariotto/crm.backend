using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Infrastructure.Data;

namespace crm.backend.CRM.Application.Services
{
    public class FileService
    {
        private readonly AppDbContext _db;

        public FileService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<FileResponse?> Download(int fileId)
        {
            var file = _db.Files.FirstOrDefault(f => f.id == fileId);

            if (file == null)
                return null;

            byte[] bytes = await File.ReadAllBytesAsync(file.file_path);

            return new FileResponse
            {
                id = file.id,
                content = Convert.ToBase64String(bytes)
            };
        }

        public async Task<string> Upload(FileRequest req)
        {
            var existingFile = _db.Files.FirstOrDefault(f =>
                f.id == req.id &&
                f.entity_name == req.entity_name &&
                f.entity_id == req.entity_id);

            var safeFileName = Path.GetFileName(req.file_name);

            var directory = Path.Combine(
                "uploads",
                req.entity_name,
                req.entity_id.ToString()
            );

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";

            var filePath = Path.Combine(directory, uniqueFileName);

            byte[] fileBytes = Convert.FromBase64String(req.content);

            await File.WriteAllBytesAsync(filePath, fileBytes);

            if (existingFile != null)
            {
                if (File.Exists(existingFile.file_path))
                {
                    File.Delete(existingFile.file_path);
                }

                existingFile.file_path = filePath;
                existingFile.file_name = safeFileName;
                existingFile.uploaded_by = req.uploaded_by;
                existingFile.created_at = DateTime.UtcNow;
            }
            else
            {
                _db.Files.Add(new Domain.Entities.File
                {
                    file_path = filePath,
                    file_name = safeFileName,
                    entity_name = req.entity_name,
                    entity_id = req.entity_id,
                    uploaded_by = req.uploaded_by,
                    created_at = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            return "File caricato correttamente";
        }

        public async Task<List<FileResponse>> GetByCompanyId(int companyId) 
        {
            var files = _db.Files.Where(f => f.entity_name == "Companies" && f.entity_id == companyId).ToList();

            return files.Select(f => new FileResponse
            {
                id = f.id,
                file_name = f.file_name,
                entity_name = f.entity_name,
                entity_id = f.entity_id,
                uploaded_by = f.uploaded_by,
                created_at = f.created_at
            }).ToList();
        }

        public async Task<List<FileResponse>> GetByTaskId(int taskId)
        {
            var files = _db.Files.Where(f => f.entity_name == "Tasks" && f.entity_id == taskId).ToList();

            return files.Select(f => new FileResponse
            {
                id = f.id,
                file_name = f.file_name,
                entity_name = f.entity_name,
                entity_id = f.entity_id,
                uploaded_by = f.uploaded_by,
                created_at = f.created_at
            }).ToList();
        }

        public async Task<string> Delete(int fileId) 
        {
            var file = _db.Files.FirstOrDefault(f => f.id == fileId);

            if (file == null)
                return "File non trovato";

            if (File.Exists(file.file_path))
            {
                File.Delete(file.file_path);
            }

            _db.Files.Remove(file);
            await _db.SaveChangesAsync();

            return "File eliminato correttamente";
        }

        public async Task<string> UpdateFileName(FileRequest req) 
        {
            var file = _db.Files.FirstOrDefault(f => f.id == req.id);

            if (file == null)
                return "File non trovato";

            var safeFileName = Path.GetFileName(req.file_name);
            var directory = Path.GetDirectoryName(file.file_path);
            var newFilePath = Path.Combine(directory, $"{Guid.NewGuid()}_{safeFileName}");

            if (File.Exists(file.file_path))
            {
                File.Move(file.file_path, newFilePath);
            }

            file.file_name = safeFileName;
            file.file_path = newFilePath;
            file.created_at = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return "Nome del file aggiornato correttamente";
        }
    }
}