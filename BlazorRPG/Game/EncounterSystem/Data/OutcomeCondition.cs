public record OutcomeCondition
{
    public EncounterResults EncounterResults { get; set; }
    public ValueTypes ValueType { get; init; }
    public int MinValue { get; init; }
    public int MaxValue { get; init; } = int.MaxValue;
    public List<Outcome> Outcomes { get; init; } = new();
}