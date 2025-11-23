/// <summary>
/// Immutable snapshot of Location state at spawn time
/// Captures categorical properties and hex position for tracing
/// </summary>
public class LocationSnapshot
{
    public string Name { get; set; }
    public AxialCoordinates HexPosition { get; set; }
    public LocationPurpose Purpose { get; set; }
    public LocationPrivacy Privacy { get; set; }
    public LocationSafety Safety { get; set; }
    public LocationActivity Activity { get; set; }
}
