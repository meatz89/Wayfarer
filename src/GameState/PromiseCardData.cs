
// Promise card data
public class PromiseCardData
{
    public string CardId { get; set; }
    public TermDetails SuccessTerms { get; set; }
    public TermDetails FailureTerms { get; set; }
    public int NegotiationDifficulty { get; set; }
    public ConnectionType TokenType { get; set; }
    public string Destination { get; set; }
    public string RecipientName { get; set; }
}
