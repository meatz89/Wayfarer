/// <summary>
/// Context for standard trust-building conversations (FriendlyChat)
/// Contains rapport and relationship building data without dictionaries
/// </summary>
public class StandardContext : ConversationContextBase
{
    public int RapportGoal { get; set; }
    public int CurrentRapport { get; set; }
    public ConnectionType TargetTokenType { get; set; }
    public PersonalityType NPCPersonality { get; set; }
    public bool HasRapportGoal { get; set; }
    public int TokenReward { get; set; }
    public string CustomText { get; set; }
    public List<ConnectionState> ValidStates { get; set; }
    public bool GrantsToken { get; set; }
    public int FlowThreshold { get; set; }
    public int CurrentFlow { get; set; }
    public int FlowBattery { get; set; }
    public AtmosphereType CurrentAtmosphere { get; set; }
    public int HiddenMomentum { get; set; }

    public StandardContext()
    {
        Type = ConversationType.FriendlyChat;
        ValidStates = new List<ConnectionState>();
        FlowThreshold = 100; // Default flow threshold for successful conversations
        TokenReward = 1; // Default token reward
        GrantsToken = true;
        CurrentAtmosphere = AtmosphereType.Neutral;
    }

    public void SetRapportGoal(int rapportGoal)
    {
        RapportGoal = rapportGoal;
        HasRapportGoal = rapportGoal > 0;
    }

    public void SetNPCPersonality(PersonalityType personality)
    {
        NPCPersonality = personality;
        
        // Map personality to appropriate token type
        TargetTokenType = personality switch
        {
            PersonalityType.DEVOTED => ConnectionType.Trust,
            PersonalityType.MERCANTILE => ConnectionType.Commerce,
            PersonalityType.PROUD => ConnectionType.Status,
            PersonalityType.CUNNING => ConnectionType.Shadow,
            PersonalityType.STEADFAST => ConnectionType.Trust,
            _ => ConnectionType.Trust
        };
    }

    public bool IsRapportGoalReached()
    {
        return HasRapportGoal && CurrentRapport >= RapportGoal;
    }

    public bool IsFlowThresholdReached()
    {
        return CurrentFlow >= FlowThreshold;
    }

    public double GetRapportProgress()
    {
        if (!HasRapportGoal || RapportGoal <= 0) return 0.0;
        return Math.Min(1.0, (double)CurrentRapport / RapportGoal);
    }

    public double GetFlowProgress()
    {
        if (FlowThreshold <= 0) return 0.0;
        return Math.Min(1.0, (double)CurrentFlow / FlowThreshold);
    }

    public string GetProgressDescription()
    {
        if (HasRapportGoal)
        {
            return $"Rapport: {CurrentRapport}/{RapportGoal} ({GetRapportProgress():P0})";
        }
        else
        {
            return $"Flow: {CurrentFlow}/{FlowThreshold} ({GetFlowProgress():P0})";
        }
    }

    public bool ShouldGrantToken()
    {
        return GrantsToken && (IsRapportGoalReached() || IsFlowThresholdReached());
    }
}