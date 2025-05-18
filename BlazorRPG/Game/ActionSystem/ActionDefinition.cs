public class ActionDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string LocationSpotId { get; set; }
    public int ActionPointCost { get; set; }
    public int SilverCost { get; set; }
    public CardTypes? RefreshCardType { get; set; }
    public int EnergyCost { get; set; }
    public int ConcentrationCost { get; set; }

    // Approaches
    public List<TimeWindowTypes> TimeWindows { get; set; } = new List<TimeWindowTypes>();

    // Movement
    public string MoveToLocation { get; set; }
    public string MoveToLocationSpot { get; set; }

    public ActionDefinition(string id, string name, string spotId)
    {
        Id = id;
        Name = name;
        LocationSpotId = spotId;
    }
}
