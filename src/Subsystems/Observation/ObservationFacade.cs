/// <summary>
/// Public facade for observation scene operations.
/// Handles scene investigation with multiple examination points and resource management.
/// This is the public interface for the Observation subsystem.
/// </summary>
public class ObservationFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly ResourceFacade _resourceFacade;
    private readonly TimeFacade _timeFacade;
    private readonly Random _random;

    public ObservationFacade(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        ResourceFacade resourceFacade,
        TimeFacade timeFacade)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _resourceFacade = resourceFacade ?? throw new ArgumentNullException(nameof(resourceFacade));
        _timeFacade = timeFacade ?? throw new ArgumentNullException(nameof(timeFacade));
        _random = new Random();
    }

    /// <summary>
    /// Create context for an observation scene screen
    /// </summary>
    public ObservationContext CreateContext(string sceneId)
    {
        ObservationScene scene = _gameWorld.ObservationScenes.FirstOrDefault(s => s.Id == sceneId);
        if (scene == null)
        {
            return new ObservationContext
            {
                IsValid = false,
                ErrorMessage = $"Observation scene '{sceneId}' not found"
            };
        }

        Location location = scene.Location;
        if (location == null)
        {
            return new ObservationContext
            {
                IsValid = false,
                ErrorMessage = "Location not found for observation scene"
            };
        }

        Player player = _gameWorld.GetPlayer();

        // Check required knowledge
        foreach (string knowledge in scene.RequiredKnowledge)
        {
            if (!player.Knowledge.Contains(knowledge))
            {
                return new ObservationContext
                {
                    IsValid = false,
                    ErrorMessage = "You don't have the required knowledge to investigate this scene"
                };
            }
        }

        // Check if already completed and not repeatable
        if (scene.IsCompleted && !scene.IsRepeatable)
        {
            return new ObservationContext
            {
                IsValid = false,
                ErrorMessage = "This scene has already been thoroughly investigated"
            };
        }

        return new ObservationContext
        {
            IsValid = true,
            Scene = scene,
            Location = location,
            CurrentFocus = player.Focus,
            MaxFocus = player.MaxFocus,
            PlayerStats = BuildPlayerStats(player),
            PlayerKnowledge = new List<string>(player.Knowledge),
            ExaminedPointIds = new List<string>(scene.ExaminedPointIds),
            TimeDisplay = _timeFacade.GetTimeString()
        };
    }

    /// <summary>
    /// Examine a point within the observation scene
    /// </summary>
    public ObservationResult ExaminePoint(string sceneId, string pointId)
    {
        ObservationScene scene = _gameWorld.ObservationScenes.FirstOrDefault(s => s.Id == sceneId);
        if (scene == null)
            return ObservationResult.Failed("Observation scene not found");

        ExaminationPoint point = scene.ExaminationPoints.FirstOrDefault(p => p.Id == pointId);
        if (point == null)
            return ObservationResult.Failed("Examination point not found");

        // Check if already examined
        if (scene.ExaminedPointIds.Contains(pointId))
            return ObservationResult.Failed("This point has already been examined");

        // Check if hidden and not revealed
        if (point.IsHidden)
        {
            bool isRevealed = scene.ExaminationPoints.Any(p =>
                scene.ExaminedPointIds.Contains(p.Id) &&
                p.RevealsExaminationPointId == pointId);

            if (!isRevealed)
                return ObservationResult.Failed("This examination point is not yet available");
        }

        Player player = _gameWorld.GetPlayer();

        // Validate resources
        if (player.Focus < point.FocusCost)
            return ObservationResult.Failed($"Not enough Focus (need {point.FocusCost}, have {player.Focus})");

        // Validate stat requirements
        if (point.RequiredStat.HasValue && point.RequiredStatLevel.HasValue)
        {
            int statLevel = player.Stats.GetLevel(point.RequiredStat.Value);
            if (statLevel < point.RequiredStatLevel.Value)
            {
                return ObservationResult.Failed(
                    $"Requires {point.RequiredStat} level {point.RequiredStatLevel} (you have {statLevel})");
            }
        }

        // Validate knowledge requirements
        foreach (string knowledge in point.RequiredKnowledge)
        {
            if (!player.Knowledge.Contains(knowledge))
            {
                return ObservationResult.Failed($"Missing required knowledge: {knowledge}");
            }
        }

        // Apply costs
        if (point.FocusCost > 0)
        {
            player.Focus -= point.FocusCost;
        }
        if (point.TimeCost > 0)
        {
            _timeFacade.AdvanceSegments(point.TimeCost);
        }

        // Mark as examined
        scene.ExaminedPointIds.Add(pointId);
        point.IsExamined = true;

        ObservationResult result = new ObservationResult
        {
            Success = true,
            PointExamined = point
        };

        // Grant knowledge
        foreach (string knowledge in point.GrantedKnowledge)
        {
            if (!player.Knowledge.Contains(knowledge))
            {
                player.Knowledge.Add(knowledge);
                result.KnowledgeGained.Add(knowledge);
                _messageSystem.AddSystemMessage($"Discovered: {knowledge}", SystemMessageTypes.Info);
            }
        }

        // Check for item finding
        if (!string.IsNullOrEmpty(point.FoundItemId) && point.FindItemChance > 0)
        {
            int roll = _random.Next(1, 101); // 1-100
            if (roll <= point.FindItemChance)
            {
                // TODO: Implement item granting when inventory system is in place
                result.ItemFound = point.FoundItemId;
                _messageSystem.AddSystemMessage($"Found item: {point.FoundItemId}", SystemMessageTypes.Info);
            }
        }

        // Spawn situations
        if (!string.IsNullOrEmpty(point.SpawnedSituationId))
        {
            result.SituationSpawned = point.SpawnedSituationId;
            _messageSystem.AddSystemMessage($"New situation available: {point.SpawnedSituationId}", SystemMessageTypes.Info);
        }

        // Spawn conversations
        if (!string.IsNullOrEmpty(point.SpawnedConversationId))
        {
            result.ConversationSpawned = point.SpawnedConversationId;
            _messageSystem.AddSystemMessage($"New conversation available: {point.SpawnedConversationId}", SystemMessageTypes.Info);
        }

        // Check for revealed points
        if (!string.IsNullOrEmpty(point.RevealsExaminationPointId))
        {
            ExaminationPoint revealedPoint = scene.ExaminationPoints
                .FirstOrDefault(p => p.Id == point.RevealsExaminationPointId);
            if (revealedPoint != null)
            {
                result.PointRevealed = revealedPoint;
                _messageSystem.AddSystemMessage($"New examination point revealed: {revealedPoint.Title}", SystemMessageTypes.Info);
            }
        }

        // Check if scene is fully examined
        int totalAvailablePoints = scene.ExaminationPoints.Count(p => !p.IsHidden);
        int totalExaminedPoints = scene.ExaminedPointIds.Count;

        if (totalExaminedPoints >= totalAvailablePoints)
        {
            scene.IsCompleted = true;
            result.SceneCompleted = true;
            _messageSystem.AddSystemMessage("Scene investigation complete", SystemMessageTypes.Info);
        }

        return result;
    }

    private Dictionary<PlayerStatType, int> BuildPlayerStats(Player player)
    {
        Dictionary<PlayerStatType, int> stats = new Dictionary<PlayerStatType, int>();

        foreach (PlayerStatType statType in Enum.GetValues(typeof(PlayerStatType)))
        {
            stats[statType] = player.Stats.GetLevel(statType);
        }

        return stats;
    }

    /// <summary>
    /// Get all observation scenes available at a specific location
    /// Checks location match, completion status, and knowledge requirements
    /// </summary>
    public List<ObservationScene> GetAvailableScenesAtLocation(string locationId)
    {
        Player player = _gameWorld.GetPlayer();

        return _gameWorld.ObservationScenes
            .Where(s => s.Location?.Id == locationId)
            .Where(s => !s.IsCompleted || s.IsRepeatable)
            .Where(s => s.RequiredKnowledge.All(k => player.Knowledge.Contains(k)))
            .ToList();
    }
}

/// <summary>
/// Result of examining a point in an observation scene
/// </summary>
public class ObservationResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public ExaminationPoint PointExamined { get; set; }
    public List<string> KnowledgeGained { get; set; } = new List<string>();
    public string ItemFound { get; set; }
    public string SituationSpawned { get; set; }
    public string ConversationSpawned { get; set; }
    public ExaminationPoint PointRevealed { get; set; }
    public bool SceneCompleted { get; set; }

    public static ObservationResult Failed(string message)
    {
        return new ObservationResult { Success = false, Message = message };
    }
}
