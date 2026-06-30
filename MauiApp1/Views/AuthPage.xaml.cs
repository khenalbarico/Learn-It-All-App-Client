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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        FormCard.TranslationY = 120;
        FormCard.Opacity = 0;
        await Task.WhenAll(
            FormCard.TranslateToAsync(0, 0, 500, Easing.SpringOut),
            FormCard.FadeToAsync(1.0, 400));
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
