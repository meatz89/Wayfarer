using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Game.MainSystem;
using Wayfarer.GameState;
using Wayfarer.Content;

namespace Wayfarer.Content.Factories

/// <summary>
/// Factory for creating route discoveries with guaranteed valid references.
/// Ensures route discoveries can only be created with existing routes and NPCs.
/// </summary>
public class RouteDiscoveryFactory
{
    public RouteDiscoveryFactory()
    {
        // No dependencies - factory is stateless
    }

    /// <summary>
    /// Create a route discovery with validated references
    /// </summary>
    public RouteDiscovery CreateRouteDiscovery(
        RouteOption route,        // Not string - actual RouteOption object
        List<NPC> knownByNPCs,   // Not strings - actual NPC objects
        int requiredTokens = 3)
    {
        if (route == null)
            throw new ArgumentNullException(nameof(route), "Route cannot be null");
        if (knownByNPCs == null || !knownByNPCs.Any())
            throw new ArgumentException("At least one NPC must know the route", nameof(knownByNPCs));

        RouteDiscovery discovery = new RouteDiscovery
        {
            RouteId = route.Id,
            KnownByNPCs = knownByNPCs.Select(n => n.ID).ToList(),
            RequiredTokensWithNPC = requiredTokens,
            DiscoveryContexts = new Dictionary<string, RouteDiscoveryContext>()
        };

        // Initialize empty contexts for each NPC
        foreach (NPC npc in knownByNPCs)
        {
            discovery.DiscoveryContexts[npc.ID] = new RouteDiscoveryContext();
        }

        return discovery;
    }

    /// <summary>
    /// Create a route discovery from string IDs with validation
    /// </summary>
    public RouteDiscovery CreateRouteDiscoveryFromIds(
        string routeId,
        List<string> knownByNPCIds,
        IEnumerable<RouteOption> availableRoutes,
        IEnumerable<NPC> availableNPCs,
        int requiredTokens = 3)
    {
        // Resolve route
        RouteOption? route = availableRoutes.FirstOrDefault(r => r.Id == routeId);
        if (route == null)
            throw new InvalidOperationException($"Cannot create route discovery: route '{routeId}' not found");

        // Resolve NPCs
        List<NPC> npcs = new List<NPC>();
        List<string> missingNPCs = new List<string>();

        foreach (string npcId in knownByNPCIds)
        {
            NPC? npc = availableNPCs.FirstOrDefault(n => n.ID == npcId);
            if (npc != null)
                npcs.Add(npc);
            else
                missingNPCs.Add(npcId);
        }

        if (!npcs.Any())
            throw new InvalidOperationException($"Cannot create route discovery: no valid NPCs found from IDs");

        if (missingNPCs.Any())
        {
            Console.WriteLine($"WARNING: Route discovery for '{routeId}' - NPCs not found: {string.Join(", ", missingNPCs)}");
        }

        return CreateRouteDiscovery(route, npcs, requiredTokens);
    }

    /// <summary>
    /// Add discovery context for a specific NPC
    /// </summary>
    public void AddDiscoveryContext(
        RouteDiscovery discovery,
        NPC npc,
        List<Item> requiredEquipment,
        string narrative)
    {
        if (discovery == null)
            throw new ArgumentNullException(nameof(discovery));
        if (npc == null)
            throw new ArgumentNullException(nameof(npc));
        if (!discovery.KnownByNPCs.Contains(npc.ID))
            throw new InvalidOperationException($"NPC '{npc.Name}' does not know this route");

        RouteDiscoveryContext context = new RouteDiscoveryContext
        {
            RequiredEquipment = requiredEquipment?.Select(e => e.Id).ToList() ?? new List<string>(),
            Narrative = narrative ?? $"{npc.Name} shares their knowledge of the route with you."
        };

        discovery.DiscoveryContexts[npc.ID] = context;
    }

    /// <summary>
    /// Add discovery context from string IDs with validation
    /// </summary>
    public void AddDiscoveryContextFromIds(
        RouteDiscovery discovery,
        string npcId,
        List<string> requiredEquipmentIds,
        IEnumerable<NPC> availableNPCs,
        IEnumerable<Item> availableItems,
        string narrative)
    {
        NPC? npc = availableNPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc == null)
            throw new InvalidOperationException($"Cannot add discovery context: NPC '{npcId}' not found");

        List<Item> equipment = new List<Item>();
        List<string> missingItems = new List<string>();

        if (requiredEquipmentIds != null)
        {
            foreach (string itemId in requiredEquipmentIds)
            {
                Item? item = availableItems.FirstOrDefault(i => i.Id == itemId);
                if (item != null)
                    equipment.Add(item);
                else
                    missingItems.Add(itemId);
            }

            if (missingItems.Any())
            {
                Console.WriteLine($"WARNING: Discovery context for NPC '{npcId}' - Items not found: {string.Join(", ", missingItems)}");
            }
        }

        AddDiscoveryContext(discovery, npc, equipment, narrative);
    }
}
}