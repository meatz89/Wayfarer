
/// <summary>
/// SPAWN FACADE - Executes spawn rules and orchestrates automatic scene spawning
///
/// TWO RESPONSIBILITIES:
/// 1. Cascading Situations: Parent situation completion spawns child situations (Sir Brante pattern)
/// 2. Automatic Scenes: Checks spawn conditions at trigger points and instantiates eligible SceneTemplates
///
/// Sir Brante Pattern (Situations):
/// - Clones template situations
/// - Applies requirement offsets (makes children easier/harder)
/// - Validates spawn conditions before execution
/// - Adds spawned situations to GameWorld and ActiveSituationIds
///
/// Automatic Spawning (Scenes):
/// - Checks SceneTemplates with spawn conditions
/// - Evaluates conditions against current player/world/entity state
/// - Instantiates eligible scenes via SceneInstantiator
/// - Called at trigger points: time advancement, location entry, NPC interaction
///
/// Called by:
/// - SituationFacade.ResolveInstantSituation() - After instant situation resolution (cascading)
/// - SituationCompletionHandler.CompleteSituation() - After challenge completion (cascading)
/// - GameFacade.RestAtLocationAsync() - Time trigger orchestration (after TimeFacade.AdvanceToNextDay)
/// - GameFacade.TravelToDestinationAsync() - Location trigger orchestration (after position update)
///
/// NO EVENTS - Synchronous execution orchestrated by GameFacade (facades never call each other)
/// </summary>
public class SpawnFacade
{
private readonly GameWorld _gameWorld;
private readonly TimeManager _timeManager;
private readonly SpawnConditionsEvaluator _conditionsEvaluator;
private readonly SceneInstanceFacade _sceneInstanceFacade;
private readonly DependentResourceOrchestrationService _dependentResourceOrchestrationService;
private readonly ProceduralAStoryService _proceduralAStoryService;

public SpawnFacade(
    GameWorld gameWorld,
    TimeManager timeManager,
    SpawnConditionsEvaluator conditionsEvaluator,
    SceneInstanceFacade sceneInstanceFacade,
    DependentResourceOrchestrationService dependentResourceOrchestrationService,
    ProceduralAStoryService proceduralAStoryService)
{
    _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
    _conditionsEvaluator = conditionsEvaluator ?? throw new ArgumentNullException(nameof(conditionsEvaluator));
    _sceneInstanceFacade = sceneInstanceFacade ?? throw new ArgumentNullException(nameof(sceneInstanceFacade));
    _dependentResourceOrchestrationService = dependentResourceOrchestrationService ?? throw new ArgumentNullException(nameof(dependentResourceOrchestrationService));
    _proceduralAStoryService = proceduralAStoryService ?? throw new ArgumentNullException(nameof(proceduralAStoryService));
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

        // Find template situation
        Situation template = _gameWorld.Scenes.SelectMany(sc => sc.Situations).FirstOrDefault(sit => sit.Id == rule.TemplateId);
        if (template == null)
        {
            // Template not found - skip this spawn
            // TODO: Log warning about missing template
            continue;
        }

        // Clone template and apply spawn modifications
        Situation spawnedSituation = CloneTemplateWithModifications(template, parentSituation, rule);

        // Add spawned situation to parent's Scene (Scene owns Situations)
        if (parentSituation.ParentScene != null)
        {
            spawnedSituation.ParentScene = parentSituation.ParentScene;
            parentSituation.ParentScene.Situations.Add(spawnedSituation);
            Console.WriteLine($"[SpawnFacade] Spawned situation '{spawnedSituation.Id}' added to scene '{parentSituation.ParentScene.Id}'");
        }
        else
        {
            Console.WriteLine($"[SpawnFacade] WARNING: Parent situation '{parentSituation.Id}' has no ParentScene - spawned situation orphaned!");
        }

        // Add to ActiveSituationIds based on placement
        AddToActiveSituations(spawnedSituation);
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
    if (!string.IsNullOrEmpty(conditions.RequiredAchievement))
    {
        bool hasAchievement = player.EarnedAchievements.Any(a => a.AchievementId == conditions.RequiredAchievement);
        if (!hasAchievement)
        {
            return false;
        }
    }

    return true; // All conditions met
}

/// <summary>
/// Clone template situation and apply spawn modifications
/// Creates new situation instance with unique ID, spawn tracking, and requirement offsets
/// </summary>
private Situation CloneTemplateWithModifications(Situation template, Situation parentSituation, SpawnRule rule)
{
    // Generate unique ID for spawned situation (template ID + parent ID + timestamp)
    string spawnedId = $"{template.Id}_spawned_{parentSituation.Id}_{_timeManager.CurrentDay}_{_timeManager.CurrentSegment}";

    // Create cloned situation
    Situation spawned = new Situation
    {
        Id = spawnedId,
        Name = template.Name,
        Description = template.Description,
        SystemType = template.SystemType,
        DeckId = template.DeckId,
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
        ParentSituationId = parentSituation.Id, // Track parent situation
        Lifecycle = new SpawnTracking
        {
            SpawnedDay = _timeManager.CurrentDay,
            SpawnedTimeBlock = _timeManager.CurrentTimeBlock,
            SpawnedSegment = _timeManager.CurrentSegment
        },

        // Clone projected consequences
        ProjectedBondChanges = template.ProjectedBondChanges.ToList(),
        ProjectedScaleShifts = template.ProjectedScaleShifts.ToList(),
        ProjectedStates = template.ProjectedStates.ToList(),

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

/// <summary>
/// Add spawned situation to ActiveSituationIds based on placement
/// PHASE 0.2: Query ParentScene for placement using GetPlacementId() helper
/// </summary>
private void AddToActiveSituations(Situation situation)
{
    string npcId = situation.GetPlacementId(PlacementType.NPC);
    if (!string.IsNullOrEmpty(npcId))
    {
        // Add to NPC's ActiveSituationIds
        NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        if (npc != null && !npc.ActiveSituationIds.Contains(situation.Id))
        {
            npc.ActiveSituationIds.Add(situation.Id);
        }
    }
    else
    {
        string locationId = situation.GetPlacementId(PlacementType.Location);
        if (!string.IsNullOrEmpty(locationId))
        {
            // Add to Location's ActiveSituationIds
            Location location = _gameWorld.GetLocation(locationId);
            if (location != null && !location.ActiveSituationIds.Contains(situation.Id))
            {
                location.ActiveSituationIds.Add(situation.Id);
            }
        }
    }
}

/// <summary>
/// AUTOMATIC SCENE SPAWNING ORCHESTRATION
/// Checks SceneTemplates for spawn eligibility and instantiates eligible scenes
/// Called at trigger points: time advancement, location entry, NPC interaction
///
/// HANDOFF IMPLEMENTATION: Phase 4 (lines 247-253)
/// - Queries SceneTemplates with isStarter=false
/// - Evaluates spawn conditions via SpawnConditionsEvaluator
/// - Instantiates eligible scenes via SceneInstantiator
/// - Prevents duplicate spawning (checks existing scenes)
/// </summary>
/// <param name="triggerType">What triggered the spawn check (Time, Location, NPC, Scene)</param>
/// <param name="contextId">Optional context ID (locationId, npcId, etc.)</param>
public async Task CheckAndSpawnEligibleScenes(SpawnTriggerType triggerType, string contextId = null)
{
    Console.WriteLine($"[SpawnOrchestration] Checking eligible scenes (Trigger: {triggerType}, Context: {contextId ?? "none"})");

    Player player = _gameWorld.GetPlayer();

    // === INFINITE A-STORY INTEGRATION ===
    // Detect A-story scene completion and generate next A-scene template
    if (triggerType == SpawnTriggerType.Scene)
    {
        // If contextId provided, use it directly
        // If not provided (tests/manual trigger), find most recently completed A-story scene
        Scene completedScene = null;

        if (!string.IsNullOrEmpty(contextId))
        {
            completedScene = _gameWorld.Scenes.FirstOrDefault(s => s.Id == contextId);
        }
        else
        {
            // Find most recently completed A-story scene (highest sequence number)
            completedScene = _gameWorld.Scenes
                .Where(s => s.State == SceneState.Completed &&
                           s.Category == StoryCategory.MainStory &&
                           s.MainStorySequence.HasValue)
                .OrderByDescending(s => s.MainStorySequence.Value)
                .FirstOrDefault();
        }

        if (completedScene != null &&
            completedScene.Category == StoryCategory.MainStory &&
            completedScene.MainStorySequence.HasValue)
        {
            int completedSequence = completedScene.MainStorySequence.Value;
            int nextSequence = completedSequence + 1;

            Console.WriteLine($"[InfiniteAStory] A{completedSequence} completed - checking for A{nextSequence} template");

            // Check if next A-scene template already exists
            bool nextTemplateExists = _proceduralAStoryService.NextTemplateExists(nextSequence);

            if (!nextTemplateExists)
            {
                Console.WriteLine($"[InfiniteAStory] A{nextSequence} template does not exist - generating procedurally");

                // Get or initialize A-story context
                AStoryContext context = _proceduralAStoryService.GetOrInitializeContext(player);

                // Generate next A-scene template (HIGHLANDER flow: DTO → JSON → PackageLoader → Template)
                // FAIL-FAST: If generation fails, throw immediately (don't catch)
                // Architectural principle: Soft-lock worse than crash
                // Player completing A-scene expecting next A-scene is unacceptable failure mode
                string templateId = await _proceduralAStoryService.GenerateNextATemplate(nextSequence, context);

                Console.WriteLine($"[InfiniteAStory] A{nextSequence} template generated: {templateId}");
                Console.WriteLine($"[InfiniteAStory] Template added to GameWorld.SceneTemplates via HIGHLANDER flow");

                // Update context after successful generation
                context.RecordCompletion(
                    completedScene.Id,
                    archetypeId: "unknown", // Would need archetype tracking in Scene
                    regionId: null, // Would extract from placement
                    personalityType: null); // Would extract from NPC
            }
            else
            {
                Console.WriteLine($"[InfiniteAStory] A{nextSequence} template already exists - skipping generation");
            }
        }
    }
    // === END INFINITE A-STORY INTEGRATION ===

    // Query procedural SceneTemplates (isStarter=false, has spawnConditions)
    List<SceneTemplate> proceduralTemplates = _gameWorld.SceneTemplates
        .Where(t => !t.IsStarter && t.SpawnConditions != null)
        .ToList();

    Console.WriteLine($"[SpawnOrchestration] Found {proceduralTemplates.Count} procedural templates to evaluate");

    int spawned = 0;
    foreach (SceneTemplate template in proceduralTemplates)
    {
        // Skip if already spawned (check GameWorld.Scenes for this templateId)
        bool alreadySpawned = _gameWorld.Scenes.Any(s => s.TemplateId == template.Id && s.State != SceneState.Completed);
        if (alreadySpawned)
        {
            continue; // Scene already active, don't spawn duplicate
        }

        // Evaluate spawn conditions
        bool isEligible = _conditionsEvaluator.EvaluateAll(template.SpawnConditions, player);

        if (isEligible)
        {
            Console.WriteLine($"[SpawnOrchestration] Template '{template.Id}' is ELIGIBLE - spawning scene");

            // CATEGORICAL SPAWNING: All procedural spawning uses Generic placement with categorical resolution
            PlacementRelation placementRelation = PlacementRelation.Generic;
            string specificPlacementId = null; // Procedural spawning never uses concrete IDs

            SceneSpawnReward spawnReward = new SceneSpawnReward
            {
                SceneTemplateId = template.Id,
                PlacementRelation = placementRelation,
                SpecificPlacementId = specificPlacementId, // Concrete ID for tutorial pattern, null for categorical
                DelayDays = 0 // Spawn immediately when eligible
            };

            SceneSpawnContext spawnContext = BuildSpawnContext(template, player);

            if (spawnContext != null)
            {
                // HIGHLANDER FLOW: Single method spawns scene (JSON → PackageLoader → Parser)
                Scene scene = await _sceneInstanceFacade.SpawnScene(template, spawnReward, spawnContext);

                if (scene != null)
                {
                    spawned++;
                    Console.WriteLine($"[SpawnOrchestration] Scene '{scene.Id}' spawned via HIGHLANDER flow");
                }
            }
            else
            {
                Console.WriteLine($"[SpawnOrchestration] Failed to build spawn context for template '{template.Id}'");
            }
        }
    }

    if (spawned > 0)
    {
        Console.WriteLine($"[SpawnOrchestration] Spawned {spawned} new scenes");
    }
}

/// <summary>
/// Build SceneSpawnContext for scene instantiation
/// </summary>
private SceneSpawnContext BuildSpawnContext(SceneTemplate template, Player player)
{
    SceneSpawnContext context = new SceneSpawnContext
    {
        Player = player,
        CurrentSituation = null // Automatic spawning is not triggered by situations
    };

    // SceneInstantiator will resolve placement from PlacementFilter
    // We just provide player and current world state context
    return context;
}
}

/// <summary>
/// Trigger types for automatic scene spawning
/// Used for logging and potential future trigger-specific logic
/// </summary>
public enum SpawnTriggerType
{
Time,      // Time advancement (day change, time block change)
Location,  // Player moved to new location
NPC,       // Player interacted with NPC
Scene      // Scene completed (cascade spawning)
}
