public class ActionDefinition
{
    // Basic properties
    public string Id { get; set; }
    public string Name { get; set; }
    public string SpotId { get; set; }
    public string Description { get; set; }
    public EncounterCategories Category { get; set; } = EncounterCategories.Neutral;

    // Resource costs
    public int CoinCost { get; set; }
    public int FoodCost { get; set; }

    // Requirements
    public int RelationshipLevel { get; set; }
    public List<TimeWindows> TimeWindows { get; set; } = new List<TimeWindows>();

    // Grants
    public int SpotXP { get; set; }

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