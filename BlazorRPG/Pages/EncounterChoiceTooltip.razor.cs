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
            NegativeConsequenceTypes.FutureCostIncrease => "Next choice costs +1 Focus",
            NegativeConsequenceTypes.TokenDisruption => "Lose 1 random token",
            NegativeConsequenceTypes.ThresholdIncrease => "Success requirements increase",
            NegativeConsequenceTypes.ProgressLoss => "Lose 1 progress",
            NegativeConsequenceTypes.FocusLoss => "Lose 1 Focus from pool",
            _ => "Unknown risk"
        };
    }

    private string GetTokenDisplayName(AspectTokenTypes tokenType)
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

    public string tooltipXpx => $"{tooltipX}px";
    public string tooltipYpx => $"{tooltipY}px";

    public ChoiceProjection Preview => GameManager.GetChoicePreview(hoveredChoice);
}