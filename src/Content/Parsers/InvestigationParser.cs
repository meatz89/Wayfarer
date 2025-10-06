using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Parser for Investigation definitions - converts DTOs to domain models
/// Creates Investigation entities with phase definitions for goal spawning
/// </summary>
public class InvestigationParser
{
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
        return new InvestigationPhaseDefinition
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Goal = dto.Goal,
            SystemType = ParseSystemType(dto.SystemType),
            EngagementTypeId = dto.EngagementTypeId,
            LocationId = dto.LocationId,
            SpotId = dto.SpotId,
            NpcId = dto.NpcId,
            RequestId = dto.RequestId,
            Requirements = ParseRequirements(dto.Requirements)
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
