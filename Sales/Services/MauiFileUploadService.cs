using Sales.Shared.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Sales.Services;

/// <summary>
/// تنفيذ رفع الصور لتطبيق MAUI - يرفع الصور إلى السيرفر
/// </summary>
public class MauiFileUploadService : IFileUploadService
{
    private readonly HttpClient _http;

    public MauiFileUploadService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> SaveImageAsync(Stream imageStream, string fileName)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(imageStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // أو استخراج النوع من fileName
            
            content.Add(streamContent, "file", fileName);

            var response = await _http.PostAsync("api/products/upload", content);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UploadResult>();
                return result?.Path ?? "";
            }
            
            return "";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Upload failed: {ex.Message}");
            return "";
        }
    }

    private class UploadResult { public string Path { get; set; } = ""; }
}
