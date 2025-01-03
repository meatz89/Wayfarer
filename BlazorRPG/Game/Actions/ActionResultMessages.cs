

public class ActionResultMessages
{
    public List<CoinsOutcome> Coins { get; init; } = new();
    public List<HealthOutcome> Health { get; init; } = new();
    public List<EnergyOutcome> Energy { get; init; } = new();
    public List<SkillLevelOutcome> SkillLevel { get; init; } = new();
    public List<ResourceOutcome> Resources { get; init; } = new();
}