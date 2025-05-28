public class PayloadProcessor
{
    private PayloadRegistry registry;
    private EncounterState state;

    public PayloadProcessor(PayloadRegistry registry, EncounterState state)
    {
        this.registry = registry;
        this.state = state;
    }

    public void ApplyPayload(string payloadID, EncounterState state)
    {
        IMechanicalEffect effect = registry.GetEffect(payloadID);

        // Apply the mechanical effect
        effect.Apply(state);

        // Log the application for UI feedback
        state.AddEventLog(new EncounterEvent
        {
            Type = EventTypes.PayloadApplied,
            Description = effect.GetDescriptionForPlayer(),
            Timestamp = state.DurationCounter
        });

        // Check if this payload completes any goals
        state.CheckGoalCompletion();
    }
}
