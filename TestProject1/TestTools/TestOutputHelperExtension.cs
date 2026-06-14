using LogicLib1;
using LogicLib1.Services.AuthService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace TestProject1.TestTools;

internal static class TestOutputHelperExtension
{
    private static IHost? host;
    public static T Get<T>(this ITestOutputHelper ctx) where T : class
    {
        host ??= new HostBuilder().ConfigureServices(svc =>
        {
            var appCfg = new AppCfg();

            svc.AddSingleton<IFirebaseCfg>(appCfg);
            svc.AddSingleton<IAppAuthentication, AppAuthentication>();
        })
        .Build();

        return host.Services.GetRequiredService<T>();
    }
}
