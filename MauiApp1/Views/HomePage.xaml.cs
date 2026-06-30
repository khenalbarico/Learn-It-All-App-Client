using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel  _vm;
    private readonly IServiceProvider _sp;
    private bool _hasAnimated;

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
        if (!_hasAnimated)
        {
            _hasAnimated = true;
            _ = AnimateEntranceAsync();
        }
    }

    private async Task AnimateEntranceAsync()
    {
        BrowseCard.TranslationX = 80;
        BrowseCard.Opacity = 0;
        MyLibCard.TranslationX = 80;
        MyLibCard.Opacity = 0;
        ComingSoonSection.TranslationY = 40;
        ComingSoonSection.Opacity = 0;

        await Task.Delay(200);

        await Task.WhenAll(
            BrowseCard.TranslateToAsync(0, 0, 400, Easing.SpringOut),
            BrowseCard.FadeToAsync(1.0, 300));

        await Task.WhenAll(
            MyLibCard.TranslateToAsync(0, 0, 400, Easing.SpringOut),
            MyLibCard.FadeToAsync(1.0, 300));

        await Task.WhenAll(
            ComingSoonSection.TranslateToAsync(0, 0, 350, Easing.CubicOut),
            ComingSoonSection.FadeToAsync(1.0, 300));
    }

    private async void OnSignInBannerTapped(object? sender, TappedEventArgs e)
        => await Navigation.PushModalAsync(_sp.GetRequiredService<AuthPage>());
}
