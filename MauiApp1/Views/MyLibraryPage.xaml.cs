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

        _vm.NavigateToAuth = NavigateToAuth;
        _vm.NavigateToRead = ShowReadPlaceholder;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }

    private Task NavigateToAuth()
        => Navigation.PushModalAsync(_sp.GetRequiredService<AuthPage>());

    private Task ShowReadPlaceholder(LogicLib1.Models.App.BookMetadata book)
        => DisplayAlertAsync("Coming Soon", $"Reading \"{book.Title}\" will be available soon.", "OK");
}
