public class EncounterChoice
{
    // Identity & Description
    public int Index { get; set; }
    public string Description { get; set; }
    public ChoiceArchetypes Archetype { get; set; }
    public ChoiceApproaches Approach { get; set; }
    public SkillTypes ChoiceRelevantSkill { get; set; }

    // Base Values (unmodified inputs from pattern)
    public List<ValueChange> BaseValueChanges { get; set; } = new();
    public List<Requirement> Requirements { get; set; } = new();
    public List<Outcome> BaseCosts { get; set; } = new();
    public List<Outcome> BaseRewards { get; set; } = new();

    // Modification results (calculated by calculator)
    public ChoiceConsequences Consequences { get; set; }
    public List<ChoiceModifierEntry> ChoiceModifierEntries { get; set; } = new();
}