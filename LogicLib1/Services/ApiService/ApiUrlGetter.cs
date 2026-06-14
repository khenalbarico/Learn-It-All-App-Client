namespace LogicLib1.Services.ApiService;

public class ApiUrlGetter : IApiUrlGetter
{
    public string GetApiUrl(string env)
    {
        env = env.Trim().ToLowerInvariant();

        return env switch
        {
            "localhost"  => "http://localhost:7041",
            "production" => "https://sample.azurewebsites.net/",
            _            => throw new ArgumentException($"Unknown environment: {env}")
        };
    }
}