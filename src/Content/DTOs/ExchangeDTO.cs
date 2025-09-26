using System.Collections.Generic;

/// <summary>
/// Data transfer object for exchange definitions
/// </summary>
public class ExchangeDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string GiveCurrency { get; set; }
    public int GiveAmount { get; set; }
    public string ReceiveCurrency { get; set; }
    public int ReceiveAmount { get; set; }
    public string ReceiveItem { get; set; } // For specific item rewards
    public Dictionary<string, int> TokenGate { get; set; }
}