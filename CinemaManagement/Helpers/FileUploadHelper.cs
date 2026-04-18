using Microsoft.AspNetCore.Http;
using System;
using System.IO;

public static class FileUploadHelper
{
    public static string SaveImage(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0) return null;

        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);
        if (!Directory.Exists(uploads))
            Directory.CreateDirectory(uploads);

        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploads, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        file.CopyTo(stream);

        return $"/uploads/{folder}/{fileName}";
    }
}
