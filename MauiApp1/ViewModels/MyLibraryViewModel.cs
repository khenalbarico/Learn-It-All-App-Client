using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using LogicLib1.Models.App;
using LogicLib1.Services.App;
using LogicLib1.Services.AuthService;
using MauiApp1.Services;

namespace MauiApp1.ViewModels;

public class DocItem
{
    public BookDocs Doc          { get; }
    public string   DisplayTitle { get; }
    public ICommand OpenCommand  { get; }

    public DocItem(BookDocs doc, Func<Task> open)
    {
        Doc          = doc;
        DisplayTitle = doc.Title?.Trim() is { Length: > 0 } t ? t : doc.Uid;
        OpenCommand  = new Command(async () => await open());
    }
}

public class LibraryBookGroup : INotifyPropertyChanged
{
    private bool _isExpanded;

    public event PropertyChangedEventHandler? PropertyChanged;

    public BookMetadata  Book     { get; }
    public List<DocItem> DocItems { get; }
    public string        Label    { get; }
    public int           DocCount => DocItems.Count;

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value) return;
            _isExpanded = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
        }
    }

    public ICommand ToggleCommand { get; }

    public LibraryBookGroup(BookMetadata book, Func<BookMetadata, BookDocs, Task> open)
    {
        Book  = book;
        Label = book.Title?.Trim() is { Length: > 0 } t ? t : book.Category ?? book.Uid;
        DocItems = book.Documents
            .Select(d => new DocItem(d, () => open(book, d)))
            .ToList();
        ToggleCommand = new Command(() => IsExpanded = !IsExpanded);
    }
}

public class MyLibraryViewModel(IAppAuthentication _auth, UserSession _userSession, IAppService _appService) : BaseViewModel
{
    private ObservableCollection<LibraryBookGroup> _libraryGroups = [];
    private bool _isCacheValid;

    public bool IsAuthenticated => _auth.IsAuthenticated;
    public bool IsGuest         => !_auth.IsAuthenticated;

    public ObservableCollection<LibraryBookGroup> LibraryGroups
    {
        get => _libraryGroups;
        set
        {
            SetProperty(ref _libraryGroups, value);
            OnPropertyChanged(nameof(HasBooks));
            OnPropertyChanged(nameof(ShowEmptyState));
        }
    }

    public bool HasBooks       => LibraryGroups.Count > 0;
    public bool ShowEmptyState => !IsBusy && IsAuthenticated && !HasBooks;

    public IReadOnlyList<BookMetadata> AllBooks
        => LibraryGroups.Select(g => g.Book).ToList();

    public Func<Task>?                               NavigateToAuth      { get; set; }
    public Func<BookMetadata, string, string, Task>? NavigateToPdfViewer { get; set; }

    public Command NavigateToAuthCommand => new(async () =>
        await (NavigateToAuth?.Invoke() ?? Task.CompletedTask));

    public void InvalidateCache()
    {
        _isCacheValid = false;
    }

    public async Task LoadAsync()
    {
        OnPropertyChanged(nameof(IsAuthenticated));
        OnPropertyChanged(nameof(IsGuest));
        if (!_auth.IsAuthenticated) return;

        if (_isCacheValid) return;

        IsBusy        = true;
        ErrorMessage  = string.Empty;
        LibraryGroups = [];
        OnPropertyChanged(nameof(ShowEmptyState));

        try
        {
            var hasLibrary = (_userSession.UserInfo?.Library.Count ?? 0) > 0;

            if (hasLibrary)
            {
                var books = await _appService.GetMyLibraryBooks();
                LibraryGroups = new ObservableCollection<LibraryBookGroup>(
                    books.Select(b => new LibraryBookGroup(b, ReadDocAsync)));
            }

            _isCacheValid = true;
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

    private async Task ReadDocAsync(BookMetadata book, BookDocs doc)
    {
        IsBusy       = true;
        ErrorMessage = string.Empty;
        OnPropertyChanged(nameof(ShowEmptyState));

        try
        {
            var pdfUrl = await _appService.GetBookUrl(book.Uid, doc.Uid);
            await (NavigateToPdfViewer?.Invoke(book, doc.Uid, pdfUrl) ?? Task.CompletedTask);
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
