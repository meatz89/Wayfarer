
// LetterDetails is defined in LetterDeckRepository.cs

// Exchange offer
public class ExchangeOffer
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Dictionary<ResourceType, int> Cost { get; set; }
    public Dictionary<ResourceType, int> Reward { get; set; }
}
