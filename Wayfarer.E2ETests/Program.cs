using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Main entry point that routes to different test programs based on command line arguments.
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Check if a specific program is requested
        var programArg = args.FirstOrDefault(a => a.StartsWith("Program="));
        
        if (programArg != null)
        {
            var programName = programArg.Substring(8);
            args = args.Where(a => !a.StartsWith("Program=")).ToArray();
            
            switch (programName)
            {
                case "RunAllE2ETests":
                    return await RunAllE2ETests.Main(args);
                case "ComprehensiveE2ETestSuite":
                    return await ComprehensiveE2ETestSuite.Main(args);
                case "FastE2ETestSuite":
                    return await FastE2ETestSuite.Main(args);
                case "ComprehensiveTutorialTest":
                    return await ComprehensiveTutorialTest.Main(args);
                case "E2ETest":
                    return await E2ETest.Main(args);
                case "FocusedWorkflowTests":
                    return await FocusedWorkflowTests.Main(args);
                default:
                    Console.WriteLine($"Unknown program: {programName}");
                    return 1;
            }
        }
        
        // Default to the comprehensive tutorial test for backward compatibility
        return await ComprehensiveTutorialTest.Main(args);
    }
}