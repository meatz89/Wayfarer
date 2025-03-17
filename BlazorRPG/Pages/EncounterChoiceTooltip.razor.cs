
using Microsoft.AspNetCore.Components;
using System.Reflection;

public partial class EncounterChoiceTooltipBase : ComponentBase
{
    [Inject] public GameManager GameManager { get; set; }
    [Inject] public GameState GameState { get; set; }
    [Parameter] public UserEncounterChoiceOption hoveredChoice { get; set; }
    [Parameter] public double mouseX { get; set; }
    [Parameter] public double mouseY { get; set; }
    public EncounterManager Encounter => GameManager.EncounterSystem.Encounter;
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

        if (Preview?.EncounterStateTagChanges == null)
            return formattedChanges;

        foreach (KeyValuePair<ApproachTags, int> tagChange in Preview.EncounterStateTagChanges)
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

    public string GetChoiceDescription(UserEncounterChoiceOption choice)
    {
        IChoice choice1 = choice.Choice;

        NarrativeResult narrativeResult = GameManager.EncounterResult.NarrativeResult;
        Dictionary<IChoice, ChoiceNarrative> choiceDescriptions = narrativeResult.ChoiceDescriptions;

        ChoiceNarrative choiceNarrative = null;

        if (choiceDescriptions != null && choiceDescriptions.ContainsKey(choice1))
            choiceNarrative = choiceDescriptions[choice1];

        string description = choice.Description;
        if (choiceNarrative != null)
        {
            description = choiceNarrative.FullDescription;
        }
        return description;
    }

}