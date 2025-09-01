public class LetterNegotiationResult
{
    public string PromiseCardId { get; init; }
    public bool NegotiationSuccess { get; init; }
    public TermDetails FinalTerms { get; init; }
    public CardInstance SourcePromiseCard { get; init; }
    public DeliveryObligation CreatedObligation { get; init; }
}
