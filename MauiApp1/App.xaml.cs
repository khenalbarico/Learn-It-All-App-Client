using MauiApp1.Views;

namespace MauiApp1;

public partial class App : Application
{
    private readonly IServiceProvider _sp;

    public App(IServiceProvider sp)
    {
        _sp = sp;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(_sp.GetRequiredService<LoadingPage>());
}
