using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using LogicLib1.Models.App;
using LogicLib1.Services.App;

namespace MauiApp1.ViewModels;

public class PdfDocItem
{
    public string   Uid          { get; }
    public string   DisplayTitle { get; }
    public bool     IsCurrent    { get; }
    public ICommand OpenCommand  { get; }

    public PdfDocItem(BookDocs doc, bool isCurrent, Func<Task> open)
    {
        Uid          = doc.Uid;
        DisplayTitle = doc.Title?.Trim() is { Length: > 0 } t ? t : doc.Uid;
        IsCurrent    = isCurrent;
        OpenCommand  = new Command(async () => await open());
    }
}

public class PdfBookGroup : INotifyPropertyChanged
{
    private bool _isExpanded;

    public event PropertyChangedEventHandler? PropertyChanged;

    public BookMetadata     Book          { get; }
    public bool             IsCurrentBook { get; }
    public string           Label         { get; }
    public List<PdfDocItem> Docs          { get; private set; } = [];

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

    public PdfBookGroup(BookMetadata book, bool isCurrentBook, string currentDocUid,
        Func<BookMetadata, BookDocs, Task> switchDoc)
    {
        Book          = book;
        IsCurrentBook = isCurrentBook;
        IsExpanded    = isCurrentBook;
        Label         = book.Title?.Trim() is { Length: > 0 } t ? t : book.Category ?? book.Uid;
        Docs          = BuildDocs(book, isCurrentBook, currentDocUid, switchDoc);
        ToggleCommand = new Command(() => IsExpanded = !IsExpanded);
    }

    public void Refresh(bool isCurrentBook, string currentDocUid,
        Func<BookMetadata, BookDocs, Task> switchDoc)
    {
        Docs = BuildDocs(Book, isCurrentBook, currentDocUid, switchDoc);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Docs)));
    }

    private static List<PdfDocItem> BuildDocs(BookMetadata book, bool isCurrentBook,
        string currentDocUid, Func<BookMetadata, BookDocs, Task> switchDoc)
        => book.Documents
            .Select(d => new PdfDocItem(d, isCurrentBook && d.Uid == currentDocUid,
                () => switchDoc(book, d)))
            .ToList();
}

public class PdfViewerViewModel(IAppService _appService) : BaseViewModel
{
    private string  _bookTitle  = string.Empty;
    private string  _docTitle   = string.Empty;
    private string  _viewerUrl  = string.Empty;
    private bool    _isReady;
    private bool    _isPanelOpen;
    private ObservableCollection<PdfBookGroup> _bookGroups = [];

    private BookMetadata? _currentBook;
    private string        _currentDocUid = string.Empty;

    public string BookTitle
    {
        get => _bookTitle;
        set => SetProperty(ref _bookTitle, value);
    }

    public string DocTitle
    {
        get => _docTitle;
        set => SetProperty(ref _docTitle, value);
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

    public bool IsPanelOpen
    {
        get => _isPanelOpen;
        set => SetProperty(ref _isPanelOpen, value);
    }

    public ObservableCollection<PdfBookGroup> BookGroups
    {
        get => _bookGroups;
        set => SetProperty(ref _bookGroups, value);
    }

    public Func<Task>?         GoBack      { get; set; }
    public Func<string, Task>? LoadBookUrl { get; set; }

    public Command GoBackCommand      => new(async () => await (GoBack?.Invoke() ?? Task.CompletedTask));
    public Command TogglePanelCommand => new(() => IsPanelOpen = !IsPanelOpen);
    public Command ClosePanelCommand  => new(() => IsPanelOpen = false);

    public void Initialize(BookMetadata currentBook, string currentDocUid, string pdfUrl,
        IReadOnlyList<BookMetadata> allBooks)
    {
        _currentBook   = currentBook;
        _currentDocUid = currentDocUid;
        BookTitle      = currentBook.Title ?? "Book";
        DocTitle       = ResolveDocTitle(currentBook, currentDocUid);
        ViewerUrl      = BuildViewerUrl(pdfUrl);
        IsBusy         = true;
        IsReady        = false;
        IsPanelOpen    = false;
        ErrorMessage   = string.Empty;
        BookGroups     = new ObservableCollection<PdfBookGroup>(
            allBooks.Select(b => new PdfBookGroup(b, b.Uid == currentBook.Uid, currentDocUid, SwitchToDocAsync)));
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

    private async Task SwitchToDocAsync(BookMetadata book, BookDocs doc)
    {
        if (book.Uid == _currentBook?.Uid && doc.Uid == _currentDocUid)
        {
            IsPanelOpen = false;
            return;
        }

        IsPanelOpen  = false;
        IsBusy       = true;
        IsReady      = false;
        ErrorMessage = string.Empty;

        try
        {
            var pdfUrl     = await _appService.GetBookUrl(book.Uid, doc.Uid);
            _currentBook   = book;
            _currentDocUid = doc.Uid;
            BookTitle      = book.Title ?? "Book";
            DocTitle       = ResolveDocTitle(book, doc.Uid);
            ViewerUrl      = BuildViewerUrl(pdfUrl);

            foreach (var group in BookGroups)
                group.Refresh(group.Book.Uid == book.Uid, doc.Uid, SwitchToDocAsync);

            await (LoadBookUrl?.Invoke(ViewerUrl) ?? Task.CompletedTask);
        }
        catch
        {
            IsBusy       = false;
            ErrorMessage = "Failed to open this document. Please try again.";
        }
    }

    private static string BuildViewerUrl(string pdfUrl)
        => $"https://docs.google.com/viewer?url={Uri.EscapeDataString(pdfUrl)}&embedded=true";

    private static string ResolveDocTitle(BookMetadata book, string docUid)
    {
        var doc = book.Documents.FirstOrDefault(d => d.Uid == docUid);
        return doc?.Title?.Trim() is { Length: > 0 } t ? t : docUid;
    }
}
