public class EncounterState
{
    public int FocusPoints { get; set; }
    public int MaxFocusPoints { get; private set; }
    public int DurationCounter { get; private set; }
    public int MaxDuration { get; private set; }
    public int ConsecutiveRecoveryCount { get; set; }
    public bool IsEncounterComplete { get; set; }
    public int EncounterSeed { get; private set; }
    public SkillCategories SkillCategory { get; private set; }
    public EncounterFlagManager FlagManager { get; private set; }
    public Character CurrentNPC { get; set; }
    public Player Player { get; private set; }

    public int CurrentProgress { get; private set; }
    public int CurrentStageIndex { get; private set; }

    private List<FlagStates> goalFlags;
    private List<SkillModifier> activeModifiers;
    private int nextCheckModifier;

    public EncounterState(Player player, SkillCategories skillCategory)
    {
        // Initialize core values
        Player = player;
        SkillCategory = skillCategory;
        MaxFocusPoints = 6;
        FocusPoints = MaxFocusPoints;
        MaxDuration = 8; 
        DurationCounter = 0;
        CurrentProgress = 0;
        CurrentStageIndex = 0;
        ConsecutiveRecoveryCount = 0;
        IsEncounterComplete = false;

        // Initialize state tracking
        FlagManager = new EncounterFlagManager();
        activeModifiers = new List<SkillModifier>();
        goalFlags = new List<FlagStates>();
        nextCheckModifier = 0;

        // Set deterministic seed for consistent random results
        EncounterSeed = Environment.TickCount;
    }

    // Focus Point management
    public bool CanAffordFocusCost(int focusCost)
    {
        return FocusPoints >= focusCost;
    }

    public void SpendFocusPoints(int amount)
    {
        FocusPoints = Math.Max(0, FocusPoints - amount);
    }

    public void RecoverFocusPoint()
    {
        if (FocusPoints < MaxFocusPoints)
        {
            FocusPoints++;
        }
    }

    // Duration and progress tracking
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

    public void AddProgress(int amount)
    {
        CurrentProgress += amount;
    }

    // Goal management
    public void SetGoalFlags(List<FlagStates> flags)
    {
        goalFlags = new List<FlagStates>(flags);
    }

    public void CheckGoalCompletion()
    {
        bool allGoalFlagsActive = true;

        foreach (FlagStates flag in goalFlags)
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

    // Skill modifiers
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

    public ChoiceProjection CreateChoiceProjection(
        ChoiceProjectionService choiceProjectionService, 
        AiChoice choice, 
        Player playerState)
    {
        return choiceProjectionService.ProjectChoice(
            choice,
            this,
            playerState);
    }

    public ChoiceProjection ApplyChoice(ChoiceProjectionService choiceProjectionService, Player playerState, Encounter encounter, AiChoice choice)
    {
        ChoiceProjection projection = CreateChoiceProjection(choiceProjectionService, choice, playerState);

        SpendFocusPoints(choice.FocusCost);

        bool skillCheckPassed = projection.SkillCheckSuccess;

        AdvanceDuration(1);

        if (DurationCounter % 2 == 0 && CurrentStageIndex < 4)
        {
            AdvanceStage();
        }

        projection.WillEncounterEnd = IsEncounterComplete;

        if (projection.WillEncounterEnd)
        {
            int successThreshold = 10; // Basic success threshold
            projection.ProjectedOutcome =
                CurrentProgress >= successThreshold
                    ? EncounterOutcomes.BasicSuccess
                    : EncounterOutcomes.Failure;
        }

        return projection;
    }
    
    public int GetDeterministicRandom(int minValue, int maxValue)
    {
        Random random = new Random(EncounterSeed + DurationCounter);
        return random.Next(minValue, maxValue);
    }
}