using LogicLib1.Models.App;

namespace LogicLib1.Services.AuthService;

public interface IAuthPersistence
{
    Task SaveAsync(AuthResult auth, DateTime expiresAt);
    Task<(AuthResult? Auth, DateTime ExpiresAt)> TryLoadAsync();
    void Clear();
}
