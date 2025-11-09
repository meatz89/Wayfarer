/// <summary>
/// Data transfer object for exchange definitions
/// </summary>
public class ExchangeDTO
{
public string Id { get; set; }
public string Name { get; set; }
public string NpcId { get; set; }
public string GiveCurrency { get; set; }
public int GiveAmount { get; set; }
public string ReceiveCurrency { get; set; }
public int ReceiveAmount { get; set; }
public string ReceiveItem { get; set; } // For specific item rewards (legacy single item)
public Dictionary<string, int> TokenGate { get; set; }

// Item costs and rewards (PRINCIPLE 4: Items as resource costs, not boolean gates)
public List<string> ConsumedItems { get; set; } = new List<string>(); // Items consumed as cost
public List<string> GrantedItems { get; set; } = new List<string>();  // Items granted as reward
}