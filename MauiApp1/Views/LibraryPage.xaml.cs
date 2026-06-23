using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class LibraryPage : ContentPage
{
    private readonly LibraryViewModel _vm;
    private readonly IServiceProvider _sp;

    public LibraryPage(LibraryViewModel vm, IServiceProvider sp)
    {
        InitializeComponent();
        _vm = vm;
        _sp = sp;
        BindingContext = vm;

        _vm.NavigateToAuth   = NavigateToAuth;
        _vm.InitiatePurchase = ShowPurchasePlaceholder;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadBooksAsync();
    }

    private Task NavigateToAuth()
        => Navigation.PushModalAsync(_sp.GetRequiredService<AuthPage>());

    private Task ShowPurchasePlaceholder(LogicLib1.Models.App.BookMetadata book)
        => DisplayAlertAsync("Coming Soon", $"Google Play purchase for \"{book.Title}\" will be available soon.", "OK");
}
