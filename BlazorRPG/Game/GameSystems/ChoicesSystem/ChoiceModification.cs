


// This represents how a choice modifies the base pattern
public class ChoiceModification
{
    public float CompletionPointMultiplier { get; set; } = 1.0f;
    public int EnergyCostIncrease { get; set; }
    public List<ValueChange> ValueChanges { get; set; } = new();
    public List<string> UnlockedOptions { get; set; } = new();
    public bool SkipRequirements { get; set; }
}