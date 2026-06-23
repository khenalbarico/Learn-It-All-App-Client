using System.Collections.ObjectModel;
using LogicLib1.Models.App;
using LogicLib1.Services.App;
using LogicLib1.Services.AuthService;

namespace MauiApp1.ViewModels;

public class MyLibraryViewModel(IAppService _appService, IAppAuthentication _auth) : BaseViewModel
{
    private ObservableCollection<BookMetadata> _purchasedBooks = [];

    public bool IsAuthenticated => _auth.IsAuthenticated;
    public bool IsGuest         => !_auth.IsAuthenticated;

    public ObservableCollection<BookMetadata> PurchasedBooks
    {
        get => _purchasedBooks;
        set => SetProperty(ref _purchasedBooks, value);
    }

    public bool HasBooks => PurchasedBooks.Count > 0;

    public Func<Task>?              NavigateToAuth { get; set; }
    public Func<BookMetadata, Task>? NavigateToRead { get; set; }

    public Command NavigateToAuthCommand => new(async () =>
        await (NavigateToAuth?.Invoke() ?? Task.CompletedTask));

    public Command<BookMetadata> ReadCommand => new(async (book) =>
        await (NavigateToRead?.Invoke(book) ?? Task.CompletedTask));

    public async Task LoadAsync()
    {
        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(IsGuest));
        if (!_auth.IsAuthenticated) return;

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var userInfo = await _appService.TryGetUserInfo();
            PurchasedBooks = new ObservableCollection<BookMetadata>();
            OnPropertyChanged(nameof(HasBooks));
        }
        catch
        {
            ErrorMessage = "Failed to load your library.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
