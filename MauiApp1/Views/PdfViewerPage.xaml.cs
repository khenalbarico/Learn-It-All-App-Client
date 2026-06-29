using LogicLib1.Models.App;
using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class PdfViewerPage : ContentPage
{
    private readonly PdfViewerViewModel _vm;
    private bool _panelAnimating;

    public PdfViewerPage(PdfViewerViewModel vm)
    {
        InitializeComponent();
        _vm            = vm;
        BindingContext = vm;

        _vm.GoBack = () => Navigation.PopModalAsync();

        _vm.LoadBookUrl = async url =>
        {
            PdfWebView.Source = new UrlWebViewSource { Url = url };
            await Task.CompletedTask;
        };

        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PdfViewerViewModel.IsPanelOpen))
                _ = AnimatePanelAsync(_vm.IsPanelOpen);
        };

        SwitcherPanel.TranslationY = 500;
    }

    public void Initialize(BookMetadata currentBook, string currentDocUid, string pdfUrl,
        IReadOnlyList<BookMetadata> allBooks)
    {
        _vm.Initialize(currentBook, currentDocUid, pdfUrl, allBooks);
        PdfWebView.Source = new UrlWebViewSource { Url = _vm.ViewerUrl };
    }

    private async Task AnimatePanelAsync(bool open)
    {
        if (_panelAnimating) return;
        _panelAnimating = true;

        if (open)
        {
            PanelOverlay.IsVisible = true;
            PanelOverlay.Opacity   = 0;

            await Task.WhenAll(
                PanelOverlay.FadeToAsync(1, 220, Easing.CubicOut),
                SwitcherPanel.TranslateToAsync(0, 0, 280, Easing.CubicOut));
        }
        else
        {
            await Task.WhenAll(
                PanelOverlay.FadeToAsync(0, 200, Easing.CubicIn),
                SwitcherPanel.TranslateToAsync(0, 500, 260, Easing.CubicIn));

            PanelOverlay.IsVisible = false;
        }

        _panelAnimating = false;
    }

    private void OnOverlayTapped(object? sender, TappedEventArgs e)
        => _vm.IsPanelOpen = false;

    private void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
    }

    private void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (e.Result == WebNavigationResult.Success)
            _vm.OnPageLoaded();
        else
            _vm.OnPageLoadFailed();
    }
}
