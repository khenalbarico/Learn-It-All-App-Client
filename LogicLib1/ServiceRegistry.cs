using LogicLib1.Services.ApiService;
using LogicLib1.Services.App;
using LogicLib1.Services.AuthService;
using Microsoft.Extensions.DependencyInjection;

namespace LogicLib1;

public static class ServiceRegistry
{
    public static void RegisterServices(this IServiceCollection svc)
    {
        var appCfg = new AppCfg();
        svc.AddSingleton<IFirebaseCfg>(appCfg);

        svc.AddSingleton<TokenCache>();
        svc.AddSingleton<IAppAuthentication, AppAuthentication>();
        svc.AddSingleton<IApiClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var http    = factory.CreateClient("LearnItAllApi");
            var cache   = sp.GetRequiredService<TokenCache>();
            var auth    = sp.GetRequiredService<IAppAuthentication>();

            return new ApiClient(http, cache, auth);
        });
        svc.AddSingleton<IAppService, AppService>();
    }
}