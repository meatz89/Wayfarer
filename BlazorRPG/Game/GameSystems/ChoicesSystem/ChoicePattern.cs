public class ChoicePattern
{
    public PositionTypes Position { get; }
    public IntentTypes Intent { get; }
    public ScopeTypes Scope { get; }

    // Base stats for each pattern combination
    public int BaseCompletionPoints { get; }
    public int BaseEnergyCost { get; }
    public List<ValueChange> StandardValueChanges { get; }
}
