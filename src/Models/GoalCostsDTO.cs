/// <summary>
/// DTO for goal costs - resources player must pay to attempt goal
/// Maps to GoalCosts entity
/// </summary>
public class GoalCostsDTO
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
