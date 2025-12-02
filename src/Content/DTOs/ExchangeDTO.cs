/// <summary>
/// Data transfer object for exchange definitions
/// Uses categorical properties to match provider NPCs (DDR-006)
/// </summary>
public class ExchangeDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public PlacementFilterDTO ProviderFilter { get; set; }
    public string GiveCurrency { get; set; }
    public int GiveAmount { get; set; }
    public string ReceiveCurrency { get; set; }
    public int ReceiveAmount { get; set; }
    // DOMAIN COLLECTION PRINCIPLE: List of objects instead of Dictionary
    public List<TokenEntryDTO> TokenGate { get; set; } = new List<TokenEntryDTO>();

    // Item costs and rewards (PRINCIPLE 4: Items as resource costs, not boolean gates)
    public List<string> ConsumedItems { get; set; } = new List<string>(); // Items consumed as cost
    public List<string> GrantedItems { get; set; } = new List<string>();  // Items granted as reward
}