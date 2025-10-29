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
        if (string.IsNullOrEmpty(dto.Archetype))
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' missing required field 'Archetype'");

        // Parse archetype enum
        if (!Enum.TryParse<SpawnPattern>(dto.Archetype, true, out SpawnPattern archetype))
        {
            throw new InvalidDataException($"SceneTemplate '{dto.Id}' has invalid Archetype value: '{dto.Archetype}'");
        }

        SceneTemplate template = new SceneTemplate
        {
            Id = dto.Id,
            Archetype = archetype,
            DisplayNameTemplate = dto.DisplayNameTemplate,
            PlacementFilter = ParsePlacementFilter(dto.PlacementFilter, dto.Id),
            SituationTemplates = ParseSituationTemplates(dto.SituationTemplates, dto.Id),
            SpawnRules = ParseSpawnRules(dto.SpawnRules, dto.Id),
            ExpirationDays = dto.ExpirationDays,
            IsStarter = dto.IsStarter,
            IntroNarrativeTemplate = dto.IntroNarrativeTemplate,
            Tier = dto.Tier
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
    private List<SituationTemplate> ParseSituationTemplates(List<SituationTemplateDTO> dtos, string sceneTemplateId)
    {
        if (dtos == null || !dtos.Any())
            throw new InvalidDataException($"SceneTemplate '{sceneTemplateId}' must have at least one SituationTemplate");

        List<SituationTemplate> templates = new List<SituationTemplate>();
        foreach (SituationTemplateDTO dto in dtos)
        {
            templates.Add(ParseSituationTemplate(dto, sceneTemplateId));
        }

        return templates;
    }

    /// <summary>
    /// Parse a single SituationTemplate
    /// </summary>
    private SituationTemplate ParseSituationTemplate(SituationTemplateDTO dto, string sceneTemplateId)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidDataException($"SituationTemplate in SceneTemplate '{sceneTemplateId}' missing required 'Id'");

        SituationTemplate template = new SituationTemplate
        {
            Id = dto.Id,
            NarrativeTemplate = dto.NarrativeTemplate,
            ChoiceTemplates = ParseChoiceTemplates(dto.ChoiceTemplates, sceneTemplateId, dto.Id),
            Priority = dto.Priority,
            NarrativeHints = ParseNarrativeHints(dto.NarrativeHints)
        };

        return template;
    }

    /// <summary>
    /// Parse embedded ChoiceTemplates
    /// </summary>
    private List<ChoiceTemplate> ParseChoiceTemplates(List<ChoiceTemplateDTO> dtos, string sceneTemplateId, string situationTemplateId)
    {
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
            TimeSegments = dto.TimeSegments
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
}
