using Microsoft.AspNetCore.Components;

public class EncounterChoiceTooltipBase : ComponentBase
{
    [Inject] public GameWorldManager GameWorldManager { get; set; }
    [Inject] public GameWorld GameWorld { get; set; }
    [Parameter] public EncounterChoice hoveredChoice { get; set; }
    [Parameter] public double tooltipX { get; set; }
    [Parameter] public double tooltipY { get; set; }

    public string GetSkillCheckInfo(EncounterChoice choice)
    {
        string skillInfo = "";
        skillInfo += choice.SkillOption.ToString();
        return skillInfo;
    }

    public string GetProgressInfo(EncounterChoice choice)
    {
        // Get the choice projection to show accurate progress information
        ChoiceProjection projection = Preview;

        string focusCost = choice.FocusCost > 0 ? $"Focus Cost: {choice.FocusCost}" : "No Focus cost";

        string positiveEffects = GetPositiveEffectsDescription(choice, projection);

        return $"{focusCost}\n{positiveEffects}";
    }

    private string GetPositiveEffectsDescription(EncounterChoice option, ChoiceProjection projection)
    {
        List<string> effects = new List<string>();

        // Direct progress
        if (projection.ProgressGained > 0)
        {
            effects.Add($"+{projection.ProgressGained} progress");
        }

        return effects.Count > 0 ? $"Positive: {string.Join(", ", effects)}" : "Positive: Minimal effect";
    }

    public List<EffectItem> GetPositiveEffectsAsList(EncounterChoice option, ChoiceProjection projection)
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
        return GameWorld.GetPlayer().GetSkillLevel(skill);
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
            return GameWorldManager.GetChoicePreview(hoveredChoice);
        }
    }
}