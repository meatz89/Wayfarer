public class NarrativeEvent
{
    public int Stage { get; }
    public string Summary { get; }
    public AiChoice ChosenOption { get; private set; }
    public string Outcome { get; private set; }
    public List<AiChoice> AvailableChoices { get; private set; }

    public NarrativeEvent(int durationCounter, string description)
    {
        Stage = durationCounter;
        Summary = description;
        AvailableChoices = new List<AiChoice>();
    }

    public void SetChosenOption(AiChoice chosenOption)
    {
        ChosenOption = chosenOption;
    }

    public void SetOutcome(string outcome)
    {
        Outcome = outcome;
    }

    public void SetAvailableChoices(List<AiChoice> availableChoices)
    {
        AvailableChoices = availableChoices;
    }
}