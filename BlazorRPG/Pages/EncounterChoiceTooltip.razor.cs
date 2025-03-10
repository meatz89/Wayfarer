using BlazorRPG.Game.EncounterManager;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

public partial class EncounterChoiceTooltipBase : ComponentBase
{
    [Inject] public GameManager GameManager { get; set; }
    [Inject] public GameState GameState { get; set; }
    [Parameter] public Encounter encounter { get; set; }
    [Parameter] public UserEncounterChoiceOption hoveredChoice { get; set; }
    [Parameter] public double mouseX { get; set; }
    [Parameter] public double mouseY { get; set; }

    public ChoiceProjection Preview => GameManager.GetChoicePreview(hoveredChoice);

    public List<ChoiceProjection.ValueComponent> GetMomentumBreakdown()
    {
        // Now we can directly use the detailed components from the projection
        return Preview?.MomentumComponents ?? new List<ChoiceProjection.ValueComponent>();
    }

    public List<ChoiceProjection.ValueComponent> GetPressureBreakdown()
    {
        // Now we can directly use the detailed components from the projection
        return Preview?.PressureComponents ?? new List<ChoiceProjection.ValueComponent>();
    }

    public Dictionary<string, int> GetApproachTagChanges()
    {
        Dictionary<string, int> formattedChanges = new Dictionary<string, int>();

        if (Preview?.ApproachTagChanges == null)
            return formattedChanges;

        foreach (var tagChange in Preview.ApproachTagChanges)
        {
            formattedChanges[tagChange.Key.ToString()] = tagChange.Value;
        }

        return formattedChanges;
    }

    public Dictionary<string, int> GetFocusTagChanges()
    {
        Dictionary<string, int> formattedChanges = new Dictionary<string, int>();

        if (Preview?.FocusTagChanges == null)
            return formattedChanges;

        foreach (var tagChange in Preview.FocusTagChanges)
        {
            formattedChanges[tagChange.Key.ToString()] = tagChange.Value;
        }

        return formattedChanges;
    }

    public bool HasActivationEffect(string tagName)
    {
        IEncounterTag tag = encounter.State.ActiveTags
            .Concat(encounter.State.Location.AvailableTags)
            .FirstOrDefault(t => t.Name == tagName);

        return tag is StrategicTag strategicTag &&
               (strategicTag.EffectType == StrategicEffectTypes.AddMomentumOnActivation ||
                strategicTag.EffectType == StrategicEffectTypes.ReducePressureOnActivation);
    }

    public string GetActivationEffectDescription(string tagName)
    {
        IEncounterTag tag = encounter.State.ActiveTags
            .Concat(encounter.State.Location.AvailableTags)
            .FirstOrDefault(t => t.Name == tagName);

        if (tag is StrategicTag strategicTag)
        {
            if (strategicTag.EffectType == StrategicEffectTypes.AddMomentumOnActivation)
                return $"+{strategicTag.EffectValue} momentum immediately";
            else if (strategicTag.EffectType == StrategicEffectTypes.ReducePressureOnActivation)
                return $"-{strategicTag.EffectValue} pressure immediately";
        }

        return string.Empty;
    }

    public string GetTagEffectDescription(string tagName)
    {
        // Map tag names to their effects (same as before)
        if (tagName.Contains("Respect"))
            return "+1 momentum to Resource choices";
        else if (tagName.Contains("Eye"))
            return "-1 pressure from Resource choices";
        else if (tagName.Contains("Wisdom"))
            return "+1 momentum to Information choices";
        else if (tagName.Contains("Network"))
            return "-1 pressure at end of each turn";
        else if (tagName.Contains("Marketplace"))
            return "Blocks Concealment approaches";
        else if (tagName.Contains("Suspicion"))
            return "Blocks Charm approaches";
        else if (tagName.Contains("Guard"))
            return "Blocks Stealth approaches";

        return "Affects encounter mechanics";
    }
}