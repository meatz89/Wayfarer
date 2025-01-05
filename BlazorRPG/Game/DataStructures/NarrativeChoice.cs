public class NarrativeChoice
{
    // Core properties from your original implementation
    public int Index { get; set; }
    public ChoiceTypes ChoiceType { get; set; }
    public string Description { get; set; }
    public string Narrative { get; set; }
    public Requirement Requirement { get; set; }
    public Outcome Cost { get; set; }
    public Outcome Reward { get; set; }
    public NarrativeStateValues NarrativeStateChanges { get; set; }
    public NarrativeStateValues ValueThresholds { get; set; }

    // New properties needed for value state modifications
    public int CompletionPoints { get; set; }
    public List<string> UnlockedOptions { get; set; } = new();

    // List to track multiple requirements if needed
    public List<Requirement> AdditionalRequirements { get; set; } = new();
}