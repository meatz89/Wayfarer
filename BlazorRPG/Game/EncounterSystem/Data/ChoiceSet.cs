public class ChoiceSet
{
    public string Name { get; }
    public List<EncounterChoice> Choices { get; }

    public ChoiceSet(string name, List<EncounterChoice> choices)
    {
        Name = name;
        Choices = choices;
    }

    internal void ApplyNarratives(List<string> choicesTexts)
    {
        Choices[0].Description = choicesTexts[0];
        Choices[1].Description = choicesTexts[1];
        Choices[2].Description = choicesTexts[2];
    }
}