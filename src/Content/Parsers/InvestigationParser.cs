using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Parser for Investigation definitions - converts DTOs to domain models
/// Creates Investigation entities with phase definitions for goal spawning
/// Validates challenge type IDs against GameWorld at parse time
/// </summary>
public class InvestigationParser
{
    private readonly GameWorld _gameWorld;

    public InvestigationParser(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    public Investigation ParseInvestigation(InvestigationDTO dto)
    {
        return new Investigation
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            IntroAction = ParseIntroAction(dto.Intro),
            ColorCode = dto.ColorCode,
            PhaseDefinitions = dto.Phases?.Select((p, index) => ParsePhaseDefinition(p, dto.Id)).ToList() ?? new List<InvestigationPhaseDefinition>()
        };
    }

    private InvestigationPhaseDefinition ParsePhaseDefinition(InvestigationPhaseDTO dto, string investigationId)
    {
        return new InvestigationPhaseDefinition
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            OutcomeNarrative = dto.OutcomeNarrative,
            Requirements = ParseRequirements(dto.Requirements),
            CompletionReward = ParseCompletionReward(dto.CompletionReward)
        };
    }

    private PhaseCompletionReward ParseCompletionReward(PhaseCompletionRewardDTO dto)
    {
        if (dto == null) return null;

        PhaseCompletionReward reward = new PhaseCompletionReward
        {
            KnowledgeGranted = dto.KnowledgeGranted ?? new List<string>()
        };

        // Parse obstacle spawns
        if (dto.ObstaclesSpawned != null && dto.ObstaclesSpawned.Count > 0)
        {
            foreach (ObstacleSpawnInfoDTO spawnDto in dto.ObstaclesSpawned)
            {
                ObstacleSpawnTargetType targetType = ParseObstacleSpawnTargetType(spawnDto.TargetType);
                Obstacle obstacle = ObstacleParser.ConvertDTOToObstacle(spawnDto.Obstacle, spawnDto.TargetEntityId, _gameWorld);

                reward.ObstaclesSpawned.Add(new ObstacleSpawnInfo
                {
                    TargetType = targetType,
                    TargetEntityId = spawnDto.TargetEntityId,
                    Obstacle = obstacle
                });
            }
        }

        return reward;
    }

    private ObstacleSpawnTargetType ParseObstacleSpawnTargetType(string typeString)
    {
        if (string.IsNullOrEmpty(typeString))
            throw new InvalidOperationException("ObstacleSpawnInfo missing required 'targetType' field");

        return typeString.ToLowerInvariant() switch
        {
            "location" => ObstacleSpawnTargetType.Location,
            "route" => ObstacleSpawnTargetType.Route,
            "npc" => ObstacleSpawnTargetType.NPC,
            _ => throw new InvalidOperationException(
                $"Invalid ObstacleSpawnTargetType '{typeString}'. Valid values: Location, Route, NPC")
        };
    }

    private void ValidateDeckId(string deckId, TacticalSystemType systemType, string phaseId)
    {
        if (string.IsNullOrEmpty(deckId))
        {
            throw new InvalidOperationException($"Investigation intro action '{phaseId}' has no deckId specified");
        }

        bool exists = systemType switch
        {
            TacticalSystemType.Mental => _gameWorld.MentalChallengeDecks.ContainsKey(deckId),
            TacticalSystemType.Physical => _gameWorld.PhysicalChallengeDecks.ContainsKey(deckId),
            TacticalSystemType.Social => _gameWorld.SocialChallengeDecks.ContainsKey(deckId),
            _ => throw new InvalidOperationException($"Unknown TacticalSystemType: {systemType}")
        };

        if (!exists)
        {
            throw new InvalidOperationException(
                $"Investigation intro action '{phaseId}' references {systemType} engagement deck '{deckId}' which does not exist in GameWorld. " +
                $"Available {systemType} engagement decks: {string.Join(", ", GetAvailableChallengeDeckIds(systemType))}"
            );
        }
    }

    private IEnumerable<string> GetAvailableChallengeDeckIds(TacticalSystemType systemType)
    {
        return systemType switch
        {
            TacticalSystemType.Mental => _gameWorld.MentalChallengeDecks.Keys,
            TacticalSystemType.Physical => _gameWorld.PhysicalChallengeDecks.Keys,
            TacticalSystemType.Social => _gameWorld.SocialChallengeDecks.Keys,
            _ => new List<string>()
        };
    }

    private GoalRequirements ParseRequirements(PhaseRequirementsDTO dto)
    {
        if (dto == null) return new GoalRequirements();

        return new GoalRequirements
        {
            RequiredKnowledge = dto.Knowledge ?? new List<string>(),
            RequiredEquipment = dto.Equipment ?? new List<string>(),
            CompletedGoals = dto.CompletedGoals ?? new List<string>()
        };
    }

    private TacticalSystemType ParseSystemType(string systemTypeString)
    {
        return Enum.TryParse<TacticalSystemType>(systemTypeString, out TacticalSystemType type)
            ? type
            : TacticalSystemType.Mental; // default
    }

    private InvestigationIntroAction ParseIntroAction(InvestigationIntroActionDTO dto)
    {
        if (dto == null) return null;

        TacticalSystemType systemType = ParseSystemType(dto.SystemType);

        // [Oracle] Validation: deckId must exist for intro action
        if (!string.IsNullOrEmpty(dto.DeckId))
        {
            ValidateDeckId(dto.DeckId, systemType, "intro");
        }

        return new InvestigationIntroAction
        {
            TriggerType = ParseTriggerType(dto.TriggerType),
            TriggerPrerequisites = ParseInvestigationPrerequisites(dto.TriggerPrerequisites),
            ActionText = dto.ActionText,
            SystemType = systemType,
            DeckId = dto.DeckId,
            LocationId = dto.LocationId,
            NpcId = dto.NpcId,
            IntroNarrative = dto.IntroNarrative
        };
    }

    private DiscoveryTriggerType ParseTriggerType(string triggerTypeString)
    {
        if (string.IsNullOrEmpty(triggerTypeString))
            return DiscoveryTriggerType.ImmediateVisibility; // default

        return Enum.TryParse<DiscoveryTriggerType>(triggerTypeString, out DiscoveryTriggerType type)
            ? type
            : DiscoveryTriggerType.ImmediateVisibility;
    }

    private InvestigationPrerequisites ParseInvestigationPrerequisites(InvestigationPrerequisitesDTO dto)
    {
        if (dto == null) return new InvestigationPrerequisites();

        return new InvestigationPrerequisites
        {
            LocationId = dto.LocationId,
            RequiredKnowledge = dto.RequiredKnowledge ?? new List<string>(),
            RequiredItems = dto.RequiredItems ?? new List<string>(),
            RequiredObligation = dto.RequiredObligation
        };
    }
}
