using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Master test runner that executes all E2E test suites and provides a consolidated report.
/// </summary>
public class RunAllE2ETests
{
    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== WAYFARER E2E TEST RUNNER ===");
        Console.WriteLine($"Starting at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
        
        var overallResults = new List<(string suite, bool passed, TimeSpan duration, string details)>();
        var overallStopwatch = Stopwatch.StartNew();
        bool allPassed = true;
        
        // Determine which tests to run based on arguments
        bool runFast = args.Length == 0 || args.Contains("--fast");
        bool runComprehensive = args.Length == 0 || args.Contains("--comprehensive");
        bool runHttp = args.Contains("--http");
        bool runTutorial = args.Length == 0 || args.Contains("--tutorial");
        bool runQuick = args.Contains("--quick");
        
        if (args.Contains("--help"))
        {
            PrintHelp();
            return 0;
        }
        
        try
        {
            // 1. Quick validation test (always run first)
            if (runQuick || args.Length == 0)
            {
                Console.WriteLine("=== QUICK VALIDATION TEST ===");
                var result = await RunTestSuite("Quick Validation", async () =>
                {
                    var test = new E2ETest();
                    return await E2ETest.Main(new string[0]) == 0;
                });
                overallResults.Add(result);
                if (!result.passed) allPassed = false;
                
                if (!result.passed)
                {
                    Console.WriteLine("\n✗ Quick validation failed - stopping other tests");
                    PrintSummary(overallResults, overallStopwatch.Elapsed);
                    return 1;
                }
            }
            
            // 2. Fast E2E test suite (in-process, no HTTP)
            if (runFast)
            {
                Console.WriteLine("\n=== FAST E2E TEST SUITE ===");
                var result = await RunTestSuite("Fast E2E Suite", async () =>
                {
                    var suite = new FastE2ETestSuite();
                    return await FastE2ETestSuite.Main(new string[0]) == 0;
                });
                overallResults.Add(result);
                if (!result.passed) allPassed = false;
            }
            
            // 3. Tutorial integration test
            if (runTutorial)
            {
                Console.WriteLine("\n=== TUTORIAL INTEGRATION TEST ===");
                var result = await RunTestSuite("Tutorial Integration", async () =>
                {
                    var test = new ComprehensiveTutorialTest();
                    return await ComprehensiveTutorialTest.Main(new string[0]) == 0;
                });
                overallResults.Add(result);
                if (!result.passed) allPassed = false;
            }
            
            // 4. Comprehensive HTTP test suite (slower, tests actual HTTP endpoints)
            if (runHttp)
            {
                Console.WriteLine("\n=== COMPREHENSIVE HTTP TEST SUITE ===");
                Console.WriteLine("Note: This suite starts a real server and tests HTTP endpoints");
                var result = await RunTestSuite("Comprehensive HTTP Suite", async () =>
                {
                    var suite = new ComprehensiveE2ETestSuite();
                    return await ComprehensiveE2ETestSuite.Main(new string[0]) == 0;
                });
                overallResults.Add(result);
                if (!result.passed) allPassed = false;
            }
            
            // 5. Service availability test
            if (runComprehensive)
            {
                Console.WriteLine("\n=== SERVICE AVAILABILITY TEST ===");
                var result = await RunTestSuite("Service Availability", async () =>
                {
                    ServiceAvailabilityTest.RunServiceTest();
                    return true; // This test prints results but doesn't return pass/fail
                });
                overallResults.Add(result);
            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ CRITICAL ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            allPassed = false;
        }
        
        overallStopwatch.Stop();
        
        // Print final summary
        PrintSummary(overallResults, overallStopwatch.Elapsed);
        
        return allPassed ? 0 : 1;
    }
    
    private static async Task<(string suite, bool passed, TimeSpan duration, string details)> RunTestSuite(
        string suiteName, Func<Task<bool>> testFunc)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            bool passed = await testFunc();
            stopwatch.Stop();
            
            if (passed)
            {
                Console.WriteLine($"\n✓ {suiteName} PASSED ({stopwatch.ElapsedMilliseconds}ms)");
                return (suiteName, true, stopwatch.Elapsed, "All tests passed");
            }
            else
            {
                Console.WriteLine($"\n✗ {suiteName} FAILED ({stopwatch.ElapsedMilliseconds}ms)");
                return (suiteName, false, stopwatch.Elapsed, "One or more tests failed");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"\n✗ {suiteName} CRASHED: {ex.Message}");
            return (suiteName, false, stopwatch.Elapsed, $"Exception: {ex.Message}");
        }
    }
    
    private static void PrintSummary(List<(string suite, bool passed, TimeSpan duration, string details)> results, 
        TimeSpan totalDuration)
    {
        Console.WriteLine("\n");
        Console.WriteLine("=" + new string('=', 50));
        Console.WriteLine("=== OVERALL TEST SUMMARY ===");
        Console.WriteLine("=" + new string('=', 50));
        
        Console.WriteLine($"\nTotal test suites: {results.Count}");
        Console.WriteLine($"Passed: {results.Count(r => r.passed)}");
        Console.WriteLine($"Failed: {results.Count(r => !r.passed)}");
        Console.WriteLine($"Total duration: {totalDuration.TotalSeconds:F1} seconds");
        
        Console.WriteLine("\nResults by suite:");
        foreach (var (suite, passed, duration, details) in results)
        {
            var status = passed ? "✓ PASS" : "✗ FAIL";
            Console.WriteLine($"  {status} {suite} ({duration.TotalMilliseconds:F0}ms)");
            if (!passed)
            {
                Console.WriteLine($"       {details}");
            }
        }
        
        var allPassed = results.All(r => r.passed);
        Console.WriteLine("\n" + new string('=', 50));
        Console.WriteLine(allPassed 
            ? "✓ ALL TEST SUITES PASSED" 
            : "✗ TEST FAILURES DETECTED");
        Console.WriteLine(new string('=', 50));
        
        if (!allPassed)
        {
            Console.WriteLine("\nTo run specific test suites:");
            Console.WriteLine("  dotnet run -- --fast              # Run only fast tests");
            Console.WriteLine("  dotnet run -- --http              # Run HTTP endpoint tests");
            Console.WriteLine("  dotnet run -- --tutorial          # Run tutorial tests");
            Console.WriteLine("  dotnet run -- --quick             # Run quick validation only");
            Console.WriteLine("  dotnet run -- --comprehensive     # Run all comprehensive tests");
        }
    }
    
    private static void PrintHelp()
    {
        Console.WriteLine("Wayfarer E2E Test Runner");
        Console.WriteLine("\nUsage: dotnet run [options]");
        Console.WriteLine("\nOptions:");
        Console.WriteLine("  --fast           Run fast in-process tests (default)");
        Console.WriteLine("  --http           Run HTTP endpoint tests");
        Console.WriteLine("  --tutorial       Run tutorial integration tests (default)");
        Console.WriteLine("  --quick          Run quick validation only");
        Console.WriteLine("  --comprehensive  Run all comprehensive tests");
        Console.WriteLine("  --help           Show this help message");
        Console.WriteLine("\nExamples:");
        Console.WriteLine("  dotnet run                    # Run default test set (fast + tutorial)");
        Console.WriteLine("  dotnet run -- --quick         # Quick validation only");
        Console.WriteLine("  dotnet run -- --http          # Run HTTP tests only");
        Console.WriteLine("  dotnet run -- --fast --http   # Run both fast and HTTP tests");
    }
}