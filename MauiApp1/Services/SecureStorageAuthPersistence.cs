using LogicLib1.Models.App;
using LogicLib1.Services.AuthService;

namespace MauiApp1.Services;

public class SecureStorageAuthPersistence : IAuthPersistence
{
    private const string TokenKey  = "auth_token";
    private const string UidKey    = "auth_uid";
    private const string EmailKey  = "auth_email";
    private const string ExpiryKey = "auth_expiry_ticks";

    public async Task SaveAsync(AuthResult auth, DateTime expiresAt)
    {
        await SecureStorage.Default.SetAsync(TokenKey, auth.Token);
        Preferences.Default.Set(UidKey,    auth.Uid);
        Preferences.Default.Set(EmailKey,  auth.Email);
        Preferences.Default.Set(ExpiryKey, expiresAt.Ticks);
    }

    public async Task<(AuthResult? Auth, DateTime ExpiresAt)> TryLoadAsync()
    {
        var token = await SecureStorage.Default.GetAsync(TokenKey);
        if (string.IsNullOrEmpty(token))
            return (null, DateTime.MinValue);

        var uid   = Preferences.Default.Get(UidKey,    string.Empty);
        var email = Preferences.Default.Get(EmailKey,  string.Empty);
        var ticks = Preferences.Default.Get(ExpiryKey, 0L);

        if (ticks == 0 || string.IsNullOrEmpty(uid))
            return (null, DateTime.MinValue);

        var expiresAt = new DateTime(ticks, DateTimeKind.Utc);
        return (new AuthResult { Token = token, Uid = uid, Email = email }, expiresAt);
    }

    public void Clear()
    {
        SecureStorage.Default.Remove(TokenKey);
        Preferences.Default.Remove(UidKey);
        Preferences.Default.Remove(EmailKey);
        Preferences.Default.Remove(ExpiryKey);
    }
}
