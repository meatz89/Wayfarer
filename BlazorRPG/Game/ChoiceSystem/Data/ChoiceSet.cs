public class ChoiceSet
{
    public string Name { get; }
    public List<Choice> Choices { get; }

    public ChoiceSet(string name, List<Choice> choices)
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
                Choice choice = Choices[index - 1];
                string oldDescription = choice.Description;

                choice.Description = narrative.Description;
                choice.Narrative = narrative.narrative;
            }
        }
    }
}