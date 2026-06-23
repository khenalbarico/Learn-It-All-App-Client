using LogicLib1.Services.AuthService;

namespace MauiApp1.ViewModels;

public class LoadingViewModel(IAppAuthentication _auth) : BaseViewModel
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
        if (_auth.IsAuthenticated)
        {
            StatusMessage = "Refreshing session...";
            try { await _auth.RefreshAsync(); }
            catch { }
        }
        IsBusy = false;
    }
}
