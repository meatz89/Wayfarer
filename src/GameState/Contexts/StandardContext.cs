/// <summary>
/// Context for standard trust-building conversations (FriendlyChat)
/// Contains rapport and relationship building data without dictionaries
/// </summary>
public class StandardContext : ConversationContextBase
{
    public int CurrentRapport { get; set; }
    public ConnectionType TargetTokenType { get; set; }
    public PersonalityType NPCPersonality { get; set; }
    public int TokenReward { get; set; }
    public string CustomText { get; set; }
    public List<ConnectionState> ValidStates { get; set; }
    public bool GrantsToken { get; set; }
    public int MomentumThreshold { get; set; }

    public StandardContext()
    {
        ConversationTypeId = "friendly_chat";
        ValidStates = new List<ConnectionState>();
        MomentumThreshold = 8; // Basic goal threshold for successful conversations
        TokenReward = 1; // Default token reward
        GrantsToken = true;
    }


    public void SetNPCPersonality(PersonalityType personality)
    {
        NPCPersonality = personality;

        // Map personality to appropriate token type
        TargetTokenType = personality switch
        {
            PersonalityType.DEVOTED => ConnectionType.Trust,
            PersonalityType.MERCANTILE => ConnectionType.Diplomacy,
            PersonalityType.PROUD => ConnectionType.Status,
            PersonalityType.CUNNING => ConnectionType.Shadow,
            PersonalityType.STEADFAST => ConnectionType.Trust,
            _ => ConnectionType.Trust
        };
    }


    public bool IsMomentumThresholdReached(int currentMomentum)
    {
        return currentMomentum >= MomentumThreshold;
    }

    public double GetMomentumProgress(int currentMomentum)
    {
        if (MomentumThreshold <= 0) return 0.0;
        return Math.Min(1.0, (double)currentMomentum / MomentumThreshold);
    }

    public string GetProgressDescription(int currentMomentum)
    {
        return $"Momentum: {currentMomentum}/{MomentumThreshold} ({GetMomentumProgress(currentMomentum):P0})";
    }

    public bool ShouldGrantToken(int currentMomentum)
    {
        return GrantsToken && IsMomentumThresholdReached(currentMomentum);
    }
}