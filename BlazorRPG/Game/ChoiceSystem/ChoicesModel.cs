public class ChoicesModel
{
    public List<Choice> Choices { get; internal set; }
    public EncounterState EncounterState { get; internal set; }

    internal void ApplyNarratives(List<ChoicesNarrative> choicesTexts)
    {
    }
}