using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Wayfarer.GameState.Constants;

/// <summary>
/// SINGLE E2E TEST THAT CATCHES ALL STARTUP AND RUNTIME ISSUES
/// Run this before deploying to catch issues like Player initialization errors
/// </summary>
public class E2ETest
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== WAYFARER E2E TEST ===");
        Console.WriteLine("This test catches ALL issues you see when starting the game\n");

        bool allTestsPassed = true;

        // TEST 1: Can we create GameWorld without errors?
        Console.WriteLine("TEST 1: GameWorld Creation");
        try
        {
            ServiceCollection services = new ServiceCollection();
            services.AddLogging(); // Add logging services
            
            // Add IConfiguration (required by GameWorldManager)
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
            ServiceProvider provider = services.BuildServiceProvider();
            GameWorld gameWorld = provider.GetRequiredService<GameWorld>();

            // Check player initialization
            Player player = gameWorld.GetPlayer();
            if (player.CurrentLocation == null || player.CurrentLocationSpot == null)
            {
                Console.WriteLine("✗ FAIL: Player location not initialized - THIS IS YOUR CURRENT ERROR!");
                allTestsPassed = false;
            }
            else
            {
                Console.WriteLine($"✓ PASS: Player at {player.CurrentLocation.Id}/{player.CurrentLocationSpot.SpotID}");
                
                // TEST: Simulate game start flow (what happens when "Begin Journey" is clicked)
                try
                {
                    GameWorldManager gameWorldManager = provider.GetRequiredService<GameWorldManager>();
                    LocationRepository locationRepo = provider.GetRequiredService<LocationRepository>();
                    
                    // Check location state (should be single source of truth in Player)
                    Console.WriteLine("\nVerifying location state (single source of truth):");
                    Console.WriteLine($"  Player.CurrentLocation: {player.CurrentLocation?.Id ?? "NULL"}");
                    Console.WriteLine($"  Player.CurrentLocationSpot: {player.CurrentLocationSpot?.SpotID ?? "NULL"}");
                    Console.WriteLine($"  LocationRepository.GetCurrentLocation: {locationRepo.GetCurrentLocation()?.Id ?? "NULL"}");
                    
                    if (player.CurrentLocation == null)
                    {
                        Console.WriteLine("✗ FAIL: Player.CurrentLocation is null!");
                        Console.WriteLine("  This should never happen after initialization");
                        allTestsPassed = false;
                    }
                    
                    // Verify LocationRepository returns player's location
                    if (locationRepo.GetCurrentLocation() != player.CurrentLocation)
                    {
                        Console.WriteLine("✗ FAIL: LocationRepository doesn't return Player.CurrentLocation!");
                        allTestsPassed = false;
                    }
                    
                    // Verify LocationRepository can get current location
                    Location currentLoc = locationRepo.GetCurrentLocation();
                    if (currentLoc == null)
                    {
                        Console.WriteLine("✗ FAIL: LocationRepository.GetCurrentLocation() returns null!");
                        Console.WriteLine("  This will crash when player clicks 'Begin Journey'");
                        allTestsPassed = false;
                    }
                    else
                    {
                        Console.WriteLine($"✓ PASS: LocationRepository current location: {currentLoc.Id}");
                    }
                    
                    // Test the actual StartGame method that crashes
                    Console.WriteLine("\nTesting GameWorldManager.StartGame()...");
                    await gameWorldManager.StartGame();
                    Console.WriteLine("✓ PASS: Game started successfully");
                    
                    // Test tutorial auto-start
                    Console.WriteLine("\nChecking tutorial auto-start...");
                    var narrativeManager = provider.GetRequiredService<NarrativeManager>();
                    var flagService = provider.GetRequiredService<FlagService>();
                    
                    if (narrativeManager.IsNarrativeActive("wayfarer_tutorial"))
                    {
                        Console.WriteLine("✓ PASS: Tutorial auto-started successfully");
                        var currentStep = narrativeManager.GetCurrentStep("wayfarer_tutorial");
                        if (currentStep != null)
                        {
                            Console.WriteLine($"  Current step: {currentStep.Name} ({currentStep.Id})");
                        }
                    }
                    else
                    {
                        Console.WriteLine("✗ FAIL: Tutorial did not auto-start!");
                        Console.WriteLine($"  Tutorial started flag: {flagService.HasFlag(FlagService.TUTORIAL_STARTED)}");
                        Console.WriteLine($"  Tutorial complete flag: {flagService.HasFlag(FlagService.TUTORIAL_COMPLETE)}");
                        allTestsPassed = false;
                    }
                }
                catch (NullReferenceException nre)
                {
                    Console.WriteLine($"✗ FAIL: NullReferenceException during game start: {nre.Message}");
                    Console.WriteLine("  This is the exact error you're seeing!");
                    Console.WriteLine($"  Stack trace: {nre.StackTrace}");
                    allTestsPassed = false;
                }
                catch (Exception gameStartEx)
                {
                    Console.WriteLine($"✗ FAIL: Error during game start: {gameStartEx.Message}");
                    allTestsPassed = false;
                }
                
                // Check for content validation errors
                if (File.Exists("content_validation_errors.log"))
                {
                    Console.WriteLine("\n⚠️ WARNING: Content validation errors detected!");
                    var errors = File.ReadAllLines("content_validation_errors.log");
                    Console.WriteLine($"⚠️ {errors[0]}"); // First line has count
                    Console.WriteLine("\nMissing content references:");
                    for (int i = 1; i < Math.Min(errors.Length, GameConstants.UI.MAX_ERROR_DISPLAY_COUNT + 1); i++) // Show first 10
                    {
                        Console.WriteLine($"  {errors[i]}");
                    }
                    if (errors.Length > 11)
                    {
                        Console.WriteLine($"  ... and {errors.Length - 11} more errors");
                    }
                    Console.WriteLine("\n⚠️ FIX YOUR JSON CONTENT DEFINITIONS!");
                    allTestsPassed = false;
                    
                    // Clean up error file after reporting
                    File.Delete("content_validation_errors.log");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"  Inner: {ex.InnerException.Message}");
            allTestsPassed = false;
        }

        // TEST 2: Can the web server start and serve pages?
        Console.WriteLine("\nTEST 2: Web Server Startup");
        Process serverProcess = null;
        try
        {
            serverProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --urls http://localhost:5013",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            // Capture any startup errors
            bool startupError = false;
            serverProcess.ErrorDataReceived += (s, e) =>
            {
                if (e.Data?.Contains("Exception") == true)
                {
                    Console.WriteLine($"✗ STARTUP ERROR: {e.Data}");
                    startupError = true;
                }
            };
            serverProcess.BeginErrorReadLine();

            // Wait for server to start
            await Task.Delay(5000);

            if (startupError)
            {
                allTestsPassed = false;
            }
            else
            {
                // Try to access the home page
                using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(GameConstants.Network.HTTP_CLIENT_TIMEOUT_SECONDS) })
                {
                    HttpResponseMessage response = await client.GetAsync("http://localhost:5013");
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("✓ PASS: Server responds to requests");
                    }
                    else
                    {
                        Console.WriteLine($"✗ FAIL: Server returned {response.StatusCode}");
                        allTestsPassed = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: Could not start server - {ex.Message}");
            allTestsPassed = false;
        }
        finally
        {
            serverProcess?.Kill();
            serverProcess?.Dispose();
        }

        // TEST 3: Check critical game services
        Console.WriteLine("\nTEST 3: Critical Services");
        try
        {
            ServiceCollection services = new ServiceCollection();
            services.AddLogging(); // Add logging services
            
            // Add IConfiguration (required by GameWorldManager)
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
            ServiceProvider provider = services.BuildServiceProvider();

            // Check all critical services can be created
            Type[] criticalServices = new Type[] {
                typeof(GameWorldManager),
                typeof(LocationRepository),
                typeof(NPCRepository),
                typeof(LetterQueueManager),
                typeof(ITimeManager)
            };

            foreach (Type serviceType in criticalServices)
            {
                try
                {
                    object service = provider.GetRequiredService(serviceType);
                    Console.WriteLine($"✓ PASS: {serviceType.Name} created");
                }
                catch
                {
                    Console.WriteLine($"✗ FAIL: Could not create {serviceType.Name}");
                    allTestsPassed = false;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: Service creation failed - {ex.Message}");
            allTestsPassed = false;
        }

        // Check for error log one more time in case it was created during other tests
        if (File.Exists("content_validation_errors.log"))
        {
            Console.WriteLine("\n⚠️ CRITICAL: Content validation error log detected!");
            allTestsPassed = false;
            File.Delete("content_validation_errors.log");
        }
        
        // FINAL RESULT
        Console.WriteLine("\n=== TEST SUMMARY ===");
        if (allTestsPassed)
        {
            Console.WriteLine("✓ ALL TESTS PASSED - Game should start without errors");
            return 0;
        }
        else
        {
            Console.WriteLine("✗ TESTS FAILED - Fix issues before running game");
            Console.WriteLine("\nPossible issues:");
            Console.WriteLine("1. Content validation errors (missing references in JSON)");
            Console.WriteLine("2. Player initialization failed");
            Console.WriteLine("3. Service creation errors");
            Console.WriteLine("\nCheck the detailed error messages above.");
            return 1;
        }
    }
}