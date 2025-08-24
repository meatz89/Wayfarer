using System;
using System.Linq;

/// <summary>
/// Phase 6: Initialize the player with a valid location and spot.
/// This MUST happen after locations and spots are loaded.
/// </summary>
public class Phase6_PlayerInitialization : IInitializationPhase
{
    public int PhaseNumber => 6;
    public string Name => "Player Initialization";
    public bool IsCritical => true; // MUST succeed

    public void Execute(InitializationContext context)
    {
        Player player = context.GameWorld.GetPlayer();
        WorldState worldState = context.GameWorld.WorldState;

        Console.WriteLine("Initializing player location...");

        // Player is the single source of truth for location - no sync needed

        // Player must always be at a spot, not just a location
        // First, check if we have a configured starting spot
        LocationSpot startingSpot = null;

        if (context.SharedData.ContainsKey("StartingLocationSpotId"))
        {
            string startingSpotId = (string)context.SharedData["StartingLocationSpotId"];
            startingSpot = worldState.locationSpots.FirstOrDefault(s => s.SpotID == startingSpotId);

            if (startingSpot != null)
            {
                player.CurrentLocationSpot = startingSpot;
                Console.WriteLine($"  Set player to configured starting spot: {startingSpot.SpotID}");
            }
            else
            {
                context.Warnings.Add($"Configured starting spot '{startingSpotId}' not found");
            }
        }

        // If no configured spot, find one based on location preference
        if (player.CurrentLocationSpot == null)
        {
            string preferredLocationId = null;

            // Check for configured starting location
            if (context.SharedData.ContainsKey("StartingLocationId"))
            {
                preferredLocationId = (string)context.SharedData["StartingLocationId"];
            }
            else
            {
                // Default to lower_ward or first available location
                Location? lowerWard = worldState.locations.FirstOrDefault(l => l.Id == "lower_ward");
                preferredLocationId = lowerWard?.Id ?? worldState.locations.FirstOrDefault()?.Id;
            }

            if (preferredLocationId != null)
            {
                // Find spots in the preferred location
                List<LocationSpot> spotsInLocation = worldState.locationSpots
                    .Where(s => s.LocationId == preferredLocationId)
                    .ToList();

                if (spotsInLocation.Any())
                {
                    // Prefer specific spot types for starting
                    // Start at copper_kettle for testing NPCs
                    startingSpot = worldState.locationSpots.FirstOrDefault(s => s.SpotID == "copper_kettle")
                        ?? spotsInLocation.FirstOrDefault(s =>
                            s.SpotID == "abandoned_warehouse" || // Tutorial start
                            s.Name.Contains("Square", StringComparison.OrdinalIgnoreCase) ||
                            s.Name.Contains("Market", StringComparison.OrdinalIgnoreCase) ||
                            s.Name.Contains("Tavern", StringComparison.OrdinalIgnoreCase))
                        ?? spotsInLocation.First();

                    player.CurrentLocationSpot = startingSpot;
                    Console.WriteLine($"  Set player to starting spot: {startingSpot.SpotID}");
                }
            }
        }

        // Final fallback - any spot
        if (player.CurrentLocationSpot == null)
        {
            if (worldState.locationSpots.Any())
            {
                player.CurrentLocationSpot = worldState.locationSpots.First();
                Console.WriteLine($"  Set player to first available spot: {player.CurrentLocationSpot.SpotID}");
            }
            else
            {
                context.Errors.Add("No location spots available for player initialization");
                return;
            }
        }

        // Player is now always at a spot, location is derived from the spot

        // No need to sync - Player is the single source of truth for current location

        // Initialize other player properties if needed
        if (string.IsNullOrEmpty(player.Name))
        {
            player.Name = "Courier";
            Console.WriteLine("  Set default player name: Courier");
        }

        if (player.Archetype == 0)
        {
            player.Archetype = Professions.Merchant;
            Console.WriteLine("  Set default player archetype: Merchant");
        }

        // Load player config from gameWorld.json if available
        if (context.SharedData.ContainsKey("PlayerConfig"))
        {
            dynamic playerConfig = context.SharedData["PlayerConfig"];

            if (playerConfig.Coins != null)
            {
                player.Coins = (int)playerConfig.Coins;
                Console.WriteLine($"  Set starting coins: {player.Coins}");
            }

            if (playerConfig.StaminaPoints != null)
            {
                player.Stamina = (int)playerConfig.StaminaPoints;
            }

            if (playerConfig.MaxStamina != null)
            {
                player.MaxStamina = (int)playerConfig.MaxStamina;
                Console.WriteLine($"  Set stamina: {player.Stamina}/{player.MaxStamina}");
            }

            if (playerConfig.Health != null)
            {
                player.Health = (int)playerConfig.Health;
                Console.WriteLine($"  Set starting health: {player.Health}");
            }

            if (playerConfig.MaxHealth != null)
            {
                player.MaxHealth = (int)playerConfig.MaxHealth;
                Console.WriteLine($"  Set max health: {player.MaxHealth}");
            }

            if (playerConfig.Food != null)
            {
                player.Food = (int)playerConfig.Food;
                Console.WriteLine($"  Set starting food: {player.Food}");
            }
        }

        // Ensure player has basic resources (fallback if not set)
        if (player.Coins == 0)
        {
            player.Coins = 10;
            Console.WriteLine("  Set starting coins: 10");
        }

        if (player.MaxStamina == 0)
        {
            player.MaxStamina = 10;
            player.Stamina = 6;
            Console.WriteLine("  Set stamina: 6/10");
        }

        if (player.MaxHealth == 0)
        {
            player.MaxHealth = 100;
            Console.WriteLine("  Set default max health: 100");
        }

        if (player.Health == 0)
        {
            player.Health = player.MaxHealth;
            Console.WriteLine($"  Set starting health: {player.Health}");
        }

        if (player.Food == 0)
        {
            player.Food = 30;
            Console.WriteLine("  Set starting food: 30");
        }

        // Location is now derived from CurrentLocationSpot, no need for separate validation
    }
}