
/// <summary>
/// Parser for converting SituationDTO to Situation domain model
/// ARCHITECTURAL NOTE: Situations have their own placement (not inherited from Scene)
/// Receives pre-resolved entities from EntityResolver (System 4)
/// </summary>
public static class SituationParser
{
    /// <summary>
    /// Convert a SituationDTO to a Situation domain model (System 5: Situation Instantiation)
    /// Receives pre-resolved entity objects from EntityResolver (System 4)
    /// Assigns placement directly to Situation properties
    /// </summary>
    public static Situation ConvertDTOToSituation(
        SituationDTO dto,
        GameWorld gameWorld,
        Location resolvedLocation,
        NPC resolvedNpc,
        RouteOption resolvedRoute)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException("Situation DTO missing required 'Id' field");
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"Situation {dto.Id} missing required 'Name' field");
        if (string.IsNullOrEmpty(dto.SystemType))
            throw new InvalidOperationException($"Situation {dto.Id} missing required 'SystemType' field");
        // DeckId optional - only required if situation has challenge choices

        // Parse system type
        if (!Enum.TryParse<TacticalSystemType>(dto.SystemType, true, out TacticalSystemType systemType))
        {
            throw new InvalidOperationException($"Situation {dto.Id} has invalid SystemType value: '{dto.SystemType}'");
        }

        // Parse consequence type
        ConsequenceType consequenceType = ParseConsequenceType(dto.ConsequenceType);

        // Parse resolution method and relationship outcome
        ResolutionMethod resolutionMethod = ParseResolutionMethod(dto.ResolutionMethod);
        RelationshipOutcome relationshipOutcome = ParseRelationshipOutcome(dto.RelationshipOutcome);

        // Parse costs and difficulty modifiers
        SituationCosts costs = ParseSituationCosts(dto.Costs);
        List<DifficultyModifier> difficultyModifiers = ParseDifficultyModifiers(dto.DifficultyModifiers);

        // Parse TimeBlocks for spawn/completion tracking
        TimeBlocks? spawnedTimeBlock = ParseTimeBlock(dto.SpawnedTimeBlock);
        TimeBlocks? completedTimeBlock = ParseTimeBlock(dto.CompletedTimeBlock);

        Situation situation = new Situation
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            // Placement properties (System 5 output - pre-resolved from System 4)
            Location = resolvedLocation,
            Npc = resolvedNpc,
            Route = resolvedRoute,
            SystemType = systemType,
            DeckId = dto.DeckId,
            IsIntroAction = dto.IsIntroAction,
            // IsAvailable and IsCompleted are computed properties from Status enum (no population needed)
            DeleteOnSuccess = dto.DeleteOnSuccess,
            Costs = costs,
            DifficultyModifiers = difficultyModifiers,
            SituationCards = new List<SituationCard>(),
            // SituationRequirements system eliminated - situations always visible, difficulty varies
            ConsequenceType = consequenceType,
            SetsResolutionMethod = resolutionMethod,
            SetsRelationshipOutcome = relationshipOutcome,
            TransformDescription = dto.TransformDescription,
            // Scene-Situation Architecture additions (spawn/completion tracking)
            TemplateId = dto.TemplateId,
            ParentSituationId = dto.ParentSituationId,
            Lifecycle = new SpawnTracking
            {
                SpawnedDay = dto.SpawnedDay,
                SpawnedTimeBlock = spawnedTimeBlock,
                SpawnedSegment = dto.SpawnedSegment,
                CompletedDay = dto.CompletedDay,
                CompletedTimeBlock = completedTimeBlock,
                CompletedSegment = dto.CompletedSegment
            },

            // Scene-Situation Architecture (interaction/requirements/consequences/spawns)
            InteractionType = ParseInteractionType(dto.InteractionType),
            NavigationPayload = ParseNavigationPayload(dto.NavigationPayload),
            CompoundRequirement = RequirementParser.ConvertDTOToCompoundRequirement(dto.CompoundRequirement),
            // ProjectedBondChanges/ProjectedScaleShifts/ProjectedStates DELETED - stored projection pattern
            SuccessSpawns = SpawnRuleParser.ParseSpawnRules(dto.SuccessSpawns, dto.Id),
            FailureSpawns = SpawnRuleParser.ParseSpawnRules(dto.FailureSpawns, dto.Id),
            Tier = dto.Tier,
            Repeatable = dto.Repeatable,
            GeneratedNarrative = dto.GeneratedNarrative,
            NarrativeHints = ParseNarrativeHints(dto.NarrativeHints)
        };

        // Resolve object references during parsing (HIGHLANDER: ID is parsing artifact, not entity property)
        // Parser uses ID from DTO as lookup key, entity stores ONLY object reference
        // PLACEMENT: Location/Npc/Route assigned directly from pre-resolved entities (System 4 output)

        if (!string.IsNullOrEmpty(dto.ObligationId))
            situation.Obligation = gameWorld.Obligations.FirstOrDefault(i => i.Id == dto.ObligationId);

        // Parse situation cards (victory conditions)
        if (dto.SituationCards != null && dto.SituationCards.Any())
        {
            foreach (SituationCardDTO situationCardDTO in dto.SituationCards)
            {
                SituationCard situationCard = ParseSituationCard(situationCardDTO, dto.Id);
                situation.SituationCards.Add(situationCard);
            }
        }
        return situation;
    }

    /// <summary>
    /// Parse a single situation card (victory condition)
    /// </summary>
    private static SituationCard ParseSituationCard(SituationCardDTO dto, string situationId)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException($"SituationCard in situation {situationId} missing required 'Id' field");

        SituationCard situationCard = new SituationCard
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            threshold = dto.threshold,
            Rewards = ParseSituationCardRewards(dto.Rewards),
            IsAchieved = false
        };

        return situationCard;
    }

    /// <summary>
    /// Parse situation card rewards
    /// Knowledge system eliminated - Understanding resource replaces Knowledge tokens
    /// </summary>
    private static SituationCardRewards ParseSituationCardRewards(SituationCardRewardsDTO dto)
    {
        if (dto == null)
            return new SituationCardRewards();

        SituationCardRewards rewards = new SituationCardRewards
        {
            Coins = dto.Coins,
            Progress = dto.Progress,
            Breakthrough = dto.Breakthrough,
            ObligationId = dto.ObligationId,
            Item = dto.Item,

            // Cube rewards (strong typing)
            InvestigationCubes = dto.InvestigationCubes,
            StoryCubes = dto.StoryCubes,
            ExplorationCubes = dto.ExplorationCubes,

            // Core Loop reward types
            EquipmentId = dto.EquipmentId,
            CreateObligationData = dto.CreateObligationData != null
                ? new CreateObligationReward
                {
                    PatronNpcId = dto.CreateObligationData.PatronNpcId,
                    StoryCubesGranted = dto.CreateObligationData.StoryCubesGranted,
                    RewardCoins = dto.CreateObligationData.RewardCoins
                }
                : null,
            RouteSegmentUnlock = dto.RouteSegmentUnlock != null
                ? new RouteSegmentUnlock
                {
                    RouteId = dto.RouteSegmentUnlock.RouteId,
                    SegmentPosition = dto.RouteSegmentUnlock.SegmentPosition,
                    PathId = dto.RouteSegmentUnlock.PathId
                }
                : null

            // SceneReduction deleted - legacy Scene architecture removed
        };

        return rewards;
    }

    /// <summary>
    /// Parse consequence type from string
    /// </summary>
    private static ConsequenceType ParseConsequenceType(string consequenceTypeString)
    {
        if (string.IsNullOrEmpty(consequenceTypeString))
            return ConsequenceType.Grant;

        if (Enum.TryParse<ConsequenceType>(consequenceTypeString, true, out ConsequenceType consequenceType))
        {
            return consequenceType;
        }
        return ConsequenceType.Grant;
    }

    /// <summary>
    /// Parse resolution method from string
    /// </summary>
    private static ResolutionMethod ParseResolutionMethod(string methodString)
    {
        if (string.IsNullOrEmpty(methodString))
            return ResolutionMethod.Unresolved;

        if (Enum.TryParse<ResolutionMethod>(methodString, true, out ResolutionMethod method))
        {
            return method;
        }
        return ResolutionMethod.Unresolved;
    }

    /// <summary>
    /// Parse relationship outcome from string
    /// </summary>
    private static RelationshipOutcome ParseRelationshipOutcome(string outcomeString)
    {
        if (string.IsNullOrEmpty(outcomeString))
            return RelationshipOutcome.Neutral;

        if (Enum.TryParse<RelationshipOutcome>(outcomeString, true, out RelationshipOutcome outcome))
        {
            return outcome;
        }
        return RelationshipOutcome.Neutral;
    }

    /// <summary>
    /// Parse situation costs from DTO
    /// </summary>
    private static SituationCosts ParseSituationCosts(SituationCostsDTO dto)
    {
        if (dto == null)
            return new SituationCosts();

        return new SituationCosts
        {
            Resolve = dto.Resolve,
            Time = dto.Time,
            Focus = dto.Focus,
            Stamina = dto.Stamina,
            Coins = dto.Coins
        };
    }

    /// <summary>
    /// Parse difficulty modifiers list from DTOs
    /// </summary>
    private static List<DifficultyModifier> ParseDifficultyModifiers(List<DifficultyModifierDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<DifficultyModifier>();

        List<DifficultyModifier> modifiers = new List<DifficultyModifier>();
        foreach (DifficultyModifierDTO dto in dtos)
        {
            DifficultyModifier modifier = ParseDifficultyModifier(dto);
            if (modifier != null)
            {
                modifiers.Add(modifier);
            }
        }

        return modifiers;
    }

    /// <summary>
    /// Parse single difficulty modifier from DTO
    /// </summary>
    private static DifficultyModifier ParseDifficultyModifier(DifficultyModifierDTO dto)
    {
        if (dto == null)
            return null;

        // Parse modifier type
        if (!Enum.TryParse<ModifierType>(dto.Type, true, out ModifierType modifierType))
        {
            return null;
        }

        return new DifficultyModifier
        {
            Type = modifierType,
            Context = dto.Context,
            Threshold = dto.Threshold,
            Effect = dto.Effect
        };
    }

    /// <summary>
    /// Parse TimeBlock from string
    /// </summary>
    private static TimeBlocks? ParseTimeBlock(string timeBlockString)
    {
        if (string.IsNullOrEmpty(timeBlockString))
            return null;

        if (Enum.TryParse<TimeBlocks>(timeBlockString, true, out TimeBlocks timeBlock))
        {
            return timeBlock;
        }

        return null; // Invalid time block returns null
    }

    // ====================
    // SCENE-SITUATION ARCHITECTURE PARSERS
    // ====================

    /// <summary>
    /// Parse InteractionType from string
    /// </summary>
    private static SituationInteractionType ParseInteractionType(string interactionTypeString)
    {
        if (string.IsNullOrEmpty(interactionTypeString))
            return SituationInteractionType.Instant; // Default

        if (Enum.TryParse<SituationInteractionType>(interactionTypeString, true, out SituationInteractionType interactionType))
        {
            return interactionType;
        }

        throw new InvalidOperationException($"Invalid InteractionType value: '{interactionTypeString}'. Must be one of: Instant, Mental, Physical, Social, Navigation");
    }

    /// <summary>
    /// Parse NavigationPayload from DTO
    /// </summary>
    private static NavigationPayload ParseNavigationPayload(NavigationPayloadDTO dto)
    {
        if (dto == null)
            return null;

        return new NavigationPayload
        {
            DestinationId = dto.DestinationId,
            AutoTriggerScene = dto.AutoTriggerScene
        };
    }

    /// <summary>
    /// Parse list of BondChange from DTOs
    /// </summary>
    private static List<BondChange> ParseBondChanges(List<BondChangeDTO> dtos)
    {
        if (dtos == null || dtos.Count == 0)
            return new List<BondChange>();

        List<BondChange> bondChanges = new List<BondChange>();
        foreach (BondChangeDTO dto in dtos)
        {
            bondChanges.Add(new BondChange
            {
                NpcId = dto.NpcId,
                Delta = dto.Delta,
                Reason = dto.Reason
            });
        }
        return bondChanges;
    }

    /// <summary>
    /// Parse list of ScaleShift from DTOs
    /// </summary>
    private static List<ScaleShift> ParseScaleShifts(List<ScaleShiftDTO> dtos)
    {
        if (dtos == null || dtos.Count == 0)
            return new List<ScaleShift>();

        List<ScaleShift> scaleShifts = new List<ScaleShift>();
        foreach (ScaleShiftDTO dto in dtos)
        {
            if (!Enum.TryParse<ScaleType>(dto.ScaleType, true, out ScaleType scaleType))
            {
                throw new InvalidOperationException($"Invalid ScaleType value: '{dto.ScaleType}'");
            }

            scaleShifts.Add(new ScaleShift
            {
                ScaleType = scaleType,
                Delta = dto.Delta,
                Reason = dto.Reason
            });
        }
        return scaleShifts;
    }

    /// <summary>
    /// Parse list of StateApplication from DTOs
    /// </summary>
    private static List<StateApplication> ParseStateApplications(List<StateApplicationDTO> dtos)
    {
        if (dtos == null || dtos.Count == 0)
            return new List<StateApplication>();

        List<StateApplication> stateApplications = new List<StateApplication>();
        foreach (StateApplicationDTO dto in dtos)
        {
            if (!Enum.TryParse<StateType>(dto.StateType, true, out StateType stateType))
            {
                throw new InvalidOperationException($"Invalid StateType value: '{dto.StateType}'");
            }

            stateApplications.Add(new StateApplication
            {
                StateType = stateType,
                Apply = dto.Apply,
                Reason = dto.Reason
            });
        }
        return stateApplications;
    }

    /// <summary>
    /// Parse NarrativeHints from DTO
    /// </summary>
    private static NarrativeHints ParseNarrativeHints(NarrativeHintsDTO dto)
    {
        if (dto == null)
            return null;

        return new NarrativeHints
        {
            Tone = dto.Tone,
            Theme = dto.Theme,
            Context = dto.Context,
            Style = dto.Style
        };
    }
}
