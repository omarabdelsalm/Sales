using Microsoft.AspNetCore.Hosting;
using Sales.Shared.Services;

namespace Sales.Web.Services;

/// <summary>
/// تنفيذ رفع الصور للويب - يحفظ الملفات في wwwroot/uploads
/// </summary>
public class WebFileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _env;

    public WebFileUploadService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveImageAsync(Stream imageStream, string fileName)
    {
        // مجلد wwwroot/uploads الحقيقي على الخادم
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir); // ينشئ المجلد إن لم يكن موجوداً

        var ext      = Path.GetExtension(fileName);
        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, uniqueName);

        await using var fs = File.Create(filePath);
        await imageStream.CopyToAsync(fs);

        // المسار النسبي الذي يُستخدم في <img src="...">
        return $"/uploads/{uniqueName}";
    }
}
