/// <summary>
/// TimeAdvancementOrchestrator - THE ONLY place for processing time advancement side effects
/// Extracted from GameOrchestrator via COMPOSITION OVER INHERITANCE principle
/// HIGHLANDER: All time-based resource changes happen here (hunger, day transitions, emergency checking)
/// Called after EVERY time advancement (Wait, Rest, Work, Travel, etc.)
/// </summary>
public class TimeAdvancementOrchestrator
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly ResourceFacade _resourceFacade;
    private readonly EmergencyFacade _emergencyFacade;

    public TimeAdvancementOrchestrator(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        ResourceFacade resourceFacade,
        EmergencyFacade emergencyFacade)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _resourceFacade = resourceFacade ?? throw new ArgumentNullException(nameof(resourceFacade));
        _emergencyFacade = emergencyFacade ?? throw new ArgumentNullException(nameof(emergencyFacade));
    }

    /// <summary>
    /// Process all side effects of time advancement
    /// HIGHLANDER: THE sync point for all time-based state changes
    /// </summary>
    public async Task ProcessTimeAdvancement(TimeAdvancementResult result)
    {
        // HUNGER: +5 per segment (universal time cost)
        // This is THE ONLY place hunger increases due to time
        int hungerIncrease = result.SegmentsAdvanced * 5;
        await _resourceFacade.IncreaseHunger(hungerIncrease, "Time passes");

        // DAY TRANSITION: Process dawn effects (NPC decay)
        // Only when crossing into Morning (new day starts)
        if (result.CrossedDayBoundary && result.NewTimeBlock == TimeBlocks.Morning)
        {
            _resourceFacade.ProcessDayTransition();
        }

        // EMERGENCY CHECKING: Check for active emergencies at sync points
        // Emergencies interrupt normal gameplay and demand immediate response
        // HIGHLANDER: ActiveEmergencyState separates mutable state from immutable template
        ActiveEmergencyState activeEmergency = _emergencyFacade.CheckForActiveEmergency(_gameWorld.GetPlayer());
        if (activeEmergency != null)
        {
            _gameWorld.ActiveEmergency = activeEmergency;
            _messageSystem.AddSystemMessage(
                $"EMERGENCY: {activeEmergency.Template.Name}",
                SystemMessageTypes.Warning);
        }

        // SCENE EXPIRATION ENFORCEMENT (HIGHLANDER sync point for time-based state changes)
        // Check all active scenes for expiration based on current day
        // Scenes with ExpiresOnDay <= CurrentDay transition to Expired state
        // Expired scenes filtered out from SceneFacade queries (no longer visible to player)
        int currentDay = _gameWorld.CurrentDay;
        List<Scene> activeScenes = _gameWorld.Scenes
            .Where(s => s.State == SceneState.Active && s.ExpiresOnDay.HasValue)
            .ToList();

        foreach (Scene scene in activeScenes)
        {
            if (currentDay >= scene.ExpiresOnDay.Value)
            {
                scene.State = SceneState.Expired;

                // PROCEDURAL CONTENT TRACING: Update scene state to Expired
                if (_gameWorld.ProceduralTracer != null && _gameWorld.ProceduralTracer.IsEnabled)
                {
                    _gameWorld.ProceduralTracer.UpdateSceneState(scene, SceneState.Expired, DateTime.UtcNow);
                }
            }
        }
    }
}
