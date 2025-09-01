/// <summary>
/// Result of a trade action
/// </summary>
public class TradeActionResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Action { get; set; }
    public string ItemId { get; set; }
    public string LocationId { get; set; }
    public int CoinsChanged { get; set; }
    public int QuantityChanged { get; set; }
    public int CoinsBefore { get; set; }
    public int CoinsAfter { get; set; }
    public bool HadItemBefore { get; set; }
    public bool HasItemAfter { get; set; }
    public string ErrorMessage { get; set; }
    public int TransactionPrice { get; set; }
}