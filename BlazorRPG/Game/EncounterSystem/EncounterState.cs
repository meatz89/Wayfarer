public class EncounterState
{
    // Core state tracking
    public int FocusPoints { get; set; }
    public int MaxFocusPoints { get; }
    public int DurationCounter { get; private set; }
    public int MaxDuration { get; }
    public int ConsecutiveRecoveryCount { get; set; }
    public bool IsEncounterComplete { get; set; }
    public int EncounterSeed { get; }

    // Skill category for this encounter
    public SkillCategories SkillCategory { get; }

    // Flag management
    public EncounterFlagManager FlagManager { get; }

    // Current context
    public Character CurrentNPC { get; set; }

    // Progress tracking
    public int CurrentProgress { get; private set; }
    public int CurrentStageIndex { get; private set; }

    // Skill modifiers
    private List<SkillModifier> activeModifiers;
    private int nextCheckModifier;

    // Player reference
    public Player Player { get; }

    // Success determination - flags that constitute success
    private List<FlagStates> goalFlags;

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

        // Initialize tracking systems
        FlagManager = new EncounterFlagManager();
        activeModifiers = new List<SkillModifier>();
        goalFlags = new List<FlagStates>();
        nextCheckModifier = 0;

        // Set deterministic seed for consistent random results
        EncounterSeed = Environment.TickCount;
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
        goalFlags = new List<FlagStates>(flags);
    }

    public void CheckGoalCompletion()
    {
        // Simple goal check: are all required flags active?
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

    public ChoiceProjection ApplyChoice(
        ChoiceProjectionService choiceProjectionService,
        Player playerState,
        Encounter encounter,
        AiChoice choice)
    {
        // Create projection to determine outcome
        ChoiceProjection projection = CreateChoiceProjection(choiceProjectionService, choice, playerState);

        // Apply Focus cost
        FocusPoints -= choice.FocusCost;

        // Select a skill option to process (for this POC, just use the first one)
        SkillOption selectedSkillOption = choice.SkillOptions.FirstOrDefault();
        if (selectedSkillOption != null)
        {
            // Find matching skill card
            SkillCard card = FindCardByName(playerState.AvailableCards, selectedSkillOption.SkillName);
            bool isUntrained = (card == null || card.IsExhausted);

            // Perform skill check
            int effectiveLevel = 0;
            int difficulty = selectedSkillOption.SCD;

            if (!isUntrained && card != null)
            {
                // Using a skill card
                effectiveLevel = card.GetEffectiveLevel(this);
                card.Exhaust(); // Exhaust the card
            }
            else
            {
                // Untrained attempt
                effectiveLevel = 0;
                difficulty += 2; // +2 difficulty for untrained
            }

            // Add any next check modifier
            effectiveLevel += GetNextCheckModifier();

            // Determine success
            bool success = effectiveLevel >= difficulty;

            // Apply appropriate payload
            PayloadRegistry payloadRegistry = new PayloadRegistry(null);
            if (success)
            {
                IMechanicalEffect effect = payloadRegistry.GetEffect(selectedSkillOption.SuccessPayload.MechanicalEffectID);
                if (effect != null)
                {
                    effect.Apply(this);
                }
                projection.ProgressGained = 2; // Default success progress
            }
            else
            {
                IMechanicalEffect effect = payloadRegistry.GetEffect(selectedSkillOption.FailurePayload.MechanicalEffectID);
                if (effect != null)
                {
                    effect.Apply(this);
                }
            }

            projection.SkillCheckSuccess = success;
        }

        // If this was a recovery action (0 Focus cost), increment consecutive recovery count
        if (choice.FocusCost == 0)
        {
            ConsecutiveRecoveryCount++;
        }
        else
        {
            // Reset consecutive recovery count for non-recovery actions
            ConsecutiveRecoveryCount = 0;
        }

        // Process skill modifiers
        ProcessModifiers();

        // Check if goal has been achieved
        CheckGoalCompletion();

        // Advance duration - basic duration advance for any action
        AdvanceDuration(1);

        // Advance stage if at a stage boundary (every 2 turns)
        if (DurationCounter % 2 == 0 && CurrentStageIndex < 4)
        {
            AdvanceStage();
        }

        // Update projection with encounter state
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

    private SkillCard FindCardByName(List<SkillCard> cards, string name)
    {
        foreach (SkillCard card in cards)
        {
            if (card.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !card.IsExhausted)
            {
                return card;
            }
        }

        return null;
    }

    public int GetDeterministicRandom(int minValue, int maxValue)
    {
        // Use encounter seed for deterministic random
        Random random = new Random(EncounterSeed + DurationCounter);
        return random.Next(minValue, maxValue);
    }
}
