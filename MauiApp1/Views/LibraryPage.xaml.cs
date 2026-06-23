using LogicLib1.Models.App;
using LogicLib1.Services.App;
using MauiApp1.Services;
using MauiApp1.ViewModels;

namespace MauiApp1.Views;

public partial class LibraryPage : ContentPage
{
    private readonly LibraryViewModel _vm;
    private readonly IServiceProvider _sp;
    private readonly IBillingService  _billing;
    private readonly IAppService      _appService;
    private readonly UserSession      _userSession;

    public LibraryPage(LibraryViewModel vm, IServiceProvider sp, IBillingService billing, IAppService appService, UserSession userSession)
    {
        InitializeComponent();
        _vm          = vm;
        _sp          = sp;
        _billing     = billing;
        _appService  = appService;
        _userSession = userSession;
        BindingContext = vm;

        _vm.NavigateToAuth   = NavigateToAuth;
        _vm.InitiatePurchase = InitiatePurchase;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Refresh();
    }

    private Task NavigateToAuth()
        => Navigation.PushModalAsync(_sp.GetRequiredService<AuthPage>());

    private async Task InitiatePurchase(BookMetadata book)
    {
        var productId = book.Uid.ToLowerInvariant().Replace("-", "_");

        PurchaseResult? result;
        try
        {
            result = await _billing.PurchaseAsync(productId);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Purchase Failed", ex.Message, "OK");
            return;
        }

        if (result is null) return;

        try
        {
            await _appService.RecordPurchase(book.Uid, result.OrderId, result.PurchaseToken, book.Price);

            _userSession.UserInfo?.Library.Add(new UserLibrary
            {
                Uid              = book.Uid,
                OrderId          = result.OrderId,
                PriceAtPurchased = book.Price,
                PurchasedAt      = DateTime.UtcNow
            });

            await DisplayAlertAsync("Purchase Successful", $"\"{book.Title}\" has been added to your library.", "OK");
        }
        catch
        {
            await DisplayAlertAsync("Recording Failed",
                $"Your payment was processed but we could not record your purchase. Please contact support with Order ID: {result.OrderId}",
                "OK");
        }
    }
}
