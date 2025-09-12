
// Card play results
public class CardPlayResult
{
    public int FinalFlow { get; init; }
    public ConnectionState? NewState { get; init; }
    public List<SingleCardResult> Results { get; init; }
    public int SetBonus { get; init; }
    public int ConnectedBonus { get; init; }
    public int EagerBonus { get; init; }
    public bool DeliveredLetter { get; init; }
    public bool ManipulatedObligations { get; init; }
    public List<LetterNegotiationResult> LetterNegotiations { get; init; } = new List<LetterNegotiationResult>();
    public string PlayerNarrative { get; init; }  // What the player said through their card
    public bool Success
    {
        get
        {
            if (Results == null) return false;
            foreach (SingleCardResult r in Results)
            {
                if (r.Success) return true;
            }
            return false;
        }
        set { } // Make it settable
    }
}
