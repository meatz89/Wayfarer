using BlazorRPG.Game.EncounterManager;
using Microsoft.AspNetCore.Components;

public partial class EncounterChoiceTooltipBase : ComponentBase
{
    [Inject] public GameManager GameManager { get; set; }
    [Inject] public GameState GameState { get; set; }
    [Parameter] public Encounter Encounter { get; set; }
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
        IEncounterTag tag = Encounter.State.ActiveTags
            .Concat(Encounter.State.Location.AvailableTags)
            .FirstOrDefault(t => t.Name == tagName);

        return tag is StrategicTag strategicTag &&
               (strategicTag.EffectType == StrategicEffectTypes.AddMomentumOnActivation ||
                strategicTag.EffectType == StrategicEffectTypes.ReducePressureOnActivation);
    }

    public string GetActivationEffectDescription(string tagName)
    {
        IEncounterTag tag = Encounter.State.ActiveTags
            .Concat(Encounter.State.Location.AvailableTags)
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


    // Check if any tags are disabled by pressure
    public bool AreTagsDisabledByPressure()
    {
        return Preview?.DisabledTagNames?.Count > 0;
    }

    // Check if a specific tag would be disabled
    public bool IsTagDisabledInProjection(string tagName)
    {
        return Preview?.DisabledTagNames?.Contains(tagName) == true;
    }

    public string GetTagEffectDescription(string tagName)
    {
        // Find the actual tag object by name
        IEncounterTag tag = Encounter.State.ActiveTags
            .Concat(Encounter.State.Location.AvailableTags)
            .FirstOrDefault(t => t.Name == tagName);

        if (tag == null)
            return "Unknown tag effect";

        // Use the tag's own description method
        if (tag is StrategicTag strategicTag)
        {
            return strategicTag.GetEffectDescription();
        }
        else if (tag is NarrativeTag narrativeTag && narrativeTag.BlockedApproach.HasValue)
        {
            return $"Blocks {narrativeTag.BlockedApproach.Value} approaches";
        }
        else if (tag is NarrativeTag narrativeTag2 && narrativeTag2.Name == "Fight Started")
        {
            // Special handling for the special "Fight Started" tag
            return "Blocks all non-Force approaches";
        }

        return "Affects encounter mechanics";
    }
}