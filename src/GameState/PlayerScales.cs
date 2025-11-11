/// <summary>
/// Player Scales - 6 moral/behavioral axes that track player choices
/// Strongly-typed nested object with 6 int properties (-10 to +10 each)
/// NOT a list or dictionary - each scale is a specific property
/// Both extremes unlock content (different archetypes, not better/worse)
/// </summary>
public class PlayerScales
{
    /// <summary>
    /// Morality scale: -10 (Exploitative) to +10 (Altruistic)
    /// Tracks selfishness vs. altruism in player choices
    /// </summary>
    public int Morality { get; set; } = 0;

    /// <summary>
    /// Lawfulness scale: -10 (Rebellious) to +10 (Establishment)
    /// Tracks respect for authority and established order
    /// </summary>
    public int Lawfulness { get; set; } = 0;

    /// <summary>
    /// Method scale: -10 (Violent) to +10 (Diplomatic)
    /// Tracks approach to conflict resolution
    /// </summary>
    public int Method { get; set; } = 0;

    /// <summary>
    /// Caution scale: -10 (Reckless) to +10 (Careful)
    /// Tracks risk-taking vs. careful planning
    /// </summary>
    public int Caution { get; set; } = 0;

    /// <summary>
    /// Transparency scale: -10 (Secretive) to +10 (Open)
    /// Tracks honesty and openness vs. secrecy and manipulation
    /// </summary>
    public int Transparency { get; set; } = 0;

    /// <summary>
    /// Fame scale: -10 (Notorious) to +10 (Celebrated)
    /// Tracks public reputation and recognition
    /// </summary>
    public int Fame { get; set; } = 0;
}
