public class EncounterFlagManager
{
    private readonly List<FlagStates> activeFlags;
    private readonly List<FlagDefinition> flagDefinitions;

    public EncounterFlagManager()
    {
        activeFlags = new List<FlagStates>();
        flagDefinitions = new List<FlagDefinition>();
        InitializeFlagDefinitions();
    }

    private void InitializeFlagDefinitions()
    {
        flagDefinitions.Add(new FlagDefinition(FlagStates.AdvantageousPosition, FlagCategories.Positional, FlagStates.DisadvantageousPosition));
        flagDefinitions.Add(new FlagDefinition(FlagStates.DisadvantageousPosition, FlagCategories.Positional, FlagStates.AdvantageousPosition));
        flagDefinitions.Add(new FlagDefinition(FlagStates.HiddenPosition, FlagCategories.Positional, FlagStates.ExposedPosition));
        flagDefinitions.Add(new FlagDefinition(FlagStates.ExposedPosition, FlagCategories.Positional, FlagStates.HiddenPosition));

        flagDefinitions.Add(new FlagDefinition(FlagStates.TrustEstablished, FlagCategories.Relational, FlagStates.DistrustTriggered));
        flagDefinitions.Add(new FlagDefinition(FlagStates.DistrustTriggered, FlagCategories.Relational, FlagStates.TrustEstablished));
        flagDefinitions.Add(new FlagDefinition(FlagStates.RespectEarned, FlagCategories.Relational, FlagStates.HostilityProvoked));
        flagDefinitions.Add(new FlagDefinition(FlagStates.HostilityProvoked, FlagCategories.Relational, FlagStates.RespectEarned));

        flagDefinitions.Add(new FlagDefinition(FlagStates.InsightGained, FlagCategories.Informational, FlagStates.ConfusionCreated));
        flagDefinitions.Add(new FlagDefinition(FlagStates.ConfusionCreated, FlagCategories.Informational, FlagStates.InsightGained));
        flagDefinitions.Add(new FlagDefinition(FlagStates.SecretRevealed, FlagCategories.Informational, FlagStates.DeceptionDetected));
        flagDefinitions.Add(new FlagDefinition(FlagStates.DeceptionDetected, FlagCategories.Informational, FlagStates.SecretRevealed));

        flagDefinitions.Add(new FlagDefinition(FlagStates.PathCleared, FlagCategories.Tactical, FlagStates.PathBlocked));
        flagDefinitions.Add(new FlagDefinition(FlagStates.PathBlocked, FlagCategories.Tactical, FlagStates.PathCleared));
        flagDefinitions.Add(new FlagDefinition(FlagStates.SurpriseAchieved, FlagCategories.Tactical, FlagStates.SurpriseAchieved));
        flagDefinitions.Add(new FlagDefinition(FlagStates.PreparationCompleted, FlagCategories.Tactical, FlagStates.PreparationCompleted));
        flagDefinitions.Add(new FlagDefinition(FlagStates.ResourceSecured, FlagCategories.Tactical, FlagStates.ResourceSecured));

        flagDefinitions.Add(new FlagDefinition(FlagStates.AreaSecured, FlagCategories.Environmental, FlagStates.ObstaclePresent));
        flagDefinitions.Add(new FlagDefinition(FlagStates.ObstaclePresent, FlagCategories.Environmental, FlagStates.AreaSecured));
        flagDefinitions.Add(new FlagDefinition(FlagStates.DistractionCreated, FlagCategories.Environmental, FlagStates.DistractionCreated));
        flagDefinitions.Add(new FlagDefinition(FlagStates.HazardNeutralized, FlagCategories.Environmental, FlagStates.HazardNeutralized));

        flagDefinitions.Add(new FlagDefinition(FlagStates.ConfidenceBuilt, FlagCategories.Emotional, FlagStates.FearInstilled));
        flagDefinitions.Add(new FlagDefinition(FlagStates.FearInstilled, FlagCategories.Emotional, FlagStates.ConfidenceBuilt));
        flagDefinitions.Add(new FlagDefinition(FlagStates.TensionIncreased, FlagCategories.Emotional, FlagStates.TensionIncreased));
        flagDefinitions.Add(new FlagDefinition(FlagStates.UrgencyCreated, FlagCategories.Emotional, FlagStates.UrgencyCreated));
    }

    public void SetFlag(FlagStates flag)
    {
        if (!activeFlags.Contains(flag))
        {
            activeFlags.Add(flag);
        }

        FlagDefinition definition = flagDefinitions.FirstOrDefault(d => d.Flag == flag);
        if (definition != null && definition.OpposingFlag != flag)
        {
            ClearFlag(definition.OpposingFlag);
        }
    }

    public void ClearFlag(FlagStates flag)
    {
        activeFlags.Remove(flag);
    }

    public bool IsActive(FlagStates flag)
    {
        return activeFlags.Contains(flag);
    }

    public List<FlagStates> GetActiveFlags()
    {
        return new List<FlagStates>(activeFlags);
    }

    public List<FlagStates> GetActiveFlagsByCategory(FlagCategories category)
    {
        List<FlagStates> categoryFlags = new List<FlagStates>();
        foreach (FlagStates flag in activeFlags)
        {
            FlagDefinition definition = flagDefinitions.FirstOrDefault(d => d.Flag == flag);
            if (definition != null && definition.Category == category)
            {
                categoryFlags.Add(flag);
            }
        }
        return categoryFlags;
    }
}
