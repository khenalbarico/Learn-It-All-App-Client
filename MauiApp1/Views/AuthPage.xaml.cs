using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class AuthPage : ContentPage
{
    private readonly AuthViewModel _vm;

    public AuthPage(AuthViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
        _vm.AuthCompleted = OnAuthCompleted;
    }

    protected override bool OnBackButtonPressed()
    {
        if (_vm.IsProfileSetupMode)
            return true;

        return base.OnBackButtonPressed();
    }

    private async Task OnAuthCompleted()
        => await Navigation.PopModalAsync();

    private async void OnCloseClicked(object? sender, EventArgs e)
        => await Navigation.PopModalAsync();
}
