using Wayfarer.GameState.Enums;

/// <summary>
/// SPAWN FACADE - Executes spawn rules to create cascading situation chains
///
/// Sir Brante Pattern: Parent situation completion spawns child situations
/// - Clones template situations
/// - Applies requirement offsets (makes children easier/harder)
/// - Validates spawn conditions before execution
/// - Adds spawned situations to GameWorld and ActiveSituationIds
///
/// Called by:
/// - SituationFacade.ResolveInstantSituation() - After instant situation resolution
/// - SituationCompletionHandler.CompleteSituation() - After challenge completion
///
/// NO EVENTS - Synchronous execution orchestrated by completion handlers
/// </summary>
public class SpawnFacade
{
    private readonly GameWorld _gameWorld;
    private readonly TimeFacade _timeFacade;

    public SpawnFacade(
        GameWorld gameWorld,
        TimeFacade timeFacade)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _timeFacade = timeFacade ?? throw new ArgumentNullException(nameof(timeFacade));
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
            if (rule.Conditions != null && !ValidateSpawnConditions(rule.Conditions, player))
            {
                // Conditions not met - skip this spawn
                continue;
            }

            // Find template situation
            Situation template = _gameWorld.Situations.FirstOrDefault(s => s.Id == rule.TemplateId);
            if (template == null)
            {
                // Template not found - skip this spawn
                // TODO: Log warning about missing template
                continue;
            }

            // Clone template and apply spawn modifications
            Situation spawnedSituation = CloneTemplateWithModifications(template, parentSituation, rule);

            // Add to GameWorld.Situations
            _gameWorld.Situations.Add(spawnedSituation);

            // Add to ActiveSituationIds based on placement
            AddToActiveSituations(spawnedSituation);
        }
    }

    /// <summary>
    /// Validate spawn conditions - all conditions must be met for spawn to occur
    /// </summary>
    private bool ValidateSpawnConditions(SpawnConditions conditions, Player player)
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
        string spawnedId = $"{template.Id}_spawned_{parentSituation.Id}_{_timeFacade.GetCurrentDay()}_{_timeFacade.GetCurrentSegment()}";

        // Create cloned situation
        Situation spawned = new Situation
        {
            Id = spawnedId,
            Name = template.Name,
            Description = template.Description,
            SystemType = template.SystemType,
            DeckId = template.DeckId,
            IsIntroAction = template.IsIntroAction,
            IsAvailable = true, // Spawned situations start available
            IsCompleted = false,
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
            Status = SituationStatus.Available,

            // Spawn tracking
            Template = null, // TODO: Replace with proper template reference when using SituationTemplate system
            ParentSituationId = parentSituation.Id, // Track parent situation
            SpawnedDay = _timeFacade.GetCurrentDay(),
            SpawnedTimeBlock = _timeFacade.GetCurrentTimeBlock(),
            SpawnedSegment = _timeFacade.GetCurrentSegment(),

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
        // ARCHITECTURE UPDATE: Use PlacementRelation enum (aligned with SceneSpawnReward)
        switch (rule.PlacementRelation)
        {
            case PlacementRelation.SameLocation:
                // Use parent's location placement
                spawned.PlacementLocation = parent.PlacementLocation;
                spawned.PlacementNpc = null;
                spawned.Obligation = parent.Obligation;
                break;

            case PlacementRelation.SameNPC:
                // Use parent's NPC placement
                spawned.PlacementNpc = parent.PlacementNpc;
                spawned.PlacementLocation = parent.PlacementNpc?.Location;
                spawned.Obligation = parent.Obligation;
                break;

            case PlacementRelation.SpecificLocation:
                // Resolve location by ID
                if (string.IsNullOrEmpty(rule.SpecificPlacementId))
                    throw new InvalidOperationException($"SpawnRule with SpecificLocation strategy missing SpecificPlacementId (template: {rule.TemplateId})");

                Location location = _gameWorld.Locations.FirstOrDefault(l => l.Id == rule.SpecificPlacementId);
                if (location == null)
                    throw new InvalidOperationException($"SpawnRule references unknown location '{rule.SpecificPlacementId}' (template: {rule.TemplateId})");

                spawned.PlacementLocation = location;
                spawned.PlacementNpc = null;
                spawned.Obligation = parent.Obligation; // Keep parent's obligation
                break;

            case PlacementRelation.SpecificNPC:
                // Resolve NPC by ID
                if (string.IsNullOrEmpty(rule.SpecificPlacementId))
                    throw new InvalidOperationException($"SpawnRule with SpecificNPC strategy missing SpecificPlacementId (template: {rule.TemplateId})");

                NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == rule.SpecificPlacementId);
                if (npc == null)
                    throw new InvalidOperationException($"SpawnRule references unknown NPC '{rule.SpecificPlacementId}' (template: {rule.TemplateId})");

                spawned.PlacementNpc = npc;
                spawned.PlacementLocation = npc.Location; // NPC's current location
                spawned.Obligation = parent.Obligation; // Keep parent's obligation
                break;

            case PlacementRelation.SpecificRoute:
            case PlacementRelation.SameRoute:
                // Routes: Use PlacementRouteId
                spawned.PlacementRouteId = rule.PlacementRelation == PlacementRelation.SameRoute
                    ? parent.PlacementRouteId
                    : rule.SpecificPlacementId;
                spawned.PlacementLocation = parent.PlacementLocation;
                spawned.PlacementNpc = null;
                spawned.Obligation = parent.Obligation;
                break;

            default:
                throw new InvalidOperationException($"Unknown PlacementRelation: {rule.PlacementRelation}");
        }
    }

    /// <summary>
    /// Add spawned situation to ActiveSituationIds based on placement
    /// </summary>
    private void AddToActiveSituations(Situation situation)
    {
        if (situation.PlacementNpc != null)
        {
            // Add to NPC's ActiveSituationIds
            if (!situation.PlacementNpc.ActiveSituationIds.Contains(situation.Id))
            {
                situation.PlacementNpc.ActiveSituationIds.Add(situation.Id);
            }
        }
        else if (situation.PlacementLocation != null)
        {
            // Add to Location's ActiveSituationIds
            if (!situation.PlacementLocation.ActiveSituationIds.Contains(situation.Id))
            {
                situation.PlacementLocation.ActiveSituationIds.Add(situation.Id);
            }
        }
    }
}
