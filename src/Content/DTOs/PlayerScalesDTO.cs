/// <summary>
/// DTO for Player Scales - 6 moral/behavioral axes
/// Strongly-typed nested object with 6 int properties
/// NOT a list or dictionary - each scale is a specific property
/// </summary>
public class PlayerScalesDTO
{
    /// <summary>
    /// Morality scale: -10 (Exploitative) to +10 (Altruistic)
    /// </summary>
    public int Morality { get; set; } = 0;

    /// <summary>
    /// Lawfulness scale: -10 (Rebellious) to +10 (Establishment)
    /// </summary>
    public int Lawfulness { get; set; } = 0;

    /// <summary>
    /// Method scale: -10 (Violent) to +10 (Diplomatic)
    /// </summary>
    public int Method { get; set; } = 0;

    /// <summary>
    /// Caution scale: -10 (Reckless) to +10 (Careful)
    /// </summary>
    public int Caution { get; set; } = 0;

    /// <summary>
    /// Transparency scale: -10 (Secretive) to +10 (Open)
    /// </summary>
    public int Transparency { get; set; } = 0;

    /// <summary>
    /// Fame scale: -10 (Notorious) to +10 (Celebrated)
    /// </summary>
    public int Fame { get; set; } = 0;
}
