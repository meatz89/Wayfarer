public class ChoiceSet
{
    public string Name { get; }
    public List<EncounterChoice> Choices { get; }

    public ChoiceSet(string name, List<EncounterChoice> choices)
    {
        Name = name;
        Choices = choices;
    }
}