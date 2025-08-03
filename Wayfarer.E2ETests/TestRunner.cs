using System;
using System.Threading.Tasks;
using Wayfarer.E2ETests.Tests;

/// <summary>
/// Simple test runner for E2E tests.
/// </summary>
public class TestRunner
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Wayfarer E2E Test Suite");
        Console.WriteLine("=======================");
        Console.WriteLine();
        
        var results = new TestResults();
        
        // Run Tutorial Tests
        await RunTest("Tutorial_CompleteFlow_SuccessfullyCompletesTourial", 
            async () => 
            {
                using var test = new TutorialE2ETests();
                await test.Tutorial_CompleteFlow_SuccessfullyCompletesTourial();
            },
            results);
            
        await RunTest("Tutorial_CanDeclineTamLetter_StillInTutorial", 
            async () => 
            {
                using var test = new TutorialE2ETests();
                await test.Tutorial_CanDeclineTamLetter_StillInTutorial();
            },
            results);
        
        // Summary
        Console.WriteLine("\n================================");
        Console.WriteLine($"Test Results: {results.Passed} passed, {results.Failed} failed");
        Console.WriteLine("================================");
        
        Environment.Exit(results.Failed > 0 ? 1 : 0);
    }
    
    private static async Task RunTest(string testName, Func<Task> testAction, TestResults results)
    {
        Console.WriteLine($"\nRunning: {testName}");
        Console.WriteLine(new string('-', testName.Length + 9));
        
        try
        {
            await testAction();
            results.Passed++;
            Console.WriteLine($"\n✅ PASSED: {testName}");
        }
        catch (Exception ex)
        {
            results.Failed++;
            Console.WriteLine($"\n❌ FAILED: {testName}");
            Console.WriteLine($"   Error: {ex.Message}");
            if (ex.StackTrace != null)
            {
                Console.WriteLine($"   Stack: {ex.StackTrace.Split('\n')[0]}");
            }
        }
    }
    
    private class TestResults
    {
        public int Passed { get; set; }
        public int Failed { get; set; }
    }
}