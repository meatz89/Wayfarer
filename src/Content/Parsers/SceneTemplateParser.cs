using Wayfarer.Content.Catalogues;
using Wayfarer.GameState.Enums;

namespace Wayfarer.Content.Parsers;

/// <summary>
/// Parser for SceneTemplate definitions - converts DTOs to domain models
/// Handles recursive parsing of embedded SituationTemplates and ChoiceTemplates
/// </summary>
public class SceneTemplateParser
{
    private readonly GameWorld _gameWorld;

    public SceneTemplateParser(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Parse a SceneTemplateDTO to SceneTemplate domain entity
    /// </summary>
    public SceneTemplate ParseSceneTemplate(SceneTemplateDTO dto)
    {
        // Validate required fields
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidDataException("SceneTemplate missing required field 'Id'");

        Console.WriteLine($"[SceneTemplateParser] Parsing SceneTemplate: {dto.Id}");

        if (string.IsNullOrEmpty(dto.Archetype))
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' missing required field 'Archetype'");

        // Parse archetype enum
        if (!Enum.TryParse<SpawnPattern>(dto.Archetype, true, out SpawnPattern archetype))
        {
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' has invalid Archetype value: '{dto.Archetype}'");
        }

        // Parse PresentationMode (defaults to Atmospheric if not specified)
        PresentationMode presentationMode = PresentationMode.Atmospheric;
        if (!string.IsNullOrEmpty(dto.PresentationMode))
        {
            if (!Enum.TryParse<PresentationMode>(dto.PresentationMode, true, out presentationMode))
            {
                throw new InvalidDataException($"SceneTemplate '{dto.Id}' has invalid PresentationMode value: '{dto.PresentationMode}'. Must be 'Atmospheric' or 'Modal'.");
            }
        }

        // Parse ProgressionMode (defaults to Breathe if not specified)
        ProgressionMode progressionMode = ProgressionMode.Breathe;
        if (!string.IsNullOrEmpty(dto.ProgressionMode))
        {
            if (!Enum.TryParse<ProgressionMode>(dto.ProgressionMode, true, out progressionMode))
            {
                throw new InvalidDataException($"SceneTemplate '{dto.Id}' has invalid ProgressionMode value: '{dto.ProgressionMode}'. Must be 'Breathe' or 'Cascade'.");
            }
        }

        SceneTemplate template = new SceneTemplate
        {
            Id = dto.Id,
            Archetype = archetype,
            DisplayNameTemplate = dto.DisplayNameTemplate,
            PlacementFilter = ParsePlacementFilter(dto.PlacementFilter, dto.Id),
            SituationTemplates = ParseSituationTemplates(dto.SituationTemplates, dto.Id, archetype),
            SpawnRules = ParseSpawnRules(dto.SpawnRules, dto.Id),
            ExpirationDays = dto.ExpirationDays,
            IsStarter = dto.IsStarter,
            IntroNarrativeTemplate = dto.IntroNarrativeTemplate,
            Tier = dto.Tier,
            PresentationMode = presentationMode,
            ProgressionMode = progressionMode
        };

        return template;
    }

    /// <summary>
    /// Parse PlacementFilter from DTO
    /// </summary>
    private PlacementFilter ParsePlacementFilter(PlacementFilterDTO dto, string sceneTemplateId)
    {
        if (dto == null)
            return null; // Optional - some SceneTemplates may not have filters

        // Validate PlacementType
        if (string.IsNullOrEmpty(dto.PlacementType))
            throw new InvalidDataException($"SceneTemplate '{sceneTemplateId}' PlacementFilter missing required 'PlacementType' field");

        if (!Enum.TryParse<PlacementType>(dto.PlacementType, true, out PlacementType placementType))
        {
            throw new InvalidDataException($"SceneTemplate '{sceneTemplateId}' PlacementFilter has invalid PlacementType: '{dto.PlacementType}'");
        }

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = placementType,
            // NPC filters
            PersonalityTypes = ParsePersonalityTypes(dto.PersonalityTypes, sceneTemplateId),
            MinBond = dto.MinBond,
            MaxBond = dto.MaxBond,
            NpcTags = dto.NpcTags,
            // Location filters
            LocationProperties = ParseLocationProperties(dto.LocationProperties, sceneTemplateId),
            LocationTags = dto.LocationTags,
            DistrictId = dto.DistrictId,
            RegionId = dto.RegionId,
            // Route filters
            TerrainTypes = dto.TerrainTypes,
            RouteTier = dto.RouteTier,
            MinDangerRating = dto.MinDangerRating,
            MaxDangerRating = dto.MaxDangerRating,
            // Player state filters
            RequiredStates = ParseStateTypes(dto.RequiredStates, sceneTemplateId, "RequiredStates"),
            ForbiddenStates = ParseStateTypes(dto.ForbiddenStates, sceneTemplateId, "ForbiddenStates"),
            RequiredAchievements = dto.RequiredAchievements,
            ScaleRequirements = ParseScaleRequirements(dto.ScaleRequirements, sceneTemplateId)
        };

        return filter;
    }

    /// <summary>
    /// Parse personality type strings to enum list
    /// </summary>
    private List<PersonalityType> ParsePersonalityTypes(List<string> typeStrings, string sceneTemplateId)
    {
        if (typeStrings == null || !typeStrings.Any())
            return new List<PersonalityType>();

        List<PersonalityType> types = new List<PersonalityType>();
        foreach (string typeString in typeStrings)
        {
            if (Enum.TryParse<PersonalityType>(typeString, true, out PersonalityType personalityType))
            {
                types.Add(personalityType);
            }
            else
            {
                throw new InvalidDataException($"SceneTemplate '{sceneTemplateId}' PlacementFilter has invalid PersonalityType: '{typeString}'");
            }
        }

        return types;
    }

    /// <summary>
    /// Parse location property strings to enum list
    /// </summary>
    private List<LocationPropertyType> ParseLocationProperties(List<string> propertyStrings, string sceneTemplateId)
    {
        if (propertyStrings == null || !propertyStrings.Any())
            return new List<LocationPropertyType>();

        List<LocationPropertyType> properties = new List<LocationPropertyType>();
        foreach (string propertyString in propertyStrings)
        {
            if (Enum.TryParse<LocationPropertyType>(propertyString, true, out LocationPropertyType property))
            {
                properties.Add(property);
            }
            else
            {
                throw new InvalidDataException($"SceneTemplate '{sceneTemplateId}' PlacementFilter has invalid LocationPropertyType: '{propertyString}'");
            }
        }

        return properties;
    }

    /// <summary>
    /// Parse state type strings to enum list
    /// </summary>
    private List<StateType> ParseStateTypes(List<string> stateStrings, string sceneTemplateId, string fieldName)
    {
        if (stateStrings == null || !stateStrings.Any())
            return new List<StateType>();

        List<StateType> states = new List<StateType>();
        foreach (string stateString in stateStrings)
        {
            if (Enum.TryParse<StateType>(stateString, true, out StateType stateType))
            {
                states.Add(stateType);
            }
            else
            {
                throw new InvalidDataException($"SceneTemplate '{sceneTemplateId}' PlacementFilter.{fieldName} has invalid StateType: '{stateString}'");
            }
        }

        return states;
    }

    /// <summary>
    /// Parse scale requirements from DTOs
    /// </summary>
    private List<ScaleRequirement> ParseScaleRequirements(List<ScaleRequirementDTO> dtos, string sceneTemplateId)
    {
        if (dtos == null || !dtos.Any())
            return new List<ScaleRequirement>();

        List<ScaleRequirement> requirements = new List<ScaleRequirement>();
        foreach (ScaleRequirementDTO dto in dtos)
        {
            if (string.IsNullOrEmpty(dto.ScaleType))
                throw new InvalidDataException($"SceneTemplate '{sceneTemplateId}' PlacementFilter ScaleRequirement missing 'ScaleType'");

            if (!Enum.TryParse<ScaleType>(dto.ScaleType, true, out ScaleType scaleType))
            {
                throw new InvalidDataException($"SceneTemplate '{sceneTemplateId}' PlacementFilter ScaleRequirement has invalid ScaleType: '{dto.ScaleType}'");
            }

            requirements.Add(new ScaleRequirement
            {
                ScaleType = scaleType,
                MinValue = dto.MinValue,
                MaxValue = dto.MaxValue
            });
        }

        return requirements;
    }

    /// <summary>
    /// Parse embedded SituationTemplates
    /// </summary>
    private List<SituationTemplate> ParseSituationTemplates(List<SituationTemplateDTO> dtos, string sceneTemplateId, SpawnPattern archetype)
    {
        if (dtos == null || !dtos.Any())
            throw new InvalidDataException($"SceneTemplate '{sceneTemplateId}' must have at least one SituationTemplate");

        List<SituationTemplate> templates = new List<SituationTemplate>();
        foreach (SituationTemplateDTO dto in dtos)
        {
            templates.Add(ParseSituationTemplate(dto, sceneTemplateId, archetype));
        }

        return templates;
    }

    /// <summary>
    /// Parse a single SituationTemplate
    /// </summary>
    private SituationTemplate ParseSituationTemplate(SituationTemplateDTO dto, string sceneTemplateId, SpawnPattern archetype)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidDataException($"SituationTemplate in SceneTemplate '{sceneTemplateId}' missing required 'Id'");

        // Parse SituationType (defaults to Normal if not specified for backward compatibility)
        SituationType situationType = SituationType.Normal;
        if (!string.IsNullOrEmpty(dto.Type))
        {
            if (!Enum.TryParse<SituationType>(dto.Type, true, out situationType))
            {
                throw new InvalidDataException($"SituationTemplate '{dto.Id}' in SceneTemplate '{sceneTemplateId}' has invalid Type value: '{dto.Type}'. Must be 'Normal' or 'Crisis'.");
            }
        }

        // ARCHETYPE-BASED GENERATION: If archetypeId present, generate 4 ChoiceTemplates from catalogue
        // Otherwise use hand-authored ChoiceTemplates from JSON (backward compatible)
        List<ChoiceTemplate> choiceTemplates;
        if (!string.IsNullOrEmpty(dto.ArchetypeId))
        {
            // PARSE-TIME ARCHETYPE GENERATION
            choiceTemplates = GenerateChoiceTemplatesFromArchetype(dto.ArchetypeId, sceneTemplateId, dto.Id);
        }
        else
        {
            // HAND-AUTHORED CHOICES (existing behavior)
            choiceTemplates = ParseChoiceTemplates(dto.ChoiceTemplates, sceneTemplateId, dto.Id, archetype);
        }

        SituationTemplate template = new SituationTemplate
        {
            Id = dto.Id,
            Type = situationType,
            ArchetypeId = dto.ArchetypeId,
            NarrativeTemplate = dto.NarrativeTemplate,
            ChoiceTemplates = choiceTemplates,
            Priority = dto.Priority,
            NarrativeHints = ParseNarrativeHints(dto.NarrativeHints),
            AutoProgressRewards = ParseChoiceReward(dto.AutoProgressRewards)
        };

        return template;
    }

    /// <summary>
    /// Parse embedded ChoiceTemplates
    /// </summary>
    private List<ChoiceTemplate> ParseChoiceTemplates(List<ChoiceTemplateDTO> dtos, string sceneTemplateId, string situationTemplateId, SpawnPattern archetype)
    {
        // AutoAdvance scenes have no choices (narrative auto-executes)
        if (archetype == SpawnPattern.AutoAdvance)
        {
            if (dtos != null && dtos.Any())
                throw new InvalidDataException($"AutoAdvance SceneTemplate '{sceneTemplateId}' SituationTemplate '{situationTemplateId}' should have EMPTY ChoiceTemplates (choices not allowed for AutoAdvance)");

            return new List<ChoiceTemplate>(); // Empty list for AutoAdvance
        }

        // Normal scenes require 2-4 choices (Sir Brante pattern)
        if (dtos == null || !dtos.Any())
            throw new InvalidDataException($"SituationTemplate '{situationTemplateId}' in SceneTemplate '{sceneTemplateId}' must have at least 2 ChoiceTemplates (Sir Brante pattern: 2-4 choices)");

        if (dtos.Count < 2 || dtos.Count > 4)
            throw new InvalidDataException($"SituationTemplate '{situationTemplateId}' in SceneTemplate '{sceneTemplateId}' has {dtos.Count} choices. Must have 2-4 choices (Sir Brante pattern)");

        List<ChoiceTemplate> templates = new List<ChoiceTemplate>();
        foreach (ChoiceTemplateDTO dto in dtos)
        {
            templates.Add(ParseChoiceTemplate(dto, sceneTemplateId, situationTemplateId));
        }

        return templates;
    }

    /// <summary>
    /// Parse a single ChoiceTemplate
    /// </summary>
    private ChoiceTemplate ParseChoiceTemplate(ChoiceTemplateDTO dto, string sceneTemplateId, string situationTemplateId)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidDataException($"ChoiceTemplate in SituationTemplate '{situationTemplateId}' (SceneTemplate '{sceneTemplateId}') missing required 'Id'");

        // Parse ActionType enum
        if (!Enum.TryParse<ChoiceActionType>(dto.ActionType, true, out ChoiceActionType actionType))
        {
            throw new InvalidDataException($"ChoiceTemplate '{dto.Id}' has invalid ActionType: '{dto.ActionType}'");
        }

        // Parse ChallengeType if present
        TacticalSystemType? challengeType = null;
        if (!string.IsNullOrEmpty(dto.ChallengeType))
        {
            if (Enum.TryParse<TacticalSystemType>(dto.ChallengeType, true, out TacticalSystemType parsedType))
            {
                challengeType = parsedType;
            }
            else
            {
                throw new InvalidDataException($"ChoiceTemplate '{dto.Id}' has invalid ChallengeType: '{dto.ChallengeType}'");
            }
        }

        ChoiceTemplate template = new ChoiceTemplate
        {
            Id = dto.Id,
            ActionTextTemplate = dto.ActionTextTemplate,
            RequirementFormula = RequirementParser.ConvertDTOToCompoundRequirement(dto.RequirementFormula),
            CostTemplate = ParseChoiceCost(dto.CostTemplate),
            RewardTemplate = ParseChoiceReward(dto.RewardTemplate),
            ActionType = actionType,
            ChallengeId = dto.ChallengeId,
            ChallengeType = challengeType,
            NavigationPayload = ParseNavigationPayload(dto.NavigationPayload)
        };

        return template;
    }

    /// <summary>
    /// Parse ChoiceCost from DTO
    /// </summary>
    private ChoiceCost ParseChoiceCost(ChoiceCostDTO dto)
    {
        if (dto == null)
            return new ChoiceCost(); // All costs default to 0

        return new ChoiceCost
        {
            Coins = dto.Coins,
            Resolve = dto.Resolve,
            TimeSegments = dto.TimeSegments,
            Health = dto.Health,
            Hunger = dto.Hunger,
            Stamina = dto.Stamina,
            Focus = dto.Focus
        };
    }

    /// <summary>
    /// Parse ChoiceReward from DTO
    /// </summary>
    private ChoiceReward ParseChoiceReward(ChoiceRewardDTO dto)
    {
        if (dto == null)
            return new ChoiceReward(); // No rewards

        return new ChoiceReward
        {
            Coins = dto.Coins,
            Resolve = dto.Resolve,
            TimeSegments = dto.TimeSegments,
            AdvanceToBlock = ParseTimeBlock(dto.AdvanceToBlock),
            AdvanceToDay = ParseDayAdvancement(dto.AdvanceToDay),
            Health = dto.Health,
            Hunger = dto.Hunger,
            Stamina = dto.Stamina,
            Focus = dto.Focus,
            FullRecovery = dto.FullRecovery,
            BondChanges = ParseBondChanges(dto.BondChanges),
            ScaleShifts = ParseScaleShifts(dto.ScaleShifts),
            StateApplications = ParseStateApplications(dto.StateApplications),
            AchievementIds = dto.AchievementIds,
            ItemIds = dto.ItemIds,
            ScenesToSpawn = ParseSceneSpawnRewards(dto.ScenesToSpawn)
        };
    }

    /// <summary>
    /// Parse BondChanges from DTOs
    /// </summary>
    private List<BondChange> ParseBondChanges(List<BondChangeDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<BondChange>();

        return dtos.Select(dto => new BondChange
        {
            NpcId = dto.NpcId,
            Delta = dto.Delta,
            Reason = dto.Reason
        }).ToList();
    }

    /// <summary>
    /// Parse ScaleShifts from DTOs
    /// </summary>
    private List<ScaleShift> ParseScaleShifts(List<ScaleShiftDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<ScaleShift>();

        List<ScaleShift> shifts = new List<ScaleShift>();
        foreach (ScaleShiftDTO dto in dtos)
        {
            if (!Enum.TryParse<ScaleType>(dto.ScaleType, true, out ScaleType scaleType))
            {
                throw new InvalidDataException($"ScaleShift has invalid ScaleType: '{dto.ScaleType}'");
            }

            shifts.Add(new ScaleShift
            {
                ScaleType = scaleType,
                Delta = dto.Delta,
                Reason = dto.Reason
            });
        }

        return shifts;
    }

    /// <summary>
    /// Parse StateApplications from DTOs
    /// </summary>
    private List<StateApplication> ParseStateApplications(List<StateApplicationDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<StateApplication>();

        List<StateApplication> applications = new List<StateApplication>();
        foreach (StateApplicationDTO dto in dtos)
        {
            if (!Enum.TryParse<StateType>(dto.StateType, true, out StateType stateType))
            {
                throw new InvalidDataException($"StateApplication has invalid StateType: '{dto.StateType}'");
            }

            applications.Add(new StateApplication
            {
                StateType = stateType,
                Apply = dto.Apply,
                Reason = dto.Reason
            });
        }

        return applications;
    }

    /// <summary>
    /// Parse SceneSpawnRewards from DTOs
    /// </summary>
    private List<SceneSpawnReward> ParseSceneSpawnRewards(List<SceneSpawnRewardDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<SceneSpawnReward>();

        List<SceneSpawnReward> rewards = new List<SceneSpawnReward>();
        foreach (SceneSpawnRewardDTO dto in dtos)
        {
            if (string.IsNullOrEmpty(dto.SceneTemplateId))
                throw new InvalidDataException("SceneSpawnReward missing required 'SceneTemplateId'");

            if (!Enum.TryParse<PlacementRelation>(dto.PlacementRelation, true, out PlacementRelation placementRelation))
            {
                throw new InvalidDataException($"SceneSpawnReward has invalid PlacementRelation: '{dto.PlacementRelation}'");
            }

            rewards.Add(new SceneSpawnReward
            {
                SceneTemplateId = dto.SceneTemplateId,
                PlacementRelation = placementRelation,
                SpecificPlacementId = dto.SpecificPlacementId,
                DelayDays = dto.DelayDays
            });
        }

        return rewards;
    }

    /// <summary>
    /// Parse NavigationPayload from DTO
    /// </summary>
    private NavigationPayload ParseNavigationPayload(NavigationPayloadDTO dto)
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
    /// Parse NarrativeHints from DTO
    /// </summary>
    private NarrativeHints ParseNarrativeHints(NarrativeHintsDTO dto)
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

    /// <summary>
    /// Parse SituationSpawnRules from DTO
    /// </summary>
    private SituationSpawnRules ParseSpawnRules(SituationSpawnRulesDTO dto, string sceneTemplateId)
    {
        if (dto == null)
            return null; // Optional - some SceneTemplates may not have complex spawn rules

        if (!Enum.TryParse<SpawnPattern>(dto.Pattern, true, out SpawnPattern pattern))
        {
            throw new InvalidDataException($"SceneTemplate '{sceneTemplateId}' SpawnRules has invalid Pattern: '{dto.Pattern}'");
        }

        return new SituationSpawnRules
        {
            Pattern = pattern,
            InitialSituationId = dto.InitialSituationId,
            Transitions = ParseSituationTransitions(dto.Transitions, sceneTemplateId),
            CompletionCondition = dto.CompletionCondition
        };
    }

    /// <summary>
    /// Parse SituationTransitions from DTOs
    /// </summary>
    private List<SituationTransition> ParseSituationTransitions(List<SituationTransitionDTO> dtos, string sceneTemplateId)
    {
        if (dtos == null || !dtos.Any())
            return new List<SituationTransition>();

        List<SituationTransition> transitions = new List<SituationTransition>();
        foreach (SituationTransitionDTO dto in dtos)
        {
            if (!Enum.TryParse<TransitionCondition>(dto.Condition, true, out TransitionCondition condition))
            {
                throw new InvalidDataException($"SceneTemplate '{sceneTemplateId}' SituationTransition has invalid Condition: '{dto.Condition}'");
            }

            transitions.Add(new SituationTransition
            {
                SourceSituationId = dto.SourceSituationId,
                DestinationSituationId = dto.DestinationSituationId,
                Condition = condition,
                SpecificChoiceId = dto.SpecificChoiceId
            });
        }

        return transitions;
    }

    /// <summary>
    /// Parse TimeBlock enum from string
    /// </summary>
    private TimeBlocks? ParseTimeBlock(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (Enum.TryParse<TimeBlocks>(value, ignoreCase: true, out TimeBlocks result))
            return result;

        throw new InvalidDataException($"Invalid TimeBlock value: '{value}'. Valid values: Morning, Midday, Afternoon, Evening");
    }

    /// <summary>
    /// Parse DayAdvancement enum from string
    /// </summary>
    private DayAdvancement? ParseDayAdvancement(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (Enum.TryParse<DayAdvancement>(value, ignoreCase: true, out DayAdvancement result))
            return result;

        throw new InvalidDataException($"Invalid DayAdvancement value: '{value}'. Valid values: CurrentDay, NextDay");
    }

    /// <summary>
    /// Generate 4 ChoiceTemplates from archetype structure (parse-time only)
    /// Called ONLY at parse time when SituationTemplate has archetypeId
    /// Creates the 4-choice pattern: stat-gated, money, challenge, fallback
    /// </summary>
    private List<ChoiceTemplate> GenerateChoiceTemplatesFromArchetype(string archetypeId, string sceneTemplateId, string situationTemplateId)
    {
        // Fetch archetype definition from catalogue (PARSE-TIME ONLY)
        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(archetypeId);

        List<ChoiceTemplate> choices = new List<ChoiceTemplate>();

        // CHOICE 1: Stat-Gated (Primary OR Secondary stat)
        // Best outcome, free if stat requirement met
        ChoiceTemplate statGatedChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_stat",
            ActionTextTemplate = GenerateStatGatedActionText(archetype),
            RequirementFormula = CreateStatRequirement(archetype),
            CostTemplate = new ChoiceCost(), // Free
            RewardTemplate = new ChoiceReward(), // Will be defined in JSON or instantiated at spawn time
            ActionType = ChoiceActionType.Instant
        };
        choices.Add(statGatedChoice);

        // CHOICE 2: Money
        // Guaranteed success, expensive
        ChoiceTemplate moneyChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_money",
            ActionTextTemplate = GenerateMoneyActionText(archetype),
            RequirementFormula = new CompoundRequirement(), // No requirements
            CostTemplate = new ChoiceCost { Coins = archetype.CoinCost },
            RewardTemplate = new ChoiceReward(),
            ActionType = ChoiceActionType.Instant
        };
        choices.Add(moneyChoice);

        // CHOICE 3: Challenge
        // Variable outcome, risky
        ChoiceTemplate challengeChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_challenge",
            ActionTextTemplate = GenerateChallengeActionText(archetype),
            RequirementFormula = new CompoundRequirement(), // No requirements (but has resource cost)
            CostTemplate = new ChoiceCost { Resolve = archetype.ResolveCost },
            RewardTemplate = new ChoiceReward(),
            ActionType = ChoiceActionType.StartChallenge,
            ChallengeId = null, // Will be set by spawn-time instantiation
            ChallengeType = archetype.ChallengeType
        };
        choices.Add(challengeChoice);

        // CHOICE 4: Fallback
        // Poor outcome, always available
        ChoiceTemplate fallbackChoice = new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_fallback",
            ActionTextTemplate = GenerateFallbackActionText(archetype),
            RequirementFormula = new CompoundRequirement(), // No requirements
            CostTemplate = new ChoiceCost { TimeSegments = archetype.FallbackTimeCost },
            RewardTemplate = new ChoiceReward(),
            ActionType = ChoiceActionType.Instant
        };
        choices.Add(fallbackChoice);

        return choices;
    }

    /// <summary>
    /// Create compound requirement with OR logic for primary/secondary stat
    /// </summary>
    private CompoundRequirement CreateStatRequirement(SituationArchetype archetype)
    {
        CompoundRequirement requirement = new CompoundRequirement();

        // Path 1: Primary stat meets threshold
        OrPath primaryPath = new OrPath
        {
            Label = $"{archetype.PrimaryStat} {archetype.StatThreshold}+",
            NumericRequirements = new List<NumericRequirement>
            {
                new NumericRequirement
                {
                    Type = "PlayerStat",
                    Context = archetype.PrimaryStat.ToString(),
                    Threshold = archetype.StatThreshold,
                    Label = $"{archetype.PrimaryStat} {archetype.StatThreshold}+"
                }
            }
        };
        requirement.OrPaths.Add(primaryPath);

        // Path 2: Secondary stat meets threshold (only if different from primary)
        if (archetype.SecondaryStat != archetype.PrimaryStat)
        {
            OrPath secondaryPath = new OrPath
            {
                Label = $"{archetype.SecondaryStat} {archetype.StatThreshold}+",
                NumericRequirements = new List<NumericRequirement>
                {
                    new NumericRequirement
                    {
                        Type = "PlayerStat",
                        Context = archetype.SecondaryStat.ToString(),
                        Threshold = archetype.StatThreshold,
                        Label = $"{archetype.SecondaryStat} {archetype.StatThreshold}+"
                    }
                }
            };
            requirement.OrPaths.Add(secondaryPath);
        }

        return requirement;
    }

    /// <summary>
    /// Generate action text template for stat-gated choice
    /// </summary>
    private string GenerateStatGatedActionText(SituationArchetype archetype)
    {
        return archetype.Id switch
        {
            "confrontation" => "Assert authority and take command",
            "negotiation" => "Negotiate favorable terms",
            "investigation" => "Deduce the solution through analysis",
            "social_maneuvering" => "Read the social dynamics and navigate skillfully",
            "crisis" => "Take decisive action with expertise",
            _ => "Use your expertise"
        };
    }

    /// <summary>
    /// Generate action text template for money choice
    /// </summary>
    private string GenerateMoneyActionText(SituationArchetype archetype)
    {
        return archetype.Id switch
        {
            "confrontation" => "Pay off the opposition",
            "negotiation" => "Pay the premium price",
            "investigation" => "Hire an expert or pay for information",
            "social_maneuvering" => "Offer a generous gift",
            "crisis" => "Pay for emergency solution",
            _ => "Pay to resolve"
        };
    }

    /// <summary>
    /// Generate action text template for challenge choice
    /// </summary>
    private string GenerateChallengeActionText(SituationArchetype archetype)
    {
        return archetype.Id switch
        {
            "confrontation" => "Attempt a physical confrontation",
            "negotiation" => "Engage in complex debate",
            "investigation" => "Work through the puzzle systematically",
            "social_maneuvering" => "Make a bold social gambit",
            "crisis" => "Risk everything on a desperate gambit",
            _ => "Accept the challenge"
        };
    }

    /// <summary>
    /// Generate action text template for fallback choice
    /// </summary>
    private string GenerateFallbackActionText(SituationArchetype archetype)
    {
        return archetype.Id switch
        {
            "confrontation" => "Back down and submit",
            "negotiation" => "Accept unfavorable terms",
            "investigation" => "Give up and move on",
            "social_maneuvering" => "Exit awkwardly",
            "crisis" => "Flee the situation",
            _ => "Accept poor outcome"
        };
    }
}
