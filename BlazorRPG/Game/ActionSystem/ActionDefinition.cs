public class ActionDefinition
{
    // Basic properties
    public string Id { get; set; }
    public string Name { get; set; }
    public string SpotId { get; set; }
    public string Description { get; set; }
    public EncounterApproaches EncounterApproach { get; set; } = EncounterApproaches.Neutral;

    // Resource costs
    public int CoinCost { get; set; }
    public int FoodCost { get; set; }

    // Requirements
    public int RelationshipLevel { get; set; }
    public List<TimeWindows> TimeWindows { get; set; } = new List<TimeWindows>();

    // Grants
    public int SpotXP { get; set; }

    // Recovery types
    public RecoveryLevels HungerRecovery { get; set; }
    public RecoveryLevels EnergyRecovery { get; set; }
    public RecoveryLevels ExhaustionRecovery { get; set; }
    public RecoveryLevels MentalStrainRecovery { get; set; }
    public RecoveryLevels IsolationRecovery { get; set; }

    // Characteristics
    public ExertionLevels Exertion { get; set; } = ExertionLevels.Low;
    public MentalLoadLevels MentalLoad { get; set; } = MentalLoadLevels.Low;
    public SocialImpactTypes SocialImpact { get; set; } = SocialImpactTypes.Solitary;

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