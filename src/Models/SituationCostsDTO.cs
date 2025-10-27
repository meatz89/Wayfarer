/// <summary>
/// DTO for situation costs - resources player must pay to attempt situation
/// Maps to SituationCosts entity
/// </summary>
public class SituationCostsDTO
{
    /// <summary>
    /// Resolve consumed (universal strategic cost)
    /// Scene-Situation Architecture - shared by ALL situation types
    /// </summary>
    public int Resolve { get; set; } = 0;

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
