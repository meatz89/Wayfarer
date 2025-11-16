/// <summary>
/// Parser for ObservationScene - Mental challenge system for scene investigation.
/// Translates JSON DTOs into domain entities with GameWorld references resolved.
/// </summary>
public static class ObservationSceneParser
{
    public static ObservationScene Parse(ObservationSceneDTO dto, GameWorld gameWorld)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        if (gameWorld == null)
            throw new ArgumentNullException(nameof(gameWorld));
        if (string.IsNullOrWhiteSpace(dto.LocationId))
            throw new InvalidOperationException($"ObservationScene '{dto.Name}' must have a LocationId");

        // Resolve location reference
        Location location = gameWorld.Locations.FirstOrDefault(l => l.Id == dto.LocationId);
        if (location == null)
            throw new InvalidOperationException($"ObservationScene '{dto.Name}' references unknown Location '{dto.LocationId}'");

        ObservationScene scene = new ObservationScene
        {
            Name = dto.Name,
            Description = dto.Description,
            Location = location, // Object reference, no LocationId
            RequiredKnowledge = new List<string>(dto.RequiredKnowledge),
            IsRepeatable = dto.IsRepeatable,
            IsCompleted = false,
            ExaminedPoints = new List<ExaminationPoint>() // Object collection, not ExaminedPointIds
        };

        // Parse examination points (first pass - no cross-references yet)
        Dictionary<string, ExaminationPoint> pointsById = new Dictionary<string, ExaminationPoint>();
        foreach (ExaminationPointDTO pointDto in dto.ExaminationPoints)
        {
            ExaminationPoint point = ParseExaminationPoint(pointDto, gameWorld);
            scene.ExaminationPoints.Add(point);

            // Track by DTO Id for second pass reference resolution
            if (!string.IsNullOrWhiteSpace(pointDto.Id))
            {
                pointsById[pointDto.Id] = point;
            }
        }

        // Second pass - resolve RevealsExaminationPoint references
        for (int i = 0; i < dto.ExaminationPoints.Count; i++)
        {
            ExaminationPointDTO pointDto = dto.ExaminationPoints[i];
            ExaminationPoint point = scene.ExaminationPoints[i];

            if (!string.IsNullOrWhiteSpace(pointDto.RevealsExaminationPointId))
            {
                if (pointsById.TryGetValue(pointDto.RevealsExaminationPointId, out ExaminationPoint revealedPoint))
                {
                    point.RevealsExaminationPoint = revealedPoint;
                }
            }
        }

        return scene;
    }

    private static ExaminationPoint ParseExaminationPoint(ExaminationPointDTO dto, GameWorld gameWorld)
    {
        ExaminationPoint point = new ExaminationPoint
        {
            Title = dto.Title,
            Description = dto.Description,
            FocusCost = dto.FocusCost,
            TimeCost = dto.TimeCost,
            RequiredKnowledge = new List<string>(dto.RequiredKnowledge),
            IsHidden = dto.IsHidden,
            IsExamined = false,
            GrantedKnowledge = new List<string>(dto.GrantedKnowledge),
            FindItemChance = dto.FindItemChance
            // RevealsExaminationPoint resolved in second pass
            // FoundItem, SpawnedSituation, SpawnedConversation: TODO resolve from gameWorld if needed
        };

        // Parse optional required stat
        if (!string.IsNullOrWhiteSpace(dto.RequiredStat))
        {
            if (Enum.TryParse<PlayerStatType>(dto.RequiredStat, true, out PlayerStatType statType))
            {
                point.RequiredStat = statType;
                point.RequiredStatLevel = dto.RequiredStatLevel;
            }
            else
            {
                throw new InvalidOperationException(
                    $"ExaminationPoint '{dto.Title}' has invalid RequiredStat '{dto.RequiredStat}'");
            }
        }

        return point;
    }
}
