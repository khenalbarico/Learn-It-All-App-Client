using System.Text;
using System.Text.Json;
using Firebase.Auth;
using Firebase.Auth.Providers;
using LogicLib1.Models.App;
using LogicLib1.Services.ApiService;

namespace LogicLib1.Services.AuthService;

public class AppAuthentication(IFirebaseCfg _cfg, TokenCache _cache) : IAppAuthentication
{
    private readonly FirebaseAuthClient _client     = _cfg.CreateAuthClient();
    private readonly HttpClient         _httpClient = new();
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
        try
        {
            _credential = await _client.SignInWithEmailAndPasswordAsync(email, password);
        }
        catch (FirebaseAuthException ex)
        {
            throw new Exception(FirebaseErrorTranslator.Translate(ex));
        }

        if (!_credential.User.Info.IsEmailVerified)
            throw new EmailNotVerifiedException();

        return await BuildAndCacheAsync();
    }

    public async Task RegisterWithEmailAsync(string email, string password)
    {
        try
        {
            _credential = await _client.CreateUserWithEmailAndPasswordAsync(email, password);
        }
        catch (FirebaseAuthException ex)
        {
            throw new Exception(FirebaseErrorTranslator.Translate(ex));
        }

        await SendVerificationEmailInternalAsync();
    }

    public async Task<AuthResult> CheckEmailVerifiedAsync()
    {
        if (_credential is null)
            throw new InvalidOperationException("No pending session to verify.");

        var idToken = await _credential.User.GetIdTokenAsync(forceRefresh: true);

        if (!ExtractEmailVerifiedClaim(idToken))
            throw new EmailNotVerifiedException();

        return await BuildAndCacheAsync();
    }

    public async Task SendEmailVerificationAsync()
    {
        if (_credential is null)
            throw new InvalidOperationException("No active user to send verification to.");

        await SendVerificationEmailInternalAsync();
    }

    public async Task<AuthResult> RefreshAsync()
    {
        var user = _credential?.User ?? _client.User;
        if (user is null)
            throw new InvalidOperationException("No active session to refresh.");

        var auth = new AuthResult
        {
            Token = await user.GetIdTokenAsync(forceRefresh: true),
            Uid   = user.Uid,
            Email = user.Info.Email ?? ""
        };

        await _cache.SetAsync(auth);
        return auth;
    }

    public Task<bool> TryRestoreSessionAsync()
        => _cache.TryRestoreAsync();

    public void SignOut()
    {
        _client.SignOut();
        _credential = null;
        _cache.Clear();
    }

    private async Task SendVerificationEmailInternalAsync()
    {
        var idToken = await _credential!.User.GetIdTokenAsync(forceRefresh: false);
        var body    = JsonSerializer.Serialize(new { requestType = "VERIFY_EMAIL", idToken });
        using var response = await _httpClient.PostAsync(
            $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_cfg.ApiKey}",
            new StringContent(body, Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
    }

    private static bool ExtractEmailVerifiedClaim(string idToken)
    {
        var parts = idToken.Split('.');
        if (parts.Length != 3) return false;

        var base64 = parts[1].Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');

        var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("email_verified", out var prop) && prop.GetBoolean();
    }

    private async Task<AuthResult> BuildAndCacheAsync()
    {
        var auth = new AuthResult
        {
            Token = await _credential!.User.GetIdTokenAsync(forceRefresh: false),
            Uid   = _credential.User.Uid,
            Email = _credential.User.Info.Email ?? ""
        };

        await _cache.SetAsync(auth);
        return auth;
    }
}
