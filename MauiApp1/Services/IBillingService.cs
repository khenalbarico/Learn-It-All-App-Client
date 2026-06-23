namespace MauiApp1.Services;

public class PurchaseResult
{
    public string OrderId       { get; init; } = string.Empty;
    public string PurchaseToken { get; init; } = string.Empty;
}

public interface IBillingService
{
    Task<PurchaseResult?> PurchaseAsync(string productId);
}
