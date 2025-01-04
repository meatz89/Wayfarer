public class NarrativeGenerator
{
    public List<NarrativeChoice> GenerateChoices(ActionContext context, NarrativeState currentState)
    {
        var choices = new List<NarrativeChoice>();

        // Generate base choices
        var directChoice = DirectChoiceFactory.Create(context);
        var carefulChoice = CarefulChoiceFactory.Create(context);
        var tacticalChoice = TacticalChoiceFactory.Create(context);

        // Only add choices if they meet value thresholds
        if (MeetsValueThresholds(directChoice.ValueThresholds, currentState))
            choices.Add(directChoice);

        if (MeetsValueThresholds(carefulChoice.ValueThresholds, currentState))
            choices.Add(carefulChoice);

        if (MeetsValueThresholds(tacticalChoice.ValueThresholds, currentState))
            choices.Add(tacticalChoice);

        // Generate recovery choice if needed
        if (HasLowValues(currentState))
        {
            //choices.Add(CreateRecoveryChoice(context, currentState));
        }

        return choices;
    }


    private bool MeetsValueThresholds(NarrativeState thresholds, NarrativeState currentState)
    {
        return currentState.Momentum >= thresholds.Momentum &&
               currentState.Advantage >= thresholds.Advantage &&
               currentState.Understanding >= thresholds.Understanding &&
               currentState.Connection >= thresholds.Connection &&
               currentState.Tension >= thresholds.Tension;
    }

    private bool HasLowValues(NarrativeState state)
    {
        const int lowThreshold = 2;
        return state.Momentum <= lowThreshold ||
               state.Advantage <= lowThreshold ||
               state.Understanding <= lowThreshold ||
               state.Connection <= lowThreshold;
    }
}