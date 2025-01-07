public class EncounterChoiceGenerator
{
    public List<EncounterChoice> GenerateChoices(EncounterActionContext context)
    {
        List<EncounterChoice> choices = new();

        // Add basic choices based on patterns
        choices.Add(StandardPatterns.DirectProgressImmediate.CreateChoice(context, choices.Count + 1));
        choices.Add(StandardPatterns.CarefulPositionInvested.CreateChoice(context, choices.Count + 1));
        choices.Add(StandardPatterns.TacticalOpportunityStrategic.CreateChoice(context, choices.Count + 1));

        // TODO: Add logic to generate special choices based on context and player state

        return choices;
    }
}