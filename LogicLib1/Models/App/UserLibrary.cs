namespace LogicLib1.Models.App;

public class UserLibrary
{
    public string?  BookUid          { get; set; }
    public string?  OrderId          { get; set; }
    public decimal  PriceAtPurchased { get; set; }
    public DateTime PurchasedAt      { get; set; }
}
