public class ChoiceSet
{
    public string Name { get; }
    public List<EncounterChoice> Choices { get; }

    public ChoiceSet(string name, List<EncounterChoice> choices)
    {
        Name = name;
        Choices = choices;
    }

    public void ApplyNarratives(List<ChoicesNarrative> choicesTexts)
    {
        foreach (ChoicesNarrative narrative in choicesTexts)
        {
            int index = narrative.choiceNumber;
            if (Choices.Count >= index)
            {
                EncounterChoice choice = Choices[index - 1];
                choice.Designation = narrative.designation;
                choice.Narrative = narrative.narrative;
            }
        }
    }
}