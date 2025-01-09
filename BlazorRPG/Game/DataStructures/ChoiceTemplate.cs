public class ChoiceTemplate
{
    public ChoiceArchetypes Archetype { get; set; }
    public ChoiceApproaches Approach { get; set; }

    public List<ValueChange> BaseValueChanges { get; set; }
    public EnergyTypes EnergyType { get; set; }
    public int BaseCost { get; set; }
    public SkillTypes RelevantSkill { get; set; }

    public List<Requirement> Requirements { get; set; }
    public List<Outcome> Costs { get; set; }
    public List<Outcome> Rewards { get; set; }
}