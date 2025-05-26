
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
        choices.Add(CreateGenerationOption(UniversalActionTypes.GenerationA, tier, state.SkillCategory, stageNumber));
        choices.Add(CreateGenerationOption(UniversalActionTypes.GenerationB, tier, state.SkillCategory, stageNumber));

        // 2 Conversion options (only if player has tokens)
        choices.Add(CreateConversionOption(UniversalActionTypes.ConversionA, tier, state.SkillCategory, stageNumber));
        choices.Add(CreateConversionOption(UniversalActionTypes.ConversionB, tier, state.SkillCategory, stageNumber));

        // 1 Hybrid option
        choices.Add(CreateHybridOption(tier, state.SkillCategory, stageNumber));

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

    private EncounterOption CreateGenerationOption(UniversalActionTypes actionType, EncounterTiers tier, SkillCategories SkillCategory, int stageNumber)
    {
        string id = $"Stage{stageNumber}_{actionType}";
        string name = GetGenerationName(actionType, SkillCategory);

        // Get tier-specific values
        (int focusCost, int tokenAmount) = GetGenerationValues(actionType, tier);

        // Map to appropriate skill based on encounter type
        SkillTypes skill = GetPrimarySkillForGeneration(actionType, SkillCategory);
        int difficulty = GetDifficultyForTier(tier);

        EncounterOption option = new EncounterOption(id, name)
        {
            Description = GetGenerationDescription(actionType, SkillCategory),
            FocusCost = focusCost,
            Skill = skill,
            Difficulty = difficulty,
            ActionType = actionType,
            NegativeConsequenceType = GetNegativeConsequenceForGeneration(actionType),
            Tags = new List<string> { "Generation", tier.ToString() }
        };

        return option;
    }

    private EncounterOption CreateConversionOption(UniversalActionTypes actionType, EncounterTiers tier, SkillCategories SkillCategory, int stageNumber)
    {
        string id = $"Stage{stageNumber}_{actionType}";
        string name = GetConversionName(actionType, SkillCategory);

        // Get tier-specific values
        (int focusCost, int tokensRequired, int progressYield) = GetConversionValues(actionType, tier);

        // Map to appropriate skill
        SkillTypes skill = GetPrimarySkillForConversion(actionType, SkillCategory);
        int difficulty = GetDifficultyForTier(tier);

        EncounterOption option = new EncounterOption(id, name)
        {
            Description = GetConversionDescription(actionType, SkillCategory),
            FocusCost = focusCost,
            Skill = skill,
            Difficulty = difficulty,
            ActionType = actionType,
            NegativeConsequenceType = GetNegativeConsequenceForConversion(actionType),
            Tags = new List<string> { "Conversion", tier.ToString() }
        };

        return option;
    }

    private EncounterOption CreateHybridOption(EncounterTiers tier, SkillCategories SkillCategory, int stageNumber)
    {
        string id = $"Stage{stageNumber}_Hybrid";
        string name = GetHybridName(SkillCategory);

        SkillTypes skill = GetSecondarySkillForSkillCategory(SkillCategory);
        int difficulty = GetDifficultyForTier(tier);

        int focusCost = 1;
        int progressAmount = 1;

        EncounterOption option = new EncounterOption(id, name)
        {
            Description = GetHybridDescription(SkillCategory),
            FocusCost = focusCost,
            Skill = skill,
            Difficulty = difficulty,
            ActionType = UniversalActionTypes.Hybrid,
            Tags = new List<string> { "Hybrid", tier.ToString() }
        };

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
            NegativeConsequenceType = GetRecoveryNegativeConsequence(state),
            Tags = new List<string> { "Recovery", "Safety" }
        };

        return option;
    }

    private NegativeConsequenceTypes GetRecoveryNegativeConsequence(EncounterState state)
    {
        return NegativeConsequenceTypes.None; // Recovery does not have a negative consequence by default
    }

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

    private SkillTypes GetPrimarySkillForGeneration(UniversalActionTypes actionType, SkillCategories SkillCategory)
    {
        return (SkillCategory, actionType) switch
        {
            (SkillCategories.Physical, UniversalActionTypes.GenerationA) => SkillTypes.Strength,
            (SkillCategories.Physical, UniversalActionTypes.GenerationB) => SkillTypes.Agility,
            (SkillCategories.Social, UniversalActionTypes.GenerationA) => SkillTypes.Intimidation,
            (SkillCategories.Social, UniversalActionTypes.GenerationB) => SkillTypes.Charm,
            (SkillCategories.Intellectual, UniversalActionTypes.GenerationA) => SkillTypes.Analysis,
            (SkillCategories.Intellectual, UniversalActionTypes.GenerationB) => SkillTypes.Observation,
            _ => SkillTypes.None
        };
    }

    private SkillTypes GetPrimarySkillForConversion(UniversalActionTypes actionType, SkillCategories SkillCategory)
    {
        return (SkillCategory, actionType) switch
        {
            (SkillCategories.Physical, UniversalActionTypes.ConversionA) => SkillTypes.Precision,
            (SkillCategories.Physical, UniversalActionTypes.ConversionB) => SkillTypes.Endurance,
            (SkillCategories.Social, UniversalActionTypes.ConversionA) => SkillTypes.Persuasion,
            (SkillCategories.Social, UniversalActionTypes.ConversionB) => SkillTypes.Deception,
            (SkillCategories.Intellectual, UniversalActionTypes.ConversionA) => SkillTypes.Knowledge,
            (SkillCategories.Intellectual, UniversalActionTypes.ConversionB) => SkillTypes.Planning,
            _ => SkillTypes.None
        };
    }

    private SkillTypes GetSecondarySkillForSkillCategory(SkillCategories SkillCategory)
    {
        return SkillCategory switch
        {
            SkillCategories.Physical => SkillTypes.Endurance,
            SkillCategories.Social => SkillTypes.Persuasion,
            SkillCategories.Intellectual => SkillTypes.Planning,
            _ => SkillTypes.None
        };
    }

    private NegativeConsequenceTypes GetNegativeConsequenceForGeneration(UniversalActionTypes actionType)
    {
        return actionType switch
        {
            UniversalActionTypes.GenerationA => NegativeConsequenceTypes.FocusLoss,
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

    // Narrative generation helpers
    private string GetGenerationName(UniversalActionTypes actionType, SkillCategories SkillCategory)
    {
        return (SkillCategory, actionType) switch
        {
            (SkillCategories.Physical, UniversalActionTypes.GenerationA) => "Apply Raw Strength",
            (SkillCategories.Physical, UniversalActionTypes.GenerationB) => "Move Fluidly",
            (SkillCategories.Social, UniversalActionTypes.GenerationA) => "Assert Dominance",
            (SkillCategories.Social, UniversalActionTypes.GenerationB) => "Read the Room",
            (SkillCategories.Intellectual, UniversalActionTypes.GenerationA) => "Direct Analysis",
            (SkillCategories.Intellectual, UniversalActionTypes.GenerationB) => "Observe Patterns",
            _ => "Generate Tokens"
        };
    }

    private string GetGenerationDescription(UniversalActionTypes actionType, SkillCategories SkillCategory)
    {
        return (SkillCategory, actionType) switch
        {
            (SkillCategories.Physical, UniversalActionTypes.GenerationA) => "Use your physical strength to overcome obstacles directly",
            (SkillCategories.Physical, UniversalActionTypes.GenerationB) => "Adapt your movements to the situation",
            (SkillCategories.Social, UniversalActionTypes.GenerationA) => "Take control of the social dynamic",
            (SkillCategories.Social, UniversalActionTypes.GenerationB) => "Adapt your approach based on social cues",
            (SkillCategories.Intellectual, UniversalActionTypes.GenerationA) => "Cut directly to the core of the problem",
            (SkillCategories.Intellectual, UniversalActionTypes.GenerationB) => "Notice emerging patterns and details",
            _ => "Build up your capabilities"
        };
    }

    private string GetConversionName(UniversalActionTypes actionType, SkillCategories SkillCategory)
    {
        return (SkillCategory, actionType) switch
        {
            (SkillCategories.Physical, UniversalActionTypes.ConversionA) => "Precise Application",
            (SkillCategories.Physical, UniversalActionTypes.ConversionB) => "Sustained Effort",
            (SkillCategories.Social, UniversalActionTypes.ConversionA) => "Persuasive Argument",
            (SkillCategories.Social, UniversalActionTypes.ConversionB) => "Maintained Deception",
            (SkillCategories.Intellectual, UniversalActionTypes.ConversionA) => "Applied Knowledge",
            (SkillCategories.Intellectual, UniversalActionTypes.ConversionB) => "Strategic Planning",
            _ => "Convert Effort"
        };
    }

    private string GetConversionDescription(UniversalActionTypes actionType, SkillCategories SkillCategory)
    {
        return (SkillCategory, actionType) switch
        {
            (SkillCategories.Physical, UniversalActionTypes.ConversionA) => "Apply precise technique to make progress",
            (SkillCategories.Physical, UniversalActionTypes.ConversionB) => "Push through with sustained physical effort",
            (SkillCategories.Social, UniversalActionTypes.ConversionA) => "Use persuasive skills to advance your position",
            (SkillCategories.Social, UniversalActionTypes.ConversionB) => "Maintain your facade to achieve your goals",
            (SkillCategories.Intellectual, UniversalActionTypes.ConversionA) => "Apply your knowledge to solve the problem",
            (SkillCategories.Intellectual, UniversalActionTypes.ConversionB) => "Execute your carefully laid plans",
            _ => "Convert your accumulated efforts into progress"
        };
    }

    private string GetHybridName(SkillCategories SkillCategory)
    {
        return SkillCategory switch
        {
            SkillCategories.Physical => "Balanced Physical Approach",
            SkillCategories.Social => "Diplomatic Balance",
            SkillCategories.Intellectual => "Thoughtful Progress",
            _ => "Balanced Approach"
        };
    }

    private string GetHybridDescription(SkillCategories SkillCategory)
    {
        return SkillCategory switch
        {
            SkillCategories.Physical => "Combine immediate action with building strength",
            SkillCategories.Social => "Make progress while maintaining relationships",
            SkillCategories.Intellectual => "Apply knowledge while gathering more insights",
            _ => "Balance immediate progress with future preparation"
        };
    }
}

