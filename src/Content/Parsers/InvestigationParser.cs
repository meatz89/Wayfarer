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
            ObligationType = ParseObligationType(dto.ObligationType),
            PatronNpcId = dto.PatronNpcId,
            DeadlineSegment = dto.DeadlineSegment,
            CompletionRewardCoins = dto.CompletionRewardCoins,
            CompletionRewardItems = dto.CompletionRewardItems ?? new List<string>(),
            CompletionRewardXP = ParseXPRewards(dto.CompletionRewardXP),
            SpawnedInvestigationIds = dto.SpawnedInvestigationIds ?? new List<string>(),
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
            // GoalRequirements system eliminated - phases progress through actual goal completion tracking
            CompletionReward = ParseCompletionReward(dto.CompletionReward)
        };
    }

    private PhaseCompletionReward ParseCompletionReward(PhaseCompletionRewardDTO dto)
    {
        if (dto == null) return null;

        PhaseCompletionReward reward = new PhaseCompletionReward
        {
            UnderstandingReward = dto.UnderstandingReward
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
            TacticalSystemType.Mental => _gameWorld.MentalChallengeDecks.Any(d => d.Id == deckId),
            TacticalSystemType.Physical => _gameWorld.PhysicalChallengeDecks.Any(d => d.Id == deckId),
            TacticalSystemType.Social => _gameWorld.SocialChallengeDecks.Any(d => d.Id == deckId),
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
            TacticalSystemType.Mental => _gameWorld.MentalChallengeDecks.Select(d => d.Id),
            TacticalSystemType.Physical => _gameWorld.PhysicalChallengeDecks.Select(d => d.Id),
            TacticalSystemType.Social => _gameWorld.SocialChallengeDecks.Select(d => d.Id),
            _ => new List<string>()
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

        return new InvestigationIntroAction
        {
            TriggerType = ParseTriggerType(dto.TriggerType),
            TriggerPrerequisites = ParseInvestigationPrerequisites(dto.TriggerPrerequisites),
            ActionText = dto.ActionText,
            LocationId = dto.LocationId,
            IntroNarrative = dto.IntroNarrative,
            CompletionReward = ParseCompletionReward(dto.CompletionReward)
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

    private InvestigationObligationType ParseObligationType(string typeString)
    {
        if (string.IsNullOrEmpty(typeString))
            return InvestigationObligationType.SelfDiscovered; // default

        return Enum.TryParse<InvestigationObligationType>(typeString, out InvestigationObligationType type)
            ? type
            : InvestigationObligationType.SelfDiscovered;
    }

    private InvestigationPrerequisites ParseInvestigationPrerequisites(InvestigationPrerequisitesDTO dto)
    {
        if (dto == null) return new InvestigationPrerequisites();

        return new InvestigationPrerequisites
        {
            LocationId = dto.LocationId
        };
    }

    private List<StatXPReward> ParseXPRewards(Dictionary<string, int> xpRewardsDto)
    {
        if (xpRewardsDto == null || xpRewardsDto.Count == 0)
            return new List<StatXPReward>();

        List<StatXPReward> rewards = new List<StatXPReward>();

        foreach (KeyValuePair<string, int> entry in xpRewardsDto)
        {
            PlayerStatType statType = ParsePlayerStatType(entry.Key);
            if (statType != PlayerStatType.None && entry.Value > 0)
            {
                rewards.Add(new StatXPReward
                {
                    Stat = statType,
                    XPAmount = entry.Value
                });
            }
        }

        return rewards;
    }

    private PlayerStatType ParsePlayerStatType(string statName)
    {
        if (string.IsNullOrEmpty(statName))
            return PlayerStatType.None;

        return Enum.TryParse<PlayerStatType>(statName, ignoreCase: true, out PlayerStatType statType)
            ? statType
            : PlayerStatType.None;
    }
}
