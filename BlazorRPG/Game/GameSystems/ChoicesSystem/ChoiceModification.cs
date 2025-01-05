
public class ChoiceModification
{
    public int EnergyCostIncrease { get; set; }
    public List<ValueChange> ValueChanges { get; set; } = new();
    public List<string> UnlockedOptions { get; set; } = new();
    public bool SkipRequirements { get; set; }
}