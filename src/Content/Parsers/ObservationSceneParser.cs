/// <summary>
/// Parser for ObservationScene - Mental challenge system for scene investigation.
/// Translates JSON DTOs into domain entities with EntityResolver for categorical matching (find-only at parse-time).
/// </summary>
public static class ObservationSceneParser
{
    public static ObservationScene Parse(ObservationSceneDTO dto, EntityResolver entityResolver)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        if (entityResolver == null)
            throw new ArgumentNullException(nameof(entityResolver));
        if (dto.LocationFilter == null)
            throw new InvalidOperationException($"ObservationScene '{dto.Name}' must have a locationFilter");

        // EntityResolver.Find pattern - find-only at parse-time (Locations should already exist)
        PlacementFilter locationFilter = PlacementFilterParser.Parse(dto.LocationFilter, $"ObservationScene:{dto.Name}", null);
        Location location = entityResolver.FindLocation(locationFilter, null);
        if (location == null)
        {
            throw new InvalidOperationException(
                $"ObservationScene '{dto.Name}' references non-existent Location via LocationFilter. " +
                "Ensure Locations are loaded before ObservationScenes.");
        }

        // HIGHLANDER: ObservationScene is immutable template - no state properties
        // State (IsCompleted, ExaminedPoints) stored in ObservationSceneState at runtime
        ObservationScene scene = new ObservationScene
        {
            Name = dto.Name,
            Description = dto.Description,
            Location = location, // Object reference, no LocationId
            RequiredKnowledge = new List<string>(dto.RequiredKnowledge),
            IsRepeatable = dto.IsRepeatable
        };

        // Parse examination points (first pass - no cross-references yet)
        // DOMAIN COLLECTION PRINCIPLE: Track (Id, Point) pairs in List, not Dictionary
        List<ExaminationPointIdPair> pointPairs = new List<ExaminationPointIdPair>();
        foreach (ExaminationPointDTO pointDto in dto.ExaminationPoints)
        {
            ExaminationPoint point = ParseExaminationPoint(pointDto);
            scene.ExaminationPoints.Add(point);

            // Track Id+Point pair for second pass reference resolution
            if (!string.IsNullOrWhiteSpace(pointDto.Id))
            {
                pointPairs.Add(new ExaminationPointIdPair { Id = pointDto.Id, Point = point });
            }
        }

        // Second pass - resolve RevealsExaminationPoint references via LINQ lookup
        for (int i = 0; i < dto.ExaminationPoints.Count; i++)
        {
            ExaminationPointDTO pointDto = dto.ExaminationPoints[i];
            ExaminationPoint point = scene.ExaminationPoints[i];

            if (!string.IsNullOrWhiteSpace(pointDto.RevealsExaminationPointId))
            {
                ExaminationPoint revealedPoint = pointPairs
                    .FirstOrDefault(p => p.Id == pointDto.RevealsExaminationPointId)?.Point;
                if (revealedPoint != null)
                {
                    point.RevealsExaminationPoint = revealedPoint;
                }
            }
        }

        return scene;
    }

    private static ExaminationPoint ParseExaminationPoint(ExaminationPointDTO dto)
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

/// <summary>
/// Helper for ExaminationPoint cross-reference resolution during parsing.
/// DOMAIN COLLECTION PRINCIPLE: Used in List instead of Dictionary for ID-based lookup.
/// </summary>
internal sealed class ExaminationPointIdPair
{
    public string Id { get; init; }
    public ExaminationPoint Point { get; init; }
}
