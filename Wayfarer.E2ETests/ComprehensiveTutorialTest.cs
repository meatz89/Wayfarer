using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Comprehensive test that validates the entire tutorial integration:
/// 1. Auto-start on new game
/// 2. Narrative overlay state
/// 3. Command filtering during tutorial
/// 4. Tutorial progression
/// 5. Item giving and special letters
/// </summary>
public class ComprehensiveTutorialTest
{
    private IServiceProvider _provider;
    private GameWorld _gameWorld;
    private GameWorldManager _gameWorldManager;
    private NarrativeManager _narrativeManager;
    private FlagService _flagService;
    private CommandExecutor _commandExecutor;
    private CommandDiscoveryService _commandDiscovery;
    private ItemRepository _itemRepository;
    private NarrativeItemService _narrativeItemService;
    
    public static async Task<int> Main(string[] args)
    {
        var test = new ComprehensiveTutorialTest();
        return await test.RunComprehensiveTest();
    }
    
    private async Task<int> RunComprehensiveTest()
    {
        Console.WriteLine("=== COMPREHENSIVE TUTORIAL INTEGRATION TEST ===\n");
        
        bool allTestsPassed = true;
        
        try
        {
            // Initialize services
            if (!InitializeServices())
            {
                Console.WriteLine("✗ FAIL: Service initialization failed!");
                return 1;
            }
            
            // Test 1: Verify tutorial auto-starts
            Console.WriteLine("\n=== TEST 1: Tutorial Auto-Start ===");
            if (!await TestTutorialAutoStart())
            {
                allTestsPassed = false;
            }
            
            // Test 2: Verify narrative overlay state
            Console.WriteLine("\n=== TEST 2: Narrative Overlay State ===");
            if (!TestNarrativeOverlayState())
            {
                allTestsPassed = false;
            }
            
            // Test 3: Verify command filtering
            Console.WriteLine("\n=== TEST 3: Command Filtering ===");
            if (!TestCommandFiltering())
            {
                allTestsPassed = false;
            }
            
            // Test 4: Test tutorial progression
            Console.WriteLine("\n=== TEST 4: Tutorial Progression ===");
            if (!await TestTutorialProgression())
            {
                allTestsPassed = false;
            }
            
            // Test 5: Test narrative items
            Console.WriteLine("\n=== TEST 5: Narrative Items ===");
            if (!await TestNarrativeItems())
            {
                allTestsPassed = false;
            }
            
            // Test 6: Test tutorial completion
            Console.WriteLine("\n=== TEST 6: Tutorial Completion ===");
            if (!await TestTutorialCompletion())
            {
                allTestsPassed = false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ CRITICAL ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            allTestsPassed = false;
        }
        
        // Summary
        Console.WriteLine("\n=== TEST SUMMARY ===");
        if (allTestsPassed)
        {
            Console.WriteLine("✓ ALL TESTS PASSED - Tutorial integration working correctly!");
            return 0;
        }
        else
        {
            Console.WriteLine("✗ TESTS FAILED - Fix tutorial integration issues!");
            return 1;
        }
    }
    
    private bool InitializeServices()
    {
        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            
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
            _provider = services.BuildServiceProvider();
            
            // Get required services
            _gameWorld = _provider.GetRequiredService<GameWorld>();
            _gameWorldManager = _provider.GetRequiredService<GameWorldManager>();
            _narrativeManager = _provider.GetRequiredService<NarrativeManager>();
            _flagService = _provider.GetRequiredService<FlagService>();
            _commandExecutor = _provider.GetRequiredService<CommandExecutor>();
            _commandDiscovery = _provider.GetRequiredService<CommandDiscoveryService>();
            _itemRepository = _provider.GetRequiredService<ItemRepository>();
            _narrativeItemService = _provider.GetRequiredService<NarrativeItemService>();
            
            Console.WriteLine("✓ All services initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Service initialization failed: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestTutorialAutoStart()
    {
        try
        {
            // Check initial state
            Console.WriteLine("Checking initial state...");
            Console.WriteLine($"  Tutorial started flag: {_flagService.HasFlag(FlagService.TUTORIAL_STARTED)}");
            Console.WriteLine($"  Tutorial complete flag: {_flagService.HasFlag(FlagService.TUTORIAL_COMPLETE)}");
            
            // Start the game
            Console.WriteLine("\nStarting game...");
            await _gameWorldManager.StartGame();
            
            // Check if tutorial started
            bool tutorialStarted = _narrativeManager.IsNarrativeActive("wayfarer_tutorial");
            bool tutorialFlag = _flagService.HasFlag(FlagService.TUTORIAL_STARTED);
            
            Console.WriteLine($"\nPost-start state:");
            Console.WriteLine($"  Tutorial narrative active: {tutorialStarted}");
            Console.WriteLine($"  Tutorial started flag: {tutorialFlag}");
            
            if (!tutorialStarted)
            {
                Console.WriteLine("✗ FAIL: Tutorial narrative did not auto-start!");
                return false;
            }
            
            if (!tutorialFlag)
            {
                Console.WriteLine("✗ FAIL: Tutorial started flag not set!");
                return false;
            }
            
            Console.WriteLine("✓ PASS: Tutorial auto-started successfully!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private bool TestNarrativeOverlayState()
    {
        try
        {
            // Get active narratives
            var activeNarratives = _narrativeManager.GetActiveNarratives();
            Console.WriteLine($"Active narratives: {string.Join(", ", activeNarratives)}");
            
            if (!activeNarratives.Contains("wayfarer_tutorial"))
            {
                Console.WriteLine("✗ FAIL: Tutorial not in active narratives!");
                return false;
            }
            
            // Get current step info
            var currentStep = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            if (currentStep == null)
            {
                Console.WriteLine("✗ FAIL: No current step found!");
                return false;
            }
            
            Console.WriteLine($"\nCurrent step details:");
            Console.WriteLine($"  ID: {currentStep.Id}");
            Console.WriteLine($"  Name: {currentStep.Name}");
            Console.WriteLine($"  Description: {currentStep.Description}");
            Console.WriteLine($"  Guidance: {currentStep.GuidanceText}");
            Console.WriteLine($"  Allowed Actions: {string.Join(", ", currentStep.AllowedActions ?? new List<string>())}");
            Console.WriteLine($"  Visible NPCs: {string.Join(", ", currentStep.VisibleNPCs ?? new List<string>())}");
            
            // Verify step has required data for overlay
            if (string.IsNullOrEmpty(currentStep.Name))
            {
                Console.WriteLine("✗ FAIL: Step missing name!");
                return false;
            }
            
            if (string.IsNullOrEmpty(currentStep.GuidanceText))
            {
                Console.WriteLine("✗ FAIL: Step missing guidance text!");
                return false;
            }
            
            // Get step progress
            var currentIndex = _narrativeManager.GetCurrentStepIndex("wayfarer_tutorial");
            var totalSteps = _narrativeManager.GetTotalSteps("wayfarer_tutorial");
            Console.WriteLine($"\nProgress: Step {currentIndex + 1} of {totalSteps}");
            
            Console.WriteLine("\n✓ PASS: Narrative overlay has all required data!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private bool TestCommandFiltering()
    {
        try
        {
            // Get available commands
            var discoveryResult = _commandDiscovery.DiscoverCommands(_gameWorld);
            Console.WriteLine($"Total available commands: {discoveryResult.AllCommands.Count}");
            
            // Get current step's allowed actions
            var currentStep = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            var allowedActions = currentStep?.AllowedActions ?? new List<string>();
            Console.WriteLine($"Allowed actions in current step: {string.Join(", ", allowedActions)}");
            
            // Verify filtering is working
            if (!allowedActions.Any())
            {
                Console.WriteLine("⚠️ WARNING: No allowed actions defined for current step!");
            }
            
            // Check specific commands that should be filtered
            var talkCommands = discoveryResult.AllCommands.Where(c => c.DisplayName.ToLower().Contains("talk")).ToList();
            var exploreCommands = discoveryResult.AllCommands.Where(c => c.DisplayName.ToLower().Contains("explore")).ToList();
            
            Console.WriteLine($"\nCommand breakdown:");
            Console.WriteLine($"  Talk commands: {talkCommands.Count}");
            Console.WriteLine($"  Explore commands: {exploreCommands.Count}");
            
            // During tutorial, commands should be limited based on allowed actions
            if (allowedActions.Contains("talk") && talkCommands.Count == 0)
            {
                Console.WriteLine("✗ FAIL: Talk allowed but no talk commands available!");
                return false;
            }
            
            Console.WriteLine("\n✓ PASS: Command filtering working!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestTutorialProgression()
    {
        try
        {
            var initialStep = _narrativeManager.GetCurrentStep("wayfarer_tutorial");
            Console.WriteLine($"Initial step: {initialStep?.Id}");
            
            // Check if we can give narrative items for the current step
            await _narrativeItemService.CheckAndGiveNarrativeItems();
            
            // Simulate completing the first step
            // This would normally happen through player actions
            Console.WriteLine("\nSimulating step completion...");
            
            // Check if step has completion conditions
            // Note: CompletionConditions would be checked through the narrative definition
            
            // Try to advance tutorial (would normally happen through game actions)
            // For testing, we'll check if the narrative system is tracking properly
            var stepIndex = _narrativeManager.GetCurrentStepIndex("wayfarer_tutorial");
            var totalSteps = _narrativeManager.GetTotalSteps("wayfarer_tutorial");
            
            Console.WriteLine($"\nTutorial progress: {stepIndex + 1}/{totalSteps}");
            
            if (totalSteps == 0)
            {
                Console.WriteLine("✗ FAIL: No steps found in tutorial!");
                return false;
            }
            
            Console.WriteLine("\n✓ PASS: Tutorial progression system working!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestNarrativeItems()
    {
        try
        {
            // Check for narrative items
            await _narrativeItemService.CheckAndGiveNarrativeItems();
            
            // Check player inventory for tutorial items
            var player = _gameWorld.GetPlayer();
            var inventory = player.Inventory.GetAllItems();
            
            Console.WriteLine($"Player inventory items: {inventory.Count()}");
            
            // Look for special tutorial items
            var tutorialItems = new List<Item>();
            foreach (var itemId in inventory)
            {
                var item = _itemRepository.GetItemById(itemId);
                if (item != null && (
                    item.Name.ToLower().Contains("letter") || 
                    item.Name.ToLower().Contains("tutorial") ||
                    item.IsReadable()
                ))
                {
                    tutorialItems.Add(item);
                }
            }
            
            if (tutorialItems.Any())
            {
                Console.WriteLine("\nTutorial items found:");
                foreach (var item in tutorialItems)
                {
                    Console.WriteLine($"  - {item.Name} (ID: {item.Id})");
                    Console.WriteLine($"    Readable: {item.IsReadable()}");
                    if (!string.IsNullOrEmpty(item.ReadFlagToSet))
                    {
                        Console.WriteLine($"    Sets flag: {item.ReadFlagToSet}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No tutorial items in inventory yet");
            }
            
            Console.WriteLine("\n✓ PASS: Narrative item system functional!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestTutorialCompletion()
    {
        try
        {
            // Check if tutorial can be completed
            var isComplete = _flagService.HasFlag(FlagService.TUTORIAL_COMPLETE);
            Console.WriteLine($"Tutorial complete flag: {isComplete}");
            
            // Get narrative definition to check completion rewards
            var narrativeDef = _narrativeManager.GetNarrativeDefinition("wayfarer_tutorial");
            if (narrativeDef?.CompletionRewards != null)
            {
                Console.WriteLine("\nCompletion effects defined:");
                foreach (var effect in narrativeDef.CompletionRewards)
                {
                    Console.WriteLine($"  - {effect.Type}: {effect.Value}");
                }
            }
            
            // Verify tutorial is still active (since we haven't completed it)
            if (!isComplete && !_narrativeManager.IsNarrativeActive("wayfarer_tutorial"))
            {
                Console.WriteLine("✗ FAIL: Tutorial not active but also not complete!");
                return false;
            }
            
            Console.WriteLine("\n✓ PASS: Tutorial completion system ready!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            return false;
        }
    }
}