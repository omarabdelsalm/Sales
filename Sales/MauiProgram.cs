using Sales.Shared.Services;
using Sales.Services;
using Sales.Shared.Data;
using Microsoft.EntityFrameworkCore;
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
        // TODO: Change this to your production API URL when deploying
        var baseAddress = DeviceInfo.Platform == DevicePlatform.Android 
            ? "https://10.0.2.2:7154" // Android Emulator access to host localhost
            : "https://localhost:7154"; 

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });

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
