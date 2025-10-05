using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Parser for Investigation Templates - converts DTOs to domain models
/// Parallel to MentalCardParser
/// </summary>
public class InvestigationParser
{
    public InvestigationTemplate ParseInvestigation(InvestigationDTO dto)
    {
        return new InvestigationTemplate
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Personality = ParsePersonalityType(dto.Personality),
            PersonalityTypes = dto.PersonalityTypes?.Select(p => ParsePersonalityType(p)).ToList() ?? new List<LocationPersonalityType>(),
            ExposureThreshold = dto.ExposureThreshold,
            TimeLimit = dto.TimeLimit,
            Phases = dto.Phases?.Select(p => ParsePhase(p)).ToList() ?? new List<InvestigationPhase>(),
            ObservationCardRewards = dto.ObservationCardRewards?.Select(r => ParseObservationReward(r)).ToList() ?? new List<InvestigationObservationReward>()
        };
    }

    private InvestigationPhase ParsePhase(InvestigationPhaseDTO dto)
    {
        return new InvestigationPhase
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Goal = dto.Goal,
            ProgressThreshold = dto.ProgressThreshold,
            SystemType = ParseSystemType(dto.SystemType),
            CardDeckIds = dto.CardDeckIds ?? new List<string>(),
            Requirements = ParseRequirements(dto.Requirements),
            CompletionReward = ParseCompletionReward(dto.CompletionReward)
        };
    }

    private PhaseRequirements ParseRequirements(PhaseRequirementsDTO dto)
    {
        if (dto == null) return new PhaseRequirements();

        Dictionary<DiscoveryType, int> discoveryQuantities = new Dictionary<DiscoveryType, int>();
        if (dto.DiscoveryQuantities != null)
        {
            foreach (var kvp in dto.DiscoveryQuantities)
            {
                if (Enum.TryParse<DiscoveryType>(kvp.Key, out DiscoveryType discoveryType))
                {
                    discoveryQuantities[discoveryType] = kvp.Value;
                }
            }
        }

        return new PhaseRequirements
        {
            CompletedPhases = dto.CompletedPhases ?? new List<int>(),
            DiscoveryQuantities = discoveryQuantities,
            SpecificDiscoveries = dto.SpecificDiscoveries ?? new List<string>(),
            Equipment = dto.Equipment ?? new List<string>(),
            Knowledge = dto.Knowledge ?? new List<string>()
        };
    }

    private PhaseCompletionReward ParseCompletionReward(PhaseCompletionRewardDTO dto)
    {
        if (dto == null) return new PhaseCompletionReward();

        return new PhaseCompletionReward
        {
            Narrative = dto.Narrative,
            DiscoveriesGranted = dto.DiscoveriesGranted ?? new List<string>(),
            UnlocksPhaseId = dto.UnlocksPhaseId
        };
    }

    private InvestigationObservationReward ParseObservationReward(InvestigationObservationRewardDTO dto)
    {
        return new InvestigationObservationReward
        {
            DiscoveryId = dto.DiscoveryId,
            NpcId = dto.NpcId,
            CardId = dto.CardId
        };
    }

    private TacticalSystemType ParseSystemType(string systemTypeString)
    {
        return Enum.TryParse<TacticalSystemType>(systemTypeString, out TacticalSystemType type)
            ? type
            : TacticalSystemType.Mental; // default
    }

    private LocationPersonalityType ParsePersonalityType(string personalityString)
    {
        return Enum.TryParse<LocationPersonalityType>(personalityString, out LocationPersonalityType personality)
            ? personality
            : LocationPersonalityType.SequentialSite; // default - methodical progression
    }
}
