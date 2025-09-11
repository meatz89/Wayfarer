using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

/// <summary>
/// Immutable state container for location data.
/// All modifications must go through operations/commands.
/// </summary>
public sealed class LocationState
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public ImmutableList<LocationConnection> Connections { get; }
    public ImmutableHashSet<string> LocationSpotIds { get; }

    // Environmental properties by time window
    public ImmutableList<string> MorningProperties { get; }
    public ImmutableList<string> AfternoonProperties { get; }
    public ImmutableList<string> EveningProperties { get; }
    public ImmutableList<string> NightProperties { get; }

    // Tag Resonance System
    public ImmutableList<string> DomainTags { get; }

    public Population? Population { get; }
    public Atmosphere? Atmosphere { get; }
    public Physical? Physical { get; }
    public Illumination? Illumination { get; }

    public int TravelTimeSegments { get; }
    public string TravelDescription { get; }
    public int Difficulty { get; }
    public int Depth { get; }
    public LocationTypes LocationType { get; }
    public ImmutableList<ServiceTypes> AvailableServices { get; }
    public bool HasBeenVisited { get; }
    public int VisitCount { get; }
    public bool PlayerKnowledge { get; }

    // Access Requirements
    public AccessRequirement AccessRequirement { get; }

    // Constructor enforces immutability
    public LocationState(
        string id,
        string name,
        string description,
        IEnumerable<LocationConnection> connections,
        IEnumerable<string> locationSpotIds,
        IEnumerable<string> morningProperties,
        IEnumerable<string> afternoonProperties,
        IEnumerable<string> eveningProperties,
        IEnumerable<string> nightProperties,
        IEnumerable<string> domainTags,
        Population? population,
        Atmosphere? atmosphere,
        Physical? physical,
        Illumination? illumination,
        int travelTimeSegments,
        string travelDescription,
        int difficulty,
        int depth,
        LocationTypes locationType,
        IEnumerable<ServiceTypes> availableServices,
        bool hasBeenVisited,
        int visitCount,
        bool playerKnowledge,
        AccessRequirement accessRequirement)
    {
        Id = id;
        Name = name;
        Description = description;
        Connections = connections?.ToImmutableList() ?? ImmutableList<LocationConnection>.Empty;
        LocationSpotIds = locationSpotIds?.ToImmutableHashSet() ?? ImmutableHashSet<string>.Empty;
        MorningProperties = morningProperties?.ToImmutableList() ?? ImmutableList<string>.Empty;
        AfternoonProperties = afternoonProperties?.ToImmutableList() ?? ImmutableList<string>.Empty;
        EveningProperties = eveningProperties?.ToImmutableList() ?? ImmutableList<string>.Empty;
        NightProperties = nightProperties?.ToImmutableList() ?? ImmutableList<string>.Empty;
        DomainTags = domainTags?.ToImmutableList() ?? ImmutableList<string>.Empty;
        Population = population;
        Atmosphere = atmosphere;
        Physical = physical;
        Illumination = illumination;
        TravelTimeSegments = travelTimeSegments;
        TravelDescription = travelDescription;
        Difficulty = difficulty;
        Depth = depth;
        LocationType = locationType;
        AvailableServices = availableServices?.ToImmutableList() ?? ImmutableList<ServiceTypes>.Empty;
        HasBeenVisited = hasBeenVisited;
        VisitCount = visitCount;
        PlayerKnowledge = playerKnowledge;
        AccessRequirement = accessRequirement;
    }

    /// <summary>
    /// Creates a new LocationState with updated visit information.
    /// </summary>
    public LocationState WithVisit()
    {
        return new LocationState(
        Id, Name, Description, Connections, LocationSpotIds,
        MorningProperties, AfternoonProperties, EveningProperties, NightProperties,
        DomainTags, Population, Atmosphere, Physical, Illumination,
        TravelTimeSegments, TravelDescription, Difficulty, Depth,
        LocationType, AvailableServices, true, VisitCount + 1, PlayerKnowledge,
        AccessRequirement);
    }

    /// <summary>
    /// Creates a new LocationState with updated player knowledge.
    /// </summary>
    public LocationState WithPlayerKnowledge(bool knowledge)
    {
        return new LocationState(
        Id, Name, Description, Connections, LocationSpotIds,
        MorningProperties, AfternoonProperties, EveningProperties, NightProperties,
        DomainTags, Population, Atmosphere, Physical, Illumination,
        TravelTimeSegments, TravelDescription, Difficulty, Depth,
        LocationType, AvailableServices, HasBeenVisited, VisitCount, knowledge,
        AccessRequirement);
    }

    /// <summary>
    /// Creates a new LocationState with an added connection.
    /// </summary>
    public LocationState WithAddedConnection(LocationConnection connection)
    {
        return new LocationState(
        Id, Name, Description, Connections.Add(connection), LocationSpotIds,
        MorningProperties, AfternoonProperties, EveningProperties, NightProperties,
        DomainTags, Population, Atmosphere, Physical, Illumination,
        TravelTimeSegments, TravelDescription, Difficulty, Depth,
        LocationType, AvailableServices, HasBeenVisited, VisitCount, PlayerKnowledge,
        AccessRequirement);
    }

    /// <summary>
    /// Creates a LocationState from a mutable Location object.
    /// </summary>
    public static LocationState FromLocation(Location location)
    {
        return new LocationState(
            location.Id,
            location.Name,
            location.Description,
            location.Connections,
            location.LocationSpotIds,
            location.MorningProperties,
            location.AfternoonProperties,
            location.EveningProperties,
            location.NightProperties,
            location.DomainTags,
            location.Population,
            location.Atmosphere,
            location.Physical,
            location.Illumination,
            location.TravelTimeSegments,
            location.TravelDescription,
            location.Difficulty,
            location.Depth,
            location.LocationType,
            location.AvailableServices,
            location.HasBeenVisited,
            location.VisitCount,
            location.PlayerKnowledge,
            location.AccessRequirement);
    }
}