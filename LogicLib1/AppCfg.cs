using LogicLib1.Services.AuthService;

namespace LogicLib1;

public class AppCfg : IFirebaseCfg
{
    public string ApiKey     { get; set; } = "AIzaSyDLf_5UWARCw-6TzIHVaMbO75hAYluCayc";
    public string AuthDomain { get; set; } = "learnitallapp.firebaseapp.com";
}
