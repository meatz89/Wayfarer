using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public class TestRunner
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=== Wayfarer E2E Test Runner ===");
        Console.WriteLine($"Starting at: {DateTime.Now}");
        Console.WriteLine();

        var testOutputHelper = new ConsoleTestOutputHelper();
        var tutorialTests = new Wayfarer.E2ETests.TutorialE2ETests(testOutputHelper);
        
        int passedTests = 0;
        int failedTests = 0;

        // Get all test methods
        var testMethods = typeof(Wayfarer.E2ETests.TutorialE2ETests)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<FactAttribute>() != null)
            .ToList();

        Console.WriteLine($"Found {testMethods.Count} tests to run\n");

        foreach (var method in testMethods)
        {
            Console.WriteLine($"Running: {method.Name}");
            try
            {
                var result = method.Invoke(tutorialTests, null);
                if (result is Task task)
                {
                    await task;
                }
                Console.WriteLine($"✓ PASSED: {method.Name}");
                passedTests++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ FAILED: {method.Name}");
                Console.WriteLine($"  Error: {ex.InnerException?.Message ?? ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  Stack: {ex.InnerException.StackTrace}");
                }
                failedTests++;
            }
            Console.WriteLine();
        }

        // Dispose the test class
        tutorialTests.Dispose();

        // Summary
        Console.WriteLine("=== Test Summary ===");
        Console.WriteLine($"Total tests: {passedTests + failedTests}");
        Console.WriteLine($"Passed: {passedTests}");
        Console.WriteLine($"Failed: {failedTests}");
        Console.WriteLine($"Completed at: {DateTime.Now}");

        if (failedTests > 0)
        {
            Console.WriteLine("\n❌ TESTS FAILED");
            Environment.Exit(1);
        }
        else
        {
            Console.WriteLine("\n✅ ALL TESTS PASSED");
        }
    }
}

// Simple console output helper for xUnit
public class ConsoleTestOutputHelper : ITestOutputHelper
{
    public void WriteLine(string message)
    {
        Console.WriteLine($"  {message}");
    }

    public void WriteLine(string format, params object[] args)
    {
        Console.WriteLine($"  {string.Format(format, args)}");
    }
}