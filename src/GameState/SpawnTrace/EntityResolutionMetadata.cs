/// <summary>
/// Resolution outcome for entity discovery/creation during scene instantiation
/// </summary>
public enum EntityResolutionOutcome
{
    /// <summary>
    /// Entity was found via categorical filter matching
    /// </summary>
    Discovered,

    /// <summary>
    /// Entity was procedurally created because no match found
    /// </summary>
    Created,

    /// <summary>
    /// Entity was obtained via RouteDestination proximity (special case)
    /// </summary>
    RouteDestination
}

/// <summary>
/// Tracks how an entity was resolved during scene instantiation
/// Captures whether entity was discovered or created, and which properties were involved
/// </summary>
public class EntityResolutionMetadata
{
    /// <summary>
    /// How the entity was obtained (Discovered, Created, RouteDestination)
    /// </summary>
    public EntityResolutionOutcome Outcome { get; set; }

    /// <summary>
    /// The PlacementFilter used to search for the entity
    /// Captures all categorical search criteria
    /// </summary>
    public PlacementFilterSnapshot Filter { get; set; }

    /// <summary>
    /// For Discovered entities: property names that matched the filter
    /// Example: ["Purpose", "Privacy", "Safety"] - all filter criteria that matched
    /// </summary>
    public List<string> MatchedProperties { get; set; } = new List<string>();

    /// <summary>
    /// For Created entities: property names that came from the filter
    /// Example: ["Purpose", "Privacy"] - properties specified in filter
    /// </summary>
    public List<string> FilterProvidedProperties { get; set; } = new List<string>();

    /// <summary>
    /// For Created entities: property names that were procedurally generated (defaults)
    /// Example: ["Safety", "Activity"] - properties not in filter, used defaults
    /// </summary>
    public List<string> GeneratedProperties { get; set; } = new List<string>();

    /// <summary>
    /// Creates metadata for a discovered entity
    /// </summary>
    public static EntityResolutionMetadata ForDiscovered(PlacementFilterSnapshot filter, List<string> matchedProperties)
    {
        return new EntityResolutionMetadata
        {
            Outcome = EntityResolutionOutcome.Discovered,
            Filter = filter,
            MatchedProperties = matchedProperties
        };
    }

    /// <summary>
    /// Creates metadata for a created entity
    /// </summary>
    public static EntityResolutionMetadata ForCreated(
        PlacementFilterSnapshot filter,
        List<string> filterProvidedProperties,
        List<string> generatedProperties)
    {
        return new EntityResolutionMetadata
        {
            Outcome = EntityResolutionOutcome.Created,
            Filter = filter,
            FilterProvidedProperties = filterProvidedProperties,
            GeneratedProperties = generatedProperties
        };
    }

    /// <summary>
    /// Creates metadata for RouteDestination proximity resolution
    /// </summary>
    public static EntityResolutionMetadata ForRouteDestination()
    {
        return new EntityResolutionMetadata
        {
            Outcome = EntityResolutionOutcome.RouteDestination
        };
    }
}
