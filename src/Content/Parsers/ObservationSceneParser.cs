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
        if (string.IsNullOrWhiteSpace(dto.Id))
            throw new InvalidOperationException("ObservationScene must have an Id");
        if (string.IsNullOrWhiteSpace(dto.LocationId))
            throw new InvalidOperationException($"ObservationScene '{dto.Id}' must have a LocationId");

        // Resolve location reference
        Location location = gameWorld.Locations.FirstOrDefault(l => l.Id == dto.LocationId);
        if (location == null)
            throw new InvalidOperationException($"ObservationScene '{dto.Id}' references unknown Location '{dto.LocationId}'");

        ObservationScene scene = new ObservationScene
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            LocationId = dto.LocationId,
            Location = location,
            RequiredKnowledge = new List<string>(dto.RequiredKnowledge),
            IsRepeatable = dto.IsRepeatable,
            IsCompleted = false,
            ExaminedPointIds = new List<string>()
        };

        // Parse examination points
        foreach (ExaminationPointDTO pointDto in dto.ExaminationPoints)
        {
            ExaminationPoint point = ParseExaminationPoint(pointDto, dto.Id);
            scene.ExaminationPoints.Add(point);
        }

        return scene;
    }

    private static ExaminationPoint ParseExaminationPoint(ExaminationPointDTO dto, string sceneId)
    {
        if (string.IsNullOrWhiteSpace(dto.Id))
            throw new InvalidOperationException($"ExaminationPoint in scene '{sceneId}' must have an Id");

        ExaminationPoint point = new ExaminationPoint
        {
            Id = dto.Id,
            Title = dto.Title,
            Description = dto.Description,
            FocusCost = dto.FocusCost,
            TimeCost = dto.TimeCost,
            RequiredKnowledge = new List<string>(dto.RequiredKnowledge),
            IsHidden = dto.IsHidden,
            IsExamined = false,
            GrantedKnowledge = new List<string>(dto.GrantedKnowledge),
            RevealsExaminationPointId = dto.RevealsExaminationPointId,
            FoundItemId = dto.FoundItemId,
            FindItemChance = dto.FindItemChance,
            SpawnedSituationId = dto.SpawnedSituationId,
            SpawnedConversationId = dto.SpawnedConversationId
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
                    $"ExaminationPoint '{dto.Id}' in scene '{sceneId}' has invalid RequiredStat '{dto.RequiredStat}'");
            }
        }

        return point;
    }
}
