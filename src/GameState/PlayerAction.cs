/// <summary>
/// Global player action available everywhere regardless of location context.
/// Unlike LocationActions which are matched by location properties,
/// PlayerActions are always potentially available to the player.
/// </summary>
public class PlayerAction
{
    // HIGHLANDER: NO Id property - PlayerAction identified by object reference

    /// <summary>
    /// Display name shown to the player
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Detailed description of what this action does
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// HIGHLANDER: Unified costs and rewards
    /// Consequence is the ONLY class for resource outcomes.
    /// Negative values = costs, Positive values = rewards
    /// </summary>
    public Consequence Consequence { get; set; } = new Consequence();

    /// <summary>
    /// Time required to complete this action in segments
    /// </summary>
    public int TimeRequired { get; set; }

    /// <summary>
    /// Priority for sorting (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Action type for execution dispatch - strongly typed enum validated by parser
    /// </summary>
    public PlayerActionType ActionType { get; set; }

    /// <summary>
    /// Location role required for this action to be available.
    /// Action only appears if location has this role.
    /// null = available everywhere (default)
    /// </summary>
    public LocationRole? RequiredLocationRole { get; set; } = null;

    /// <summary>
    /// Location environment required for this action to be available.
    /// Action only appears if location has this environment.
    /// null = available in any environment (default)
    /// </summary>
    public LocationEnvironment? RequiredEnvironment { get; set; } = null;
}
