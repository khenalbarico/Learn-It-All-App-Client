using System.Collections.ObjectModel;
using System.Text.Json;
using LogicLib1.Models.App;
using LogicLib1.Services.App;
using LogicLib1.Services.AuthService;

namespace MauiApp1.ViewModels;

public class LibraryViewModel(IAppService _appService, IAppAuthentication _auth) : BaseViewModel
{
    private static readonly Dictionary<string, List<BookMetadata>> _sessionCache = new();

    private ObservableCollection<BookMetadata> _books = [];
    private string  _searchText       = string.Empty;
    private string? _selectedCategory;
    private bool    _isRefreshing;

    public bool IsGuest             => !_auth.IsAuthenticated;
    public bool HasSelectedCategory => _selectedCategory is not null;
    public bool IsCollegeCourseSelected => _selectedCategory == "CollegeCourse";
    public bool IsReviewerSelected  => _selectedCategory == "Reviewer";

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

    public Func<Task>?               NavigateToAuth   { get; set; }
    public Func<BookMetadata, Task>? InitiatePurchase { get; set; }

    public Command<string> SelectCategoryCommand => new(async (category) =>
    {
        if (_selectedCategory == category) return;
        _selectedCategory = category;
        SearchText        = string.Empty;
        NotifyCategoryChanged();
        await LoadBooksByCategoryAsync(category);
    });

    public Command RefreshCommand => new(async () =>
    {
        if (_selectedCategory is null) return;
        IsRefreshing = true;
        _sessionCache.Remove(_selectedCategory);
        await LoadBooksByCategoryAsync(_selectedCategory);
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

    public void Refresh()
    {
        OnPropertyChanged(nameof(IsGuest));
    }

    private async Task LoadBooksByCategoryAsync(string category)
    {
        OnPropertyChanged(nameof(IsGuest));

        if (!_auth.IsAuthenticated)
        {
            var dummy = await LoadDummyBooksAsync();
            Books = new ObservableCollection<BookMetadata>(dummy.Where(b =>
                b.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true));
            return;
        }

        if (_sessionCache.TryGetValue(category, out var cached))
        {
            Books = new ObservableCollection<BookMetadata>(cached);
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _appService.GetBooksByCategory(category);
            _sessionCache[category] = result;
            Books = new ObservableCollection<BookMetadata>(result);
        }
        catch
        {
            var dummy = await LoadDummyBooksAsync();
            Books = new ObservableCollection<BookMetadata>(dummy.Where(b =>
                b.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true));
            ErrorMessage = "Could not connect to server. Showing sample books.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void NotifyCategoryChanged()
    {
        OnPropertyChanged(nameof(HasSelectedCategory));
        OnPropertyChanged(nameof(IsCollegeCourseSelected));
        OnPropertyChanged(nameof(IsReviewerSelected));
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
