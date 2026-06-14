using LogicLib1.Models.Api;

namespace LogicLib1.Services.ApiService;

public interface IApiClient
{
    Task<T> GetAsync<T>(ApiFunctions apiFunction, object? payload = null);
    Task<T> SubmitAsync<T>(ApiFunctions apiFunction, object? payload = null);
    Task SubmitAsync(ApiFunctions apiFunction, object? payload = null);
}
