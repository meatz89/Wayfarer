/// <summary>
/// Projected bond strength change with an NPC
/// Shows relationship consequence before player commits to situation
/// </summary>
public class BondChange
{
/// <summary>
/// NPC ID whose bond will change
/// </summary>
public string NpcId { get; set; }

/// <summary>
/// Bond strength delta (positive or negative)
/// Applied to NPC.BondStrength (clamped to -10 to +10 range)
/// </summary>
public int Delta { get; set; }

/// <summary>
/// Human-readable explanation of why bond changes
/// Example: "Martha appreciates your honesty"
/// </summary>
public string Reason { get; set; }
}
