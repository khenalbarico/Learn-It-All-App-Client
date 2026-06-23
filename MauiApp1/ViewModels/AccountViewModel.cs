using LogicLib1.Models.App;
using LogicLib1.Services.App;
using LogicLib1.Services.AuthService;

namespace MauiApp1.ViewModels;

public class AccountViewModel(IAppService _appService, IAppAuthentication _auth) : BaseViewModel
{
    private UserInfo? _userInfo;

    public UserInfo? UserInfo
    {
        get => _userInfo;
        set
        {
            SetProperty(ref _userInfo, value);
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(SubscriptionLabel));
            OnPropertyChanged(nameof(IsPremium));
        }
    }

    public bool   IsAuthenticated  => _auth.IsAuthenticated;
    public bool   IsGuest          => !_auth.IsAuthenticated;
    public string DisplayName      => $"{_userInfo?.FirstName} {_userInfo?.LastName}".Trim().NullIfEmpty() ?? "User";
    public string SubscriptionLabel => _userInfo?.Subscription == UserSubscription.Premium ? "Premium" : "Free Plan";
    public bool   IsPremium        => _userInfo?.Subscription == UserSubscription.Premium;

    public Func<Task>? NavigateToAuth { get; set; }

    public Command NavigateToAuthCommand => new(async () =>
        await (NavigateToAuth?.Invoke() ?? Task.CompletedTask));

    public Command SignOutCommand => new(() =>
    {
        _auth.SignOut();
        UserInfo = null;
        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(IsGuest));
    });

    public async Task LoadAsync()
    {
        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(IsGuest));
        if (!_auth.IsAuthenticated) return;

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            UserInfo = await _appService.TryGetUserInfo();
        }
        catch
        {
            ErrorMessage = "Failed to load account info.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

internal static class StringExtensions
{
    public static string? NullIfEmpty(this string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;
}
