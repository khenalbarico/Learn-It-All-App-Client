using System.Collections.ObjectModel;
using LogicLib1.Models.App;
using LogicLib1.Services.App;
using LogicLib1.Services.AuthService;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public class MyLibraryViewModel(IAppAuthentication _auth, UserSession _userSession, IAppService _appService) : BaseViewModel
{
    private ObservableCollection<BookMetadata> _purchasedBooks = [];

    public bool IsAuthenticated => _auth.IsAuthenticated;
    public bool IsGuest         => !_auth.IsAuthenticated;

    public ObservableCollection<BookMetadata> PurchasedBooks
    {
        get => _purchasedBooks;
        set
        {
            SetProperty(ref _purchasedBooks, value);
            OnPropertyChanged(nameof(HasBooks));
            OnPropertyChanged(nameof(ShowEmptyState));
        }
    }

    public bool HasBooks      => PurchasedBooks.Count > 0;
    public bool ShowEmptyState => !IsBusy && IsAuthenticated && !HasBooks;

    public Func<Task>?                       NavigateToAuth      { get; set; }
    public Func<BookMetadata, string, Task>? NavigateToPdfViewer { get; set; }
    public Func<string[], Task<string?>>?    SelectDocument      { get; set; }

    public Command NavigateToAuthCommand => new(async () =>
        await (NavigateToAuth?.Invoke() ?? Task.CompletedTask));

    public Command<BookMetadata> ReadCommand => new(async (book) => await ReadBookAsync(book));

    public async Task LoadAsync()
    {
        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(IsGuest));
        if (!_auth.IsAuthenticated) return;

        IsBusy       = true;
        ErrorMessage = string.Empty;
        PurchasedBooks = [];
        OnPropertyChanged(nameof(ShowEmptyState));

        try
        {
            var hasLibrary = (_userSession.UserInfo?.Library.Count ?? 0) > 0;

            if (hasLibrary)
            {
                var books      = await _appService.GetMyLibraryBooks();
                PurchasedBooks = new ObservableCollection<BookMetadata>(books);
            }
        }
        catch
        {
            ErrorMessage = "Failed to load your library. Please check your connection and try again.";
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(ShowEmptyState));
        }
    }

    private async Task ReadBookAsync(BookMetadata book)
    {
        if (book.Documents.Count == 0)
        {
            ErrorMessage = "This book has no readable documents.";
            return;
        }

        string? docUid;

        if (book.Documents.Count == 1)
        {
            docUid = book.Documents[0].Uid;
        }
        else
        {
            var titles  = book.Documents.Select(d => d.Title ?? d.Uid).ToArray();
            var choice  = SelectDocument is not null ? await SelectDocument(titles) : titles[0];
            if (choice is null) return;
            docUid = book.Documents.FirstOrDefault(d => (d.Title ?? d.Uid) == choice)?.Uid;
        }

        if (string.IsNullOrWhiteSpace(docUid))
        {
            ErrorMessage = "Could not determine which document to open.";
            return;
        }

        IsBusy       = true;
        ErrorMessage = string.Empty;
        OnPropertyChanged(nameof(ShowEmptyState));

        try
        {
            var pdfUrl = await _appService.GetBookUrl(book.Uid, docUid);
            await (NavigateToPdfViewer?.Invoke(book, pdfUrl) ?? Task.CompletedTask);
        }
        catch
        {
            ErrorMessage = "Failed to open the book. Please check your connection and try again.";
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(ShowEmptyState));
        }
    }
}
