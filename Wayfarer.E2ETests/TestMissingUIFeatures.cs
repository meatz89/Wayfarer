using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

public class TestMissingUIFeatures
{
    public static async Task Main(string[] args)
    {
        var factory = new WebApplicationFactory<Wayfarer.Program>();
        
        using (var scope = factory.Services.CreateScope())
        {
            var gameWorld = scope.ServiceProvider.GetRequiredService<GameWorld>();
            var gameFacade = scope.ServiceProvider.GetRequiredService<IGameFacade>();
            var locationRepo = scope.ServiceProvider.GetRequiredService<LocationRepository>();
            var npcRepo = scope.ServiceProvider.GetRequiredService<NPCRepository>();
            var flagService = scope.ServiceProvider.GetRequiredService<FlagService>();
            
            // Complete tutorial first
            flagService.SetFlag(FlagService.TUTORIAL_COMPLETE);
            
            Console.WriteLine("=== Testing Missing UI Features ===\n");
            
            // Test 1: Resource Gathering at FEATURE locations
            Console.WriteLine("TEST 1: Resource Gathering at FEATURE locations");
            Console.WriteLine("------------------------------------------------");
            
            var player = gameWorld.GetPlayer();
            var allSpots = locationRepo.GetAllLocationSpots();
            var featureSpots = allSpots.Where(s => s.Type == LocationSpotTypes.FEATURE).ToList();
            
            Console.WriteLine($"Found {featureSpots.Count} FEATURE locations:");
            foreach (var spot in featureSpots)
            {
                Console.WriteLine($"  - {spot.Name} at {spot.LocationId}");
            }
            
            if (featureSpots.Any())
            {
                // Move to first feature location
                var firstFeature = featureSpots.First();
                var location = locationRepo.GetLocation(firstFeature.LocationId);
                player.CurrentLocationSpot = firstFeature;
                
                Console.WriteLine($"\nMoved to: {firstFeature.Name}");
                
                // Get available actions
                var actions = gameFacade.GetLocationActions();
                var gatherAction = actions.ActionGroups
                    .SelectMany(g => g.Actions)
                    .FirstOrDefault(a => a.Description.Contains("Gather resources"));
                    
                if (gatherAction != null)
                {
                    Console.WriteLine($"✓ Gather resources action available: {gatherAction.Description}");
                    Console.WriteLine($"  ID: {gatherAction.Id}");
                    Console.WriteLine($"  Cost: {gatherAction.StaminaCost} stamina, {gatherAction.TimeCost} hours");
                }
                else
                {
                    Console.WriteLine("✗ No gather resources action found!");
                }
            }
            
            // Test 2: Borrow Money from NPCs
            Console.WriteLine("\n\nTEST 2: Borrow Money from NPCs with Trade/Shadow tokens");
            Console.WriteLine("--------------------------------------------------------");
            
            var connectionTokenManager = scope.ServiceProvider.GetRequiredService<ConnectionTokenManager>();
            
            // Find NPCs that can lend money
            var allNPCs = npcRepo.GetAllNPCs();
            var lendingNPCs = allNPCs.Where(npc => 
                npc.LetterTokenTypes.Contains(ConnectionType.Commerce) ||
                npc.LetterTokenTypes.Contains(ConnectionType.Shadow)).ToList();
                
            Console.WriteLine($"Found {lendingNPCs.Count} NPCs who can lend money:");
            foreach (var npc in lendingNPCs)
            {
                var tokenTypes = string.Join(", ", npc.LetterTokenTypes);
                Console.WriteLine($"  - {npc.Name} ({tokenTypes} tokens)");
            }
            
            // Give player some tokens and check if borrow action appears
            if (lendingNPCs.Any())
            {
                var firstLender = lendingNPCs.First();
                var tokenType = firstLender.LetterTokenTypes.First();
                
                // Add tokens to player
                connectionTokenManager.AddTokens(tokenType, 5, firstLender.ID);
                Console.WriteLine($"\nGave player 5 {tokenType} tokens with {firstLender.Name}");
                
                // Move to NPC's location
                var npcSpot = locationRepo.GetLocationSpot(firstLender.SpotId);
                if (npcSpot != null)
                {
                    player.CurrentLocationSpot = npcSpot;
                    Console.WriteLine($"Moved to: {npcSpot.Name}");
                    
                    // Get available actions
                    var actionsWithNPC = gameFacade.GetLocationActions();
                    var borrowAction = actionsWithNPC.ActionGroups
                        .SelectMany(g => g.Actions)
                        .FirstOrDefault(a => a.Description.Contains("Borrow money"));
                        
                    if (borrowAction != null)
                    {
                        Console.WriteLine($"✓ Borrow money action available: {borrowAction.Description}");
                        Console.WriteLine($"  ID: {borrowAction.Id}");
                        Console.WriteLine($"  Token cost: {borrowAction.TokenCost}");
                    }
                    else
                    {
                        Console.WriteLine("✗ No borrow money action found!");
                    }
                }
            }
            
            // Test 3: Route Discovery
            Console.WriteLine("\n\nTEST 3: Route Discovery");
            Console.WriteLine("-----------------------");
            Console.WriteLine("Note: Route discovery is not yet implemented in CommandDiscoveryService");
            
            Console.WriteLine("\n=== Test Complete ===");
        }
    }
}