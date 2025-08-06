using Microsoft.AspNetCore.Components;

public class ConversationChoiceTooltipBase : ComponentBase
{
    [Inject] public GameFacade GameFacade { get; set; }
    [Inject] public GameWorld GameWorld { get; set; }
    [Parameter] public ConversationChoice hoveredChoice { get; set; }
    [Parameter] public double tooltipX { get; set; }
    [Parameter] public double tooltipY { get; set; }

    public string GetSkillCheckInfo(ConversationChoice choice)
    {
        string skillInfo = "";
        skillInfo += choice.SkillOption.ToString();
        return skillInfo;
    }

    public string GetProgressInfo(ConversationChoice choice)
    {
        // Get the choice projection to show accurate progress information
        ChoiceProjection projection = Preview;

        string focusCost = choice.FocusCost > 0 ? $"Focus Cost: {choice.FocusCost}" : "No Focus cost";

        string positiveEffects = GetPositiveEffectsDescription(choice, projection);

        return $"{focusCost}\n{positiveEffects}";
    }

    private string GetPositiveEffectsDescription(ConversationChoice option, ChoiceProjection projection)
    {
        List<string> effects = new List<string>();

        // Direct progress
        if (projection.ProgressGained > 0)
        {
            effects.Add($"+{projection.ProgressGained} progress");
        }

        return effects.Count > 0 ? $"Positive: {string.Join(", ", effects)}" : "Positive: Minimal effect";
    }

    public List<EffectItem> GetPositiveEffectsAsList(ConversationChoice option, ChoiceProjection projection)
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

    public int GetPlayerSkillLevel(SkillTypes skill)
    {
        // Skills removed - using token system
        return 1;
    }

    public string tooltipXpx => $"{tooltipX}px";

    public string tooltipYpx => $"{tooltipY}px";

    public ChoiceProjection Preview =>
            // TODO: Implement choice preview with GameFacade
            null; // GameFacade.GetChoicePreview(hoveredChoice);
}