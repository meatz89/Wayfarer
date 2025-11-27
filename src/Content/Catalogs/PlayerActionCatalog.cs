
/// <summary>
/// Catalogue for generating universal PlayerActions available at all locations.
/// Called by Parser ONLY - runtime never touches this.
///
/// PARSE-TIME ENTITY GENERATION:
/// Parser calls GenerateUniversalActions() → Catalogue generates complete PlayerAction entities
/// → Parser adds to GameWorld.PlayerActions → Runtime queries GameWorld (NO catalogue calls)
/// </summary>
public static class PlayerActionCatalog
{
    /// <summary>
    /// Generate all universal player actions available at any location.
    /// These are meta-actions that don't depend on location properties.
    /// </summary>
    public static List<PlayerAction> GenerateUniversalActions()
    {
        return new List<PlayerAction>
    {
        // Check Belongings - View inventory/equipment
        new PlayerAction
        {
            Name = "Check Belongings",
            Description = "Review your current equipment and inventory",
            ActionType = PlayerActionType.CheckBelongings,
            Costs = ActionCosts.None(),
            Rewards = ActionRewards.None()
        },

        // Wait - Skip time segment
        new PlayerAction
        {
            Name = "Wait",
            Description = "Pass time without activity. Advances 1 time segment with no resource recovery. Hunger increases by +5 automatically.",
            ActionType = PlayerActionType.Wait,
            Costs = new ActionCosts
            {
                // Waiting costs no resources, just time (handled by intent)
            },
            Rewards = ActionRewards.None()
        },

        // Sleep Outside - Emergency rest without shelter
        new PlayerAction
        {
            Name = "Sleep Outside",
            Description = "Sleep rough without shelter. Saves coins but costs 2 Health and provides no recovery. Cold, uncomfortable, risky.",
            ActionType = PlayerActionType.SleepOutside,
            Costs = new ActionCosts
            {
                Health = 2  // Sleeping outside damages health
            },
            Rewards = ActionRewards.None(),  // No recovery from sleeping outside
            RequiredLocationRole = null  // Available everywhere - filtering happens at execution time based on Environment
        },

        // Look Around - Navigate to LookingAround view
        new PlayerAction
        {
            Name = "Look Around",
            Description = "See who's here and what's happening - people, challenges, opportunities",
            ActionType = PlayerActionType.LookAround,
            Costs = ActionCosts.None(),  // No resource cost
            Rewards = ActionRewards.None(),  // Navigation-only action
            Priority = 1  // TOP priority - should appear first in list
        }
    };
    }
}
