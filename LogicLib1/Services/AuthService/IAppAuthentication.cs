using LogicLib1.Models.App;

namespace LogicLib1.Services.AuthService;

public interface IAppAuthentication
{
    Task<AuthResult> SignInWithGoogleAsync(string idToken);
    Task<AuthResult> SignInWithFacebookAsync(string accessToken);
    Task<AuthResult> SignInWithEmailAsync(string email, string password);
    Task RegisterWithEmailAsync(string email, string password);
    Task<AuthResult> CheckEmailVerifiedAsync();
    Task SendEmailVerificationAsync();
    Task<AuthResult> RefreshAsync();
    Task<bool> TryRestoreSessionAsync();
    bool IsAuthenticated { get; }
    void SignOut();
}