using LogicLib1.Models.App;
using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class MyLibraryPage : ContentPage
{
    private readonly MyLibraryViewModel _vm;
    private readonly IServiceProvider   _sp;

    public MyLibraryPage(MyLibraryViewModel vm, IServiceProvider sp)
    {
        InitializeComponent();
        _vm = vm;
        _sp = sp;
        BindingContext = vm;

        _vm.NavigateToAuth      = NavigateToAuth;
        _vm.NavigateToPdfViewer = NavigateToPdfViewer;
        _vm.SelectDocument      = SelectDocument;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }

    private Task NavigateToAuth()
        => Navigation.PushModalAsync(_sp.GetRequiredService<AuthPage>());

    private async Task NavigateToPdfViewer(BookMetadata book, string pdfUrl)
    {
        var page = _sp.GetRequiredService<PdfViewerPage>();
        page.Initialize(book, pdfUrl, _vm.PurchasedBooks);
        await Navigation.PushModalAsync(page);
    }

    private async Task<string?> SelectDocument(string[] titles)
    {
        var cancel = "Cancel";
        var choice = await DisplayActionSheetAsync("Select a document", cancel, null, titles);
        return choice == cancel ? null : choice;
    }
}
