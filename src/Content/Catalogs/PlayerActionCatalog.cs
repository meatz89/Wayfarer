
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
            Id = "check_belongings",  // Match JSON ID
            Name = "Check Belongings",
            Description = "Review your current equipment and inventory",  // Match JSON description
            ActionType = PlayerActionType.CheckBelongings,
            Costs = ActionCosts.None(),
            Rewards = ActionRewards.None()
        },

        // Wait - Skip time segment
        new PlayerAction
        {
            Id = "wait",  // Match JSON ID
            Name = "Wait",
            Description = "Pass time without activity. Advances 1 time segment with no resource recovery. Hunger increases by +5 automatically.",  // Match JSON description
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
            Id = "sleep_outside",  // Match JSON ID
            Name = "Sleep Outside",
            Description = "Sleep rough without shelter. Saves coins but costs 2 Health and provides no recovery. Cold, uncomfortable, risky.",  // Match JSON description
            ActionType = PlayerActionType.SleepOutside,
            Costs = new ActionCosts
            {
                Health = 2  // Sleeping outside damages health
            },
            Rewards = ActionRewards.None(),  // No recovery from sleeping outside
            RequiredLocationProperties = new List<LocationPropertyType>
            {
                LocationPropertyType.Outdoor  // Requires open-air outdoor space
            }
        },

        // Look Around - Navigate to LookingAround view
        new PlayerAction
        {
            Id = "look_around",
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
