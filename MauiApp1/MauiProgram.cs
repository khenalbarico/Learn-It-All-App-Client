using LogicLib1;
using LogicLib1.Services.ApiService;
using Microsoft.Extensions.Logging;

namespace MauiApp1
{
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.RegisterServices();

#if DEBUG
            builder.Services.AddCustomHttpClient("localhost");
            builder.Logging.AddDebug();
#else
            builder.Services.AddCustomHttpClient("production");
#endif

            return builder.Build();
        }
    }
}