using System.Collections.Generic;

/// <summary>
/// Global player action available everywhere regardless of location context.
/// Unlike LocationActions which are matched by location properties,
/// PlayerActions are always potentially available to the player.
/// </summary>
public class PlayerAction
{
    /// <summary>
    /// Unique identifier for this action
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name shown to the player
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Detailed description of what this action does
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Resource costs required to perform this action
    /// </summary>
    public ActionCosts Costs { get; set; } = new ActionCosts();

    /// <summary>
    /// Resources rewarded for performing this action
    /// </summary>
    public ActionRewards Rewards { get; set; } = new ActionRewards();

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
}
