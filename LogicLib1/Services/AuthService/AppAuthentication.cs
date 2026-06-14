using Firebase.Auth;
using Firebase.Auth.Providers;
using LogicLib1.Models.App;
using LogicLib1.Services.ApiService;

namespace LogicLib1.Services.AuthService;

public class AppAuthentication(IFirebaseCfg _cfg, TokenCache _cache) : IAppAuthentication
{
    private readonly FirebaseAuthClient _client = _cfg.CreateAuthClient();
    private UserCredential? _credential;

    public bool IsAuthenticated => _cache.TryGet(out _);

    public async Task<AuthResult> SignInWithGoogleAsync(string idToken)
    {
        _credential = await _client.SignInWithCredentialAsync(GoogleProvider.GetCredential(idToken));
        return await BuildAndCacheAsync();
    }

    public async Task<AuthResult> SignInWithFacebookAsync(string accessToken)
    {
        _credential = await _client.SignInWithCredentialAsync(FacebookProvider.GetCredential(accessToken));
        return await BuildAndCacheAsync();
    }

    public async Task<AuthResult> SignInWithEmailAsync(string email, string password)
    {
        _credential = await _client.SignInWithEmailAndPasswordAsync(email, password);
        return await BuildAndCacheAsync();
    }

    public async Task<AuthResult> RegisterWithEmailAsync(string email, string password)
    {
        _credential = await _client.CreateUserWithEmailAndPasswordAsync(email, password);
        return await BuildAndCacheAsync();
    }

    public async Task<AuthResult> RefreshAsync()
    {
        if (_credential is null)
            throw new InvalidOperationException("No active session to refresh.");

        return await BuildAndCacheAsync();
    }

    public void SignOut()
    {
        _client.SignOut();
        _credential = null;
        _cache.Clear();
    }

    private async Task<AuthResult> BuildAndCacheAsync()
    {
        var auth = new AuthResult
        {
            Token = await _credential!.User.GetIdTokenAsync(forceRefresh: false),
            Uid   = _credential.User.Uid,
            Email = _credential.User.Info.Email ?? ""
        };

        _cache.Set(auth);
        return auth;
    }
}