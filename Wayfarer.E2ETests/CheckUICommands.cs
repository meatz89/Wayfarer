using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

public class CheckUICommands
{
    public static async Task Main(string[] args)
    {
        try
        {
            var factory = new WebApplicationFactory<Wayfarer.Program>();
            
            using (var scope = factory.Services.CreateScope())
            {
                var gameWorld = scope.ServiceProvider.GetRequiredService<GameWorld>();
                var commandDiscovery = scope.ServiceProvider.GetRequiredService<CommandDiscoveryService>();
                var locationRepo = scope.ServiceProvider.GetRequiredService<LocationRepository>();
                var flagService = scope.ServiceProvider.GetRequiredService<FlagService>();
                var player = gameWorld.GetPlayer();
                
                // Complete tutorial
                flagService.SetFlag(FlagService.TUTORIAL_COMPLETE);
                
                Console.WriteLine("=== Checking UI Commands ===\n");
                
                // Test 1: Check for resource gathering at FEATURE locations
                Console.WriteLine("1. Checking Resource Gathering at FEATURE locations:");
                Console.WriteLine("----------------------------------------------------");
                
                // Move to abandoned warehouse (FEATURE location)
                var warehouse = locationRepo.GetLocationSpot("abandoned_warehouse");
                if (warehouse != null)
                {
                    player.CurrentLocationSpot = warehouse;
                    Console.WriteLine($"Moved to: {warehouse.Name} (Type: {warehouse.Type})");
                    
                    // Discover commands
                    var discovery = commandDiscovery.DiscoverCommands(gameWorld);
                    
                    // Look for gather resources command
                    var gatherCommand = discovery.AllCommands.FirstOrDefault(c => 
                        c.DisplayName.Contains("Gather") || 
                        c.Description.Contains("resource"));
                    
                    if (gatherCommand != null)
                    {
                        Console.WriteLine($"✓ Found gather command: {gatherCommand.DisplayName}");
                        Console.WriteLine($"  Description: {gatherCommand.Description}");
                        Console.WriteLine($"  Available: {gatherCommand.IsAvailable}");
                        Console.WriteLine($"  ID: {gatherCommand.UniqueId}");
                    }
                    else
                    {
                        Console.WriteLine("✗ No gather resources command found!");
                    }
                }
                
                // Test 2: Check for borrow money from NPCs
                Console.WriteLine("\n2. Checking Borrow Money from NPCs:");
                Console.WriteLine("------------------------------------");
                
                var npcRepo = scope.ServiceProvider.GetRequiredService<NPCRepository>();
                var tokenManager = scope.ServiceProvider.GetRequiredService<ConnectionTokenManager>();
                
                // Find shadow banker NPC (has Shadow tokens, can lend money)
                var shadowBanker = npcRepo.GetById("shadow_banker");
                if (shadowBanker != null)
                {
                    // Move to shadow banker location
                    var bankerSpot = locationRepo.GetLocationSpot(shadowBanker.SpotId);
                    if (bankerSpot != null)
                    {
                        player.CurrentLocationSpot = bankerSpot;
                        Console.WriteLine($"Moved to: {bankerSpot.Name} where {shadowBanker.Name} is located");
                        
                        // Give player some shadow tokens
                        tokenManager.AddTokens(ConnectionType.Shadow, 5, shadowBanker.ID);
                        Console.WriteLine($"Added 5 Shadow tokens with {shadowBanker.Name}");
                        
                        // Discover commands
                        var discoveryWithNPC = commandDiscovery.DiscoverCommands(gameWorld);
                        
                        // Look for borrow money command
                        var borrowCommand = discoveryWithNPC.AllCommands.FirstOrDefault(c => 
                            c.DisplayName.Contains("Borrow") || 
                            c.Description.Contains("money"));
                        
                        if (borrowCommand != null)
                        {
                            Console.WriteLine($"✓ Found borrow command: {borrowCommand.DisplayName}");
                            Console.WriteLine($"  Description: {borrowCommand.Description}");
                            Console.WriteLine($"  Available: {borrowCommand.IsAvailable}");
                            Console.WriteLine($"  ID: {borrowCommand.UniqueId}");
                        }
                        else
                        {
                            Console.WriteLine("✗ No borrow money command found!");
                        }
                    }
                }
                
                Console.WriteLine("\n=== Command Discovery Complete ===");
                return 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return 1;
        }
    }
}