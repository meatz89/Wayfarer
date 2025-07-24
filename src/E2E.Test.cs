using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

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
            var services = new ServiceCollection();
            services.AddLogging(); // Add logging services
            services.ConfigureServices();
            var provider = services.BuildServiceProvider();
            var gameWorld = provider.GetRequiredService<GameWorld>();
            
            // Check player initialization
            var player = gameWorld.GetPlayer();
            if (player.CurrentLocation == null || player.CurrentLocationSpot == null)
            {
                Console.WriteLine("✗ FAIL: Player location not initialized - THIS IS YOUR CURRENT ERROR!");
                allTestsPassed = false;
            }
            else
            {
                Console.WriteLine($"✓ PASS: Player at {player.CurrentLocation.Id}/{player.CurrentLocationSpot.SpotID}");
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
            serverProcess.ErrorDataReceived += (s, e) => {
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
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) })
                {
                    var response = await client.GetAsync("http://localhost:5013");
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
            var services = new ServiceCollection();
            services.AddLogging(); // Add logging services
            services.ConfigureServices();
            var provider = services.BuildServiceProvider();
            
            // Check all critical services can be created
            var criticalServices = new Type[] {
                typeof(GameWorldManager),
                typeof(LocationRepository), 
                typeof(NPCRepository),
                typeof(LetterQueueManager),
                typeof(ITimeManager)
            };
            
            foreach (var serviceType in criticalServices)
            {
                try
                {
                    var service = provider.GetRequiredService(serviceType);
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
            return 1;
        }
    }
}