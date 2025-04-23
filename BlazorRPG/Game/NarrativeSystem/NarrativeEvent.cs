public class NarrativeEvent
{
    public int TurnNumber { get; }
    public string Summary { get; }
    public Dictionary<CardDefinition, ChoiceNarrative> ChoiceDescriptions { get; set; } = new();
    public CardDefinition ChosenOption { get; set; }
    public ChoiceNarrative ChoiceNarrative { get; set; }
    public string Outcome { get; set; }

    public NarrativeEvent(
        int turnNumber,
        string sceneDescription)
    {
        TurnNumber = turnNumber;
        Summary = sceneDescription;
    }

    public void SetAvailableChoiceDescriptions(Dictionary<CardDefinition, ChoiceNarrative> choiceDescriptions)
    {
        ChoiceDescriptions = choiceDescriptions;
    }

    public void SetChosenOption(CardDefinition chosenOption)
    {
        ChosenOption = chosenOption;
    }

    public void SetChoiceNarrative(ChoiceNarrative choiceNarrative)
    {
        this.ChoiceNarrative = choiceNarrative;
    }

    public void SetOutcome(string? outcome)
    {
        Outcome = outcome;
    }
}
