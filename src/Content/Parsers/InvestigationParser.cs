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
        TacticalSystemType systemType = ParseSystemType(dto.SystemType);

        // [Oracle] Validation: challengeTypeId must exist in GameWorld
        ValidateChallengeTypeId(dto.ChallengeTypeId, systemType, dto.Id);

        return new InvestigationPhaseDefinition
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Goal = dto.Goal,
            OutcomeNarrative = dto.OutcomeNarrative,
            SystemType = systemType,
            ChallengeTypeId = dto.ChallengeTypeId,
            LocationId = dto.LocationId,
            SpotId = dto.SpotId,
            NpcId = dto.NpcId,
            RequestId = dto.RequestId,
            Requirements = ParseRequirements(dto.Requirements),
            CompletionReward = ParseCompletionReward(dto.CompletionReward)
        };
    }

    private PhaseCompletionReward ParseCompletionReward(PhaseCompletionRewardDTO dto)
    {
        if (dto == null) return null;

        return new PhaseCompletionReward
        {
            KnowledgeGranted = dto.KnowledgeGranted ?? new List<string>()
        };
    }

    private void ValidateChallengeTypeId(string challengeTypeId, TacticalSystemType systemType, string phaseId)
    {
        if (string.IsNullOrEmpty(challengeTypeId))
        {
            throw new InvalidOperationException($"Investigation phase '{phaseId}' has no challengeTypeId specified");
        }

        bool exists = systemType switch
        {
            TacticalSystemType.Mental => _gameWorld.MentalChallengeTypes.ContainsKey(challengeTypeId),
            TacticalSystemType.Physical => _gameWorld.PhysicalChallengeTypes.ContainsKey(challengeTypeId),
            TacticalSystemType.Social => _gameWorld.SocialChallengeTypes.ContainsKey(challengeTypeId),
            _ => throw new InvalidOperationException($"Unknown TacticalSystemType: {systemType}")
        };

        if (!exists)
        {
            throw new InvalidOperationException(
                $"Investigation phase '{phaseId}' references {systemType} challenge type '{challengeTypeId}' which does not exist in GameWorld. " +
                $"Available {systemType} challenge types: {string.Join(", ", GetAvailableChallengeTypeIds(systemType))}"
            );
        }
    }

    private IEnumerable<string> GetAvailableChallengeTypeIds(TacticalSystemType systemType)
    {
        return systemType switch
        {
            TacticalSystemType.Mental => _gameWorld.MentalChallengeTypes.Keys,
            TacticalSystemType.Physical => _gameWorld.PhysicalChallengeTypes.Keys,
            TacticalSystemType.Social => _gameWorld.SocialChallengeTypes.Keys,
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

        // [Oracle] Validation: challengeTypeId must exist for intro action
        if (!string.IsNullOrEmpty(dto.ChallengeTypeId))
        {
            ValidateChallengeTypeId(dto.ChallengeTypeId, systemType, "intro");
        }

        return new InvestigationIntroAction
        {
            TriggerType = ParseTriggerType(dto.TriggerType),
            TriggerPrerequisites = ParseInvestigationPrerequisites(dto.TriggerPrerequisites),
            ActionText = dto.ActionText,
            SystemType = systemType,
            ChallengeTypeId = dto.ChallengeTypeId,
            LocationId = dto.LocationId,
            SpotId = dto.SpotId,
            NpcId = dto.NpcId,
            RequestId = dto.RequestId,
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
            SpotId = dto.SpotId,
            MinLocationFamiliarity = dto.MinLocationFamiliarity,
            RequiredKnowledge = dto.RequiredKnowledge ?? new List<string>(),
            RequiredItems = dto.RequiredItems ?? new List<string>(),
            RequiredObligation = dto.RequiredObligation
        };
    }
}
