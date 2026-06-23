using LogicLib1.Models.App;

namespace LogicLib1.Services.App;

public interface IAppService
{
    Task<List<BookMetadata>> GetAllBooks();
    Task<UserInfo?> TryGetUserInfo();
    Task CreateUser(string firstName, string lastName, string phoneNumber);
}