using LogicLib1.Models.App;

namespace LogicLib1.Services.AuthService;

public interface IAppAuthentication
{
    Task<AuthResult> SignInWithGoogleAsync(string idToken);
    Task<AuthResult> SignInWithFacebookAsync(string accessToken);
    Task<AuthResult> SignInWithEmailAsync(string email, string password);
    Task<AuthResult> RegisterWithEmailAsync(string email, string password);
    Task<AuthResult> RefreshAsync();
    bool IsAuthenticated { get; }
    void SignOut();
}