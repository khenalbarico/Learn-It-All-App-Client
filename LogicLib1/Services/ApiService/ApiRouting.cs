using LogicLib1.Models.Api;

namespace LogicLib1.Services.ApiService;

public class ApiRouting
{
    public static readonly Dictionary<ApiFunctions, string> Routes = new()
    {
        [ApiFunctions.TryGetUser]  = "TryGetUser",
        [ApiFunctions.GetAllBooks] = "GetAllBooks",
    };
}
