using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class AccountPage : ContentPage
{
    private readonly AccountViewModel _vm;
    private readonly IServiceProvider _sp;

    public AccountPage(AccountViewModel vm, IServiceProvider sp)
    {
        InitializeComponent();
        _vm = vm;
        _sp = sp;
        BindingContext = vm;

        _vm.NavigateToAuth = NavigateToAuth;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }

    private Task NavigateToAuth()
        => Navigation.PushModalAsync(_sp.GetRequiredService<AuthPage>());
}
