public class NarrativeGenerator
{
    public List<NarrativeChoice> GenerateChoices(ActionContext context, NarrativeState currentState)
    {
        var generator = new NarrativeChoiceGenerator();
        var choices = generator.GenerateChoices(context, currentState);

        return choices;
    }

}