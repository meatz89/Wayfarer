public class ChoiceSystem
{
    public ChoiceSystem(NarrativeGenerator narrativeGenerator)
    {
        NarrativeGenerator = narrativeGenerator;
    }

    public NarrativeGenerator NarrativeGenerator { get; }

    public List<NarrativeChoice> GenerateExampleChoices()
    {
        NarrativeState state = NarrativeState.InitialState;

        var context = new ActionContext
        {
            ActionType = ActionTypes.Mingle,
            LocationType = LocationTypes.Social,
            TimeSlot = TimeSlots.Night,
            NarrativeState = state
        };

        var generator = new NarrativeGenerator();
        List<NarrativeChoice> choices = generator.GenerateChoices(context, state);

        return choices;

    }
}
