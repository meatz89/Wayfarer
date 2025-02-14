public class EncounterStageState
{
    public int Momentum;

    public EncounterStageState(int momentum)
    {
        Momentum = momentum;
    }

    public ChoiceArchetypes LastChoiceType { get; internal set; }
    public ChoiceApproaches LastChoiceApproach { get; internal set; }
    public EncounterChoice LastChoice { get; internal set; }
}