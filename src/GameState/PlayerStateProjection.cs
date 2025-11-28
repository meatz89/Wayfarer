/// <summary>
/// Read-only projection of what player state would be after applying a Consequence.
/// Used for Perfect Information display in UI (Sir Brante pattern).
/// Does NOT represent actual game state - purely a preview.
/// </summary>
public class PlayerStateProjection
{
    // Resources
    public int Coins { get; init; }
    public int Resolve { get; init; }
    public int Health { get; init; }
    public int Stamina { get; init; }
    public int Focus { get; init; }
    public int Hunger { get; init; }

    // Five Stats
    public int Insight { get; init; }
    public int Rapport { get; init; }
    public int Authority { get; init; }
    public int Diplomacy { get; init; }
    public int Cunning { get; init; }

    // Mental Progression
    public int Understanding { get; init; }

    /// <summary>
    /// Create a projection matching current player state (no changes)
    /// </summary>
    public static PlayerStateProjection FromPlayer(Player player)
    {
        return new PlayerStateProjection
        {
            Coins = player.Coins,
            Resolve = player.Resolve,
            Health = player.Health,
            Stamina = player.Stamina,
            Focus = player.Focus,
            Hunger = player.Hunger,
            Insight = player.Insight,
            Rapport = player.Rapport,
            Authority = player.Authority,
            Diplomacy = player.Diplomacy,
            Cunning = player.Cunning,
            Understanding = player.Understanding
        };
    }
}
