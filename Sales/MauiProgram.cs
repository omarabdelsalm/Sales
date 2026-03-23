using Sales.Shared.Services;
using Sales.Services;
using Sales.Shared.Data;
using Microsoft.Extensions.Logging;

namespace Sales;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add device-specific services used by the Sales.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddScoped<IFileUploadService, MauiFileUploadService>();

        // Configure HttpClient for API communication
        var baseAddress = "https://sales3abed.runasp.net/"; 

        builder.Services.AddScoped(sp => 
        {
            var handler = new HttpClientHandler();
            // Ignore SSL certificate errors for localhost in debug/dev
            if (baseAddress.Contains("localhost") || baseAddress.Contains("10.0.2.2"))
            {
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }
            return new HttpClient(handler) { BaseAddress = new Uri(baseAddress) };
        });

        // Add Data Service (API Client for MAUI)
        builder.Services.AddScoped<IDataService, ApiDataService>();

        // Auth service
        builder.Services.AddScoped<AuthService>();

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddTransient<MainPage>();


#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
