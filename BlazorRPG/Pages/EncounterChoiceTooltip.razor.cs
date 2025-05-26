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
        if (choice.Choice is AiChoice option)
        {
            string skillInfo = "";
            skillInfo += option.SkillOption.ToString();
            return skillInfo;
        }
        return "No skill check required";
    }

    protected string GetProgressInfo(UserEncounterChoiceOption choice)
    {
        if (choice.Choice is AiChoice option)
        {
            // Get the choice projection to show accurate progress information
            ChoiceProjection projection = Preview;

            string focusCost = option.FocusCost > 0 ? $"Focus Cost: {option.FocusCost}" : "No Focus cost";

            string positiveEffects = GetPositiveEffectsDescription(option, projection);

            return $"{focusCost}\n{positiveEffects}";
        }
        return "";
    }

    private string GetPositiveEffectsDescription(AiChoice option, ChoiceProjection projection)
    {
        List<string> effects = new List<string>();

        // Direct progress
        if (projection.ProgressGained > 0)
        {
            effects.Add($"+{projection.ProgressGained} progress");
        }

        return effects.Count > 0 ? $"Positive: {string.Join(", ", effects)}" : "Positive: Minimal effect";
    }

    protected List<EffectItem> GetPositiveEffectsAsList(AiChoice option, ChoiceProjection projection)
    {
        List<EffectItem> effects = new List<EffectItem>();

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

        return effects;
    }

    protected class EffectItem
    {
        public int Value { get; set; }
        public string Description { get; set; }
        public bool IsTokenEffect { get; set; }
        public bool IsProgressEffect { get; set; }
        public bool IsFocusEffect { get; set; }
    }

    protected int GetPlayerSkillLevel(SkillTypes skill)
    {
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