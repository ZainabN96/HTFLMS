using HTFLMS.Models;

namespace HTFLMS.Helpers
{
    public static class FileUploadHelper
    {
        public static void ApplyCourseStatus(Course course, string? status)
        {
            course.IsActive = true;
            course.IsPublished = status == "Active";
        }

        public static async Task<string> SaveFileAsync(
            IFormFile file,
            string folderPath,
            IWebHostEnvironment env)
        {
            var uploadsFolder = Path.Combine(env.WebRootPath, folderPath);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/" + folderPath.Replace("\\", "/") + "/" + fileName;
        }

        public static void DeleteOldFile(string? filePath, IWebHostEnvironment env)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            var cleanPath = filePath
                .TrimStart('/')
                .Replace("/", Path.DirectorySeparatorChar.ToString());

            var fullPath = Path.Combine(env.WebRootPath, cleanPath);

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        public static async Task ReplaceFileIfUploadedAsync(
            IFormFile? newFile,
            string? oldFilePath,
            string folderPath,
            IWebHostEnvironment env,
            Action<string> setNewPath)
        {
            if (newFile == null || newFile.Length == 0)
                return;

            DeleteOldFile(oldFilePath, env);

            var newPath = await SaveFileAsync(newFile, folderPath, env);
            setNewPath(newPath);
        }
    }
}