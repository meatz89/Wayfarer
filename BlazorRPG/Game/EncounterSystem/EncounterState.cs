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
        PreviousEncounterState = this;

        // Initialize Universal Encounter System components
        InitializeUniversalEncounterSystem(encounterInfo);
    }

    private void InitializeUniversalEncounterSystem(Encounter encounterInfo)
    {
        // Set Focus Points based on encounter length (standard 3-round encounters get 5-6 points)
        int stageCount = encounterInfo.Stages.Count;
        MaxFocusPoints = stageCount switch
        {
            <= 2 => 6, // Short encounters get more Focus
            3 => 5,    // Standard encounters
            >= 4 => 7  // Long encounters get more Focus
        };
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

    private void ApplyPositiveEffects(ChoiceProjection projection)
    {
        // Handle Recovery option
        if (projection.Choice.ActionType == UniversalActionTypes.Recovery)
        {
            RecoverFocusPoint();
            return;
        }

        // Add aspect tokens from positive effects
        foreach (AspectTokenTypes tokenType in Enum.GetValues<AspectTokenTypes>())
        {
            int tokenGain = projection.GetTokenGain(tokenType);
            if (tokenGain > 0)
            {
                AddAspectTokens(tokenType, tokenGain);
            }
        }
    }

    public static EncounterState CreateDeepCopy(EncounterState originalState, PlayerState playerState)
    {
        EncounterState copy = new EncounterState(originalState.EncounterInfo, playerState.Serialize());
        copy.CurrentProgress = originalState.CurrentProgress;
        copy.CurrentStageIndex = originalState.CurrentStageIndex;
        copy.CurrentTurn = originalState.CurrentTurn;
        copy.LocationSpot = originalState.LocationSpot;

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
        CurrentProgress += projection.ProgressGained;
        CurrentTurn++;

        // Apply Universal Encounter System effects
        ApplyUniversalEncounterEffects(projection, playerState);

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
        // Spend Focus Points for the choice
        SpendFocusPoints(projection.FocusCost);

        // Apply positive effects (token generation or conversion)
        ApplyPositiveEffects(projection);

        // Apply negative consequences if skill check failed
        if (!projection.SkillCheckSuccess)
        {
            ApplyNegativeConsequences(projection, playerState);
        }
    }

    private void ApplyTokenConversion(ChoiceProjection projection)
    {
        // Spend the required tokens for conversion
        foreach (AspectTokenTypes tokenType in Enum.GetValues<AspectTokenTypes>())
        {
            int tokenCost = projection.GetTokenCost(tokenType);
            if (tokenCost > 0)
            {
                SpendAspectTokens(tokenType, tokenCost);
            }
        }
    }

    private void ApplyNegativeConsequences(ChoiceProjection projection, PlayerState playerState)
    {
        NegativeConsequenceTypes consequenceType = projection.NegativeConsequenceType;

        switch (consequenceType)
        {
            case NegativeConsequenceTypes.FutureCostIncrease:
                // Next choice costs +1 Focus (handled in choice generation)
                break;

            case NegativeConsequenceTypes.TokenDisruption:
                // Discard 1 random token
                AspectTokens.DiscardRandomToken();
                break;

            case NegativeConsequenceTypes.ThresholdIncrease:
                // Increase final success requirements
                EncounterInfo.TotalProgress += 1;
                break;

            case NegativeConsequenceTypes.ProgressLoss:
                // Lose progress
                CurrentProgress = Math.Max(0, CurrentProgress - 1);
                break;

            case NegativeConsequenceTypes.FocusLoss:
                // Lose Focus from encounter pool
                FocusPoints = Math.Max(0, FocusPoints - 1);
                break;
        }
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
        tokens[tokenType] += amount;
    }

    public void SpendTokens(AspectTokenTypes tokenType, int amount)
    {
        tokens[tokenType] = Math.Max(0, tokens[tokenType] - amount);
    }

    public int GetTokenCount(AspectTokenTypes tokenType)
    {
        return tokens[tokenType];
    }

    public void DiscardRandomToken()
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
    }

    public AspectTokenPool CreateCopy()
    {
        AspectTokenPool copy = new AspectTokenPool();
        foreach (KeyValuePair<AspectTokenTypes, int> kvp in tokens)
        {
            copy.tokens[kvp.Key] = kvp.Value;
        }
        return copy;
    }

    public Dictionary<AspectTokenTypes, int> GetAllTokenCounts()
    {
        return new Dictionary<AspectTokenTypes, int>(tokens);
    }
}

public enum AspectTokenTypes
{
    Force,    // Red - Direct, powerful application
    Flow,     // Blue - Adaptive, responsive approach
    Focus,    // Yellow - Concentrated, precise effort
    Fortitude // Green - Sustained, patient approach
}

public enum NegativeConsequenceTypes
{
    None,
    FutureCostIncrease,  // Next choice costs +1 Focus
    TokenDisruption,     // Discard 1 random token
    ThresholdIncrease,   // Final success requirements +1
    ProgressLoss,        // Lose 1 Progress Marker
    FocusLoss,           // Lose 1 Focus Point from pool
}
