using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Phase 5: Load complex entities that may have multiple dependencies.
/// This includes: Standing Obligations, Route Discovery
/// </summary>
public class Phase5_ComplexEntities : IInitializationPhase
{
    public int PhaseNumber => 5;
    public string Name => "Complex Entities";
    public bool IsCritical => false; // Game can run without these

    public void Execute(InitializationContext context)
    {
        // 1. Load Standing Obligations (depends on NPCs)
        LoadStandingObligations(context);

        // 2. Load Route Discovery (depends on Routes and NPCs)
        LoadRouteDiscovery(context);
    }

    private void LoadStandingObligations(InitializationContext context)
    {
        string[] obligationFiles = new[] { "standing_obligations.json", "scaling_obligations.json", "tutorial_obligations.json" };
        List<StandingObligationDTO> allObligationDTOs = new List<StandingObligationDTO>();

        foreach (string? fileName in obligationFiles)
        {
            string obligationsPath = Path.Combine(context.ContentPath, fileName);

            if (!File.Exists(obligationsPath))
            {
                Console.WriteLine($"INFO: {fileName} not found, skipping");
                continue;
            }

            try
            {
                List<StandingObligationDTO> obligationDTOs = context.ContentLoader.LoadValidatedContent<List<StandingObligationDTO>>(obligationsPath);

                if (obligationDTOs != null && obligationDTOs.Any())
                {
                    allObligationDTOs.AddRange(obligationDTOs);
                    Console.WriteLine($"  Loaded {obligationDTOs.Count} obligations from {fileName}");
                }
            }
            catch (Exception ex)
            {
                context.Warnings.Add($"Failed to load {fileName}: {ex.Message}");
            }
        }

        if (!allObligationDTOs.Any())
        {
            Console.WriteLine("INFO: No standing obligations found in any file");
            return;
        }

        StandingObligationFactory obligationFactory = new StandingObligationFactory();
        List<NPC> npcs = context.GameWorld.WorldState.NPCs;

        foreach (StandingObligationDTO dto in allObligationDTOs)
        {
            try
            {
                // Store NPC reference for Phase 7 validation
                context.ValidationTracker.ObligationNPCs[dto.ID] = dto.Source;

                // Create obligation (don't validate NPC yet - Phase 6 will handle missing refs)
                StandingObligation obligation = obligationFactory.CreateStandingObligationFromDTO(dto, npcs);

                context.GameWorld.WorldState.StandingObligationTemplates.Add(obligation);
                Console.WriteLine($"  Loaded obligation: {obligation.Name} from {dto.Source}");
            }
            catch (Exception ex)
            {
                context.Warnings.Add($"Failed to create obligation {dto.ID}: {ex.Message}");
            }
        }

        Console.WriteLine($"Loaded {context.GameWorld.WorldState.StandingObligationTemplates.Count} standing obligations total");
    }



    private void LoadRouteDiscovery(InitializationContext context)
    {
        string discoveryPath = Path.Combine(context.ContentPath, "Progression", "route_discovery.json");

        if (!File.Exists(discoveryPath))
        {
            Console.WriteLine("INFO: route_discovery.json not found, all routes start discovered");
            return;
        }

        List<RouteDiscoveryDTO> discoveryDTOs = context.ContentLoader.LoadValidatedContent<List<RouteDiscoveryDTO>>(discoveryPath);

        if (discoveryDTOs == null || !discoveryDTOs.Any())
        {
            Console.WriteLine("INFO: No route discoveries found");
            return;
        }

        RouteDiscoveryFactory discoveryFactory = new RouteDiscoveryFactory();

        foreach (RouteDiscoveryDTO dto in discoveryDTOs)
        {
            // Store references for Phase 7 validation
            context.ValidationTracker.RouteDiscoveryRoutes.Add(dto.RouteId);
            if (!context.ValidationTracker.RouteDiscoveryNPCs.ContainsKey(dto.RouteId))
            {
                context.ValidationTracker.RouteDiscoveryNPCs[dto.RouteId] = new List<string>();
            }
            foreach (string npcId in dto.KnownByNPCs ?? new List<string>())
            {
                context.ValidationTracker.RouteDiscoveryNPCs[dto.RouteId].Add(npcId);
            }

            // Create discovery
            RouteDiscovery discovery = discoveryFactory.CreateRouteDiscoveryFromIds(
                    dto.RouteId,
                    dto.KnownByNPCs ?? new List<string>(),
                    context.GameWorld.WorldState.Routes,
                    context.GameWorld.WorldState.NPCs,
                    dto.RequiredTokensWithNPC
                );

            context.GameWorld.WorldState.RouteDiscoveries.Add(discovery);
            Console.WriteLine($"  Loaded route discovery: {dto.RouteId}");
        }

        Console.WriteLine($"Loaded {context.GameWorld.WorldState.RouteDiscoveries.Count} route discoveries");
    }
}