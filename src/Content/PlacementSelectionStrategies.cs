/// <summary>
/// Selection strategies for choosing entities from multiple matching candidates.
/// DDR-007 COMPLIANT: All selection is deterministic from categorical properties (no Random).
/// DOMAIN COLLECTION PRINCIPLE: Uses List<T> with LINQ, not Dictionary.
/// </summary>
internal static class PlacementSelectionStrategies
{
    private const int SEGMENTS_PER_DAY = 16;
    private const int SEGMENTS_PER_TIME_BLOCK = 4;

    /// <summary>
    /// Apply selection strategy to choose ONE NPC from multiple matching candidates
    /// DDR-007: All selection is deterministic from categorical properties
    /// </summary>
    public static NPC ApplyStrategyNPC(List<NPC> candidates, PlacementSelectionStrategy strategy, Player player)
    {
        if (candidates == null || candidates.Count == 0)
            return null;

        if (candidates.Count == 1)
            return candidates[0];

        return strategy switch
        {
            PlacementSelectionStrategy.Closest => SelectClosestNPC(candidates, player),
            PlacementSelectionStrategy.HighestBond => SelectHighestBondNPC(candidates),
            PlacementSelectionStrategy.LeastRecent => SelectLeastRecentNPC(candidates, player),
            _ => SelectFirstByName(candidates)
        };
    }

    /// <summary>
    /// Apply selection strategy to choose ONE Location from multiple matching candidates.
    /// DDR-007: All selection is deterministic from categorical properties.
    /// </summary>
    public static Location ApplyStrategyLocation(List<Location> candidates, PlacementSelectionStrategy strategy, Player player)
    {
        if (candidates == null || candidates.Count == 0)
            return null;

        if (candidates.Count == 1)
            return candidates[0];

        return strategy switch
        {
            PlacementSelectionStrategy.Closest => SelectClosestLocation(candidates, player),
            PlacementSelectionStrategy.HighestBond => SelectFirstLocationByName(candidates),
            PlacementSelectionStrategy.LeastRecent => SelectLeastRecentLocation(candidates, player),
            _ => SelectFirstLocationByName(candidates)
        };
    }

    /// <summary>
    /// DDR-007: Deterministic selection by Name for NPC candidates.
    /// </summary>
    public static NPC SelectFirstByName(List<NPC> candidates)
    {
        return candidates.OrderBy(npc => npc.Name).First();
    }

    /// <summary>
    /// DDR-007: Deterministic selection by Name for Location candidates.
    /// </summary>
    public static Location SelectFirstLocationByName(List<Location> candidates)
    {
        return candidates.OrderBy(loc => loc.Name).First();
    }

    /// <summary>
    /// Select NPC closest to player's current position using hex grid distance
    /// </summary>
    public static NPC SelectClosestNPC(List<NPC> candidates, Player player)
    {
        NPC closest = null;
        int minDistance = int.MaxValue;

        foreach (NPC npc in candidates)
        {
            if (npc.Location?.HexPosition == null)
                continue;

            int distance = player.CurrentPosition.DistanceTo(npc.Location.HexPosition.Value);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = npc;
            }
        }

        return closest ?? candidates[0];
    }

    /// <summary>
    /// Select Location closest to player's current position using hex grid distance
    /// </summary>
    public static Location SelectClosestLocation(List<Location> candidates, Player player)
    {
        Location closest = null;
        int minDistance = int.MaxValue;

        foreach (Location location in candidates)
        {
            if (location.HexPosition == null)
                continue;

            int distance = player.CurrentPosition.DistanceTo(location.HexPosition.Value);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = location;
            }
        }

        return closest ?? candidates[0];
    }

    /// <summary>
    /// Select NPC with highest bond strength
    /// Good for "trusted ally" or "close friend" scenarios
    /// </summary>
    public static NPC SelectHighestBondNPC(List<NPC> candidates)
    {
        return candidates.OrderByDescending(npc => npc.BondStrength).First();
    }

    /// <summary>
    /// Select NPC least recently interacted with for content variety.
    /// Uses Player.NPCInteractions timestamp data to find oldest interaction.
    /// DDR-007: Falls back to deterministic selection if no interaction history exists.
    /// DOMAIN COLLECTION PRINCIPLE: Uses List<T> with LINQ, not Dictionary.
    /// </summary>
    public static NPC SelectLeastRecentNPC(List<NPC> candidates, Player player)
    {
        NPC leastRecentNPC = null;
        long oldestTimestamp = long.MaxValue;

        foreach (NPC candidate in candidates)
        {
            NPCInteractionRecord record = player.NPCInteractions
                .FirstOrDefault(interaction => interaction.Npc == candidate);

            if (record == null)
            {
                return candidate;
            }

            long timestamp = CalculateTimestamp(record.LastInteractionDay, record.LastInteractionTimeBlock, record.LastInteractionSegment);

            if (timestamp < oldestTimestamp)
            {
                oldestTimestamp = timestamp;
                leastRecentNPC = candidate;
            }
        }

        return leastRecentNPC ?? SelectFirstByName(candidates);
    }

    /// <summary>
    /// Select Location least recently visited for content variety
    /// Uses Player.LocationVisits timestamp data to find oldest visit
    /// DDR-007: Falls back to deterministic selection if no visit history exists
    /// </summary>
    public static Location SelectLeastRecentLocation(List<Location> candidates, Player player)
    {
        Location leastRecentLocation = null;
        long oldestTimestamp = long.MaxValue;

        foreach (Location candidate in candidates)
        {
            LocationVisitRecord record = player.LocationVisits
                .FirstOrDefault(visit => visit.Location == candidate);

            if (record == null)
            {
                return candidate;
            }

            long timestamp = CalculateTimestamp(record.LastVisitDay, record.LastVisitTimeBlock, record.LastVisitSegment);

            if (timestamp < oldestTimestamp)
            {
                oldestTimestamp = timestamp;
                leastRecentLocation = candidate;
            }
        }

        return leastRecentLocation ?? SelectFirstLocationByName(candidates);
    }

    /// <summary>
    /// Calculate timestamp from game time components for chronological comparison
    /// Formula: (Day * 16) + (TimeBlockValue * 4) + Segment
    /// </summary>
    public static long CalculateTimestamp(int day, TimeBlocks timeBlock, int segment)
    {
        int timeBlockValue = timeBlock switch
        {
            TimeBlocks.Morning => 0,
            TimeBlocks.Midday => 1,
            TimeBlocks.Afternoon => 2,
            TimeBlocks.Evening => 3,
            _ => 0
        };

        return (day * SEGMENTS_PER_DAY) + (timeBlockValue * SEGMENTS_PER_TIME_BLOCK) + segment;
    }
}
