

public class ActionResultMessages
{
    public List<CoinsOutcome> Coins { get; init; } = new();
    public List<FoodOutcome> Food { get; init; } = new();
    public List<HealthOutcome> Health { get; init; } = new();
    public List<PhysicalEnergyOutcome> PhysicalEnergy { get; init; } = new();
    public List<FocusEnergyOutcome> FocusEnergy { get; init; } = new();
    public List<SocialEnergyOutcome> SocialEnergy { get; init; } = new();
    public List<SkillLevelOutcome> SkillLevel { get; init; } = new();
    public List<ResourceOutcome> Resources { get; init; } = new();
    public EndDayOutcome EndDay { get; set; }
}