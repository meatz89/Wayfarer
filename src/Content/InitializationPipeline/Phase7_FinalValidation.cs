using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Phase 7: Final validation and creation of dummy entities for missing references.
/// This ensures the game can ALWAYS run, even with broken content.
/// </summary>
public class Phase7_FinalValidation : IInitializationPhase
{
    public int PhaseNumber => 7;
    public string Name => "Final Validation & Dummy Data Creation";
    public bool IsCritical => true; // Must succeed to ensure game runs

    public void Execute(InitializationContext context)
    {
        GameWorld gameWorld = context.GameWorld;
        List<string> createdDummies = new List<string>();

        // 1. Ensure player has location and spot
        EnsurePlayerLocation(context, gameWorld, createdDummies);

        // 2. Check all NPC references and create missing NPCs
        ValidateNPCReferences(context, gameWorld, createdDummies);

        // 3. Check all route references and create missing routes
        ValidateRouteReferences(context, gameWorld, createdDummies);

        // 4. Check all location references in spots
        ValidateLocationReferences(context, gameWorld, createdDummies);

        // 5. Ensure at least one letter template exists
        EnsureLetterTemplates(context, gameWorld, createdDummies);

        // 6. Final player validation
        FinalPlayerValidation(context, gameWorld);

        // Report dummy data creation - MUST BE HIGHLY VISIBLE
        if (createdDummies.Any())
        {
            // Create error file for E2E test detection
            string errorFilePath = "content_validation_errors.log";
            System.IO.File.WriteAllText(errorFilePath, $"CONTENT VALIDATION ERRORS DETECTED: {createdDummies.Count} dummy entities created\n");
            System.IO.File.AppendAllLines(errorFilePath, createdDummies);

            // Highly visible console output
            Console.WriteLine("\n" + new string('!', 80));
            Console.WriteLine("!!! CRITICAL CONTENT VALIDATION ERRORS !!!");
            Console.WriteLine(new string('!', 80));
            Console.WriteLine($"\n⚠️⚠️⚠️ CREATED {createdDummies.Count} DUMMY ENTITIES TO PREVENT GAME CRASH ⚠️⚠️⚠️\n");
            Console.WriteLine("THE FOLLOWING REFERENCES WERE MISSING FROM JSON CONTENT:\n");

            foreach (string dummy in createdDummies)
            {
                Console.WriteLine($"  ❌ {dummy}");
            }

            Console.WriteLine($"\n{new string('!', 80)}");
            Console.WriteLine("!!! FIX YOUR JSON CONTENT DEFINITIONS !!!");
            Console.WriteLine(new string('!', 80) + "\n");

            // Add multiple warnings to context
            context.Warnings.Add($"CRITICAL: Created {createdDummies.Count} dummy entities - CHECK content_validation_errors.log");
            context.Warnings.Add("CONTENT VALIDATION FAILED - Game running with placeholder data");

            // Also log to standard error for CI/CD visibility
            Console.Error.WriteLine($"ERROR: Content validation failed - {createdDummies.Count} missing references");
        }
    }

    private void EnsurePlayerLocation(InitializationContext context, GameWorld gameWorld, List<string> createdDummies)
    {
        Player player = gameWorld.GetPlayer();

        // Player must always have a spot
        if (player.CurrentLocationSpot == null)
        {
            // First, ensure we have at least one location
            if (!gameWorld.WorldState.locations.Any())
            {
                LocationFactory locationFactory = new LocationFactory();
                Location dummyLocation = locationFactory.CreateMinimalLocation("dummy_start_location");
                dummyLocation.Description = "A small village where your journey begins.";
                dummyLocation.DomainTags = new List<string> { "SAFE", "STARTING" };

                gameWorld.WorldState.locations.Add(dummyLocation);
                createdDummies.Add($"Location: {dummyLocation.Id} (fallback) - NO LOCATIONS IN JSON");
                Console.WriteLine($"  ⚠️ Created dummy location: {dummyLocation.Id}");
                Console.Error.WriteLine($"CONTENT ERROR: No locations defined in JSON");
            }

            // Now create a spot
            string targetLocationId = gameWorld.WorldState.locations.First().Id;
            LocationSpotFactory spotFactory = new LocationSpotFactory();
            LocationSpot dummySpot = spotFactory.CreateMinimalSpot($"{targetLocationId}_square", targetLocationId);
            // Description removed - generated from SpotPropertyType combinations
            dummySpot.InitialState = "A quiet village square with a few people going about their day.";
            dummySpot.DomainTags = new List<string> { "SOCIAL", "SAFE" };

            gameWorld.WorldState.locationSpots.Add(dummySpot);
            player.CurrentLocationSpot = dummySpot;
            createdDummies.Add($"LocationSpot: {dummySpot.SpotID} (player start) - MISSING FROM JSON");
            Console.WriteLine($"  ⚠️ Created dummy spot for player: {dummySpot.SpotID}");
            Console.Error.WriteLine($"CONTENT ERROR: Missing player start spot");
        }
    }

    private void ValidateNPCReferences(InitializationContext context, GameWorld gameWorld, List<string> createdDummies)
    {
        List<NPC> npcs = gameWorld.WorldState.NPCs;
        HashSet<string> npcIds = npcs.Select(n => n.ID).ToHashSet();
        HashSet<string> missingNPCs = new HashSet<string>();

        // Collect all NPC references from ValidationTracker
        // Check obligation NPCs
        foreach (KeyValuePair<string, string> kvp in context.ValidationTracker.ObligationNPCs)
        {
            if (!npcIds.Contains(kvp.Value))
            {
                missingNPCs.Add(kvp.Value);
            }
        }

        // Check route discovery NPCs
        foreach (List<string> routeNPCs in context.ValidationTracker.RouteDiscoveryNPCs.Values)
        {
            foreach (string npcId in routeNPCs)
            {
                if (!npcIds.Contains(npcId))
                {
                    missingNPCs.Add(npcId);
                }
            }
        }

        // Check obligations
        foreach (StandingObligation obligation in gameWorld.WorldState.StandingObligationTemplates)
        {
            if (!npcIds.Contains(obligation.Source))
            {
                missingNPCs.Add(obligation.Source);
            }
        }


        // NO FALLBACKS - crash if NPCs are missing
        foreach (string missingId in missingNPCs)
        {
            throw new InvalidOperationException($"NPC '{missingId}' is referenced but not defined in npcs.json - add NPC definition to content files");
        }
    }

    private void ValidateRouteReferences(InitializationContext context, GameWorld gameWorld, List<string> createdDummies)
    {
        List<RouteOption> routes = gameWorld.WorldState.Routes;
        HashSet<string> routeIds = routes.Select(r => r.Id).ToHashSet();
        HashSet<string> missingRoutes = new HashSet<string>();

        // Check route discoveries
        foreach (RouteDiscovery discovery in gameWorld.WorldState.RouteDiscoveries)
        {
            if (!routeIds.Contains(discovery.RouteId))
            {
                missingRoutes.Add(discovery.RouteId);
            }
        }

        // Create missing routes
        RouteFactory routeFactory = new RouteFactory();
        List<Location> locations = gameWorld.WorldState.locations;

        foreach (string missingId in missingRoutes)
        {
            if (locations.Count < 2) continue;

            // Create a simple route between first two locations
            Location origin = locations[0];
            Location destination = locations.Count > 1 ? locations[1] : locations[0];

            RouteOption dummyRoute = routeFactory.CreateMinimalRoute(missingId, origin.Id, destination.Id);

            gameWorld.WorldState.Routes.Add(dummyRoute);
            createdDummies.Add($"Route: {missingId} ({origin.Id} -> {destination.Id}) - REFERENCED BUT NOT DEFINED IN JSON");
            Console.WriteLine($"  ⚠️ Created dummy route: {missingId}");
            Console.Error.WriteLine($"CONTENT ERROR: Missing route definition for '{missingId}'");
        }
    }

    private void ValidateLocationReferences(InitializationContext context, GameWorld gameWorld, List<string> createdDummies)
    {
        List<Location> locations = gameWorld.WorldState.locations;
        HashSet<string> locationIds = locations.Select(l => l.Id).ToHashSet();

        // Check all spots reference valid locations
        List<LocationSpot> spotsToRemove = new List<LocationSpot>();
        foreach (LocationSpot spot in gameWorld.WorldState.locationSpots)
        {
            if (!locationIds.Contains(spot.LocationId))
            {
                // Can't create a location for a spot, so remove the spot
                spotsToRemove.Add(spot);
                context.Warnings.Add($"Removed spot {spot.SpotID} - references missing location {spot.LocationId}");
            }
        }

        foreach (LocationSpot spot in spotsToRemove)
        {
            gameWorld.WorldState.locationSpots.Remove(spot);
        }
    }

    private void EnsureLetterTemplates(InitializationContext context, GameWorld gameWorld, List<string> createdDummies)
    {
        if (!gameWorld.WorldState.LetterTemplates.Any())
        {
            // Create at least one basic template
            LetterTemplate basicTemplate = new LetterTemplate
            {
                Id = "dummy_basic_letter",
                Description = "A simple letter delivery task",
                TokenType = ConnectionType.Trust,
                MinPayment = 3,
                MaxPayment = 5,
                MinDeadlineInMinutes = 1440,
                MaxDeadlineInMinutes = 2880,
                Category = LetterCategory.Basic
            };

            gameWorld.WorldState.LetterTemplates.Add(basicTemplate);
            createdDummies.Add($"LetterTemplate: {basicTemplate.Id} (basic delivery) - NO LETTER TEMPLATES IN JSON");
            Console.WriteLine($"  ⚠️ Created dummy letter template: {basicTemplate.Id}");
            Console.Error.WriteLine($"CONTENT ERROR: No letter templates defined in JSON");
        }
    }

    private void FinalPlayerValidation(InitializationContext context, GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();

        // CRITICAL: Player must ALWAYS have a spot
        if (player.CurrentLocationSpot == null)
        {
            throw new InvalidOperationException(
                "CRITICAL: Player spot initialization failed after all phases. " +
                $"CurrentLocationSpot: {player.CurrentLocationSpot?.SpotID ?? "NULL"}");
        }

        // Verify the spot is in WorldState
        if (!gameWorld.WorldState.locationSpots.Contains(player.CurrentLocationSpot))
        {
            throw new InvalidOperationException(
                $"CRITICAL: Player spot {player.CurrentLocationSpot.SpotID} not found in WorldState");
        }

        // Verify the spot's location exists
        Location? spotLocation = gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == player.CurrentLocationSpot.LocationId);
        if (spotLocation == null)
        {
            throw new InvalidOperationException(
                $"CRITICAL: Player spot's location {player.CurrentLocationSpot.LocationId} not found in WorldState");
        }

        Console.WriteLine($"\nPlayer initialized successfully:");
        Console.WriteLine($"  Location: {spotLocation.Id} ({spotLocation.Name})");
        Console.WriteLine($"  Spot: {player.CurrentLocationSpot.SpotID} ({player.CurrentLocationSpot.Name})");
        Console.WriteLine($"  Resources: {player.Coins} coins, {player.Stamina}/{player.MaxStamina} stamina");
    }

    private string FormatIdAsName(string id)
    {
        // Convert snake_case or kebab-case to Title Case
        return string.Join(" ",
            id.Replace('_', ' ').Replace('-', ' ')
              .Split(' ')
              .Select(word => string.IsNullOrEmpty(word) ? "" :
                  char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }
}