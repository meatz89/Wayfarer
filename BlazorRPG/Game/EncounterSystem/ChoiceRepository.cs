public class ChoiceRepository
{
    private readonly ILogger<ChoiceRepository> _logger;

    public ChoiceRepository(ILogger<ChoiceRepository> logger)
    {
        _logger = logger;
    }

    public List<EncounterOption> GetForEncounter(EncounterState state)
    {
        int stageNumber = state.CurrentStageIndex + 1;
        EncounterTiers tier = GetTierForStage(stageNumber);

        // Generate exactly 6 choices per stage following the universal template distribution
        List<EncounterOption> choices = new List<EncounterOption>();

        // 2 Generation options
        choices.Add(CreateGenerationOption(UniversalActionTypes.GenerationA, tier, state.EncounterType, stageNumber));
        choices.Add(CreateGenerationOption(UniversalActionTypes.GenerationB, tier, state.EncounterType, stageNumber));

        // 2 Conversion options (only if player has tokens)
        choices.Add(CreateConversionOption(UniversalActionTypes.ConversionA, tier, state.EncounterType, stageNumber));
        choices.Add(CreateConversionOption(UniversalActionTypes.ConversionB, tier, state.EncounterType, stageNumber));

        // 1 Hybrid option
        choices.Add(CreateHybridOption(tier, state.EncounterType, stageNumber));

        // 1 Recovery option (always available at 0 Focus)
        choices.Add(CreateRecoveryOption(state));

        return choices;
    }

    private EncounterTiers GetTierForStage(int stageNumber)
    {
        return stageNumber switch
        {
            1 or 2 => EncounterTiers.Foundation,
            3 or 4 => EncounterTiers.Development,
            5 => EncounterTiers.Execution,
            _ => EncounterTiers.Foundation
        };
    }

    private EncounterOption CreateGenerationOption(UniversalActionTypes actionType, EncounterTiers tier, CardTypes encounterType, int stageNumber)
    {
        string id = $"Stage{stageNumber}_{actionType}";
        string name = GetGenerationName(actionType, encounterType);

        // Get tier-specific values
        (int focusCost, int tokenAmount) = GetGenerationValues(actionType, tier);

        // Map to appropriate skill based on encounter type
        SkillTypes skill = GetPrimarySkillForGeneration(actionType, encounterType);
        int difficulty = GetDifficultyForTier(tier);

        EncounterOption option = new EncounterOption(id, name)
        {
            Description = GetGenerationDescription(actionType, encounterType),
            FocusCost = focusCost,
            Skill = skill,
            Difficulty = difficulty,
            ActionType = actionType,
            TokenGeneration = new Dictionary<AspectTokenTypes, int>(),
            TokenCosts = new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceType = GetNegativeConsequenceForGeneration(actionType),
            Tags = new List<string> { "Generation", tier.ToString() }
        };

        // Set which token type this generates based on skill
        AspectTokenTypes tokenType = MapSkillToToken(skill, encounterType);
        option.TokenGeneration[tokenType] = tokenAmount;

        return option;
    }

    public static AspectTokenTypes MapSkillToToken(SkillTypes skill, CardTypes encounterType)
    {
        return encounterType switch
        {
            CardTypes.Physical => MapPhysicalSkillToToken(skill),
            CardTypes.Social => MapSocialSkillToToken(skill),
            CardTypes.Intellectual => MapIntellectualSkillToToken(skill),
            _ => AspectTokenTypes.Force // Default fallback
        };
    }

    private static AspectTokenTypes MapPhysicalSkillToToken(SkillTypes skill)
    {
        return skill switch
        {
            SkillTypes.Strength => AspectTokenTypes.Force,
            SkillTypes.Agility => AspectTokenTypes.Flow,
            SkillTypes.Precision => AspectTokenTypes.Focus,
            SkillTypes.Endurance => AspectTokenTypes.Fortitude,
            _ => AspectTokenTypes.Force
        };
    }

    private static AspectTokenTypes MapSocialSkillToToken(SkillTypes skill)
    {
        return skill switch
        {
            SkillTypes.Intimidation => AspectTokenTypes.Force,
            SkillTypes.Charm => AspectTokenTypes.Flow,
            SkillTypes.Persuasion => AspectTokenTypes.Focus,
            SkillTypes.Deception => AspectTokenTypes.Fortitude,
            _ => AspectTokenTypes.Flow
        };
    }

    private static AspectTokenTypes MapIntellectualSkillToToken(SkillTypes skill)
    {
        return skill switch
        {
            SkillTypes.Analysis => AspectTokenTypes.Force,
            SkillTypes.Observation => AspectTokenTypes.Flow,
            SkillTypes.Knowledge => AspectTokenTypes.Focus,
            SkillTypes.Planning => AspectTokenTypes.Fortitude,
            _ => AspectTokenTypes.Focus
        };
    }



    private EncounterOption CreateConversionOption(UniversalActionTypes actionType, EncounterTiers tier, CardTypes encounterType, int stageNumber)
    {
        string id = $"Stage{stageNumber}_{actionType}";
        string name = GetConversionName(actionType, encounterType);

        // Get tier-specific values
        (int focusCost, int tokensRequired, int progressYield) = GetConversionValues(actionType, tier);

        // Map to appropriate skill
        SkillTypes skill = GetPrimarySkillForConversion(actionType, encounterType);
        int difficulty = GetDifficultyForTier(tier);

        EncounterOption option = new EncounterOption(id, name)
        {
            Description = GetConversionDescription(actionType, encounterType),
            FocusCost = focusCost,
            Skill = skill,
            Difficulty = difficulty,
            ActionType = actionType,
            SuccessProgress = progressYield,
            TokenGeneration = new Dictionary<AspectTokenTypes, int>(),
            TokenCosts = new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceType = GetNegativeConsequenceForConversion(actionType),
            Tags = new List<string> { "Conversion", tier.ToString() }
        };

        // Set token costs based on conversion type
        SetConversionTokenCosts(option, actionType, tokensRequired, encounterType);

        return option;
    }

    private EncounterOption CreateHybridOption(EncounterTiers tier, CardTypes encounterType, int stageNumber)
    {
        string id = $"Stage{stageNumber}_Hybrid";
        string name = GetHybridName(encounterType);

        // Get tier-specific values
        (int focusCost, int tokenAmount, int progressAmount) = GetHybridValues(tier);

        SkillTypes skill = GetSecondarySkillForEncounterType(encounterType);
        int difficulty = GetDifficultyForTier(tier);

        EncounterOption option = new EncounterOption(id, name)
        {
            Description = GetHybridDescription(encounterType),
            FocusCost = focusCost,
            Skill = skill,
            Difficulty = difficulty,
            ActionType = UniversalActionTypes.Hybrid,
            SuccessProgress = progressAmount,
            TokenGeneration = new Dictionary<AspectTokenTypes, int>(),
            TokenCosts = new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceType = NegativeConsequenceTypes.TokenDisruption,
            Tags = new List<string> { "Hybrid", tier.ToString() }
        };

        // Hybrid generates tokens AND progress
        AspectTokenTypes tokenType = MapSkillToToken(skill, encounterType);
        option.TokenGeneration[tokenType] = tokenAmount;

        return option;
    }

    private EncounterOption CreateRecoveryOption(EncounterState state)
    {
        string id = $"Stage{state.CurrentStageIndex + 1}_Recovery";

        EncounterOption option = new EncounterOption(id, "Gather Resolve")
        {
            Description = "Take a moment to gather your strength and focus",
            FocusCost = 0, // Always 0 cost
            Skill = SkillTypes.Planning, // Use Planning for recovery
            Difficulty = 4, // Standard difficulty
            ActionType = UniversalActionTypes.Recovery,
            SuccessProgress = 0,
            TokenGeneration = new Dictionary<AspectTokenTypes, int>(),
            TokenCosts = new Dictionary<AspectTokenTypes, int>(),
            NegativeConsequenceType = GetRecoveryNegativeConsequence(state),
            Tags = new List<string> { "Recovery", "Safety" }
        };

        return option;
    }

    // Value lookup methods based on the refined numerical framework
    private (int focusCost, int tokenAmount) GetGenerationValues(UniversalActionTypes actionType, EncounterTiers tier)
    {
        return (tier, actionType) switch
        {
            // Foundation Tier (Stages 1-2)
            (EncounterTiers.Foundation, UniversalActionTypes.GenerationA) => (1, 3),
            (EncounterTiers.Foundation, UniversalActionTypes.GenerationB) => (1, 2),

            // Development Tier (Stages 3-4)
            (EncounterTiers.Development, UniversalActionTypes.GenerationA) => (1, 4),
            (EncounterTiers.Development, UniversalActionTypes.GenerationB) => (2, 6),

            // Execution Tier (Stage 5)
            (EncounterTiers.Execution, UniversalActionTypes.GenerationA) => (1, 4),
            (EncounterTiers.Execution, UniversalActionTypes.GenerationB) => (2, 6),

            _ => (1, 2)
        };
    }

    private (int focusCost, int tokensRequired, int progressYield) GetConversionValues(UniversalActionTypes actionType, EncounterTiers tier)
    {
        return (tier, actionType) switch
        {
            // Foundation Tier
            (EncounterTiers.Foundation, UniversalActionTypes.ConversionA) => (1, 1, 2),
            (EncounterTiers.Foundation, UniversalActionTypes.ConversionB) => (1, 2, 4),

            // Development Tier
            (EncounterTiers.Development, UniversalActionTypes.ConversionA) => (1, 2, 4),
            (EncounterTiers.Development, UniversalActionTypes.ConversionB) => (1, 3, 6),

            // Execution Tier
            (EncounterTiers.Execution, UniversalActionTypes.ConversionA) => (1, 2, 5),
            (EncounterTiers.Execution, UniversalActionTypes.ConversionB) => (1, 3, 7),

            _ => (1, 1, 2)
        };
    }

    private (int focusCost, int tokenAmount, int progressAmount) GetHybridValues(EncounterTiers tier)
    {
        return tier switch
        {
            EncounterTiers.Foundation => (1, 1, 1),
            EncounterTiers.Development => (2, 3, 2),
            EncounterTiers.Execution => (1, 1, 3),
            _ => (1, 1, 1)
        };
    }

    private int GetDifficultyForTier(EncounterTiers tier)
    {
        return tier switch
        {
            EncounterTiers.Foundation => 3,
            EncounterTiers.Development => 4,
            EncounterTiers.Execution => 4,
            _ => 3
        };
    }

    private void SetConversionTokenCosts(EncounterOption option, UniversalActionTypes actionType, int tokensRequired, CardTypes encounterType)
    {
        // For simplicity, we'll require a mix of tokens based on the conversion type
        if (actionType == UniversalActionTypes.ConversionA)
        {
            // Single token type conversion
            AspectTokenTypes primaryToken = GetPrimaryTokenForEncounterType(encounterType);
            option.TokenCosts[primaryToken] = tokensRequired;
        }
        else
        {
            // Multi-token conversion
            if (tokensRequired == 2)
            {
                option.TokenCosts[AspectTokenTypes.Force] = 1;
                option.TokenCosts[AspectTokenTypes.Flow] = 1;
            }
            else if (tokensRequired == 3)
            {
                option.TokenCosts[AspectTokenTypes.Force] = 1;
                option.TokenCosts[AspectTokenTypes.Flow] = 1;
                option.TokenCosts[AspectTokenTypes.Focus] = 1;
            }
        }
    }

    private AspectTokenTypes GetPrimaryTokenForEncounterType(CardTypes encounterType)
    {
        return encounterType switch
        {
            CardTypes.Physical => AspectTokenTypes.Force,
            CardTypes.Social => AspectTokenTypes.Flow,
            CardTypes.Intellectual => AspectTokenTypes.Focus,
            _ => AspectTokenTypes.Force
        };
    }

    private SkillTypes GetPrimarySkillForGeneration(UniversalActionTypes actionType, CardTypes encounterType)
    {
        return (encounterType, actionType) switch
        {
            (CardTypes.Physical, UniversalActionTypes.GenerationA) => SkillTypes.Strength,
            (CardTypes.Physical, UniversalActionTypes.GenerationB) => SkillTypes.Agility,
            (CardTypes.Social, UniversalActionTypes.GenerationA) => SkillTypes.Intimidation,
            (CardTypes.Social, UniversalActionTypes.GenerationB) => SkillTypes.Charm,
            (CardTypes.Intellectual, UniversalActionTypes.GenerationA) => SkillTypes.Analysis,
            (CardTypes.Intellectual, UniversalActionTypes.GenerationB) => SkillTypes.Observation,
            _ => SkillTypes.None
        };
    }

    private SkillTypes GetPrimarySkillForConversion(UniversalActionTypes actionType, CardTypes encounterType)
    {
        return (encounterType, actionType) switch
        {
            (CardTypes.Physical, UniversalActionTypes.ConversionA) => SkillTypes.Precision,
            (CardTypes.Physical, UniversalActionTypes.ConversionB) => SkillTypes.Endurance,
            (CardTypes.Social, UniversalActionTypes.ConversionA) => SkillTypes.Persuasion,
            (CardTypes.Social, UniversalActionTypes.ConversionB) => SkillTypes.Deception,
            (CardTypes.Intellectual, UniversalActionTypes.ConversionA) => SkillTypes.Knowledge,
            (CardTypes.Intellectual, UniversalActionTypes.ConversionB) => SkillTypes.Planning,
            _ => SkillTypes.None
        };
    }

    private SkillTypes GetSecondarySkillForEncounterType(CardTypes encounterType)
    {
        return encounterType switch
        {
            CardTypes.Physical => SkillTypes.Endurance,
            CardTypes.Social => SkillTypes.Persuasion,
            CardTypes.Intellectual => SkillTypes.Planning,
            _ => SkillTypes.None
        };
    }

    private NegativeConsequenceTypes GetNegativeConsequenceForGeneration(UniversalActionTypes actionType)
    {
        return actionType switch
        {
            UniversalActionTypes.GenerationA => NegativeConsequenceTypes.FocusLoss,
            UniversalActionTypes.GenerationB => NegativeConsequenceTypes.TokenDisruption,
            _ => NegativeConsequenceTypes.ThresholdIncrease
        };
    }

    private NegativeConsequenceTypes GetNegativeConsequenceForConversion(UniversalActionTypes actionType)
    {
        return actionType switch
        {
            UniversalActionTypes.ConversionA => NegativeConsequenceTypes.ConversionReduction,
            UniversalActionTypes.ConversionB => NegativeConsequenceTypes.ProgressLoss,
            _ => NegativeConsequenceTypes.ThresholdIncrease
        };
    }

    private NegativeConsequenceTypes GetRecoveryNegativeConsequence(EncounterState state)
    {
        // Cascading negative structure based on current state
        if (state.CurrentProgress > 0)
            return NegativeConsequenceTypes.ProgressLoss;
        else if (state.AspectTokens.GetAllTokenCounts().Values.Sum() >= 2)
            return NegativeConsequenceTypes.TokenDisruption;
        else
            return NegativeConsequenceTypes.ThresholdIncrease;
    }

    // Narrative generation helpers
    private string GetGenerationName(UniversalActionTypes actionType, CardTypes encounterType)
    {
        return (encounterType, actionType) switch
        {
            (CardTypes.Physical, UniversalActionTypes.GenerationA) => "Apply Raw Strength",
            (CardTypes.Physical, UniversalActionTypes.GenerationB) => "Move Fluidly",
            (CardTypes.Social, UniversalActionTypes.GenerationA) => "Assert Dominance",
            (CardTypes.Social, UniversalActionTypes.GenerationB) => "Read the Room",
            (CardTypes.Intellectual, UniversalActionTypes.GenerationA) => "Direct Analysis",
            (CardTypes.Intellectual, UniversalActionTypes.GenerationB) => "Observe Patterns",
            _ => "Generate Tokens"
        };
    }

    private string GetGenerationDescription(UniversalActionTypes actionType, CardTypes encounterType)
    {
        return (encounterType, actionType) switch
        {
            (CardTypes.Physical, UniversalActionTypes.GenerationA) => "Use your physical strength to overcome obstacles directly",
            (CardTypes.Physical, UniversalActionTypes.GenerationB) => "Adapt your movements to the situation",
            (CardTypes.Social, UniversalActionTypes.GenerationA) => "Take control of the social dynamic",
            (CardTypes.Social, UniversalActionTypes.GenerationB) => "Adapt your approach based on social cues",
            (CardTypes.Intellectual, UniversalActionTypes.GenerationA) => "Cut directly to the core of the problem",
            (CardTypes.Intellectual, UniversalActionTypes.GenerationB) => "Notice emerging patterns and details",
            _ => "Build up your capabilities"
        };
    }

    private string GetConversionName(UniversalActionTypes actionType, CardTypes encounterType)
    {
        return (encounterType, actionType) switch
        {
            (CardTypes.Physical, UniversalActionTypes.ConversionA) => "Precise Application",
            (CardTypes.Physical, UniversalActionTypes.ConversionB) => "Sustained Effort",
            (CardTypes.Social, UniversalActionTypes.ConversionA) => "Persuasive Argument",
            (CardTypes.Social, UniversalActionTypes.ConversionB) => "Maintained Deception",
            (CardTypes.Intellectual, UniversalActionTypes.ConversionA) => "Applied Knowledge",
            (CardTypes.Intellectual, UniversalActionTypes.ConversionB) => "Strategic Planning",
            _ => "Convert Effort"
        };
    }

    private string GetConversionDescription(UniversalActionTypes actionType, CardTypes encounterType)
    {
        return (encounterType, actionType) switch
        {
            (CardTypes.Physical, UniversalActionTypes.ConversionA) => "Apply precise technique to make progress",
            (CardTypes.Physical, UniversalActionTypes.ConversionB) => "Push through with sustained physical effort",
            (CardTypes.Social, UniversalActionTypes.ConversionA) => "Use persuasive skills to advance your position",
            (CardTypes.Social, UniversalActionTypes.ConversionB) => "Maintain your facade to achieve your goals",
            (CardTypes.Intellectual, UniversalActionTypes.ConversionA) => "Apply your knowledge to solve the problem",
            (CardTypes.Intellectual, UniversalActionTypes.ConversionB) => "Execute your carefully laid plans",
            _ => "Convert your accumulated efforts into progress"
        };
    }

    private string GetHybridName(CardTypes encounterType)
    {
        return encounterType switch
        {
            CardTypes.Physical => "Balanced Physical Approach",
            CardTypes.Social => "Diplomatic Balance",
            CardTypes.Intellectual => "Thoughtful Progress",
            _ => "Balanced Approach"
        };
    }

    private string GetHybridDescription(CardTypes encounterType)
    {
        return encounterType switch
        {
            CardTypes.Physical => "Combine immediate action with building strength",
            CardTypes.Social => "Make progress while maintaining relationships",
            CardTypes.Intellectual => "Apply knowledge while gathering more insights",
            _ => "Balance immediate progress with future preparation"
        };
    }
}

