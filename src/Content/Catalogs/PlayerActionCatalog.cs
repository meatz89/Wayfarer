
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
                Consequence = Consequence.None()  // HIGHLANDER: Free action
            },

            // Wait - Skip time segment
            new PlayerAction
            {
                Name = "Wait",
                Description = "Pass time without activity. Advances 1 time segment with no resource recovery. Hunger increases by +5 automatically.",
                ActionType = PlayerActionType.Wait,
                Consequence = Consequence.None()  // HIGHLANDER: Time cost handled by intent
            },

            // Sleep Outside - Emergency rest without shelter (outdoor only)
            new PlayerAction
            {
                Name = "Sleep Outside",
                Description = "Sleep rough without shelter. Saves coins but costs 2 Health and provides no recovery. Cold, uncomfortable, risky.",
                ActionType = PlayerActionType.SleepOutside,
                Consequence = new Consequence { Health = -2 },  // HIGHLANDER: Negative = cost
                RequiredLocationRole = null,
                RequiredEnvironment = LocationEnvironment.Outdoor  // Only available at outdoor locations
            },

            // Look Around - Navigate to LookingAround view
            new PlayerAction
            {
                Name = "Look Around",
                Description = "See who's here and what's happening - people, challenges, opportunities",
                ActionType = PlayerActionType.LookAround,
                Consequence = Consequence.None(),  // HIGHLANDER: Navigation-only action
                Priority = 1  // TOP priority - should appear first in list
            }
        };
    }
}
