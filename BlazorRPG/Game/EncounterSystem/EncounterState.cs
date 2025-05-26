public partial class EncounterState
{
    public int FocusPoints { get; set; }
    public int MaxFocusPoints { get; }
    public int DurationCounter { get; private set; }
    public int MaxDuration { get; }
    public int ConsecutiveRecoveryCount { get; set; }
    public bool isEncounterComplete { get; set; }
    public int EncounterSeed { get; }
    public SkillCategories SkillCategory { get; }
    public EncounterFlagManager FlagManager { get; }
    public Character CurrentNPC { get; set; }
    public Player Player { get; }
    public int CurrentProgress { get; }
    public int CurrentStageIndex { get; }

    private readonly List<SkillModifier> activeModifiers = new List<SkillModifier>();
    private int nextCheckModifier = 0;
    private readonly List<FlagStates> goalFlags = new List<FlagStates>();

    public EncounterState(Player player, SkillCategories skillCategory)
    {
        CurrentProgress = 0;
        CurrentStageIndex = 0;
        DurationCounter = 0;
        MaxFocusPoints = 6;
        FocusPoints = MaxFocusPoints;

        // Set encounter type for skill mapping
        SkillCategory = skillCategory;

    }
    public void AdvanceDuration(int amount)
    {
        DurationCounter += amount;
        if (DurationCounter >= MaxDuration)
        {
            isEncounterComplete = true;
        }
    }

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
    }

    public ChoiceProjection CreateChoiceProjection(
        EncounterOption choice,
        Player playerState)
    {
        Location location = playerState.CurrentLocation;

        return ChoiceProjectionService.CreateUniversalChoiceProjection(
            choice,
            this,
            playerState,
            location);
    }

    public bool IsEncounterComplete()
    {
        return CurrentStageIndex >= 4; // 5 stages (0-4), complete when past stage 4
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
        nextCheckModifier = 0;
        return modifier;
    }

    public void ProcessModifiers()
    {
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
        Random random = new Random(EncounterSeed + DurationCounter);
        return random.Next(minValue, maxValue);
    }

    internal ChoiceProjection ApplyChoice(Player playerState, Encounter encounter, EncounterOption choice)
    {
        string s = "";
        return new ChoiceProjection(choice);
    }
}