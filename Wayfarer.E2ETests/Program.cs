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
                case "TutorialConversationDebugTest":
                    return await TutorialConversationDebugTest.Main(args);
                case "ConversationFlowTest":
                    await Wayfarer.E2ETests.ConversationFlowTest.RunTest();
                    return 0;
                case "SimpleConversationTest":
                    return await SimpleConversationTest.Main(args);
                case "ManualConversationFlowTest":
                    return await ManualConversationFlowTest.Main(args);
                case "DebugTamConversation":
                    return await DebugTamConversation.Main(args);
                case "TestMissingUIFeatures":
                    return await TestMissingUIFeatures.Main(args);
                case "CheckUICommands":
                    return await CheckUICommands.Main(args);
                case "TestThreeCommands":
                    await TestThreeCommands.RunTest();
                    return 0;
                default:
                    Console.WriteLine($"Unknown program: {programName}");
                    return 1;
            }
        }
        
        // Default to the comprehensive tutorial test for backward compatibility
        return await ComprehensiveTutorialTest.Main(args);
    }
}