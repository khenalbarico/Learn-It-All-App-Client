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
                throw new Exception("Could not connect to Google Play billing service.");

            var purchase = await billing.PurchaseAsync(productId, ItemType.InAppPurchase);

            if (purchase is null)
                throw new Exception($"Purchase returned null for product '{productId}'. Check that this product ID exists in Play Console.");

            if (purchase.State != PurchaseState.Purchased)
                throw new Exception($"Purchase state was '{purchase.State}' for product '{productId}'.");

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
