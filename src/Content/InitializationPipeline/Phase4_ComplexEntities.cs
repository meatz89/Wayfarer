using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Phase 4: Load complex entities that may have multiple dependencies.
/// This includes: Standing Obligations, Token Favors, Network Unlocks, Route Discovery
/// </summary>
public class Phase4_ComplexEntities : IInitializationPhase
{
    public int PhaseNumber => 4;
    public string Name => "Complex Entities";
    public bool IsCritical => false; // Game can run without these

    public void Execute(InitializationContext context)
    {
        // 1. Load Standing Obligations (depends on NPCs)
        LoadStandingObligations(context);
        
        // 2. Load Token Favors (depends on NPCs, Routes, Items)
        LoadTokenFavors(context);
        
        // 3. Load Network Unlocks (depends on NPCs)
        LoadNetworkUnlocks(context);
        
        // 4. Load Route Discovery (depends on Routes and NPCs)
        LoadRouteDiscovery(context);
    }
    
    private void LoadStandingObligations(InitializationContext context)
    {
        var obligationFiles = new[] { "standing_obligations.json", "scaling_obligations.json", "tutorial_obligations.json" };
        var allObligationDTOs = new List<StandingObligationDTO>();
        
        foreach (var fileName in obligationFiles)
        {
            var obligationsPath = Path.Combine(context.ContentPath, fileName);
            
            if (!File.Exists(obligationsPath))
            {
                Console.WriteLine($"INFO: {fileName} not found, skipping");
                continue;
            }
            
            try
            {
                var obligationDTOs = context.ContentLoader.LoadValidatedContent<List<StandingObligationDTO>>(obligationsPath);
                
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
        
        var obligationFactory = new StandingObligationFactory();
        var npcs = context.GameWorld.WorldState.NPCs;
        
        foreach (var dto in allObligationDTOs)
        {
            try
            {
                // Store NPC reference for Phase 6 validation
                context.SharedData[$"obligation_{dto.ID}_npc"] = dto.Source;
                
                // Create obligation (don't validate NPC yet - Phase 6 will handle missing refs)
                var obligation = obligationFactory.CreateStandingObligationFromDTO(dto, npcs);
                
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
    
    private void LoadTokenFavors(InitializationContext context)
    {
        var favorsPath = Path.Combine(context.ContentPath, "token_favors.json");
        
        if (!File.Exists(favorsPath))
        {
            Console.WriteLine("INFO: token_favors.json not found, no favors loaded");
            return;
        }
        
        try
        {
            var favors = context.ContentLoader.LoadValidatedContentWithParser(favorsPath,
                json => TokenFavorParser.ParseTokenFavorArray(json));
            
            if (favors == null || !favors.Any())
            {
                Console.WriteLine("INFO: No token favors found");
                return;
            }
            
            var validFavors = new List<TokenFavor>();
            
            foreach (var favor in favors)
            {
                // Store references for Phase 6 validation
                context.SharedData[$"favor_{favor.Id}_npc"] = favor.NPCId;
                
                // Add favor without validation - Phase 6 will create missing NPCs
                validFavors.Add(favor);
                Console.WriteLine($"  Loaded favor: {favor.Id} from {favor.NPCId}");
            }
            
            context.GameWorld.WorldState.TokenFavors.AddRange(validFavors);
            Console.WriteLine($"Loaded {validFavors.Count} token favors");
        }
        catch (Exception ex)
        {
            context.Warnings.Add($"Failed to load token favors: {ex.Message}");
        }
    }
    
    private void LoadNetworkUnlocks(InitializationContext context)
    {
        var unlocksPath = Path.Combine(context.ContentPath, "Progression", "progression_unlocks.json");
        
        if (!File.Exists(unlocksPath))
        {
            Console.WriteLine("INFO: progression_unlocks.json not found, no network unlocks loaded");
            return;
        }
        
        var unlockDTOs = context.ContentLoader.LoadValidatedContent<List<NetworkUnlockDTO>>(unlocksPath);
            
        if (unlockDTOs == null || !unlockDTOs.Any())
        {
            Console.WriteLine("INFO: No network unlocks found");
            return;
        }
            
        var unlockFactory = new NetworkUnlockFactory();
            
        foreach (var dto in unlockDTOs)
        {
            try
            {
                // Store references for Phase 6
                context.SharedData[$"unlock_{dto.Id}_unlocker"] = dto.UnlockerNpcId;
                foreach (var target in dto.Unlocks ?? new List<NetworkUnlockTargetDTO>())
                {
                    context.SharedData[$"unlock_{dto.Id}_target_{target.NpcId}"] = target.NpcId;
                }
                    
                // Create unlock targets
                var targets = new List<NetworkUnlockTarget>();
                foreach (var targetDTO in dto.Unlocks ?? new List<NetworkUnlockTargetDTO>())
                {
                    var target = new NetworkUnlockTarget
                    {
                        NpcId = targetDTO.NpcId,
                        IntroductionText = targetDTO.IntroductionText ?? "You've been introduced."
                    };
                    targets.Add(target);
                }
                    
                // Create network unlock
                var targetDefinitions = targets.Select(t => (t.NpcId, t.IntroductionText)).ToList();
                var unlock = unlockFactory.CreateNetworkUnlockFromIds(
                    dto.Id,
                    dto.UnlockerNpcId,
                    dto.TokensRequired,
                    dto.UnlockDescription ?? $"Unlock connections through {dto.UnlockerNpcId}",
                    targetDefinitions,
                    context.GameWorld.WorldState.NPCs
                );
                    
                context.GameWorld.WorldState.NetworkUnlocks.Add(unlock);
                Console.WriteLine($"  Loaded network unlock: {unlock.Id} via {dto.UnlockerNpcId}");
            }
            catch (Exception ex)
            {
                context.Warnings.Add($"Failed to create network unlock {dto.Id}: {ex.Message}");
            }
        }
            
        Console.WriteLine($"Loaded {context.GameWorld.WorldState.NetworkUnlocks.Count} network unlocks");
    }
    
    private void LoadRouteDiscovery(InitializationContext context)
    {
        var discoveryPath = Path.Combine(context.ContentPath, "Progression", "route_discovery.json");
        
        if (!File.Exists(discoveryPath))
        {
            Console.WriteLine("INFO: route_discovery.json not found, all routes start discovered");
            return;
        }
        
        var discoveryDTOs = context.ContentLoader.LoadValidatedContent<List<RouteDiscoveryDTO>>(discoveryPath);
            
        if (discoveryDTOs == null || !discoveryDTOs.Any())
        {
            Console.WriteLine("INFO: No route discoveries found");
            return;
        }
            
        var discoveryFactory = new RouteDiscoveryFactory();
            
        foreach (var dto in discoveryDTOs)
        {
                // Store references for Phase 6
                context.SharedData[$"discovery_{dto.RouteId}_route"] = dto.RouteId;
                foreach (var npcId in dto.KnownByNPCs ?? new List<string>())
                {
                    context.SharedData[$"discovery_{dto.RouteId}_npc_{npcId}"] = npcId;
                }
                    
                // Create discovery
                var discovery = discoveryFactory.CreateRouteDiscoveryFromIds(
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