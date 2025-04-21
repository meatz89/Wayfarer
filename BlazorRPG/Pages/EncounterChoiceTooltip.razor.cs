using Microsoft.AspNetCore.Components;

public partial class EncounterChoiceTooltipBase : ComponentBase
{
    [Inject] public GameManager GameManager { get; set; }
    [Inject] public GameState GameState { get; set; }
    [Parameter] public UserEncounterChoiceOption hoveredChoice { get; set; }
    [Parameter] public double tooltipX { get; set; }
    [Parameter] public double tooltipY { get; set; }

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

        foreach (KeyValuePair<ApproachTags, int> tagChange in Preview.ApproachTagChanges)
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

        foreach (KeyValuePair<FocusTags, int> tagChange in Preview.FocusTagChanges)
        {
            formattedChanges[tagChange.Key.ToString()] = tagChange.Value;
        }

        return formattedChanges;
    }

    public string GetChoiceNarrative(UserEncounterChoiceOption choice)
    {
        CardDefinition choiceCard = choice.Choice;
        Dictionary<CardDefinition, ChoiceNarrative> choiceDescriptions = choice.NarrativeResult?.ChoiceDescriptions;
        ChoiceNarrative choiceNarrative = null;

        if (choiceDescriptions != null && choiceDescriptions.ContainsKey(choiceCard))
            choiceNarrative = choiceDescriptions[choiceCard];

        string description = choiceCard.Name;
        if (choiceNarrative != null)
        {
            description = choiceNarrative.FullDescription;
        }
        return description;
    }
}