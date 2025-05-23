public class NarrativeEvent
{
    public int TurnNumber { get; }
    public string Summary { get; }
    public Dictionary<string, ChoiceNarrative> ChoiceDescriptions { get; set; } = new();
    public EncounterOption ChosenOption { get; set; }
    public ChoiceNarrative ChoiceNarrative { get; set; }
    public string Outcome { get; set; }

    public NarrativeEvent(
        int turnNumber,
        string sceneDescription)
    {
        TurnNumber = turnNumber;
        Summary = sceneDescription;
    }

    public void SetAvailableChoiceDescriptions(Dictionary<string, ChoiceNarrative> choiceDescriptions)
    {
        ChoiceDescriptions = choiceDescriptions;
    }

    public void SetChosenOption(EncounterOption chosenOption)
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
