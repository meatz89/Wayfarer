public class EncounterStageState
{
    public int Momentum;

    public EncounterStageState(int momentum)
    {
        Momentum = momentum;
    }

    public EffectTypes LastChoiceEffectType { get; internal set; }
    public ApproachTypes LastChoiceApproach { get; internal set; }
    public FocusTypes LastChoiceFocusType { get; internal set; }
    public Choice LastChoice { get; internal set; }
}