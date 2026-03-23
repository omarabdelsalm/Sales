namespace Sales.Shared.Services;

/// <summary>
/// واجهة حفظ ملفات الصور - تنفيذ مختلف في الويب والـ MAUI
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// يحفظ Stream الصورة ويرجع المسار النسبي للوصول إليها (مثلاً /uploads/abc.jpg)
    /// </summary>
    Task<string> SaveImageAsync(Stream imageStream, string fileName);
}
