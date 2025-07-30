using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class TestThreeCommands
{
    public static async Task RunTest()
    {
        Console.WriteLine("=== TESTING GATHERRESOURCES, BORROWMONEY, AND BROWSE COMMANDS ===\n");

        // Create service collection and configure services
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
        
        // Add IConfiguration
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "useMemory", "false" },
                { "processStateChanges", "true" },
                { "DefaultAIProvider", "Ollama" }
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        
        services.ConfigureServices();

        var serviceProvider = services.BuildServiceProvider();

        try
        {
            // Get required services
            var gameWorld = serviceProvider.GetRequiredService<GameWorld>();
            var commandDiscovery = serviceProvider.GetRequiredService<CommandDiscoveryService>();
            var locationActionsService = serviceProvider.GetRequiredService<LocationActionsUIService>();
            var timeManager = serviceProvider.GetRequiredService<ITimeManager>();
            var locationRepository = serviceProvider.GetRequiredService<LocationRepository>();
            var spotRepository = serviceProvider.GetRequiredService<LocationSpotRepository>();
            var npcRepository = serviceProvider.GetRequiredService<NPCRepository>();
            
            var player = gameWorld.GetPlayer();
            
            Console.WriteLine($"Initial Location: {player.CurrentLocationSpot?.Name ?? "Unknown"}");
            Console.WriteLine($"Initial Time: {timeManager.GetCurrentTimeBlock()}\n");
            
            // Test 1: Find a FEATURE location for GatherResourcesCommand
            Console.WriteLine("=== TEST 1: GATHER RESOURCES COMMAND ===");
            
            var allSpots = spotRepository.GetAllLocationSpots();
            var featureSpots = allSpots.Where(s => s.Type == LocationSpotTypes.FEATURE).ToList();
            
            Console.WriteLine($"Found {featureSpots.Count} FEATURE locations:");
            foreach (var spot in featureSpots)
            {
                Console.WriteLine($"  - {spot.Name} ({spot.SpotID}) at {spot.LocationId}");
            }
            
            if (featureSpots.Any())
            {
                // Move to a FEATURE spot
                var targetSpot = featureSpots.First();
                var targetLocation = locationRepository.GetLocationById(targetSpot.LocationId);
                
                Console.WriteLine($"\nMoving to {targetSpot.Name} at {targetLocation.Name}...");
                locationRepository.SetCurrentLocation(targetLocation);
                player.CurrentLocationSpot = targetSpot;
                
                // Discover commands
                var discovery = commandDiscovery.DiscoverCommands(gameWorld);
                var gatherCommand = discovery.AllCommands.FirstOrDefault(c => c.Command is GatherResourcesCommand);
                
                if (gatherCommand != null)
                {
                    Console.WriteLine("✅ GatherResourcesCommand FOUND in CommandDiscoveryService");
                    Console.WriteLine($"   Display Name: {gatherCommand.DisplayName}");
                    Console.WriteLine($"   Available: {gatherCommand.IsAvailable}");
                    Console.WriteLine($"   Category: {gatherCommand.Category}");
                    
                    // Check UI exposure
                    var viewModel = locationActionsService.GetLocationActionsViewModel();
                    var gatherAction = viewModel.ActionGroups
                        .SelectMany(g => g.Actions)
                        .FirstOrDefault(a => a.Description.Contains("Gather"));
                    
                    if (gatherAction != null)
                    {
                        Console.WriteLine("✅ Gather Resources EXPOSED in UI");
                        Console.WriteLine($"   Action ID: {gatherAction.Id}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Gather Resources NOT exposed in UI");
                    }
                }
                else
                {
                    Console.WriteLine("❌ GatherResourcesCommand NOT found");
                }
            }
            
            // Test 2: Find NPCs that offer loans for BorrowMoneyCommand
            Console.WriteLine("\n=== TEST 2: BORROW MONEY COMMAND ===");
            
            var allNPCs = npcRepository.GetAllNPCs();
            var lenderNPCs = allNPCs.Where(npc => 
                npc.LetterTokenTypes.Contains(ConnectionType.Trade) || 
                npc.LetterTokenTypes.Contains(ConnectionType.Shadow)
            ).ToList();
            
            Console.WriteLine($"Found {lenderNPCs.Count} NPCs that can lend money:");
            foreach (var npc in lenderNPCs.Take(5))
            {
                Console.WriteLine($"  - {npc.Name} ({npc.ID}) - Tokens: {string.Join(", ", npc.LetterTokenTypes)}");
            }
            
            if (lenderNPCs.Any())
            {
                // Find a lender NPC that's available now
                var currentTime = timeManager.GetCurrentTimeBlock();
                var availableLender = lenderNPCs.FirstOrDefault(npc => 
                    npc.IsAvailable(currentTime) && npc.SpotId != null
                );
                
                if (availableLender != null)
                {
                    var lenderSpot = spotRepository.GetLocationSpotById(availableLender.SpotId);
                    var lenderLocation = locationRepository.GetLocationById(lenderSpot.LocationId);
                    
                    Console.WriteLine($"\nMoving to {availableLender.Name} at {lenderSpot.Name}...");
                    locationRepository.SetCurrentLocation(lenderLocation);
                    player.CurrentLocationSpot = lenderSpot;
                    
                    // Discover commands
                    var discovery = commandDiscovery.DiscoverCommands(gameWorld);
                    var borrowCommand = discovery.AllCommands.FirstOrDefault(c => c.Command is BorrowMoneyCommand);
                    
                    if (borrowCommand != null)
                    {
                        Console.WriteLine("✅ BorrowMoneyCommand FOUND in CommandDiscoveryService");
                        Console.WriteLine($"   Display Name: {borrowCommand.DisplayName}");
                        Console.WriteLine($"   Available: {borrowCommand.IsAvailable}");
                        Console.WriteLine($"   Category: {borrowCommand.Category}");
                        if (!borrowCommand.IsAvailable)
                        {
                            Console.WriteLine($"   Reason: {borrowCommand.UnavailableReason}");
                        }
                        
                        // Check UI exposure
                        var viewModel = locationActionsService.GetLocationActionsViewModel();
                        var borrowAction = viewModel.ActionGroups
                            .SelectMany(g => g.Actions)
                            .FirstOrDefault(a => a.Description.Contains("Borrow money"));
                        
                        if (borrowAction != null)
                        {
                            Console.WriteLine("✅ Borrow Money EXPOSED in UI");
                            Console.WriteLine($"   Action ID: {borrowAction.Id}");
                        }
                        else
                        {
                            Console.WriteLine("❌ Borrow Money NOT exposed in UI");
                        }
                    }
                    else
                    {
                        Console.WriteLine("❌ BorrowMoneyCommand NOT found");
                    }
                }
                else
                {
                    Console.WriteLine("No lender NPCs available at current time");
                }
            }
            
            // Test 3: Find a location with a market for BrowseCommand
            Console.WriteLine("\n=== TEST 3: BROWSE COMMAND ===");
            
            // Markets are available during day time
            if (timeManager.GetCurrentTimeBlock() == TimeBlocks.Night)
            {
                Console.WriteLine("Advancing time to day for market availability...");
                while (timeManager.GetCurrentTimeBlock() == TimeBlocks.Night)
                {
                    timeManager.AdvanceTime(1);
                }
            }
            
            // Try different locations to find a market
            var locations = locationRepository.GetAllLocations();
            foreach (var location in locations)
            {
                locationRepository.SetCurrentLocation(location);
                var spots = spotRepository.GetAllLocationSpots()
                    .Where(s => s.LocationId == location.Id)
                    .ToList();
                
                if (spots.Any())
                {
                    player.CurrentLocationSpot = spots.First();
                    
                    // Discover commands
                    var discovery = commandDiscovery.DiscoverCommands(gameWorld);
                    var browseCommand = discovery.AllCommands.FirstOrDefault(c => c.Command is BrowseCommand);
                    
                    if (browseCommand != null)
                    {
                        Console.WriteLine($"\nFound market at {location.Name}!");
                        Console.WriteLine("✅ BrowseCommand FOUND in CommandDiscoveryService");
                        Console.WriteLine($"   Display Name: {browseCommand.DisplayName}");
                        Console.WriteLine($"   Available: {browseCommand.IsAvailable}");
                        Console.WriteLine($"   Category: {browseCommand.Category}");
                        
                        // Check UI exposure
                        var viewModel = locationActionsService.GetLocationActionsViewModel();
                        var browseAction = viewModel.ActionGroups
                            .SelectMany(g => g.Actions)
                            .FirstOrDefault(a => a.Description.Contains("Browse"));
                        
                        if (browseAction != null)
                        {
                            Console.WriteLine("✅ Browse Market EXPOSED in UI");
                            Console.WriteLine($"   Action ID: {browseAction.Id}");
                        }
                        else
                        {
                            Console.WriteLine("❌ Browse Market NOT exposed in UI");
                        }
                        break;
                    }
                }
            }
            
            // Summary
            Console.WriteLine("\n=== SUMMARY ===");
            
            // Do a final check at current location
            var finalDiscovery = commandDiscovery.DiscoverCommands(gameWorld);
            var finalViewModel = locationActionsService.GetLocationActionsViewModel();
            
            Console.WriteLine($"\nTotal commands discovered: {finalDiscovery.AllCommands.Count}");
            Console.WriteLine($"Available commands: {finalDiscovery.AvailableCommands.Count}");
            
            Console.WriteLine("\nCommands by category:");
            foreach (var categoryGroup in finalDiscovery.CommandsByCategory)
            {
                Console.WriteLine($"  {categoryGroup.Key}: {categoryGroup.Value.Count} commands");
                foreach (var cmd in categoryGroup.Value.Take(3))
                {
                    Console.WriteLine($"    - {cmd.DisplayName}");
                }
            }
            
            Console.WriteLine("\nUI Action Groups:");
            foreach (var group in finalViewModel.ActionGroups)
            {
                Console.WriteLine($"  {group.ActionType}: {group.Actions.Count} actions");
            }
            
            Console.WriteLine("\n✅ Test completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test failed with error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }
}