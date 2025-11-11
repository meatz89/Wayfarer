/// <summary>
/// Central rule engine that implements all game mechanics calculations.
/// All game rules are driven by configuration, not hard-coded values.
/// </summary>
public class GameRuleEngine : IGameRuleEngine
{
    private readonly GameConfiguration _config;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly NPCRepository _npcRepository;
    private readonly TimeManager _timeManager;

    public GameRuleEngine(
        GameConfiguration config,
        TokenMechanicsManager tokenManager,
        NPCRepository npcRepository,
        TimeManager timeManager)
    {
        _config = config;
        _tokenManager = tokenManager;
        _npcRepository = npcRepository;
        _timeManager = timeManager;
    }

    // Travel mechanics
    public int CalculateTravelStamina(RouteOption route)
    {
        int baseCost = _config.Travel.BaseStaminaCost;

        // Apply terrain modifiers
        foreach (TerrainCategory terrain in route.TerrainCategories)
        {
            string terrainName = terrain.ToString();
            if (_config.Travel.TerrainStaminaModifiers.TryGetValue(terrainName, out float modifier))
            {
                baseCost = (int)(baseCost * modifier);
            }
        }

        return baseCost;
    }

    public bool CanTravel(Player player, RouteOption route)
    {
        int staminaCost = CalculateTravelStamina(route);
        int segmentCost = route.TravelTimeSegments;

        return player.Stamina >= staminaCost &&
               _timeManager.SegmentsRemainingInDay >= segmentCost;
    }

    // Time management
    public TimeBlocks GetTimeBlock(int segment)
    {
        foreach ((string blockName, TimeBlockDefinition definition) in _config.Time.TimeBlocks)
        {
            int endSegment = definition.StartSegment + definition.SegmentCount;
            if (segment >= definition.StartSegment && segment < endSegment)
            {
                return EnumParser.Parse<TimeBlocks>(blockName, "TimeBlock");
            }
        }

        return TimeBlocks.Evening; // Default
    }

    public int GetActiveSegmentsRemaining(int currentSegment)
    {
        if (currentSegment >= 36) // End of day
            return 0;

        return 36 - currentSegment;
    }

    public bool IsNPCAvailable(NPC npc, TimeBlocks timeBlock)
    {
        return true;
    }

    // Stamina and recovery
    public int CalculateRestRecovery(string lodgingType)
    {
        return _config.Stamina.RecoveryByLodging.GetValueOrDefault(
            lodgingType,
            _config.Stamina.RecoveryRest
        );
    }

    public int CalculateMaxStamina(Player player)
    {
        // Could be modified by equipment, traits, etc.
        return _config.Stamina.MaxStamina;
    }

}
