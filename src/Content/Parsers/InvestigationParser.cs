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
            SystemType = systemType,
            ChallengeTypeId = dto.ChallengeTypeId,
            LocationId = dto.LocationId,
            SpotId = dto.SpotId,
            NpcId = dto.NpcId,
            RequestId = dto.RequestId,
            Requirements = ParseRequirements(dto.Requirements)
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

        GoalRequirements requirements = new GoalRequirements
        {
            RequiredKnowledge = dto.Knowledge ?? new List<string>(),
            RequiredEquipment = dto.Equipment ?? new List<string>()
        };

        // Map CompletedPhases (phase indices) to CompletedGoals (goal IDs)
        // This will be resolved at runtime by InvestigationActivity when creating goals
        if (dto.CompletedPhases != null && dto.CompletedPhases.Any())
        {
            // Phase indices are stored as goal IDs like "investigation_id:phase_index"
            // InvestigationActivity will resolve these to actual phase IDs
            foreach (int phaseIndex in dto.CompletedPhases)
            {
                requirements.CompletedGoals.Add($"phase:{phaseIndex}");
            }
        }

        return requirements;
    }

    private TacticalSystemType ParseSystemType(string systemTypeString)
    {
        return Enum.TryParse<TacticalSystemType>(systemTypeString, out TacticalSystemType type)
            ? type
            : TacticalSystemType.Mental; // default
    }
}
