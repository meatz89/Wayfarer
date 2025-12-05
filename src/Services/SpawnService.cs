
/// <summary>
/// SPAWN SERVICE - Executes cascading situation spawn rules (Sir Brante pattern)
///
/// RESPONSIBILITY:
/// - Parent situation completion spawns child situations
/// - Clones template situations
/// - Applies requirement offsets (makes children easier/harder)
/// - Validates spawn conditions before execution
/// - Adds spawned situations to parent Scene.Situations collection
///
/// Scene spawning moved to reward-driven architecture (RewardApplicationService)
/// Scenes spawn via ScenesToSpawn rewards from choice execution, not condition-based triggers
///
/// Called by:
/// - SituationCompletionHandler.CompleteSituation() - After challenge completion (cascading)
/// - SituationCompletionHandler.FailSituation() - After failure (cascading)
///
/// ARCHITECTURE: Domain service (no facade dependencies) - can be called by other services
/// </summary>
public class SpawnService
{
    private readonly GameWorld _gameWorld;
    private readonly TimeManager _timeManager;
    private readonly ProceduralContentTracer _proceduralTracer;

    public SpawnService(
        GameWorld gameWorld,
        TimeManager timeManager,
        ProceduralContentTracer proceduralTracer)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _proceduralTracer = proceduralTracer ?? throw new ArgumentNullException(nameof(proceduralTracer));
    }

    /// <summary>
    /// Execute spawn rules - create child situations from parent situation completion
    /// Validates conditions, clones templates, applies offsets, adds to GameWorld
    /// </summary>
    public void ExecuteSpawnRules(List<SpawnRule> spawnRules, Situation parentSituation)
    {
        if (spawnRules == null || spawnRules.Count == 0)
            return;

        if (parentSituation == null)
            throw new ArgumentNullException(nameof(parentSituation));

        Player player = _gameWorld.GetPlayer();

        foreach (SpawnRule rule in spawnRules)
        {
            // Validate spawn conditions (if present)
            if (rule.Conditions != null && !ValidateSituationSpawnConditions(rule.Conditions, player))
            {
                // Conditions not met - skip this spawn
                continue;
            }

            // Find template situation by TemplateId
            Situation template = _gameWorld.Scenes.SelectMany(sc => sc.Situations).FirstOrDefault(sit => sit.TemplateId == rule.TemplateId);
            if (template == null)
            {
                // Template not found - skip this spawn
                Console.WriteLine($"[SpawnService] WARNING: Template '{rule.TemplateId}' not found - skipping spawn");
                continue;
            }

            // Clone template and apply spawn modifications
            Situation spawnedSituation = CloneTemplateWithModifications(template, parentSituation, rule);

            // Add spawned situation to parent's Scene (Scene owns Situations)
            if (parentSituation.ParentScene != null)
            {
                spawnedSituation.ParentScene = parentSituation.ParentScene;
                parentSituation.ParentScene.Situations.Add(spawnedSituation);
                Console.WriteLine($"[SpawnService] Spawned situation '{spawnedSituation.Name}' added to scene");

                // PROCEDURAL CONTENT TRACING: Record cascading situation spawn
                SceneSpawnNode parentSceneNode = _proceduralTracer.GetNodeForScene(parentSituation.ParentScene);
                if (parentSceneNode != null)
                {
                    // Push parent situation context so cascading situation links to it
                    SituationSpawnNode parentSituationNode = _proceduralTracer.GetNodeForSituation(parentSituation);
                    if (parentSituationNode != null)
                    {
                        _proceduralTracer.PushSituationContext(parentSituationNode);
                    }

                    // Record situation spawn (auto-links to parent situation via context stack)
                    // Note: SpawnRule doesn't differentiate Success/Failure, so we use SuccessSpawn
                    // Consider enhancing SpawnRule to track trigger type if needed
                    _proceduralTracer.RecordSituationSpawn(
                        spawnedSituation,
                        parentSceneNode,
                        SituationSpawnTriggerType.SuccessSpawn,
                        EntityResolutionContext.Empty()
                    );

                    // Pop context after recording
                    if (parentSituationNode != null)
                    {
                        _proceduralTracer.PopSituationContext();
                    }
                }
            }
            else
            {
                Console.WriteLine($"[SpawnService] WARNING: Parent situation '{parentSituation.Name}' has no ParentScene - spawned situation orphaned!");
            }

            // ActiveSituationIds DELETED from NPC/Location - situations now tracked via Scene.Situations
        }
    }

    /// <summary>
    /// Validate spawn conditions - all conditions must be met for spawn to occur
    /// </summary>
    private bool ValidateSituationSpawnConditions(SituationSpawnConditions conditions, Player player)
    {
        // Check MinResolve
        if (conditions.MinResolve.HasValue && player.Resolve < conditions.MinResolve.Value)
        {
            return false;
        }

        // Check RequiredState
        if (!string.IsNullOrEmpty(conditions.RequiredState))
        {
            if (Enum.TryParse<StateType>(conditions.RequiredState, true, out StateType requiredStateType))
            {
                bool hasState = player.ActiveStates.Any(s => s.Type == requiredStateType);
                if (!hasState)
                {
                    return false;
                }
            }
        }

        // Check RequiredAchievement
        if (conditions.RequiredAchievement != null)
        {
            // HIGHLANDER: Compare Achievement objects directly
            bool hasAchievement = player.EarnedAchievements.Any(a => a.Achievement == conditions.RequiredAchievement);
            if (!hasAchievement)
            {
                return false;
            }
        }

        return true; // All conditions met
    }

    /// <summary>
    /// Clone template situation and apply spawn modifications
    /// Creates new situation instance with spawn tracking and requirement offsets
    /// HIGHLANDER: NO Id property - situations identified by object reference
    /// </summary>
    private Situation CloneTemplateWithModifications(Situation template, Situation parentSituation, SpawnRule rule)
    {
        // Create cloned situation (NO Id generation - HIGHLANDER principle)
        Situation spawned = new Situation
        {
            Name = template.Name,
            Description = template.Description,
            SystemType = template.SystemType,
            Deck = template.Deck, // Object reference, not DeckId
            IsIntroAction = template.IsIntroAction,
            // IsAvailable and IsCompleted are computed properties from Status enum (no assignment needed)
            DeleteOnSuccess = template.DeleteOnSuccess,
            EntryCost = CloneEntryCost(template.EntryCost),
            DifficultyModifiers = template.DifficultyModifiers.ToList(),
            SituationCards = template.SituationCards.ToList(),
            ConsequenceType = template.ConsequenceType,
            SetsResolutionMethod = template.SetsResolutionMethod,
            SetsRelationshipOutcome = template.SetsRelationshipOutcome,
            TransformDescription = template.TransformDescription,
            // PropertyReduction DELETED - old equipment system removed
            InteractionType = template.InteractionType,
            NavigationPayload = template.NavigationPayload,
            Repeatable = template.Repeatable,
            GeneratedNarrative = null, // Don't copy cached narrative
            NarrativeHints = template.NarrativeHints,
            Category = template.Category,
            ConnectionType = template.ConnectionType,
            LifecycleStatus = LifecycleStatus.Selectable,

            // Spawn tracking
            Template = null,
            ParentSituation = parentSituation, // Object reference, not ParentSituationId
            Lifecycle = new SpawnTracking
            {
                SpawnedDay = _timeManager.CurrentDay,
                SpawnedTimeBlock = _timeManager.CurrentTimeBlock,
                SpawnedSegment = _timeManager.CurrentSegment
            },

            // ProjectedBondChanges/ProjectedScaleShifts/ProjectedStates DELETED - stored projection pattern

            // Clone spawn rules (spawned situations can spawn children)
            SuccessSpawns = template.SuccessSpawns.ToList(),
            FailureSpawns = template.FailureSpawns.ToList(),

            // Apply CompoundRequirement with offsets
            CompoundRequirement = ApplyRequirementOffsets(template.CompoundRequirement, rule.RequirementOffsets)
        };

        // Resolve placement based on PlacementStrategy (strongly-typed enum - NO string matching)
        ResolvePlacement(spawned, parentSituation, rule);

        return spawned;
    }

    /// <summary>
    /// HIGHLANDER: Clone entry cost (Consequence with negative values)
    /// </summary>
    private Consequence CloneEntryCost(Consequence original)
    {
        if (original == null)
            return Consequence.None();

        return new Consequence
        {
            Resolve = original.Resolve,
            TimeSegments = original.TimeSegments,
            Focus = original.Focus,
            Stamina = original.Stamina,
            Coins = original.Coins,
            Health = original.Health,
            Hunger = original.Hunger
        };
    }

    /// <summary>
    /// Apply requirement offsets to CompoundRequirement
    /// Makes spawned situation easier/harder based on parent situation outcome
    /// Uses Explicit Property Principle - directly modifies typed properties instead of string-based routing
    /// </summary>
    private CompoundRequirement ApplyRequirementOffsets(CompoundRequirement original, RequirementOffsets offsets)
    {
        if (original == null || original.OrPaths.Count == 0)
            return null; // No requirements

        if (offsets == null || (!offsets.BondStrengthOffset.HasValue && !offsets.ScaleOffset.HasValue && !offsets.NumericOffset.HasValue))
            return original; // No offsets to apply

        // Clone CompoundRequirement with modified thresholds
        CompoundRequirement modified = new CompoundRequirement
        {
            OrPaths = new List<OrPath>()
        };

        foreach (OrPath originalPath in original.OrPaths)
        {
            OrPath modifiedPath = new OrPath
            {
                Label = originalPath.Label,
                // Clone explicit stat properties with NumericOffset applied
                InsightRequired = ApplyOffset(originalPath.InsightRequired, offsets.NumericOffset),
                RapportRequired = ApplyOffset(originalPath.RapportRequired, offsets.NumericOffset),
                AuthorityRequired = ApplyOffset(originalPath.AuthorityRequired, offsets.NumericOffset),
                DiplomacyRequired = ApplyOffset(originalPath.DiplomacyRequired, offsets.NumericOffset),
                CunningRequired = ApplyOffset(originalPath.CunningRequired, offsets.NumericOffset),
                // Clone resource properties with NumericOffset applied
                ResolveRequired = ApplyOffset(originalPath.ResolveRequired, offsets.NumericOffset),
                CoinsRequired = ApplyOffset(originalPath.CoinsRequired, offsets.NumericOffset),
                // Clone progression property
                SituationCountRequired = ApplyOffset(originalPath.SituationCountRequired, offsets.NumericOffset),
                // Clone relationship properties with BondStrengthOffset
                BondNpc = originalPath.BondNpc,
                BondStrengthRequired = ApplyOffset(originalPath.BondStrengthRequired, offsets.BondStrengthOffset),
                // Clone scale properties with ScaleOffset
                RequiredScaleType = originalPath.RequiredScaleType,
                ScaleValueRequired = ApplyScaleOffset(originalPath.ScaleValueRequired, offsets.ScaleOffset),
                // Clone boolean requirement references
                RequiredAchievement = originalPath.RequiredAchievement,
                RequiredState = originalPath.RequiredState,
                RequiredItem = originalPath.RequiredItem
            };

            modified.OrPaths.Add(modifiedPath);
        }

        return modified;
    }

    /// <summary>
    /// Apply offset to nullable int, clamping to minimum 0
    /// </summary>
    private int? ApplyOffset(int? original, int? offset)
    {
        if (!original.HasValue) return null;
        if (!offset.HasValue) return original;
        return Math.Max(0, original.Value + offset.Value);
    }

    /// <summary>
    /// Apply offset to scale value, clamping to -10 to +10 range
    /// </summary>
    private int? ApplyScaleOffset(int? original, int? offset)
    {
        if (!original.HasValue) return null;
        if (!offset.HasValue) return original;
        return Math.Clamp(original.Value + offset.Value, -10, 10);
    }

    /// <summary>
    /// Resolve placement for spawned situation based on PlacementStrategy
    /// NO STRING MATCHING - uses strongly-typed enum
    /// </summary>
    private void ResolvePlacement(Situation spawned, Situation parent, SpawnRule rule)
    {
        spawned.Obligation = parent.Obligation;
    }

}
