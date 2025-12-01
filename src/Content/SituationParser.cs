
/// <summary>
/// Parser for converting SituationDTO to Situation domain model
/// ARCHITECTURAL NOTE: Situations have their own placement (not inherited from Scene)
/// THREE-TIER TIMING: Creates situations with NULL entity references, resolved at activation
/// </summary>
public static class SituationParser
{
    /// <summary>
    /// Convert a SituationDTO to a Situation domain model (System 5: Situation Instantiation)
    /// Entity references NULL at parse time - resolved by ActivateScene() INTEGRATED process
    /// PlacementFilters stored by caller (SceneParser), entities assigned at scene activation
    /// </summary>
    public static Situation ConvertDTOToSituation(
        SituationDTO dto,
        GameWorld gameWorld)
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

        // Parse entry costs and difficulty modifiers
        // HIGHLANDER: Entry costs use Consequence with negative values
        Consequence entryCost = ParseEntryCost(dto.EntryCost);
        List<DifficultyModifier> difficultyModifiers = ParseDifficultyModifiers(dto.DifficultyModifiers);

        // Parse TimeBlocks for spawn/completion tracking
        TimeBlocks? spawnedTimeBlock = ParseTimeBlock(dto.SpawnedTimeBlock);
        TimeBlocks? completedTimeBlock = ParseTimeBlock(dto.CompletedTimeBlock);

        Situation situation = new Situation
        {
            Name = dto.Name,
            Description = dto.Description,
            // Entity references NULL at parse time - resolved by ActivateScene() at scene activation
            Location = null,  // Resolved via EntityResolver.FindLocation() + PackageLoader.CreateSingleLocation()
            Npc = null,       // Resolved via EntityResolver.FindNPC() + PackageLoader.CreateSingleNpc()
            Route = null,     // Resolved via EntityResolver.FindRoute() (FAIL FAST if not found)
            SystemType = systemType,
            IsIntroAction = dto.IsIntroAction,
            // IsAvailable and IsCompleted are computed properties from Status enum (no population needed)
            DeleteOnSuccess = dto.DeleteOnSuccess,
            EntryCost = entryCost,
            DifficultyModifiers = difficultyModifiers,
            SituationCards = new List<SituationCard>(),
            // SituationRequirements system eliminated - situations always visible, difficulty varies
            ConsequenceType = consequenceType,
            SetsResolutionMethod = resolutionMethod,
            SetsRelationshipOutcome = relationshipOutcome,
            TransformDescription = dto.TransformDescription,
            // Scene-Situation Architecture additions (spawn/completion tracking)
            TemplateId = dto.TemplateId,
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
            NavigationPayload = ParseNavigationPayload(dto.NavigationPayload, gameWorld),
            CompoundRequirement = RequirementParser.ConvertDTOToCompoundRequirement(dto.CompoundRequirement, gameWorld),
            // ProjectedBondChanges/ProjectedScaleShifts/ProjectedStates DELETED - stored projection pattern
            SuccessSpawns = SpawnRuleParser.ParseSpawnRules(dto.SuccessSpawns, dto.Name, gameWorld),
            FailureSpawns = SpawnRuleParser.ParseSpawnRules(dto.FailureSpawns, dto.Name, gameWorld),
            Repeatable = dto.Repeatable,
            GeneratedNarrative = dto.GeneratedNarrative,
            NarrativeHints = ParseNarrativeHints(dto.NarrativeHints)
            // Note: Situation.Location bound at activation time via PlacementFilter resolution
        };

        // Resolve object references during parsing (HIGHLANDER: Name is natural key)
        // Parser uses ID from DTO as lookup key, entity stores ONLY object reference

        if (!string.IsNullOrEmpty(dto.ObligationId))
            situation.Obligation = gameWorld.Obligations.FirstOrDefault(i => i.Id == dto.ObligationId);

        // Resolve Deck object reference from DeckId (parse-time translation)
        if (!string.IsNullOrEmpty(dto.DeckId))
        {
            situation.Deck = gameWorld.SocialChallengeDecks.FirstOrDefault(d => d.Id == dto.DeckId)
                ?? (object)gameWorld.MentalChallengeDecks.FirstOrDefault(d => d.Id == dto.DeckId)
                ?? gameWorld.PhysicalChallengeDecks.FirstOrDefault(d => d.Id == dto.DeckId);
        }

        // Resolve ParentSituation from ParentSituationId - needs to search all situations in all scenes
        if (!string.IsNullOrEmpty(dto.ParentSituationId))
        {
            situation.ParentSituation = gameWorld.Scenes
                .SelectMany(s => s.Situations)
                .FirstOrDefault(s => s.Template?.Id == dto.ParentSituationId);
        }

        // Parse situation cards (victory conditions)
        if (dto.SituationCards != null && dto.SituationCards.Any())
        {
            foreach (SituationCardDTO situationCardDTO in dto.SituationCards)
            {
                SituationCard situationCard = ParseSituationCard(situationCardDTO, dto.Name, gameWorld);
                situation.SituationCards.Add(situationCard);
            }
        }
        return situation;
    }

    /// <summary>
    /// Parse a single situation card (victory condition)
    /// </summary>
    private static SituationCard ParseSituationCard(SituationCardDTO dto, string situationId, GameWorld gameWorld)
    {
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"SituationCard in situation {situationId} missing required 'Name' field");

        SituationCard situationCard = new SituationCard
        {
            Name = dto.Name,
            Description = dto.Description,
            threshold = dto.threshold,
            Rewards = ParseSituationCardRewards(dto.Rewards, gameWorld),
            IsAchieved = false
        };

        return situationCard;
    }

    /// <summary>
    /// Parse situation card rewards
    /// Knowledge system eliminated - Understanding resource replaces Knowledge tokens
    /// </summary>
    private static SituationCardRewards ParseSituationCardRewards(SituationCardRewardsDTO dto, GameWorld gameWorld)
    {
        if (dto == null)
            return new SituationCardRewards();

        SituationCardRewards rewards = new SituationCardRewards
        {
            Coins = dto.Coins,
            Progress = dto.Progress,
            Breakthrough = dto.Breakthrough,

            // Cube rewards (strong typing)
            InvestigationCubes = dto.InvestigationCubes,
            StoryCubes = dto.StoryCubes,
            ExplorationCubes = dto.ExplorationCubes,

            // Core Loop reward types
            CreateObligationData = dto.CreateObligationData != null
                ? new CreateObligationReward
                {
                    StoryCubesGranted = dto.CreateObligationData.StoryCubesGranted,
                    RewardCoins = dto.CreateObligationData.RewardCoins
                }
                : null,
            RouteSegmentUnlock = dto.RouteSegmentUnlock != null
                ? new RouteSegmentUnlock
                {
                    SegmentPosition = dto.RouteSegmentUnlock.SegmentPosition
                }
                : null

            // SceneReduction deleted - legacy Scene architecture removed
        };

        // Resolve Obligation object from ObligationId (parse-time translation)
        if (!string.IsNullOrEmpty(dto.ObligationId))
        {
            rewards.Obligation = gameWorld.Obligations.FirstOrDefault(o => o.Id == dto.ObligationId);
        }

        // Resolve Item object from Item name or EquipmentId (parse-time translation)
        if (!string.IsNullOrEmpty(dto.Item))
        {
            rewards.Item = gameWorld.Items.FirstOrDefault(i => i.Name == dto.Item);
        }
        else if (!string.IsNullOrEmpty(dto.EquipmentId))
        {
            rewards.Item = gameWorld.Items.FirstOrDefault(i => i.Name == dto.EquipmentId);
        }

        // Resolve PatronNpc object for CreateObligationData (parse-time translation)
        if (rewards.CreateObligationData != null && !string.IsNullOrEmpty(dto.CreateObligationData.PatronNpcId))
        {
            rewards.CreateObligationData.PatronNpc = gameWorld.NPCs.FirstOrDefault(n => n.Name == dto.CreateObligationData.PatronNpcId);
        }

        // Resolve Route and Path objects for RouteSegmentUnlock (parse-time translation)
        if (rewards.RouteSegmentUnlock != null)
        {
            if (!string.IsNullOrEmpty(dto.RouteSegmentUnlock.RouteId))
            {
                rewards.RouteSegmentUnlock.Route = gameWorld.Routes.FirstOrDefault(r => r.Name == dto.RouteSegmentUnlock.RouteId);
            }

            // Resolve Path object from PathId (parse-time translation)
            // Creates minimal PathCard domain entity for name-based lookup in SituationCompletionHandler
            if (!string.IsNullOrEmpty(dto.RouteSegmentUnlock.PathId) && rewards.RouteSegmentUnlock.Route != null)
            {
                RouteOption route = rewards.RouteSegmentUnlock.Route;
                int segmentPos = rewards.RouteSegmentUnlock.SegmentPosition;

                // Validate segment position within route
                if (segmentPos >= 0 && segmentPos < route.Segments.Count)
                {
                    RouteSegment segment = route.Segments[segmentPos];

                    // Find PathCardDTO in segment's PathCollection
                    if (segment.PathCollection != null)
                    {
                        PathCardDTO pathCardDto = segment.PathCollection.PathCards.FirstOrDefault(p => p.Name == dto.RouteSegmentUnlock.PathId);
                        if (pathCardDto != null)
                        {
                            // Create minimal PathCard domain entity for SituationCompletionHandler lookup
                            // SituationCompletionHandler matches by Name to find the DTO and set IsHidden = false
                            rewards.RouteSegmentUnlock.Path = new PathCard
                            {
                                Name = pathCardDto.Name,
                                NarrativeText = pathCardDto.NarrativeText
                            };
                        }
                        else
                        {
                            Console.WriteLine($"[SituationParser.ParseSituationCardRewards] WARNING: PathId '{dto.RouteSegmentUnlock.PathId}' not found in segment {segmentPos} of route '{route.Name}'");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"[SituationParser.ParseSituationCardRewards] WARNING: SegmentPosition {segmentPos} out of range for route '{route.Name}' (has {route.Segments.Count} segments)");
                }
            }
        }

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
    /// HIGHLANDER: Parse entry costs as Consequence (negative values = costs)
    /// Converts positive cost values from JSON to negative values in Consequence
    /// </summary>
    private static Consequence ParseEntryCost(ConsequenceDTO dto)
    {
        if (dto == null)
            return Consequence.None();

        // HIGHLANDER: Entry costs are stored as NEGATIVE values (cost convention)
        // JSON specifies positive values (Focus: 20 means "costs 20 Focus")
        // Consequence stores negatives (Focus: -20 means "pay 20 Focus")
        return new Consequence
        {
            Resolve = -Math.Abs(dto.Resolve),
            TimeSegments = dto.TimeSegments,  // Time is always positive (forward movement)
            Focus = -Math.Abs(dto.Focus),
            Stamina = -Math.Abs(dto.Stamina),
            Coins = -Math.Abs(dto.Coins),
            Health = -Math.Abs(dto.Health),
            Hunger = dto.Hunger  // Hunger convention is inverted (positive = bad)
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
    /// Resolves Destination Location object from ID
    /// </summary>
    private static NavigationPayload ParseNavigationPayload(NavigationPayloadDTO dto, GameWorld gameWorld)
    {
        if (dto == null)
            return null;

        // Resolve destination location from DestinationId
        Location destination = null;
        if (!string.IsNullOrEmpty(dto.DestinationId))
        {
            destination = gameWorld.Locations.FirstOrDefault(l => l.Name == dto.DestinationId);
            if (destination == null)
                throw new InvalidOperationException($"NavigationPayload references unknown Location: '{dto.DestinationId}'");
        }

        return new NavigationPayload
        {
            Destination = destination,
            AutoTriggerScene = dto.AutoTriggerScene
        };
    }

    /// <summary>
    /// Parse list of BondChange from DTOs
    /// NOTE: Currently dead code - BondChanges parsed in SceneTemplateParser only
    /// Resolves NPC object references from NpcId strings
    /// </summary>
    private static List<BondChange> ParseBondChanges(List<BondChangeDTO> dtos, GameWorld gameWorld)
    {
        if (dtos == null || dtos.Count == 0)
            return new List<BondChange>();

        List<BondChange> bondChanges = new List<BondChange>();
        foreach (BondChangeDTO dto in dtos)
        {
            NPC npc = gameWorld.NPCs.FirstOrDefault(n => n.Name == dto.NpcId);
            if (npc == null)
            {
                Console.WriteLine($"[SituationParser.ParseBondChanges] WARNING: NPC '{dto.NpcId}' not found for BondChange");
                continue; // Skip invalid bond change
            }

            bondChanges.Add(new BondChange
            {
                Npc = npc, // Object reference, NO ID
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
