/// <summary>
/// Physical danger and threat level of location
/// Orthogonal categorical dimension for entity resolution
/// Determines risk of violence, conflict, or harm
/// </summary>
public enum LocationSafety
{
    /// <summary>
    /// Active threats present - hostile forces, environmental hazards
    /// High risk of violence, unsafe conditions
    /// Example: Bandit-controlled road, monster den, war zone, crumbling ruin
    /// </summary>
    Dangerous,

    /// <summary>
    /// Normal risk level - typical safety for location type
    /// No active threats but not guaranteed secure
    /// Example: Average street, common road, unguarded building, wilderness path
    /// </summary>
    Neutral,

    /// <summary>
    /// Protected, secure, low threat of harm
    /// Guards present, sanctified ground, peaceful haven
    /// Example: Temple sanctuary, guarded noble district, secure inn, town center
    /// </summary>
    Safe
}
