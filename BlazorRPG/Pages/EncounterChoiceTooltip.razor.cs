using Microsoft.AspNetCore.Components;

public partial class EncounterChoiceTooltipBase : ComponentBase
{
    [Inject] public GameManager GameManager { get; set; }
    [Inject] public GameState GameState { get; set; }
    [Parameter] public UserEncounterChoiceOption hoveredChoice { get; set; }
    [Parameter] public double tooltipX { get; set; }
    [Parameter] public double tooltipY { get; set; }

    protected string GetSkillCheckInfo(UserEncounterChoiceOption choice)
    {
        if (choice.Choice is EncounterOption option && option.Skill != SkillTypes.None)
        {
            string skillName = option.Skill.ToString();
            int difficulty = option.Difficulty;
            string locationEffect = option.LocationModifier switch
            {
                < 0 => $" (Easier due to location: -{Math.Abs(option.LocationModifier)})",
                > 0 => $" (Harder due to location: +{option.LocationModifier})",
                _ => ""
            };

            return $"Requires {skillName} check (Difficulty: {difficulty}{locationEffect})";
        }
        return "No skill check required";
    }

    protected string GetProgressInfo(UserEncounterChoiceOption choice)
    {
        if (choice.Choice is EncounterOption option)
        {
            // Get the choice projection to show accurate progress information
            ChoiceProjection projection = Preview;

            string focusCost = option.FocusCost > 0 ? $"Focus Cost: {option.FocusCost}" : "No Focus cost";

            string positiveEffects = GetPositiveEffectsDescription(option, projection);
            string negativeRisk = GetNegativeRiskDescription(option, projection);

            return $"{focusCost}\n{positiveEffects}\n{negativeRisk}";
        }
        return "";
    }

    private string GetPositiveEffectsDescription(EncounterOption option, ChoiceProjection projection)
    {
        List<string> effects = new List<string>();

        // Token generation
        foreach (KeyValuePair<AspectTokenTypes, int> tokenGen in option.TokenGeneration)
        {
            if (tokenGen.Value > 0)
            {
                effects.Add($"+{tokenGen.Value} {GetTokenDisplayName(tokenGen.Key)} tokens");
            }
        }

        // Direct progress
        if (projection.ProgressGained > 0)
        {
            effects.Add($"+{projection.ProgressGained} progress");
        }

        return effects.Count > 0 ? $"Positive: {string.Join(", ", effects)}" : "Positive: Minimal effect";
    }

    private string GetNegativeRiskDescription(EncounterOption option, ChoiceProjection projection)
    {
        if (option.Skill == SkillTypes.None)
        {
            return GetNegativeConsequenceDescription(option.NegativeConsequenceType);
        }

        string riskLevel = projection.SkillCheckSuccess ? "Low Risk" : "High Risk";
        string consequence = GetNegativeConsequenceDescription(option.NegativeConsequenceType);

        return $"{riskLevel}: {consequence}";
    }

    private string GetNegativeConsequenceDescription(NegativeConsequenceTypes consequenceType)
    {
        return consequenceType switch
        {
            NegativeConsequenceTypes.TokenDisruption => "Lose 1 random token",
            NegativeConsequenceTypes.ThresholdIncrease => "Success requirements increase",
            NegativeConsequenceTypes.ProgressLoss => "Lose 1 progress",
            NegativeConsequenceTypes.FocusLoss => "Lose 1 Focus from pool",
            _ => "Unknown risk"
        };
    }

    protected string GetTokenDisplayName(AspectTokenTypes tokenType)
    {
        return tokenType switch
        {
            AspectTokenTypes.Force => "Force",
            AspectTokenTypes.Flow => "Flow",
            AspectTokenTypes.Focus => "Focus",
            AspectTokenTypes.Fortitude => "Fortitude",
            _ => tokenType.ToString()
        };
    }

    // NEW METHODS FOR THE ENHANCED TOOLTIP

    protected string GetActionTypeClass(EncounterOption option)
    {
        return option.ActionType.ToString().ToLowerInvariant();
    }

    protected string GetActionTypeName(UniversalActionTypes actionType)
    {
        return actionType switch
        {
            UniversalActionTypes.Recovery => "Recovery",
            UniversalActionTypes.GenerationA => "Generation",
            UniversalActionTypes.GenerationB => "Generation",
            UniversalActionTypes.ConversionA => "Conversion",
            UniversalActionTypes.ConversionB => "Conversion",
            UniversalActionTypes.Hybrid => "Hybrid",
            _ => actionType.ToString()
        };
    }

    protected List<EffectItem> GetPositiveEffectsAsList(EncounterOption option, ChoiceProjection projection)
    {
        List<EffectItem> effects = new List<EffectItem>();

        // Token generation
        foreach (KeyValuePair<AspectTokenTypes, int> tokenGen in option.TokenGeneration)
        {
            if (tokenGen.Value > 0)
            {
                effects.Add(new EffectItem
                {
                    TokenType = tokenGen.Key,
                    Value = tokenGen.Value,
                    Description = $"Gain {tokenGen.Value} {GetTokenDisplayName(tokenGen.Key)} tokens",
                    IsTokenEffect = true
                });
            }
        }

        // Direct progress
        if (projection.ProgressGained > 0)
        {
            effects.Add(new EffectItem
            {
                Value = projection.ProgressGained,
                Description = $"Gain {projection.ProgressGained} Progress",
                IsProgressEffect = true
            });
        }

        // Focus gain (for Recovery actions)
        if (option.ActionType == UniversalActionTypes.Recovery)
        {
            effects.Add(new EffectItem
            {
                Value = 1,
                Description = $"Restore 1 Focus",
                IsFocusEffect = true
            });
        }

        return effects;
    }

    // Add this class to your component
    protected class EffectItem
    {
        public AspectTokenTypes TokenType { get; set; }
        public int Value { get; set; }
        public string Description { get; set; }
        public bool IsTokenEffect { get; set; }
        public bool IsProgressEffect { get; set; }
        public bool IsFocusEffect { get; set; }
    }

    protected string GetFormattedRiskDescription(EncounterOption option)
    {
        return option.NegativeConsequenceType switch
        {
            NegativeConsequenceTypes.TokenDisruption => "token loss",
            NegativeConsequenceTypes.ThresholdIncrease => "increased success requirements",
            NegativeConsequenceTypes.ProgressLoss => "progress loss",
            NegativeConsequenceTypes.FocusLoss => "Focus point loss",
            _ => "unknown consequence"
        };
    }

    protected string GetTokenEmoji(AspectTokenTypes tokenType)
    {
        return tokenType switch
        {
            AspectTokenTypes.Force => "🔴",
            AspectTokenTypes.Flow => "🔵",
            AspectTokenTypes.Focus => "🟡",
            AspectTokenTypes.Fortitude => "🟢",
            _ => "⚪"
        };
    }

    protected int GetSkillCheckSuccessChance(UserEncounterChoiceOption choice)
    {
        if (!choice.Choice.HasSkillCheck) return 100;
        
        // This is a simplified version - you might need to update this
        // to match your actual skill check calculation logic
        var skillLevel = GetPlayerSkillLevel(choice.Choice.Skill);
        var difficulty = choice.Choice.Difficulty + choice.Choice.LocationModifier;
        
        if (skillLevel >= difficulty + 2) return 100;
        if (skillLevel >= difficulty + 1) return 75;
        if (skillLevel == difficulty) return 50;
        if (skillLevel == difficulty - 1) return 25;
        return 0;
    }

    protected int GetPlayerSkillLevel(SkillTypes skill)
    {
        // This is a simplified placeholder - you'll need to update this
        // to get the actual player's skill level from your game state
        return GameState.PlayerState.GetSkillLevel(skill);
    }

    public string tooltipXpx
    {
        get
        {
            return $"{tooltipX}px";
        }
    }

    public string tooltipYpx
    {
        get
        {
            return $"{tooltipY}px";
        }
    }

    public ChoiceProjection Preview
    {
        get
        {
            return GameManager.GetChoicePreview(hoveredChoice);
        }
    }
}