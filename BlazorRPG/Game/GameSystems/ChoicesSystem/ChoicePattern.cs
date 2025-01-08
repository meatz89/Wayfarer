public class ChoicePattern
{
    public List<ValueChange> BaseValueChanges { get; set; }
    public EnergyTypes EnergyType { get; set; }
    public int BaseCost { get; set; }

    public List<Requirement> Requirements { get; set; }
    public List<Outcome> Costs { get; set; }
    public List<Outcome> Rewards { get; set; }
}