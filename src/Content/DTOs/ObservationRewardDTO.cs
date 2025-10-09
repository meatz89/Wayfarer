using System.Collections.Generic;

/// <summary>
/// DTO for observation reward data from JSON packages
/// Represents rewards earned from observing at specific familiarity levels
/// </summary>
public class ObservationRewardDTO
{
    public string VenueId { get; set; }
    public int FamiliarityRequired { get; set; }
    public int? PriorObservationRequired { get; set; }
    public ObservationCardDTO ObservationCard { get; set; }
}

/// <summary>
/// DTO for observation cards that go into NPC observation decks
/// </summary>
public class ObservationCardDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string TargetNpcId { get; set; }
    public string Effect { get; set; }
    public string Description { get; set; }
}