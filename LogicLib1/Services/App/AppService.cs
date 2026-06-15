using LogicLib1.Models.Api;
using LogicLib1.Models.App;
using LogicLib1.Services.ApiService;
using System.Net;

namespace LogicLib1.Services.App;

public class AppService(IApiClient _api) : IAppService
{
    public async Task<List<BookMetadata>> GetAllBooks()
    {
        return await _api.GetAsync<List<BookMetadata>>(ApiFunctions.GetAllBooks);
    }

    public async Task<UserInfo?> TryGetUserInfo()
    {
        try
        {
            return await _api.GetAsync<UserInfo>(ApiFunctions.TryGetUser);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}