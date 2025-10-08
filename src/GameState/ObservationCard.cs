public class ObservationCard
{
    public string Id { get; internal set; }
    public string Title { get; internal set; }
    public string LocationDiscovered { get; internal set; }
    public string ItemName { get; internal set; }
    public string ObservationId { get; internal set; }
    public string TimeDiscovered { get; internal set; }
    public ConnectionType TokenType { get; internal set; }
    public SuccessEffectType SuccessType { get; internal set; }
    public string DialogueText { get; internal set; }
    public int InitiativeCost { get; internal set; }
    public PersistenceType Persistence { get; internal set; }
}