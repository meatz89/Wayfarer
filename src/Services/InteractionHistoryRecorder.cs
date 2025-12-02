/// <summary>
/// InteractionHistoryRecorder - records player interactions with locations, NPCs, and routes
/// Extracted from GameOrchestrator via COMPOSITION OVER INHERITANCE principle
/// Maintains player history using update-in-place pattern (ONE record per entity)
/// </summary>
public class InteractionHistoryRecorder
{
    private readonly GameWorld _gameWorld;
    private readonly TimeFacade _timeFacade;

    public InteractionHistoryRecorder(GameWorld gameWorld, TimeFacade timeFacade)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _timeFacade = timeFacade ?? throw new ArgumentNullException(nameof(timeFacade));
    }

    /// <summary>
    /// Record location visit in player interaction history
    /// Update-in-place pattern: Find existing record or create new
    /// ONE record per location (replaces previous timestamp)
    /// </summary>
    public void RecordLocationVisit(Location location)
    {
        Player player = _gameWorld.GetPlayer();

        LocationVisitRecord existingRecord = player.LocationVisits
            .FirstOrDefault(record => record.Location == location);

        if (existingRecord != null)
        {
            existingRecord.LastVisitDay = _timeFacade.GetCurrentDay();
            existingRecord.LastVisitTimeBlock = _timeFacade.GetCurrentTimeBlock();
            existingRecord.LastVisitSegment = _timeFacade.GetCurrentSegment();
        }
        else
        {
            player.LocationVisits.Add(new LocationVisitRecord
            {
                Location = location,
                LastVisitDay = _timeFacade.GetCurrentDay(),
                LastVisitTimeBlock = _timeFacade.GetCurrentTimeBlock(),
                LastVisitSegment = _timeFacade.GetCurrentSegment()
            });
        }
    }

    /// <summary>
    /// Record NPC interaction in player interaction history
    /// Update-in-place pattern: Find existing record or create new
    /// ONE record per NPC (replaces previous timestamp)
    /// HIGHLANDER: Accept NPC object, use object equality
    /// </summary>
    public void RecordNPCInteraction(NPC npc)
    {
        Player player = _gameWorld.GetPlayer();

        NPCInteractionRecord existingRecord = player.NPCInteractions
            .FirstOrDefault(record => record.Npc == npc);

        if (existingRecord != null)
        {
            existingRecord.LastInteractionDay = _timeFacade.GetCurrentDay();
            existingRecord.LastInteractionTimeBlock = _timeFacade.GetCurrentTimeBlock();
            existingRecord.LastInteractionSegment = _timeFacade.GetCurrentSegment();
        }
        else
        {
            player.NPCInteractions.Add(new NPCInteractionRecord
            {
                Npc = npc,
                LastInteractionDay = _timeFacade.GetCurrentDay(),
                LastInteractionTimeBlock = _timeFacade.GetCurrentTimeBlock(),
                LastInteractionSegment = _timeFacade.GetCurrentSegment()
            });
        }
    }

    /// <summary>
    /// Record route traversal in player interaction history
    /// Update-in-place pattern: Find existing record or create new
    /// ONE record per route (replaces previous timestamp)
    /// </summary>
    public void RecordRouteTraversal(RouteOption route)
    {
        Player player = _gameWorld.GetPlayer();

        RouteTraversalRecord existingRecord = player.RouteTraversals
            .FirstOrDefault(record => record.Route == route);

        if (existingRecord != null)
        {
            existingRecord.LastTraversalDay = _timeFacade.GetCurrentDay();
            existingRecord.LastTraversalTimeBlock = _timeFacade.GetCurrentTimeBlock();
            existingRecord.LastTraversalSegment = _timeFacade.GetCurrentSegment();
        }
        else
        {
            player.RouteTraversals.Add(new RouteTraversalRecord
            {
                Route = route,
                LastTraversalDay = _timeFacade.GetCurrentDay(),
                LastTraversalTimeBlock = _timeFacade.GetCurrentTimeBlock(),
                LastTraversalSegment = _timeFacade.GetCurrentSegment()
            });
        }
    }
}
