public class EncounterState
{
    public int FocusPoints { get; set; }
    public int MaxFocusPoints { get; set; }
    public int DurationCounter { get; set; }
    public int MaxDuration { get; set; }
    public bool IsEncounterComplete { get; set; }
    public BeatOutcomes EncounterOutcome { get; set; }
    public List<FlagStates> GoalFlags { get; set; }
    public EncounterFlagManager FlagManager { get; set; }
    public Player Player { get; set; }

    public int ConsecutiveRecoveryCount { get; set; }
    public int EncounterSeed { get; }

    public SkillCategories SkillCategory { get; }

    public NPC CurrentNPC { get; set; }

    public int CurrentProgress { get; private set; }
    public int CurrentStageIndex { get; private set; }
    public string CurrentNarrative { get; set; }

    private List<SkillModifier> activeModifiers;
    private int nextCheckModifier;

    public EncounterState(Player player, int maxFocusPoints, int maxDuration, string currentNarrative)
    {
        Player = player;
        MaxFocusPoints = maxFocusPoints;
        MaxDuration = maxDuration;
        FocusPoints = MaxFocusPoints;
        MaxDuration = 8;
        DurationCounter = 0;
        CurrentProgress = 0;
        CurrentStageIndex = 0;
        ConsecutiveRecoveryCount = 0;
        IsEncounterComplete = false;

        FlagManager = new EncounterFlagManager();
        activeModifiers = new List<SkillModifier>();
        GoalFlags = new List<FlagStates>();
        nextCheckModifier = 0;

        EncounterSeed = Environment.TickCount;
        CurrentNarrative = currentNarrative;
    }

    public void AdvanceDuration(int amount)
    {
        DurationCounter += amount;

        // Check for duration limit
        if (DurationCounter >= MaxDuration)
        {
            IsEncounterComplete = true;
        }
    }

    public void AdvanceStage()
    {
        CurrentStageIndex++;
        if (CurrentStageIndex >= 5) // 5 stages total (0-4)
        {
            IsEncounterComplete = true;
        }
    }
    public void SpendFocusPoints(int focusCost)
    {
        FocusPoints -= focusCost;
    }

    public void AddProgress(int amount)
    {
        CurrentProgress += amount;
    }

    public void SetGoalFlags(List<FlagStates> flags)
    {
        GoalFlags = new List<FlagStates>(flags);
    }

    public void CheckGoalCompletion()
    {
        // Simple goal check: are all required flags active?
        bool allGoalFlagsActive = true;

        foreach (FlagStates flag in GoalFlags)
        {
            if (!FlagManager.IsActive(flag))
            {
                allGoalFlagsActive = false;
                break;
            }
        }

        if (allGoalFlagsActive)
        {
            IsEncounterComplete = true;
        }
    }

    public void AddModifier(SkillModifier modifier)
    {
        activeModifiers.Add(modifier);
    }

    public void SetNextCheckModifier(int modifier)
    {
        nextCheckModifier = modifier;
    }

    public int GetNextCheckModifier()
    {
        int modifier = nextCheckModifier;
        nextCheckModifier = 0; // Reset after retrieval
        return modifier;
    }

    public void ProcessModifiers()
    {
        // Decrement durations and remove expired modifiers
        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            activeModifiers[i].DecrementDuration();
            if (activeModifiers[i].HasExpired())
            {
                activeModifiers.RemoveAt(i);
            }
        }
    }

    public List<SkillModifier> GetActiveModifiers(SkillTypes skillType)
    {
        List<SkillModifier> result = new List<SkillModifier>();

        foreach (SkillModifier modifier in activeModifiers)
        {
            if (modifier.TargetSkill == skillType)
            {
                result.Add(modifier);
            }
        }

        return result;
    }

    public int GetDeterministicRandom(int minValue, int maxValue)
    {
        // Use encounterContext seed for deterministic random
        Random random = new Random(EncounterSeed + DurationCounter);
        return random.Next(minValue, maxValue);
    }
}
