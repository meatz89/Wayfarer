
// Card play results - updated for 4-resource system
public class CardPlayResult
{
    public int MomentumGenerated { get; init; }
    public int DoubtGenerated { get; init; }
    public int InitiativeGenerated { get; init; }
    public int CadenceChange { get; init; }
    public ConnectionState? NewState { get; init; } // DEPRECATED: Will be removed in favor of resource-based outcomes
    public List<SingleCardResult> Results { get; init; }
    public int SetBonus { get; init; }
    public int ConnectedBonus { get; init; }
    public int EagerBonus { get; init; }
    public bool DeliveredLetter { get; init; }
    public bool ManipulatedObligations { get; init; }
    public List<LetterNegotiationResult> LetterNegotiations { get; init; } = new List<LetterNegotiationResult>();
    public string PlayerNarrative { get; init; }  // What the player said through their card

    // Legacy compatibility property - maps to MomentumGenerated
    [Obsolete("Use MomentumGenerated instead")]
    public int FinalFlow
    {
        get => MomentumGenerated;
        init => MomentumGenerated = value;
    }
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
