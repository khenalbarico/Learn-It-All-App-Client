using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel  _vm;
    private readonly IServiceProvider _sp;

    public HomePage(HomeViewModel vm, IServiceProvider sp)
    {
        InitializeComponent();
        _vm = vm;
        _sp = sp;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Refresh();
    }

    private async void OnSignInBannerTapped(object? sender, TappedEventArgs e)
        => await Navigation.PushModalAsync(_sp.GetRequiredService<AuthPage>());
}
