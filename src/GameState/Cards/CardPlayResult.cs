public class CardPlayResult
{
    public int MomentumGenerated { get; init; }
    public List<SingleCardResult> Results { get; init; }
    public string PlayerNarrative { get; init; }  // What the player said through their card
    public bool EndsConversation { get; init; }  // Request cards end conversation immediately

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
