
// LetterDetails is defined in LetterDeckRepository.cs

// Exchange offer
public class ExchangeOffer
{
    // HIGHLANDER: NO Id property - ExchangeOffer identified by object reference
    public string Name { get; set; }
    public Dictionary<ResourceType, int> Cost { get; set; }
    public Dictionary<ResourceType, int> Reward { get; set; }
}
