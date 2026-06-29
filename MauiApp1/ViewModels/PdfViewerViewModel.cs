using System.Collections.ObjectModel;
using LogicLib1.Models.App;
using LogicLib1.Services.App;

namespace MauiApp1.ViewModels;

public record LibraryBookItem(BookMetadata Book, bool IsCurrentBook);

public class PdfViewerViewModel(IAppService _appService) : BaseViewModel
{
    private string                              _bookTitle         = string.Empty;
    private string                              _viewerUrl         = string.Empty;
    private bool                                _isReady;
    private bool                                _isLibraryPanelOpen;
    private ObservableCollection<LibraryBookItem> _libraryItems    = [];

    public string BookTitle
    {
        get => _bookTitle;
        set => SetProperty(ref _bookTitle, value);
    }

    public string ViewerUrl
    {
        get => _viewerUrl;
        set => SetProperty(ref _viewerUrl, value);
    }

    public bool IsReady
    {
        get => _isReady;
        set => SetProperty(ref _isReady, value);
    }

    public bool IsLibraryPanelOpen
    {
        get => _isLibraryPanelOpen;
        set => SetProperty(ref _isLibraryPanelOpen, value);
    }

    public ObservableCollection<LibraryBookItem> LibraryItems
    {
        get => _libraryItems;
        set
        {
            SetProperty(ref _libraryItems, value);
            OnPropertyChanged(nameof(HasMultipleBooks));
        }
    }

    public bool HasMultipleBooks => _libraryItems.Count > 1;

    public Func<Task>?                    GoBack          { get; set; }
    public Func<string[], Task<string?>>? SelectDocument  { get; set; }
    public Func<string, Task>?            LoadBookUrl     { get; set; }

    public Command GoBackCommand => new(async () =>
        await (GoBack?.Invoke() ?? Task.CompletedTask));

    public Command ToggleLibraryPanelCommand => new(() =>
        IsLibraryPanelOpen = !IsLibraryPanelOpen);

    public Command CloseLibraryPanelCommand => new(() =>
        IsLibraryPanelOpen = false);

    public Command<LibraryBookItem> SwitchBookCommand => new(async (item) =>
        await SwitchToBookAsync(item.Book));

    public void Initialize(BookMetadata currentBook, string pdfUrl, IReadOnlyList<BookMetadata> allBooks)
    {
        BookTitle          = currentBook.Title ?? "Book";
        ViewerUrl          = BuildViewerUrl(pdfUrl);
        LibraryItems       = BuildLibraryItems(allBooks, currentBook.Uid);
        IsBusy             = true;
        IsReady            = false;
        IsLibraryPanelOpen = false;
        ErrorMessage       = string.Empty;
    }

    public void OnPageLoaded()
    {
        IsBusy  = false;
        IsReady = true;
    }

    public void OnPageLoadFailed()
    {
        IsBusy       = false;
        IsReady      = false;
        ErrorMessage = "Failed to load the book. Please check your connection and try again.";
    }

    private async Task SwitchToBookAsync(BookMetadata book)
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
            var titles = book.Documents.Select(d => d.Title ?? d.Uid).ToArray();
            var choice = SelectDocument is not null ? await SelectDocument(titles) : titles[0];
            if (choice is null) return;
            docUid = book.Documents.FirstOrDefault(d => (d.Title ?? d.Uid) == choice)?.Uid;
        }

        if (string.IsNullOrWhiteSpace(docUid))
        {
            ErrorMessage = "Could not determine which document to open.";
            return;
        }

        IsLibraryPanelOpen = false;
        IsBusy             = true;
        IsReady            = false;
        ErrorMessage       = string.Empty;

        try
        {
            var pdfUrl  = await _appService.GetBookUrl(book.Uid, docUid);
            var viewUrl = BuildViewerUrl(pdfUrl);
            BookTitle    = book.Title ?? "Book";
            ViewerUrl    = viewUrl;
            LibraryItems = BuildLibraryItems(LibraryItems.Select(i => i.Book).ToList(), book.Uid);
            await (LoadBookUrl?.Invoke(viewUrl) ?? Task.CompletedTask);
        }
        catch
        {
            IsBusy       = false;
            ErrorMessage = "Failed to open this book. Please try again.";
        }
    }

    private static string BuildViewerUrl(string pdfUrl)
        => $"https://docs.google.com/viewer?url={Uri.EscapeDataString(pdfUrl)}&embedded=true";

    private static ObservableCollection<LibraryBookItem> BuildLibraryItems(
        IEnumerable<BookMetadata> books, string currentBookUid)
        => new(books.Select(b => new LibraryBookItem(b, b.Uid == currentBookUid)));
}
