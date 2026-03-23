using Sales.Shared.Services;

namespace Sales.Services;

/// <summary>
/// تنفيذ رفع الصور لتطبيق MAUI - يحفظ الملفات محلياً في مجلد البيانات الخاص بالتطبيق
/// </summary>
public class MauiFileUploadService : IFileUploadService
{
    public async Task<string> SaveImageAsync(Stream imageStream, string fileName)
    {
        // في تطبيق الموبايل، نحفظ الصور في مجلد البيانات المحلي
        var uploadsDir = Path.Combine(FileSystem.AppDataDirectory, "uploads");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(fileName);
        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, uniqueName);

        using var fs = File.Create(filePath);
        await imageStream.CopyToAsync(fs);

        // ملاحظة: في MAUI Blazor، قد نحتاج لمعاملة خاصة لعرض الصور المحلية
        // ولكن للتبسيط حالياً نعيد المسار الكامل أو نستخدم بروتوكول مخصص
        return filePath; 
    }
}
