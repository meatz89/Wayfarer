using Microsoft.AspNetCore.Components;

public partial class EncounterChoiceTooltipBase : ComponentBase
{
    [Inject] public GameManager GameManager { get; set; }
    [Inject] public GameState GameState { get; set; }
    [Parameter] public UserEncounterChoiceOption hoveredChoice { get; set; }
    [Parameter] public double tooltipX { get; set; }
    [Parameter] public double tooltipY { get; set; }
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

    public string GetChoiceNarrative(UserEncounterChoiceOption choice)
    {
        EncounterOption choiceCard = choice.Choice;
        Dictionary<EncounterOption, ChoiceNarrative> choiceDescriptions = choice.NarrativeResult?.ChoiceDescriptions;
        ChoiceNarrative choiceNarrative = null;

        if (choiceDescriptions != null && choiceDescriptions.ContainsKey(choiceCard))
            choiceNarrative = choiceDescriptions[choiceCard];

        string description = choiceCard.Id;
        if (choiceNarrative != null)
        {
            description = choiceNarrative.FullDescription;
        }
        return description;
    }
}