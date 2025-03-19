public class NarrativeEvent
{
    public int TurnNumber { get; }
    public string SceneDescription { get; }
    public Dictionary<IChoice, ChoiceNarrative> ChoiceDescriptions { get; set; } = new();
    public IChoice ChosenOption { get; set; }
    public ChoiceNarrative ChoiceNarrative { get; set; }
    public string Outcome { get; set; }

    public NarrativeEvent(
        int turnNumber,
        string sceneDescription)
    {
        TurnNumber = turnNumber;
        SceneDescription = sceneDescription;
    }

    public void SetAvailableChoiceDescriptions(Dictionary<IChoice, ChoiceNarrative> choiceDescriptions)
    {
        ChoiceDescriptions = choiceDescriptions;
    }

    internal void SetChosenOption(IChoice chosenOption)
    {
        ChosenOption = chosenOption;
    }

    internal void SetChoiceNarrative(ChoiceNarrative choiceNarrative)
    {
        this.ChoiceNarrative = choiceNarrative;
    }

    internal void SetOutcome(string? outcome)
    {
        Outcome = outcome;
    }
}
