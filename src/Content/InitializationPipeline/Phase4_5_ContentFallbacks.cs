using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Phase 4.5: Apply content fallbacks for any missing references.
/// This phase runs after conversations are loaded but before complex entities.
/// It ensures all referenced content exists, creating fallbacks where needed.
/// Dependencies: Conversations loaded (Phase 4)
/// </summary>
public class Phase4_5_ContentFallbacks : IInitializationPhase
{
    public int PhaseNumber => 45; // Between Phase 4 and Phase 5
    public string Name => "Content Fallback Resolution";
    public bool IsCritical => false; // Never critical - this phase exists to prevent crashes

    public void Execute(InitializationContext context)
    {
        Console.WriteLine("Checking for missing content references and creating fallbacks...");

        // Get repositories from GameWorld
        // Note: We can't easily create DebugLogger here as it needs TimeManager and ConversationStateManager
        // For now, we'll create a minimal version or skip the fallback service in the repository
        var npcRepository = new NPCRepository(
            context.GameWorld,
            null, // DebugLogger not available during initialization
            new NPCVisibilityService()
        );
        
        var locationRepository = new LocationRepository(context.GameWorld);
        var routeRepository = new RouteRepository(context.GameWorld);

        // Create fallback service
        var fallbackService = new ContentFallbackService(
            npcRepository,
            locationRepository,
            routeRepository
        );

        // Get loaded conversations from shared data
        var conversations = context.SharedData.ContainsKey("Conversations") 
            ? (Dictionary<string, ConversationDefinition>)context.SharedData["Conversations"]
            : new Dictionary<string, ConversationDefinition>();

        // Get current letters if any
        var player = context.GameWorld.GetPlayer();
        List<Letter> letters = player?.LetterQueue != null 
            ? player.LetterQueue.Where(l => l != null).ToList()
            : new List<Letter>();

        // Get routes
        var routes = context.GameWorld.WorldState.Routes ?? new List<RouteOption>();

        // Validate and patch all content
        var report = fallbackService.ValidateAndPatchContent(conversations, letters, routes);

        // Store fallback service for later use
        context.SharedData["ContentFallbackService"] = fallbackService;
        context.SharedData["ContentFallbackReport"] = report;

        // Log results
        if (report.HasFallbacks)
        {
            Console.WriteLine($"\n{fallbackService.GetFallbackSummary()}");
            
            // Add warnings for each fallback
            foreach (var detail in report.Details)
            {
                context.Warnings.Add($"Created fallback {detail.Type} '{detail.MissingId}': {detail.Reason}");
            }

            // Also add a prominent warning that will be visible in the UI
            context.Warnings.Insert(0, 
                $"⚠️ CONTENT FALLBACKS ACTIVE: {report.TotalFallbacksCreated} missing references were auto-generated. " +
                "Check console for details.");
        }
        else
        {
            Console.WriteLine("All content references are valid - no fallbacks needed.");
        }

        // Connect the fallback service to the NPC repository
        npcRepository.SetFallbackService(fallbackService);
    }
}