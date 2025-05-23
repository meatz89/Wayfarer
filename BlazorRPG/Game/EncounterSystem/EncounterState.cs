public class EncounterState
{
    public static EncounterState PreviousEncounterState { get; set; }
    public EncounterOption PreviousChoice { get; set; }
    public int CurrentProgress { get; private set; }
    public int CurrentStageIndex { get; private set; }
    public int CurrentTurn { get; private set; }
    public Encounter EncounterInfo { get; }
    public LocationSpot LocationSpot { get; set; }

    public int FocusPoints { get; private set; }
    public int MaxFocusPoints { get; private set; }
    public AspectTokenPool AspectTokens { get; private set; }
    public CardTypes EncounterType { get; private set; }
    public int OutcomeThresholdModifier { get; set; }

    public EncounterState(Encounter encounterInfo, PlayerState playerState)
    {
        CurrentProgress = 0;
        CurrentStageIndex = 0;
        CurrentTurn = 0;
        EncounterInfo = encounterInfo;
        OutcomeThresholdModifier = 0;
        PreviousEncounterState = this;

        // Initialize Universal Encounter System components
        InitializeUniversalEncounterSystem(encounterInfo);
    }

    private void InitializeUniversalEncounterSystem(Encounter encounterInfo)
    {
        // All encounters use exactly 6 Focus Points (5-stage system)
        MaxFocusPoints = 6;
        FocusPoints = MaxFocusPoints;

        // Initialize empty Aspect Token pool
        AspectTokens = new AspectTokenPool();

        // Set encounter type for skill mapping
        EncounterType = encounterInfo.EncounterType;
    }

    public bool CanAffordFocusCost(int focusCost)
    {
        return FocusPoints >= focusCost;
    }

    public void SpendFocusPoints(int amount)
    {
        FocusPoints = Math.Max(0, FocusPoints - amount);
    }

    public void AddAspectTokens(AspectTokenTypes tokenType, int amount)
    {
        AspectTokens.AddTokens(tokenType, amount);
    }

    public bool HasAspectTokens(AspectTokenTypes tokenType, int amount)
    {
        return AspectTokens.GetTokenCount(tokenType) >= amount;
    }

    public void SpendAspectTokens(AspectTokenTypes tokenType, int amount)
    {
        AspectTokens.SpendTokens(tokenType, amount);
    }

    public void RecoverFocusPoint()
    {
        if (FocusPoints < MaxFocusPoints)
        {
            FocusPoints++;
        }
    }

    public void AddProgress(int amount)
    {
        CurrentProgress = Math.Max(0, CurrentProgress + amount);
    }

    public void LoseProgress(int amount)
    {
        CurrentProgress = Math.Max(0, CurrentProgress - amount);
    }

    public static EncounterState CreateDeepCopy(EncounterState originalState, PlayerState playerState)
    {
        EncounterState copy = new EncounterState(originalState.EncounterInfo, playerState);
        copy.CurrentProgress = originalState.CurrentProgress;
        copy.CurrentStageIndex = originalState.CurrentStageIndex;
        copy.CurrentTurn = originalState.CurrentTurn;
        copy.LocationSpot = originalState.LocationSpot;
        copy.OutcomeThresholdModifier = originalState.OutcomeThresholdModifier;

        // Copy Universal Encounter System state
        copy.FocusPoints = originalState.FocusPoints;
        copy.MaxFocusPoints = originalState.MaxFocusPoints;
        copy.AspectTokens = originalState.AspectTokens.CreateCopy();
        copy.EncounterType = originalState.EncounterType;

        return copy;
    }

    public ChoiceProjection ApplyChoice(PlayerState playerState, Encounter encounterInfo, EncounterOption choice)
    {
        UpdateStateHistory(choice);
        ChoiceProjection projection = CreateChoiceProjection(choice, playerState);
        ApplyChoiceProjection(playerState, encounterInfo, projection);
        return projection;
    }

    private void ApplyChoiceProjection(PlayerState playerState, Encounter encounterInfo, ChoiceProjection projection)
    {
        // Apply Universal Encounter System effects
        ApplyUniversalEncounterEffects(projection, playerState);

        // Advance turn and stage
        CurrentTurn++;
        if (CurrentStageIndex < EncounterInfo.Stages.Count)
        {
            EncounterInfo.Stages[CurrentStageIndex].IsCompleted = true;
            if (CurrentStageIndex < EncounterInfo.Stages.Count - 1)
            {
                CurrentStageIndex++;
            }
        }
    }

    private void ApplyUniversalEncounterEffects(ChoiceProjection projection, PlayerState playerState)
    {
        EncounterOption choice = projection.Choice;

        // Spend Focus Points for the choice
        SpendFocusPoints(choice.FocusCost);

        // Handle token conversion actions first (spend tokens)
        if (choice.RequiresTokens() && projection.IsAffordableAspectTokens)
        {
            ApplyTokenConversion(choice);
        }

        // Apply positive effects
        ApplyPositiveEffects(projection, choice);

        // Apply negative consequences if skill check failed
        if (!projection.SkillCheckSuccess)
        {
            ApplyNegativeConsequences(choice, playerState);
        }
    }

    private void ApplyTokenConversion(EncounterOption choice)
    {
        // Spend the required tokens for conversion actions
        foreach (var tokenCost in choice.TokenCosts)
        {
            SpendAspectTokens(tokenCost.Key, tokenCost.Value);
        }
    }

    private void ApplyPositiveEffects(ChoiceProjection projection, EncounterOption choice)
    {
        // Handle Recovery option
        if (choice.ActionType == UniversalActionTypes.Recovery)
        {
            RecoverFocusPoint();
            return;
        }

        // Add aspect tokens from generation effects
        foreach (var tokenGain in projection.AspectTokensGained)
        {
            if (tokenGain.Value > 0)
            {
                AddAspectTokens(tokenGain.Key, tokenGain.Value);
            }
        }

        // Add progress from conversion/hybrid effects
        if (projection.ProgressGained > 0)
        {
            AddProgress(projection.ProgressGained);
        }
    }

    private void ApplyNegativeConsequences(EncounterOption choice, PlayerState playerState)
    {
        NegativeConsequenceTypes consequenceType = choice.NegativeConsequenceType;

        // Handle Recovery cascading negative
        if (choice.ActionType == UniversalActionTypes.Recovery)
        {
            consequenceType = DetermineRecoveryNegative();
        }

        switch (consequenceType)
        {
            case NegativeConsequenceTypes.ProgressLoss:
                LoseProgress(1);
                break;

            case NegativeConsequenceTypes.FocusLoss:
                SpendFocusPoints(1);
                break;

            case NegativeConsequenceTypes.TokenDisruption:
                AspectTokens.DiscardRandomTokens(2); // Recovery discard is 2 tokens
                break;

            case NegativeConsequenceTypes.ThresholdIncrease:
                OutcomeThresholdModifier++;
                break;

            case NegativeConsequenceTypes.ConversionReduction:
                // This is handled in ChoiceProjectionService during projection
                // The progress is already reduced in the projection
                break;

            case NegativeConsequenceTypes.None:
                // No consequence to apply
                break;
        }
    }

    private NegativeConsequenceTypes DetermineRecoveryNegative()
    {
        // Cascading recovery negative system
        if (CurrentProgress > 0)
            return NegativeConsequenceTypes.ProgressLoss;
        else if (AspectTokens.GetTotalTokenCount() >= 2)
            return NegativeConsequenceTypes.TokenDisruption;
        else
            return NegativeConsequenceTypes.ThresholdIncrease;
    }

    public void UpdateStateHistory(EncounterOption selectedChoice)
    {
        PreviousChoice = selectedChoice;
    }

    public ChoiceProjection CreateChoiceProjection(
        EncounterOption choice,
        PlayerState playerState)
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

    public EncounterOutcomes GetFinalOutcome()
    {
        if (!IsEncounterComplete()) return EncounterOutcomes.None;

        int basicThreshold = 10 + OutcomeThresholdModifier;
        int goodThreshold = 14 + OutcomeThresholdModifier;
        int excellentThreshold = 18 + OutcomeThresholdModifier;

        if (CurrentProgress >= excellentThreshold)
            return EncounterOutcomes.ExcellentSuccess;
        else if (CurrentProgress >= goodThreshold)
            return EncounterOutcomes.GoodSuccess;
        else if (CurrentProgress >= basicThreshold)
            return EncounterOutcomes.BasicSuccess;
        else
            return EncounterOutcomes.Failure;
    }
}

public class AspectTokenPool
{
    private Dictionary<AspectTokenTypes, int> tokens;

    public AspectTokenPool()
    {
        tokens = new Dictionary<AspectTokenTypes, int>();
        foreach (AspectTokenTypes tokenType in Enum.GetValues<AspectTokenTypes>())
        {
            tokens[tokenType] = 0;
        }
    }

    public void AddTokens(AspectTokenTypes tokenType, int amount)
    {
        tokens[tokenType] += Math.Max(0, amount);
    }

    public void SpendTokens(AspectTokenTypes tokenType, int amount)
    {
        tokens[tokenType] = Math.Max(0, tokens[tokenType] - amount);
    }

    public int GetTokenCount(AspectTokenTypes tokenType)
    {
        return tokens[tokenType];
    }

    public int GetTotalTokenCount()
    {
        return tokens.Values.Sum();
    }

    public void DiscardRandomToken()
    {
        DiscardRandomTokens(1);
    }

    public void DiscardRandomTokens(int count)
    {
        for (int i = 0; i < count; i++)
        {
            List<AspectTokenTypes> availableTypes = tokens
                .Where(kvp => kvp.Value > 0)
                .Select(kvp => kvp.Key)
                .ToList();

            if (availableTypes.Count > 0)
            {
                Random random = new Random();
                AspectTokenTypes randomType = availableTypes[random.Next(availableTypes.Count)];
                SpendTokens(randomType, 1);
            }
            else
            {
                break; // No more tokens to discard
            }
        }
    }

    public AspectTokenPool CreateCopy()
    {
        AspectTokenPool copy = new AspectTokenPool();
        foreach (var kvp in tokens)
        {
            copy.tokens[kvp.Key] = kvp.Value;
        }
        return copy;
    }

    public Dictionary<AspectTokenTypes, int> GetAllTokenCounts()
    {
        return new Dictionary<AspectTokenTypes, int>(tokens);
    }

    public bool HasTokens(AspectTokenTypes tokenType, int amount)
    {
        return tokens[tokenType] >= amount;
    }

    public bool HasAnyTokens()
    {
        return tokens.Values.Any(count => count > 0);
    }
}
