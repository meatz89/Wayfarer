/// <summary>
/// DTO for deserializing action costs from JSON.
/// Strongly-typed properties match JSON field names exactly (no JsonPropertyName needed).
/// </summary>
public class ActionCostsDTO
{
    /// <summary>
    /// Coin cost required to perform this action
    /// </summary>
    public int Coins { get; set; }

    /// <summary>
    /// Focus cost required to perform this action
    /// </summary>
    public int Focus { get; set; }

    /// <summary>
    /// Stamina cost required to perform this action
    /// </summary>
    public int Stamina { get; set; }

    /// <summary>
    /// Health cost required to perform this action
    /// </summary>
    public int Health { get; set; }
}
