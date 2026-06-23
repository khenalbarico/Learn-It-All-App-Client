using Plugin.InAppBilling;

namespace MauiApp1.Services;

public class BillingService : IBillingService
{
    public async Task<PurchaseResult?> PurchaseAsync(string productId)
    {
        if (!CrossInAppBilling.IsSupported)
            return null;

        var billing = CrossInAppBilling.Current;
        try
        {
            var connected = await billing.ConnectAsync();
            if (!connected)
                return null;

            var purchase = await billing.PurchaseAsync(productId, ItemType.InAppPurchase);

            if (purchase?.State != PurchaseState.Purchased)
                return null;

            await billing.FinalizePurchaseAsync(purchase.PurchaseToken);

            return new PurchaseResult
            {
                OrderId       = purchase.Id,
                PurchaseToken = purchase.PurchaseToken
            };
        }
        finally
        {
            await billing.DisconnectAsync();
        }
    }
}
