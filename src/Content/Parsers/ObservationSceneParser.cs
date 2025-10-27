using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;
using Wayfarer.GameState.Enums;

/// <summary>
/// Parser for converting ObservationSceneDTO to ObservationScene domain model
/// </summary>
public static class ObservationSceneParser
{
    /// <summary>
    /// Convert ObservationSceneDTO to ObservationScene entity
    /// </summary>
    public static ObservationScene Parse(ObservationSceneDTO dto, GameWorld gameWorld)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException("ObservationScene missing required 'Id' field");

        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"ObservationScene '{dto.Id}' missing required 'Name' field");

        if (string.IsNullOrEmpty(dto.LocationId))
            throw new InvalidOperationException($"ObservationScene '{dto.Id}' missing required 'LocationId' field");

        if (dto.ExaminationPoints == null || dto.ExaminationPoints.Count == 0)
            throw new InvalidOperationException($"ObservationScene '{dto.Id}' must have at least one examination point");

        // Verify location exists
        Location location = gameWorld.Locations.FirstOrDefault(l => l.Id == dto.LocationId);
        if (location == null)
        {
            throw new InvalidOperationException(
                $"ObservationScene '{dto.Id}' references unknown location '{dto.LocationId}'");
        }

        ObservationScene scene = new ObservationScene
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description ?? "",
            Location = location,  // Resolve object reference during parsing (HIGHLANDER: ID is parsing artifact)
            RequiredKnowledge = dto.RequiredKnowledge ?? new List<string>(),
            IsRepeatable = dto.IsRepeatable,
            IsCompleted = dto.IsCompleted,
            ExaminedPointIds = dto.ExaminedPointIds ?? new List<string>()
        };

        // Parse examination points
        foreach (ExaminationPointDTO pointDto in dto.ExaminationPoints)
        {
            ExaminationPoint point = ParseExaminationPoint(pointDto, dto.Id);
            scene.ExaminationPoints.Add(point);
        }

        // Validate revelation references
        foreach (ExaminationPoint point in scene.ExaminationPoints)
        {
            if (!string.IsNullOrEmpty(point.RevealsExaminationPointId))
            {
                if (!scene.ExaminationPoints.Any(p => p.Id == point.RevealsExaminationPointId))
                {
                    throw new InvalidOperationException(
                        $"ExaminationPoint '{point.Id}' in scene '{dto.Id}' has invalid RevealsExaminationPointId '{point.RevealsExaminationPointId}'. " +
                        $"No examination point with that ID exists in this scene.");
                }
            }
        }

        return scene;
    }

    /// <summary>
    /// Parse an examination point from DTO
    /// </summary>
    private static ExaminationPoint ParseExaminationPoint(ExaminationPointDTO dto, string sceneId)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException($"ExaminationPoint in scene '{sceneId}' missing required 'Id' field");

        if (string.IsNullOrEmpty(dto.Title))
            throw new InvalidOperationException($"ExaminationPoint '{dto.Id}' in scene '{sceneId}' missing required 'Title' field");

        // Parse required stat if present
        PlayerStatType? requiredStat = null;
        if (!string.IsNullOrEmpty(dto.RequiredStat))
        {
            if (Enum.TryParse<PlayerStatType>(dto.RequiredStat, ignoreCase: true, out PlayerStatType parsedStat))
            {
                requiredStat = parsedStat;
            }
            else
            {
                throw new InvalidOperationException(
                    $"ExaminationPoint '{dto.Id}' has invalid RequiredStat '{dto.RequiredStat}'. " +
                    $"Must be one of: {string.Join(", ", Enum.GetNames<PlayerStatType>())}");
            }
        }

        // Use defaults from catalogue if costs not specified
        int focusCost = dto.FocusCost;
        if (focusCost == 0)
        {
            focusCost = ObservationCatalog.GetDefaultFocusCost();
        }

        int timeCost = dto.TimeCost;
        if (timeCost == 0)
        {
            timeCost = ObservationCatalog.GetDefaultTimeCost();
        }

        // Validate find item chance
        if (dto.FindItemChance < 0 || dto.FindItemChance > 100)
        {
            throw new InvalidOperationException(
                $"ExaminationPoint '{dto.Id}' has invalid FindItemChance '{dto.FindItemChance}'. " +
                $"Must be between 0 and 100.");
        }

        ExaminationPoint point = new ExaminationPoint
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description ?? "",
            FocusCost = focusCost,
            TimeCost = timeCost,
            RequiredStat = requiredStat,
            RequiredStatLevel = dto.RequiredStatLevel,
            RequiredKnowledge = dto.RequiredKnowledge ?? new List<string>(),
            GrantedKnowledge = dto.GrantedKnowledge ?? new List<string>(),
            SpawnedSituationId = dto.SpawnedSituationId,
            SpawnedConversationId = dto.SpawnedConversationId,
            FoundItemId = dto.FoundItemId,
            FindItemChance = dto.FindItemChance,
            RevealsExaminationPointId = dto.RevealsExaminationPointId,
            IsHidden = dto.IsHidden,
            IsExamined = dto.IsExamined
        };

        return point;
    }
}
