/// <summary>
/// Context for resolution conversations where burden obligations are resolved
/// Contains burden-specific data without dictionaries
/// </summary>
public class ResolutionContext : ConversationContextBase
{
    public List<ConversationCard> BurdenCards { get; set; }
    public ConversationCard SelectedBurden { get; set; }
    public int BurdenCount { get; set; }
    public int MinimumBurdensRequired { get; set; }
    public bool CanResolve { get; set; }
    public string BurdenDescription { get; set; }
    public string BurdenId { get; set; }
    public ConnectionType TokenType { get; set; }
    public int TokenReward { get; set; }
    public int FlowBonus { get; set; }
    public string ResolutionMethod { get; set; }
    public bool RequiresMultipleBurdens { get; set; }
    public PersonalityType NPCPersonality { get; set; }
    public List<string> ResolutionOptions { get; set; }

    public ResolutionContext()
    {
        Type = ConversationType.Resolution;
        BurdenCards = new List<ConversationCard>();
        ResolutionOptions = new List<string>();
        MinimumBurdensRequired = 2; // Default minimum burdens to resolve
        FlowBonus = 10; // Default flow bonus for successful resolution
        TokenReward = 2; // Default token reward
        RequiresMultipleBurdens = true;
    }

    public void SetBurdenCards(List<ConversationCard> burdens)
    {
        BurdenCards = burdens ?? new List<ConversationCard>();
        BurdenCount = BurdenCards.Count;
        CanResolve = BurdenCount >= MinimumBurdensRequired;
    }

    public void SetSelectedBurden(ConversationCard burden)
    {
        SelectedBurden = burden;
        if (burden != null)
        {
            BurdenId = burden.Id;
            BurdenDescription = burden.Description;
            TokenType = burden.TokenType;
        }
    }

    public void SetNPCPersonality(PersonalityType personality)
    {
        NPCPersonality = personality;

        // Set resolution options based on personality
        ResolutionOptions.Clear();
        switch (personality)
        {
            case PersonalityType.DEVOTED:
                ResolutionOptions.AddRange(new[] { "Confess burden", "Seek forgiveness", "Make amends" });
                break;
            case PersonalityType.MERCANTILE:
                ResolutionOptions.AddRange(new[] { "Negotiate settlement", "Offer compensation", "Trade for resolution" });
                break;
            case PersonalityType.PROUD:
                ResolutionOptions.AddRange(new[] { "Appeal to honor", "Challenge directly", "Demand satisfaction" });
                break;
            case PersonalityType.CUNNING:
                ResolutionOptions.AddRange(new[] { "Reveal information", "Propose alliance", "Offer mutual benefit" });
                break;
            case PersonalityType.STEADFAST:
                ResolutionOptions.AddRange(new[] { "Accept consequences", "Work steadily", "Prove dedication" });
                break;
            default:
                ResolutionOptions.AddRange(new[] { "Discuss openly", "Find compromise", "Seek understanding" });
                break;
        }
    }

    public bool HasSufficientBurdens()
    {
        return BurdenCount >= MinimumBurdensRequired;
    }

    public bool CanResolveWithNPC()
    {
        return CanResolve && Npc != null && HasSufficientBurdens();
    }

    public string GetBurdenSummary()
    {
        if (BurdenCount == 0) return "No burdens to resolve";
        if (BurdenCount == 1) return $"1 burden ({BurdenCards.First().Description})";
        return $"{BurdenCount} burdens to resolve";
    }

    public string GetResolutionRequirement()
    {
        if (HasSufficientBurdens())
        {
            return $"Ready to resolve {BurdenCount} burdens";
        }
        else
        {
            int needed = MinimumBurdensRequired - BurdenCount;
            return $"Need {needed} more burden{(needed > 1 ? "s" : "")} to resolve";
        }
    }

    public double GetResolutionProgress()
    {
        if (MinimumBurdensRequired <= 0) return 1.0;
        return Math.Min(1.0, (double)BurdenCount / MinimumBurdensRequired);
    }
}