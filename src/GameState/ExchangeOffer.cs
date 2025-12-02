
// LetterDetails is defined in LetterDeckRepository.cs

// Exchange offer
public class ExchangeOffer
{
    // HIGHLANDER: NO Id property - ExchangeOffer identified by object reference
    public string Name { get; set; }
    // DOMAIN COLLECTION PRINCIPLE: List<T> instead of Dictionary
    public List<ResourceAmount> Cost { get; set; } = new List<ResourceAmount>();
    public List<ResourceAmount> Reward { get; set; } = new List<ResourceAmount>();
}
