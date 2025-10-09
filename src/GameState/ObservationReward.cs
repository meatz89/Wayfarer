/// <summary>
/// Represents a reward that can be earned from observing at a location
/// Each observation requires a specific familiarity level and may require prior observations
/// </summary>
public class ObservationReward
{
    public string VenueId { get; set; }
    public int FamiliarityRequired { get; set; }
    public int? PriorObservationRequired { get; set; }
    public ObservationCardReward ObservationCard { get; set; }
}

/// <summary>
/// Represents an observation card that will be added to an NPC's observation deck
/// </summary>
public class ObservationCardReward
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string TargetNpcId { get; set; }
    public string Effect { get; set; }
    public string Description { get; set; }
}