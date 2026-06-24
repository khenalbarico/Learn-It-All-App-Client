namespace LogicLib1.Services.ApiService;

public class ApiUrlGetter : IApiUrlGetter
{
    public string GetApiUrl(string env)
    {
        env = env.Trim().ToLowerInvariant();

        return env switch
        {
            "localhost"        => "http://10.0.2.2:7041/api/",
            "localhost-device" => "http://192.168.1.3:7041/api/",
            "production"       => "https://learn-it-all-api-dev1.azurewebsites.net/api/",
            _                  => throw new ArgumentException($"Unknown environment: {env}")
        };
    }
}