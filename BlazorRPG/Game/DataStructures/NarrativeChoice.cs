public class NarrativeChoice
{
    public int Index { get; set; }
    public string Description { get; set; }
    public string Narrative { get; set; }
    public ChoiceTypes ChoiceType { get; set; }

    public Requirement Requirement { get; set; }
    public Outcome Cost { get; set; } 
    public Outcome Reward { get; set; }

    public NarrativeState NarrativeStateChanges { get; set; }
}
