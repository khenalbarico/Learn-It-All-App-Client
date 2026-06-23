using LogicLib1.Services.AuthService;

namespace MauiApp1.ViewModels;

public class HomeViewModel(IAppAuthentication _auth) : BaseViewModel
{
    public bool IsGuest         => !_auth.IsAuthenticated;
    public bool IsAuthenticated =>  _auth.IsAuthenticated;

    public void Refresh()
    {
        OnPropertyChanged(nameof(IsGuest));
        OnPropertyChanged(nameof(IsAuthenticated));
    }

    public Command BrowseLibraryCommand => new(async () =>
        await Shell.Current.GoToAsync("//library"));

    public Command MyLibraryCommand => new(async () =>
        await Shell.Current.GoToAsync("//mylibrary"));
}
