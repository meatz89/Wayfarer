public class NarrativeEvent
{
    public int Stage { get; private set; }
    public string Summary { get; private set; }
    public AiChoice ChosenOption { get; private set; }
    public string Outcome { get; private set; }
    public List<AiChoice> AvailableChoices { get; private set; } = new List<AiChoice>();

    public NarrativeEvent(int stage, string summary)
    {
        Stage = stage;
        Summary = summary;
    }

    public void SetChosenOption(AiChoice choice)
    {
        ChosenOption = choice;
    }

    public void SetOutcome(string outcome)
    {
        Outcome = outcome;
    }

    public void SetAvailableChoices(List<AiChoice> choices) 
    {
        AvailableChoices = choices;
    }
}