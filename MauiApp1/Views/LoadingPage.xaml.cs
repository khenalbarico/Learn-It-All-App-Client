using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class LoadingPage : ContentPage
{
    private readonly LoadingViewModel _vm;
    private readonly IServiceProvider _sp;
    private CancellationTokenSource _animationCts = new();

    public LoadingPage(LoadingViewModel vm, IServiceProvider sp)
    {
        InitializeComponent();
        _vm = vm;
        _sp = sp;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await AnimateEntranceAsync();
        StartDotBounceLoop();
        await _vm.InitializeAsync();
        _animationCts.Cancel();
        Application.Current!.Windows[0].Page = _sp.GetRequiredService<AppShell>();
    }

    private async Task AnimateEntranceAsync()
    {
        AppLogoImage.Scale = 0.6;
        AppLogoImage.Opacity = 0;
        Dot1.TranslationY = 16;
        Dot2.TranslationY = 16;
        Dot3.TranslationY = 16;
        Dot1.Opacity = 0;
        Dot2.Opacity = 0;
        Dot3.Opacity = 0;

        await Task.WhenAll(
            AppLogoImage.ScaleToAsync(1.0, 700, Easing.SpringOut),
            AppLogoImage.FadeToAsync(1.0, 500));

        await Task.WhenAll(
            Dot1.FadeToAsync(1.0, 200),
            Dot1.TranslateToAsync(0, 0, 200, Easing.SpringOut));
        await Task.WhenAll(
            Dot2.FadeToAsync(1.0, 200),
            Dot2.TranslateToAsync(0, 0, 200, Easing.SpringOut));
        await Task.WhenAll(
            Dot3.FadeToAsync(1.0, 200),
            Dot3.TranslateToAsync(0, 0, 200, Easing.SpringOut));
    }

    private void StartDotBounceLoop()
    {
        var token = _animationCts.Token;
        _ = RunDotBounceAsync(Dot1, 0, token);
        _ = RunDotBounceAsync(Dot2, 160, token);
        _ = RunDotBounceAsync(Dot3, 320, token);
        _ = RunLogoPulseAsync(token);
    }

    private static async Task RunDotBounceAsync(View dot, int initialDelayMs, CancellationToken ct)
    {
        try
        {
            await Task.Delay(initialDelayMs, ct);
            while (!ct.IsCancellationRequested)
            {
                await dot.TranslateToAsync(0, -10, 280, Easing.CubicOut);
                await dot.TranslateToAsync(0, 0, 280, Easing.BounceOut);
                await Task.Delay(500, ct);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { }
    }

    private async Task RunLogoPulseAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await AppLogoImage.ScaleToAsync(1.07, 900, Easing.SinInOut);
                await AppLogoImage.ScaleToAsync(1.0, 900, Easing.SinInOut);
                await Task.Delay(300, ct);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception) { }
    }
}
