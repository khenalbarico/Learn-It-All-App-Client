using LogicLib1;
using LogicLib1.Services.ApiService;
using LogicLib1.Services.AuthService;
using MauiApp1.Services;
using MauiApp1.ViewModels;
using MauiApp1.Views;
using Microsoft.Extensions.Logging;

namespace MauiApp1;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf",   "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf",  "OpenSansSemibold");
            });

        // Services
        builder.Services.AddSingleton<IAuthPersistence, SecureStorageAuthPersistence>();
        builder.Services.AddSingleton<UserSession>();
        builder.Services.AddSingleton<IBillingService, BillingService>();
        builder.Services.RegisterServices();

#if DEBUG
        var env = DeviceInfo.DeviceType == DeviceType.Virtual ? "localhost" : "localhost-device";
        builder.Services.AddCustomHttpClient(env);
        builder.Logging.AddDebug();
#else
        builder.Services.AddCustomHttpClient("production");
#endif

        // ViewModels
        builder.Services.AddTransient<LoadingViewModel>();
        builder.Services.AddTransient<LibraryViewModel>();
        builder.Services.AddTransient<AuthViewModel>();
        builder.Services.AddTransient<MyLibraryViewModel>();
        builder.Services.AddTransient<AccountViewModel>();
        builder.Services.AddSingleton<HomeViewModel>();

        // Views (Transient except AppShell which is Singleton)
        builder.Services.AddTransient<LoadingPage>();
        builder.Services.AddTransient<LibraryPage>();
        builder.Services.AddTransient<AuthPage>();
        builder.Services.AddTransient<MyLibraryPage>();
        builder.Services.AddTransient<AccountPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}
