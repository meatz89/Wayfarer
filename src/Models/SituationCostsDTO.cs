/// <summary>
/// DTO for situation costs - resources player must pay to attempt situation
/// Maps to SituationCosts entity
/// </summary>
public class SituationCostsDTO
{
    /// <summary>
    /// Time segments consumed
    /// </summary>
    public int Time { get; set; }

    /// <summary>
    /// Focus consumed (Mental challenges)
    /// </summary>
    public int Focus { get; set; }

    /// <summary>
    /// Stamina consumed (Physical challenges)
    /// </summary>
    public int Stamina { get; set; }

    /// <summary>
    /// Coins spent (rare)
    /// </summary>
    public int Coins { get; set; }
}
