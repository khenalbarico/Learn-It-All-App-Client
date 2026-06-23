using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class LoadingPage : ContentPage
{
    private readonly LoadingViewModel _vm;
    private readonly IServiceProvider _sp;

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
        await _vm.InitializeAsync();
        Application.Current!.Windows[0].Page = _sp.GetRequiredService<AppShell>();
    }
}
