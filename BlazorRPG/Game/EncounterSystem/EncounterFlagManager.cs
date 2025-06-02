
public class EncounterFlagManager
{
    private HashSet<FlagStates> activeFlags = new HashSet<FlagStates>();
    private List<FlagDefinition> flagDefinitions = new List<FlagDefinition>();

    public EncounterFlagManager()
    {
        InitializeFlagDefinitions();
    }

    private void InitializeFlagDefinitions()
    {
        // Positional flags
        flagDefinitions.Add(new FlagDefinition(
            FlagStates.AdvantageousPosition,
            FlagCategories.Positional,
            FlagStates.DisadvantageousPosition));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.DisadvantageousPosition,
            FlagCategories.Positional,
            FlagStates.AdvantageousPosition));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.HiddenPosition,
            FlagCategories.Positional,
            FlagStates.ExposedPosition));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.ExposedPosition,
            FlagCategories.Positional,
            FlagStates.HiddenPosition));

        // Relational flags
        flagDefinitions.Add(new FlagDefinition(
            FlagStates.TrustEstablished,
            FlagCategories.Relational,
            FlagStates.DistrustTriggered));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.DistrustTriggered,
            FlagCategories.Relational,
            FlagStates.TrustEstablished));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.RespectEarned,
            FlagCategories.Relational,
            FlagStates.HostilityProvoked));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.HostilityProvoked,
            FlagCategories.Relational,
            FlagStates.RespectEarned));

        // Informational flags
        flagDefinitions.Add(new FlagDefinition(
            FlagStates.InsightGained,
            FlagCategories.Informational,
            FlagStates.ConfusionCreated));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.ConfusionCreated,
            FlagCategories.Informational,
            FlagStates.InsightGained));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.SecretRevealed,
            FlagCategories.Informational,
            FlagStates.DeceptionDetected));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.DeceptionDetected,
            FlagCategories.Informational,
            FlagStates.SecretRevealed));

        // Tactical flags
        flagDefinitions.Add(new FlagDefinition(
            FlagStates.SurpriseAchieved,
            FlagCategories.Tactical,
            FlagStates.SurpriseAchieved)); // Self-opposing indicates no natural opposite

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.PreparationCompleted,
            FlagCategories.Tactical,
            FlagStates.PreparationCompleted));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.PathCleared,
            FlagCategories.Tactical,
            FlagStates.ObstaclePresent));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.ObstaclePresent,
            FlagCategories.Tactical,
            FlagStates.PathCleared));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.ResourceSecured,
            FlagCategories.Tactical,
            FlagStates.ResourceSecured));

        // Environmental flags
        flagDefinitions.Add(new FlagDefinition(
            FlagStates.AreaSecured,
            FlagCategories.Environmental,
            FlagStates.ObstacleActive));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.ObstacleActive,
            FlagCategories.Environmental,
            FlagStates.AreaSecured));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.DistractionCreated,
            FlagCategories.Environmental,
            FlagStates.DistractionCreated));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.HazardNeutralized,
            FlagCategories.Environmental,
            FlagStates.HazardNeutralized));

        // Emotional flags
        flagDefinitions.Add(new FlagDefinition(
            FlagStates.ConfidenceBuilt,
            FlagCategories.Emotional,
            FlagStates.FearInstilled));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.FearInstilled,
            FlagCategories.Emotional,
            FlagStates.ConfidenceBuilt));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.TensionIncreased,
            FlagCategories.Emotional,
            FlagStates.TensionIncreased));

        flagDefinitions.Add(new FlagDefinition(
            FlagStates.UrgencyCreated,
            FlagCategories.Emotional,
            FlagStates.UrgencyCreated));
    }

    public bool IsActive(FlagStates flag)
    {
        return activeFlags.Contains(flag);
    }

    public void SetFlag(FlagStates flag)
    {
        // Find the flag definition
        FlagDefinition definition = GetFlagDefinition(flag);

        if (definition != null)
        {
            // Clear opposing flag if it exists
            activeFlags.Remove(definition.OpposingFlag);

            // Set the new flag
            activeFlags.Add(flag);
        }
    }

    public void ClearFlag(FlagStates flag)
    {
        activeFlags.Remove(flag);
    }

    public List<FlagStates> GetActiveFlags()
    {
        return activeFlags.ToList();
    }

    public List<FlagStates> GetActiveFlagsByCategory(FlagCategories category)
    {
        List<FlagStates> result = new List<FlagStates>();

        foreach (FlagStates flag in activeFlags)
        {
            FlagDefinition definition = GetFlagDefinition(flag);
            if (definition != null && definition.Category == category)
            {
                result.Add(flag);
            }
        }

        return result;
    }

    private FlagDefinition GetFlagDefinition(FlagStates flag)
    {
        foreach (FlagDefinition definition in flagDefinitions)
        {
            if (definition.Flag == flag)
            {
                return definition;
            }
        }

        return null;
    }

    public void ProcessFlagChange(FlagStates newFlag, EncounterState state)
    {
        // Apply the flag change
        state.FlagManager.SetFlag(newFlag);

        // Check for goal completion
        if (state.GoalFlags.Contains(newFlag))
        {
            state.CheckGoalCompletion();
        }
    }

    public List<FlagStates> GetRecentlySetFlags()
    {
        throw new NotImplementedException();
    }
}