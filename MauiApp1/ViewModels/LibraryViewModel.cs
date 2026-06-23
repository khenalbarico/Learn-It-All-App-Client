using System.Collections.ObjectModel;
using System.Text.Json;
using LogicLib1.Models.App;
using LogicLib1.Services.App;
using LogicLib1.Services.AuthService;

namespace MauiApp1.ViewModels;

public class LibraryViewModel(IAppService _appService, IAppAuthentication _auth) : BaseViewModel
{
    private ObservableCollection<BookMetadata> _books = [];
    private string _searchText = string.Empty;
    private bool   _isRefreshing;

    public bool IsGuest => !_auth.IsAuthenticated;

    public ObservableCollection<BookMetadata> Books
    {
        get => _books;
        set
        {
            SetProperty(ref _books, value);
            OnPropertyChanged(nameof(FilteredBooks));
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            OnPropertyChanged(nameof(FilteredBooks));
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public IEnumerable<BookMetadata> FilteredBooks =>
        string.IsNullOrWhiteSpace(SearchText)
            ? Books
            : Books.Where(b =>
                b.Title?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                b.Category?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);

    public Func<Task>?              NavigateToAuth  { get; set; }
    public Func<BookMetadata, Task>? InitiatePurchase { get; set; }

    public Command RefreshCommand => new(async () =>
    {
        IsRefreshing = true;
        await LoadBooksAsync();
        IsRefreshing = false;
    });

    public Command<BookMetadata> BuyCommand => new(async (book) =>
    {
        if (!_auth.IsAuthenticated)
        {
            await (NavigateToAuth?.Invoke() ?? Task.CompletedTask);
            return;
        }
        await (InitiatePurchase?.Invoke(book) ?? Task.CompletedTask);
    });

    public async Task LoadBooksAsync()
    {
        OnPropertyChanged(nameof(IsGuest));
        if (!_auth.IsAuthenticated)
        {
            Books = new ObservableCollection<BookMetadata>(await LoadDummyBooksAsync());
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _appService.GetAllBooks();
            Books = new ObservableCollection<BookMetadata>(result);
        }
        catch
        {
            Books = new ObservableCollection<BookMetadata>(await LoadDummyBooksAsync());
            ErrorMessage = "Could not connect to server. Showing sample books.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task<List<BookMetadata>> LoadDummyBooksAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("dummy_books.json");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return await JsonSerializer.DeserializeAsync<List<BookMetadata>>(stream, options) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
