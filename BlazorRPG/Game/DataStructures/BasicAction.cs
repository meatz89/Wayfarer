public class BasicAction
{
    public string Name { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public List<TimeWindows> TimeSlots = new();
    public List<Requirement> Requirements { get; set; } = new();
    public List<Outcome> Rewards { get; set; } = new();
    public List<Outcome> Costs { get; set; }
    public int TimeInvestment { get; set; }

    public bool CanExecute(PlayerState player)
    {
        return Requirements.All(r => r.IsSatisfied(player));
    }

    public void Execute(PlayerState player)
    {
        foreach (Outcome outcome in Rewards)
        {
            outcome.Apply(player);
        }
    }
}