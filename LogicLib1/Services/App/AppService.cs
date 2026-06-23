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

    public async Task<List<BookMetadata>> GetBooksByCategory(string category)
    {
        return await _api.GetAsync<List<BookMetadata>>(ApiFunctions.GetBooksByCategory, new { Category = category });
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

    public Task CreateUser(string firstName, string lastName, string phoneNumber)
        => _api.SubmitAsync(ApiFunctions.CreateUser, new
        {
            FirstName   = firstName,
            LastName    = lastName,
            PhoneNumber = phoneNumber
        });
}