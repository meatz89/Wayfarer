/// <summary>
/// System messages used in conversation UI for various actions and states.
/// </summary>
public class SystemMessages
{
public string listeningCarefully { get; set; }
public string drewCards { get; set; }
public string decliningExchange { get; set; }
public string tradingExchange { get; set; }
public string playingCard { get; set; }
public string cardsSucceeded { get; set; }
public string cardsFailed { get; set; }
public Dictionary<string, string> conversationExhausted { get; set; }
public Dictionary<string, string> letterNegotiation { get; set; }
}
