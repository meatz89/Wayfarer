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
    /// HIGHLANDER: Accepts ObservationScene object, not string ID
    /// </summary>
    public ObservationContext CreateContext(ObservationScene scene)
    {
        // HIGHLANDER: Scene object passed directly, no lookup needed
        if (scene == null)
        {
            return new ObservationContext
            {
                IsValid = false,
                ErrorMessage = "Observation scene not provided"
            };
        }

        // ObservationScene has its own Location property (not situation-based)
        Location location = scene.Location;
        if (location == null)
        {
            return new ObservationContext
            {
                IsValid = false,
                ErrorMessage = "Observation scene has no location"
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
            ExaminedPoints = new List<ExaminationPoint>(scene.ExaminedPoints), // Object collection, not string IDs
            TimeDisplay = _timeFacade.GetTimeString()
        };
    }

    /// <summary>
    /// Examine a point within the observation scene
    /// HIGHLANDER: Accepts ExaminationPoint object, not pointId string
    /// </summary>
    public ObservationResult ExaminePoint(ObservationScene scene, ExaminationPoint point)
    {
        if (scene == null)
            return ObservationResult.Failed("Observation scene not found");

        if (point == null)
            return ObservationResult.Failed("Examination point not found");

        // Verify point belongs to this scene
        if (!scene.ExaminationPoints.Contains(point))
            return ObservationResult.Failed("Examination point does not belong to this scene");

        // Check if already examined (using object collection)
        if (scene.ExaminedPoints.Contains(point))
            return ObservationResult.Failed("This point has already been examined");

        // Check if hidden and not revealed
        if (point.IsHidden)
        {
            bool isRevealed = scene.ExaminationPoints.Any(p =>
                scene.ExaminedPoints.Contains(p) &&
                p.RevealsExaminationPoint == point); // Object reference, not ID string

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
            int statLevel = point.RequiredStat.Value switch
            {
                PlayerStatType.Insight => player.Insight,
                PlayerStatType.Rapport => player.Rapport,
                PlayerStatType.Authority => player.Authority,
                PlayerStatType.Diplomacy => player.Diplomacy,
                PlayerStatType.Cunning => player.Cunning,
                _ => 0
            };
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

        // Mark as examined (add to object collection)
        scene.ExaminedPoints.Add(point);
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

        // Check for item finding (using object reference)
        if (point.FoundItem != null && point.FindItemChance > 0)
        {
            int roll = _random.Next(1, 101);
            if (roll <= point.FindItemChance)
            {
                result.ItemFound = point.FoundItem;
                _messageSystem.AddSystemMessage($"Found item: {point.FoundItem.Name}", SystemMessageTypes.Info);
            }
        }

        // Spawn situations (using object reference)
        if (point.SpawnedSituation != null)
        {
            result.SituationSpawned = point.SpawnedSituation;
            _messageSystem.AddSystemMessage($"New situation available: {point.SpawnedSituation.Name}", SystemMessageTypes.Info);
        }

        // Spawn conversations (using object reference)
        if (point.SpawnedConversation != null)
        {
            result.ConversationSpawned = point.SpawnedConversation;
            _messageSystem.AddSystemMessage($"New conversation available: {point.SpawnedConversation.Name}", SystemMessageTypes.Info);
        }

        // Check for revealed points (using object reference)
        if (point.RevealsExaminationPoint != null)
        {
            result.PointRevealed = point.RevealsExaminationPoint;
            _messageSystem.AddSystemMessage($"New examination point revealed: {point.RevealsExaminationPoint.Title}", SystemMessageTypes.Info);
        }

        // Check if scene is fully examined
        int totalAvailablePoints = scene.ExaminationPoints.Count(p => !p.IsHidden);
        int totalExaminedPoints = scene.ExaminedPoints.Count;

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
        Dictionary<PlayerStatType, int> stats = new Dictionary<PlayerStatType, int>
        {
            { PlayerStatType.Insight, player.Insight },
            { PlayerStatType.Rapport, player.Rapport },
            { PlayerStatType.Authority, player.Authority },
            { PlayerStatType.Diplomacy, player.Diplomacy },
            { PlayerStatType.Cunning, player.Cunning }
        };

        return stats;
    }

    /// <summary>
    /// Get all observation scenes available at a specific location
    /// Checks location match, completion status, and knowledge requirements
    /// </summary>
    public List<ObservationScene> GetAvailableScenesAtLocation(Location location)
    {
        Player player = _gameWorld.GetPlayer();

        return _gameWorld.ObservationScenes
            .Where(s => s.Location == location)
            .Where(s => !s.IsCompleted || s.IsRepeatable)
            .Where(s => s.RequiredKnowledge.All(k => player.Knowledge.Contains(k)))
            .ToList();
    }
}

/// <summary>
/// Result of examining a point in an observation scene
/// HIGHLANDER: Object references ONLY, no string IDs
/// </summary>
public class ObservationResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public ExaminationPoint PointExamined { get; set; }
    public List<string> KnowledgeGained { get; set; } = new List<string>();
    public Item ItemFound { get; set; } // Object reference, not ItemFound string ID
    public Situation SituationSpawned { get; set; } // Object reference, not SituationSpawned string ID
    public ConversationTree ConversationSpawned { get; set; } // Object reference, not ConversationSpawned string ID
    public ExaminationPoint PointRevealed { get; set; }
    public bool SceneCompleted { get; set; }

    public static ObservationResult Failed(string message)
    {
        return new ObservationResult { Success = false, Message = message };
    }
}
