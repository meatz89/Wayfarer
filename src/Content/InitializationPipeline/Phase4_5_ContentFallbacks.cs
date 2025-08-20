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

        // During initialization, we need to create minimal services
        // These will be replaced by DI-injected versions after initialization
        NPCRepository npcRepository = new NPCRepository(
            context.GameWorld,
            null, // DebugLogger not available during initialization
            new NPCVisibilityService()
        );

        LocationRepository locationRepository = new LocationRepository(context.GameWorld);
        ItemRepository itemRepository = new ItemRepository(context.GameWorld);
        RouteRepository routeRepository = new RouteRepository(context.GameWorld, itemRepository);

        // Create fallback service
        ContentFallbackService fallbackService = new ContentFallbackService(
            npcRepository,
            locationRepository,
            routeRepository
        );

        // Get loaded conversations from shared data
        Dictionary<string, ConversationDefinition> conversations = context.SharedData.ContainsKey("Conversations")
            ? (Dictionary<string, ConversationDefinition>)context.SharedData["Conversations"]
            : new Dictionary<string, ConversationDefinition>();

        // Get current obligations if any
        Player player = context.GameWorld.GetPlayer();
        List<DeliveryObligation> obligations = player?.ObligationQueue != null
            ? player.ObligationQueue.Where(l => l != null).ToList()
            : new List<DeliveryObligation>();

        // Get routes
        List<RouteOption> routes = context.GameWorld.WorldState.Routes ?? new List<RouteOption>();

        // Validate and patch all content
        ContentFallbackReport report = fallbackService.ValidateAndPatchContent(conversations, obligations, routes);

        // Store fallback service for later use
        context.SharedData["ContentFallbackService"] = fallbackService;
        context.SharedData["ContentFallbackReport"] = report;

        // Log results
        if (report.HasFallbacks)
        {
            Console.WriteLine($"\n{fallbackService.GetFallbackSummary()}");

            // Add warnings for each fallback
            foreach (ContentFallbackEntry detail in report.Details)
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

        // NOTE: SetFallbackService violates our architectural principles
        // The NPCRepository should get its fallback service through DI, not setter injection
    }
}