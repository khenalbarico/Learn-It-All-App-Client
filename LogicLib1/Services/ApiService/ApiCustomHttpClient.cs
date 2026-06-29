using Microsoft.Extensions.DependencyInjection;

namespace LogicLib1.Services.ApiService;

public static class ApiCustomHttpClient
{
    public static void AddCustomHttpClient(this IServiceCollection svc, string env)
    {
        svc.AddSingleton<IApiUrlGetter, ApiUrlGetter>();
        svc.AddHttpClient("LearnItAllApi", (serviceProvider, client) =>
        {
            var apiUrlGetter = serviceProvider.GetRequiredService<IApiUrlGetter>();
            var apiUrl       = apiUrlGetter.GetApiUrl(env);

            client.BaseAddress = new Uri(apiUrl);
            client.Timeout     = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
    }
}