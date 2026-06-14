using LogicLib1.Models.App;

namespace LogicLib1.Services.ApiService;

public class TokenCache
{
    private AuthResult? _cached;
    private DateTime _expiresAt                   = DateTime.MinValue;
    private static readonly TimeSpan SafetyBuffer = TimeSpan.FromMinutes(5);

    public bool TryGet(out AuthResult? result)
    {
        if (_cached is not null && DateTime.UtcNow < _expiresAt)
        {
            result = _cached;
            return true;
        }

        result = null;
        return false;
    }

    public void Set(AuthResult auth)
    {
        _cached    = auth;
        _expiresAt = DateTime.UtcNow.Add(TimeSpan.FromHours(1) - SafetyBuffer);
    }

    public void Clear()
    {
        _cached    = null;
        _expiresAt = DateTime.MinValue;
    }
}