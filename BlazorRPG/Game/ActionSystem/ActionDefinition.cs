public class ActionDefinition
{
    // Basic properties
    public string Id { get; set; }
    public string Name { get; set; }
    public string SpotId { get; set; }
    public string Description { get; set; }

    // Approaches
    public List<Approach> Approaches { get; set; } = new List<Approach>();

    // Requirements
    public int RelationshipLevel { get; set; }
    public int CoinCost { get; set; }
    public int FoodCost { get; set; }

    // Time windows - updated to use the new TimeWindows class
    public List<TimeWindowTypes> TimeWindows { get; set; } = new List<TimeWindowTypes>();

    // Movement
    public string MoveToLocation { get; set; }
    public string MoveToLocationSpot { get; set; }

    public ActionDefinition(string id, string name, string spotId)
    {
        Id = id;
        Name = name;
        SpotId = spotId;
    }
}