
/// <summary>
/// SPAWN FACADE - Executes cascading situation spawn rules (Sir Brante pattern)
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
/// - SituationFacade.ResolveInstantSituation() - After instant situation resolution (cascading)
/// - SituationCompletionHandler.CompleteSituation() - After challenge completion (cascading)
///
/// NO EVENTS - Synchronous execution orchestrated by GameFacade (facades never call each other)
/// </summary>
public class SpawnFacade
{
    private readonly GameWorld _gameWorld;
    private readonly TimeManager _timeManager;
    private readonly DependentResourceOrchestrationService _dependentResourceOrchestrationService;

    public SpawnFacade(
        GameWorld gameWorld,
        TimeManager timeManager,
        DependentResourceOrchestrationService dependentResourceOrchestrationService)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _dependentResourceOrchestrationService = dependentResourceOrchestrationService ?? throw new ArgumentNullException(nameof(dependentResourceOrchestrationService));
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
                Console.WriteLine($"[SpawnFacade] WARNING: Template '{rule.TemplateId}' not found - skipping spawn");
                continue;
            }

            // Clone template and apply spawn modifications
            Situation spawnedSituation = CloneTemplateWithModifications(template, parentSituation, rule);

            // Add spawned situation to parent's Scene (Scene owns Situations)
            if (parentSituation.ParentScene != null)
            {
                spawnedSituation.ParentScene = parentSituation.ParentScene;
                parentSituation.ParentScene.Situations.Add(spawnedSituation);
                Console.WriteLine($"[SpawnFacade] Spawned situation '{spawnedSituation.Name}' added to scene");

                // PROCEDURAL CONTENT TRACING: Record cascading situation spawn
                if (_gameWorld.ProceduralTracer != null && _gameWorld.ProceduralTracer.IsEnabled)
                {
                    string parentSceneNodeId = _gameWorld.ProceduralTracer.GetNodeIdForScene(parentSituation.ParentScene);
                    if (parentSceneNodeId != null)
                    {
                        // Push parent situation context so cascading situation links to it
                        string parentSituationNodeId = _gameWorld.ProceduralTracer.GetNodeIdForSituation(parentSituation);
                        if (parentSituationNodeId != null)
                        {
                            _gameWorld.ProceduralTracer.PushSituationContext(parentSituationNodeId);
                        }

                        // Record situation spawn (auto-links to parent situation via context stack)
                        // Note: SpawnRule doesn't differentiate Success/Failure, so we use SuccessSpawn
                        // Consider enhancing SpawnRule to track trigger type if needed
                        _gameWorld.ProceduralTracer.RecordSituationSpawn(
                            spawnedSituation,
                            parentSceneNodeId,
                            SituationSpawnTriggerType.SuccessSpawn
                        );

                        // Pop context after recording
                        if (parentSituationNodeId != null)
                        {
                            _gameWorld.ProceduralTracer.PopSituationContext();
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"[SpawnFacade] WARNING: Parent situation '{parentSituation.Name}' has no ParentScene - spawned situation orphaned!");
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
            Costs = CloneCosts(template.Costs),
            DifficultyModifiers = template.DifficultyModifiers.ToList(),
            SituationCards = template.SituationCards.ToList(),
            ConsequenceType = template.ConsequenceType,
            SetsResolutionMethod = template.SetsResolutionMethod,
            SetsRelationshipOutcome = template.SetsRelationshipOutcome,
            TransformDescription = template.TransformDescription,
            // PropertyReduction DELETED - old equipment system removed
            InteractionType = template.InteractionType,
            NavigationPayload = template.NavigationPayload, // TODO: Clone if mutable
            Tier = template.Tier,
            Repeatable = template.Repeatable,
            GeneratedNarrative = null, // Don't copy cached narrative
            NarrativeHints = template.NarrativeHints, // TODO: Clone if mutable
            Category = template.Category,
            ConnectionType = template.ConnectionType,
            LifecycleStatus = LifecycleStatus.Selectable,

            // Spawn tracking
            Template = null, // TODO: Replace with proper template reference when using SituationTemplate system
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
    /// Clone situation costs
    /// </summary>
    private SituationCosts CloneCosts(SituationCosts original)
    {
        if (original == null)
            return new SituationCosts();

        return new SituationCosts
        {
            Resolve = original.Resolve,
            Time = original.Time,
            Focus = original.Focus,
            Stamina = original.Stamina,
            Coins = original.Coins
        };
    }

    /// <summary>
    /// Apply requirement offsets to CompoundRequirement
    /// Makes spawned situation easier/harder based on parent situation outcome
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
                NumericRequirements = new List<NumericRequirement>()
            };

            foreach (NumericRequirement originalReq in originalPath.NumericRequirements)
            {
                NumericRequirement modifiedReq = new NumericRequirement
                {
                    Type = originalReq.Type,
                    Context = originalReq.Context,
                    Threshold = originalReq.Threshold,
                    Label = originalReq.Label
                };

                // Apply offsets based on requirement type
                if (originalReq.Type == "BondStrength" && offsets.BondStrengthOffset.HasValue)
                {
                    modifiedReq.Threshold = Math.Max(0, modifiedReq.Threshold + offsets.BondStrengthOffset.Value);
                }
                else if (originalReq.Type == "Scale" && offsets.ScaleOffset.HasValue)
                {
                    modifiedReq.Threshold = Math.Clamp(modifiedReq.Threshold + offsets.ScaleOffset.Value, -10, 10);
                }
                else if (offsets.NumericOffset.HasValue)
                {
                    // Apply numeric offset to other requirement types (Resolve, Coins, etc.)
                    modifiedReq.Threshold = Math.Max(0, modifiedReq.Threshold + offsets.NumericOffset.Value);
                }

                modifiedPath.NumericRequirements.Add(modifiedReq);
            }

            modified.OrPaths.Add(modifiedPath);
        }

        return modified;
    }

    /// <summary>
    /// Resolve placement for spawned situation based on PlacementStrategy
    /// NO STRING MATCHING - uses strongly-typed enum
    /// </summary>
    private void ResolvePlacement(Situation spawned, Situation parent, SpawnRule rule)
    {
        // PHASE 0.2: Placement properties DELETED from Situation - Scene owns placement now
        // Spawned situations inherit parent scene, which owns placement through Scene.PlacementLocation/PlacementNpc/PlacementRouteId
        // SpawnFacade NO LONGER sets placement properties on situations
        // Scene-Situation architecture: Scene is OWNER of placement hierarchy

        // TODO Phase 0.2: Implement Scene-based placement inheritance once Scene system refactored
        // For now, spawned situations will not have direct placement (rely on ParentScene lookup)
        spawned.Obligation = parent.Obligation;
    }

}
