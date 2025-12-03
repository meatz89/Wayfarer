/// <summary>
/// Public facade for observation scene operations.
/// Handles scene investigation with multiple examination points and resource management.
/// This is the public interface for the Observation subsystem.
/// HIGHLANDER: Mutable state (ObservationSceneState) separated from immutable templates (ObservationScene).
/// </summary>
public class ObservationFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly TimeManager _timeManager;
    private readonly RewardApplicationService _rewardApplicationService;

    public ObservationFacade(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        TimeManager timeManager,
        RewardApplicationService rewardApplicationService)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _rewardApplicationService = rewardApplicationService ?? throw new ArgumentNullException(nameof(rewardApplicationService));
    }

    /// <summary>
    /// Get or create ObservationSceneState for a template.
    /// HIGHLANDER: Mutable state separated from immutable template.
    /// </summary>
    private ObservationSceneState GetOrCreateState(ObservationScene template)
    {
        ObservationSceneState state = _gameWorld.ObservationSceneStates.FirstOrDefault(s => s.Template == template);
        if (state == null)
        {
            state = new ObservationSceneState { Template = template };
            _gameWorld.ObservationSceneStates.Add(state);
        }
        return state;
    }

    /// <summary>
    /// Create context for an observation scene screen
    /// HIGHLANDER: Accepts ObservationScene object, accesses state via GetOrCreateState.
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
        ObservationSceneState state = GetOrCreateState(scene);

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

        // Check if already completed and not repeatable (state, not template)
        if (state.IsCompleted && !scene.IsRepeatable)
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
            // DOMAIN COLLECTION PRINCIPLE: Explicit properties for player stats
            InsightLevel = player.Insight,
            RapportLevel = player.Rapport,
            AuthorityLevel = player.Authority,
            DiplomacyLevel = player.Diplomacy,
            CunningLevel = player.Cunning,
            PlayerKnowledge = new List<string>(player.Knowledge),
            ExaminedPoints = new List<ExaminationPoint>(state.ExaminedPoints), // From state, not template
            TimeDisplay = _timeManager.GetSegmentDisplay()
        };
    }

    /// <summary>
    /// Examine a point within the observation scene
    /// HIGHLANDER: Accepts ExaminationPoint object, not pointId string. Mutates state, not template.
    /// TWO PILLARS: Uses CompoundRequirement for availability, Consequence for costs
    /// </summary>
    public async Task<ObservationResult> ExaminePoint(ObservationScene scene, ExaminationPoint point)
    {
        if (scene == null)
            return ObservationResult.Failed("Observation scene not found");

        if (point == null)
            return ObservationResult.Failed("Examination point not found");

        // Verify point belongs to this scene
        if (!scene.ExaminationPoints.Contains(point))
            return ObservationResult.Failed("Examination point does not belong to this scene");

        ObservationSceneState state = GetOrCreateState(scene);

        // Check if already examined (using state, not template)
        if (state.ExaminedPoints.Contains(point))
            return ObservationResult.Failed("This point has already been examined");

        // Check if hidden and not revealed
        if (point.IsHidden)
        {
            bool isRevealed = scene.ExaminationPoints.Any(p =>
                state.ExaminedPoints.Contains(p) &&
                p.RevealsExaminationPoint == point); // Object reference, not ID string

            if (!isRevealed)
                return ObservationResult.Failed("This examination point is not yet available");
        }

        Player player = _gameWorld.GetPlayer();

        // TWO PILLARS: Validate resources via CompoundRequirement
        CompoundRequirement requirement = new CompoundRequirement();
        OrPath requirementPath = new OrPath { FocusRequired = point.FocusCost };

        // Add stat requirement if present
        if (point.RequiredStat.HasValue && point.RequiredStatLevel.HasValue)
        {
            switch (point.RequiredStat.Value)
            {
                case PlayerStatType.Insight:
                    requirementPath.InsightRequired = point.RequiredStatLevel.Value;
                    break;
                case PlayerStatType.Rapport:
                    requirementPath.RapportRequired = point.RequiredStatLevel.Value;
                    break;
                case PlayerStatType.Authority:
                    requirementPath.AuthorityRequired = point.RequiredStatLevel.Value;
                    break;
                case PlayerStatType.Diplomacy:
                    requirementPath.DiplomacyRequired = point.RequiredStatLevel.Value;
                    break;
                case PlayerStatType.Cunning:
                    requirementPath.CunningRequired = point.RequiredStatLevel.Value;
                    break;
            }
        }
        requirement.OrPaths.Add(requirementPath);

        if (!requirement.IsAnySatisfied(player, _gameWorld))
        {
            return ObservationResult.Failed("Requirements not met");
        }

        // Validate knowledge requirements
        foreach (string knowledge in point.RequiredKnowledge)
        {
            if (!player.Knowledge.Contains(knowledge))
            {
                return ObservationResult.Failed($"Missing required knowledge: {knowledge}");
            }
        }

        // TWO PILLARS: Apply costs via Consequence class
        if (point.FocusCost > 0)
        {
            Consequence focusCost = new Consequence { Focus = -point.FocusCost };
            await _rewardApplicationService.ApplyConsequence(focusCost, null);
        }
        if (point.TimeCost > 0)
        {
            _timeManager.AdvanceSegments(point.TimeCost);
        }

        // Mark as examined (add to state's object collection, not template)
        state.ExaminedPoints.Add(point);
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
                _messageSystem.AddSystemMessage($"Discovered: {knowledge}", SystemMessageTypes.Info, null);
            }
        }

        // Check for item finding (using object reference)
        // DDR-007: Deterministic item finding based on point properties
        if (point.FoundItem != null && point.FindItemChance > 0)
        {
            // Deterministic outcome based on examination point title hash
            // Same point always produces same result (predictable)
            int deterministicValue = Math.Abs(point.Title.GetHashCode()) % 100 + 1;
            if (deterministicValue <= point.FindItemChance)
            {
                result.ItemFound = point.FoundItem;
                _messageSystem.AddSystemMessage($"Found item: {point.FoundItem.Name}", SystemMessageTypes.Info, null);
            }
        }

        // Spawn situations (using object reference)
        if (point.SpawnedSituation != null)
        {
            result.SituationSpawned = point.SpawnedSituation;
            _messageSystem.AddSystemMessage($"New situation available: {point.SpawnedSituation.Name}", SystemMessageTypes.Info, null);
        }

        // Spawn conversations (using object reference)
        if (point.SpawnedConversation != null)
        {
            result.ConversationSpawned = point.SpawnedConversation;
            _messageSystem.AddSystemMessage($"New conversation available: {point.SpawnedConversation.Name}", SystemMessageTypes.Info, null);
        }

        // Check for revealed points (using object reference)
        if (point.RevealsExaminationPoint != null)
        {
            result.PointRevealed = point.RevealsExaminationPoint;
            _messageSystem.AddSystemMessage($"New examination point revealed: {point.RevealsExaminationPoint.Title}", SystemMessageTypes.Info, null);
        }

        // Check if scene is fully examined (using state, not template)
        int totalAvailablePoints = scene.ExaminationPoints.Count(p => !p.IsHidden);
        int totalExaminedPoints = state.ExaminedPoints.Count;

        if (totalExaminedPoints >= totalAvailablePoints)
        {
            state.IsCompleted = true; // Mutate state, not template
            result.SceneCompleted = true;
            _messageSystem.AddSystemMessage("Scene investigation complete", SystemMessageTypes.Info, null);
        }

        return result;
    }


    /// <summary>
    /// Get all observation scenes available at a specific location
    /// Checks location match, completion status (from state), and knowledge requirements
    /// HIGHLANDER: Checks state for completion, not template.
    /// </summary>
    public List<ObservationScene> GetAvailableScenesAtLocation(Location location)
    {
        Player player = _gameWorld.GetPlayer();

        return _gameWorld.ObservationScenes
            .Where(template => template.Location == location)
            .Where(template =>
            {
                ObservationSceneState state = GetOrCreateState(template);
                return !state.IsCompleted || template.IsRepeatable;
            })
            .Where(template => template.RequiredKnowledge.All(k => player.Knowledge.Contains(k)))
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
