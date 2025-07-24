using System;
using System.Linq;

/// <summary>
/// Phase 5: Initialize the player with a valid location and spot.
/// This MUST happen after locations and spots are loaded.
/// </summary>
public class Phase5_PlayerInitialization : IInitializationPhase
{
    public int PhaseNumber => 5;
    public string Name => "Player Initialization";
    public bool IsCritical => true; // MUST succeed

    public void Execute(InitializationContext context)
    {
        var player = context.GameWorld.GetPlayer();
        var worldState = context.GameWorld.WorldState;
        
        Console.WriteLine("Initializing player location...");
        
        // Player is the single source of truth for location - no sync needed
        
        // Check if we have starting location from gameWorld.json
        if (context.SharedData.ContainsKey("StartingLocationId"))
        {
            var startingLocationId = (string)context.SharedData["StartingLocationId"];
            var startingLocation = worldState.locations.FirstOrDefault(l => l.Id == startingLocationId);
            
            if (startingLocation != null)
            {
                player.CurrentLocation = startingLocation;
                Console.WriteLine($"  Set player to configured starting location: {startingLocation.Id}");
            }
            else
            {
                context.Warnings.Add($"Configured starting location '{startingLocationId}' not found");
            }
        }
        
        // If player still has no location, find a suitable starting location
        if (player.CurrentLocation == null)
        {
            // Try to find Millbrook as the default starting location
            var millbrook = worldState.locations.FirstOrDefault(l => 
                l.Id == "millbrook" || 
                l.Name.Contains("Millbrook", StringComparison.OrdinalIgnoreCase));
            
            if (millbrook != null)
            {
                player.CurrentLocation = millbrook;
                Console.WriteLine($"  Set player to default starting location: {millbrook.Id}");
            }
            else if (worldState.locations.Any())
            {
                // Fall back to first available location
                player.CurrentLocation = worldState.locations.First();
                Console.WriteLine($"  Set player to first available location: {player.CurrentLocation.Id}");
            }
            else
            {
                context.Errors.Add("No locations available for player initialization");
                return;
            }
        }
        
        // Now find a spot in the player's location
        if (player.CurrentLocationSpot == null && player.CurrentLocation != null)
        {
            // Check if we have starting spot from gameWorld.json
            if (context.SharedData.ContainsKey("StartingLocationSpotId"))
            {
                var startingSpotId = (string)context.SharedData["StartingLocationSpotId"];
                var startingSpot = worldState.locationSpots.FirstOrDefault(s => s.SpotID == startingSpotId);
                
                if (startingSpot != null && startingSpot.LocationId == player.CurrentLocation.Id)
                {
                    player.CurrentLocationSpot = startingSpot;
                    Console.WriteLine($"  Set player to configured starting spot: {startingSpot.SpotID}");
                }
                else
                {
                    context.Warnings.Add($"Configured starting spot '{startingSpotId}' not found or not in current location");
                }
            }
            
            // If no configured spot or it failed, find one
            if (player.CurrentLocationSpot == null)
            {
                var spotsInLocation = worldState.locationSpots
                    .Where(s => s.LocationId == player.CurrentLocation.Id)
                    .ToList();
                
                if (spotsInLocation.Any())
                {
                    // Prefer a market or tavern as starting spot
                    var preferredSpot = spotsInLocation.FirstOrDefault(s => 
                        s.Name.Contains("Market", StringComparison.OrdinalIgnoreCase) ||
                        s.Name.Contains("Tavern", StringComparison.OrdinalIgnoreCase) ||
                        s.Name.Contains("Square", StringComparison.OrdinalIgnoreCase));
                    
                    player.CurrentLocationSpot = preferredSpot ?? spotsInLocation.First();
                    Console.WriteLine($"  Set player to spot: {player.CurrentLocationSpot.SpotID}");
                }
                else
                {
                    // No spots in this location - this will be fixed in Phase 6
                    context.SharedData["player_needs_spot"] = player.CurrentLocation.Id;
                    Console.WriteLine($"  WARNING: No spots found in {player.CurrentLocation.Id} - will create in Phase 6");
                }
            }
        }
        
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
        
        // Final validation - if we still don't have a location, it will be handled in Phase 6
        if (player.CurrentLocation == null)
        {
            context.SharedData["player_needs_location"] = true;
            Console.WriteLine("  CRITICAL: Player still has no location - will create in Phase 6");
        }
    }
}