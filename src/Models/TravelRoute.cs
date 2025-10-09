/// <summary>
/// Strongly-typed representation of a travel route between two locations.
/// Replaces tuple usage in TravelTimeCalculator for route keys.
/// </summary>
public record TravelRoute
{
    /// <summary>
    /// The starting Venue ID
    /// </summary>
    public string FromVenueId { get; init; }

    /// <summary>
    /// The destination Venue ID
    /// </summary>
    public string ToVenueId { get; init; }

    /// <summary>
    /// Create a travel route
    /// </summary>
    public TravelRoute(string fromVenueId, string toVenueId)
    {
        FromVenueId = fromVenueId;
        ToVenueId = toVenueId;
    }

    /// <summary>
    /// Override GetHashCode for dictionary usage
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(FromVenueId, ToVenueId);
    }

    /// <summary>
    /// String representation for debugging
    /// </summary>
    public override string ToString()
    {
        return $"{FromVenueId} -> {ToVenueId}";
    }
}
