using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Factory for creating network unlocks with guaranteed valid references.
/// Ensures network unlocks can only be created with existing NPCs.
/// </summary>
public class NetworkUnlockFactory
{
    public NetworkUnlockFactory()
    {
        // No dependencies - factory is stateless
    }

    /// <summary>
    /// Create a network unlock with validated references
    /// </summary>
    public NetworkUnlock CreateNetworkUnlock(
        string id,
        NPC unlockerNpc,     // Not string - actual NPC object
        int tokensRequired,
        string unlockDescription,
        List<NetworkUnlockTarget> targets)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Network unlock ID cannot be empty", nameof(id));
        if (unlockerNpc == null)
            throw new ArgumentNullException(nameof(unlockerNpc), "Unlocker NPC cannot be null");
        if (targets == null || !targets.Any())
            throw new ArgumentException("At least one unlock target must be specified", nameof(targets));

        NetworkUnlock networkUnlock = new NetworkUnlock
        {
            Id = id,
            Type = "npc_network",
            UnlockerNpcId = unlockerNpc.ID,
            TokensRequired = tokensRequired,
            UnlockDescription = unlockDescription ?? $"Unlock network access through {unlockerNpc.Name}",
            Unlocks = targets
        };

        return networkUnlock;
    }

    /// <summary>
    /// Create network unlock targets with validated NPC references
    /// </summary>
    public List<NetworkUnlockTarget> CreateUnlockTargets(
        List<(NPC npc, string introductionText)> targetDefinitions)
    {
        if (targetDefinitions == null || !targetDefinitions.Any())
            throw new ArgumentException("Target definitions cannot be empty", nameof(targetDefinitions));

        List<NetworkUnlockTarget> targets = new List<NetworkUnlockTarget>();

        foreach ((NPC npc, string introductionText) in targetDefinitions)
        {
            if (npc == null)
                throw new ArgumentNullException("Target NPC cannot be null");

            targets.Add(new NetworkUnlockTarget
            {
                NpcId = npc.ID,
                IntroductionText = introductionText ?? $"{npc.Name} becomes available through your network connections."
            });
        }

        return targets;
    }

    /// <summary>
    /// Create a network unlock from string IDs with validation
    /// </summary>
    public NetworkUnlock CreateNetworkUnlockFromIds(
        string id,
        string unlockerNpcId,
        int tokensRequired,
        string unlockDescription,
        List<(string npcId, string introductionText)> targetDefinitions,
        IEnumerable<NPC> availableNPCs)
    {
        // Resolve unlocker NPC
        NPC? unlockerNpc = availableNPCs.FirstOrDefault(n => n.ID == unlockerNpcId);
        if (unlockerNpc == null)
            throw new InvalidOperationException($"Cannot create network unlock: unlocker NPC '{unlockerNpcId}' not found");

        // Resolve target NPCs
        List<NetworkUnlockTarget> targets = new List<NetworkUnlockTarget>();
        foreach ((string npcId, string introductionText) in targetDefinitions)
        {
            NPC? targetNpc = availableNPCs.FirstOrDefault(n => n.ID == npcId);
            if (targetNpc == null)
            {
                Console.WriteLine($"WARNING: Network unlock '{id}' - Target NPC '{npcId}' not found, skipping");
                continue;
            }

            targets.Add(new NetworkUnlockTarget
            {
                NpcId = targetNpc.ID,
                IntroductionText = introductionText ?? $"{targetNpc.Name} becomes available through your network connections."
            });
        }

        if (!targets.Any())
            throw new InvalidOperationException($"Cannot create network unlock '{id}': no valid target NPCs found");

        return CreateNetworkUnlock(id, unlockerNpc, tokensRequired, unlockDescription, targets);
    }

    /// <summary>
    /// Add a target to an existing network unlock
    /// </summary>
    public void AddTarget(NetworkUnlock networkUnlock, NPC targetNpc, string introductionText)
    {
        if (networkUnlock == null)
            throw new ArgumentNullException(nameof(networkUnlock));
        if (targetNpc == null)
            throw new ArgumentNullException(nameof(targetNpc));

        // Check if target already exists
        if (networkUnlock.Unlocks.Any(t => t.NpcId == targetNpc.ID))
        {
            Console.WriteLine($"WARNING: Target NPC '{targetNpc.ID}' already exists in network unlock '{networkUnlock.Id}'");
            return;
        }

        networkUnlock.Unlocks.Add(new NetworkUnlockTarget
        {
            NpcId = targetNpc.ID,
            IntroductionText = introductionText ?? $"{targetNpc.Name} becomes available through your network connections."
        });
    }
}