public class ChoiceTemplate
{
    public ChoiceArchetypes ChoiceArchetype { get; set; }
    public ChoiceApproaches ChoiceApproach { get; set; }

    public List<ValueChange> BaseValueChanges { get; set; }
    public EnergyTypes EnergyType { get; set; }
    public int BaseEnergyCost { get; set; }
    public SkillTypes RelevantSkill { get; set; }

    public List<Requirement> Requirements { get; set; }
    public List<Outcome> Costs { get; set; }
    public List<Outcome> Rewards { get; set; }
}