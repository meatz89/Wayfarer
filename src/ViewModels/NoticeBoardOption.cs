public class NoticeBoardOption
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int TokenCost { get; set; }
    public NoticeBoardOptionType OptionType { get; set; }
    public bool RequiresDirection { get; set; }
    public bool RequiresTokenType { get; set; }
}

public enum NoticeBoardOptionType
{
    AnythingHeading,
    LookingForWork,
    UrgentDeliveries
}