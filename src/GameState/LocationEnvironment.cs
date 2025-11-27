/// <summary>
/// Physical enclosure of a location. Orthogonal categorical dimension.
/// Determines weather exposure, conversation atmosphere, and environmental effects.
/// </summary>
public enum LocationEnvironment
{
    /// <summary>Fully enclosed space - complete weather shelter, private conversations</summary>
    Indoor,

    /// <summary>Open-air space - exposed to weather, public conversations</summary>
    Outdoor,

    /// <summary>Partially covered - some weather protection (porches, awnings, arcades)</summary>
    Covered,

    /// <summary>Below ground - caves, cellars, catacombs</summary>
    Underground
}
