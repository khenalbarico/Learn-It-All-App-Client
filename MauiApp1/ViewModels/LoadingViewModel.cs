using LogicLib1.Services.App;
using LogicLib1.Services.AuthService;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public class LoadingViewModel(IAppAuthentication _auth, IAppService _appService, UserSession _userSession) : BaseViewModel
{
    private string _statusMessage = "Loading...";

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;

        StatusMessage = "Restoring session...";
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            await _auth.TryRestoreSessionAsync().WaitAsync(cts.Token);
        }
        catch { }

        if (_auth.IsAuthenticated)
        {
            StatusMessage = "Loading your profile...";
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                _userSession.UserInfo = await _appService.TryGetUserInfo().WaitAsync(cts.Token);
            }
            catch { }
        }

        IsBusy = false;
    }
}
