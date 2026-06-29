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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }

    private Task NavigateToAuth()
        => Navigation.PushModalAsync(_sp.GetRequiredService<AuthPage>());

    private async Task NavigateToPdfViewer(BookMetadata book, string docUid, string pdfUrl)
    {
        var page = _sp.GetRequiredService<PdfViewerPage>();
        page.Initialize(book, docUid, pdfUrl, _vm.AllBooks);
        await Navigation.PushModalAsync(page);
    }
}
