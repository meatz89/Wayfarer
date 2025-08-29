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

        if (!string.IsNullOrEmpty(context.GameWorld.InitialLocationSpotId))
        {
            string startingSpotId = context.GameWorld.InitialLocationSpotId;
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

            // Check for configured starting location from GameWorld
            if (!string.IsNullOrEmpty(context.GameWorld.InitialLocationId))
            {
                preferredLocationId = context.GameWorld.InitialLocationId;
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
                    // Use mechanical property to find starting location
                    // First check if any location is marked as starting location
                    var startingLocation = worldState.locations.FirstOrDefault(l => l.IsStartingLocation);
                    if (startingLocation != null)
                    {
                        var spotsInStartingLocation = worldState.locationSpots
                            .Where(s => s.LocationId == startingLocation.Id)
                            .ToList();
                        startingSpot = spotsInStartingLocation.FirstOrDefault();
                    }
                    
                    player.CurrentLocationSpot = startingSpot;
                    Console.WriteLine($"  Set player to starting spot: {startingSpot.SpotID}");
                }
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

        // Load player config from GameWorld if available
        if (context.GameWorld.InitialPlayerConfig != null)
        {
            dynamic playerConfig = context.GameWorld.InitialPlayerConfig;

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

        // Ensure player has basic resources
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
        
        // INITIALIZE PLAYER OBSERVATION DECK (POC Architecture)
        InitializePlayerObservationDeck(player, context);
    }
    
    private void InitializePlayerObservationDeck(Player player, InitializationContext context)
    {
        Console.WriteLine("Initializing Player Observation Deck...");
        
        // Get observation cards from GameWorld (loaded in Phase0)
        var observationCards = context.GameWorld.PlayerObservationCards;
        
        if (observationCards == null || !observationCards.Any())
        {
            Console.WriteLine("  No observation cards loaded - player starts with empty deck");
            return;
        }
        
        // Initialize the player's observation deck
        if (player.ObservationDeck == null)
        {
            player.ObservationDeck = new CardDeck();
        }
        
        // Add all observation cards to the player's deck
        // In the full game, these would be gained through exploration
        // For the POC, we give the player some starting observations
        int cardsAdded = 0;
        foreach (var card in observationCards.Take(3)) // Start with 3 observation cards
        {
            player.ObservationDeck.AddCard(card);
            cardsAdded++;
            Console.WriteLine($"  Added observation: {card.DisplayName}");
        }
        
        Console.WriteLine($"  Player Observation Deck initialized with {cardsAdded} cards");
    }
}