/// <summary>
/// Context for ObservationScene screens containing scene state and metadata.
/// Provides view model data for scene investigation with multiple examination points.
/// </summary>
public class ObservationContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }

    // Scene data
    public ObservationScene Scene { get; set; }
    public Location Location { get; set; }

    // Player state
    public int CurrentFocus { get; set; }
    public int MaxFocus { get; set; }
    // DOMAIN COLLECTION PRINCIPLE: Explicit properties for fixed enum (PlayerStatType)
    public int InsightLevel { get; set; }
    public int RapportLevel { get; set; }
    public int AuthorityLevel { get; set; }
    public int DiplomacyLevel { get; set; }
    public int CunningLevel { get; set; }

    public int GetStatLevel(PlayerStatType stat) => stat switch
    {
        PlayerStatType.Insight => InsightLevel,
        PlayerStatType.Rapport => RapportLevel,
        PlayerStatType.Authority => AuthorityLevel,
        PlayerStatType.Diplomacy => DiplomacyLevel,
        PlayerStatType.Cunning => CunningLevel,
        _ => 0
    };
    public List<string> PlayerKnowledge { get; set; }

    // Examination tracking (HIGHLANDER: Object collection, not string IDs)
    public List<ExaminationPoint> ExaminedPoints { get; set; }

    // Display info
    public string TimeDisplay { get; set; }

    public ObservationContext()
    {
        IsValid = true;
        ErrorMessage = string.Empty;
        PlayerKnowledge = new List<string>();
        ExaminedPoints = new List<ExaminationPoint>();
    }

    // Helper methods for UI
    public List<ExaminationPoint> GetAvailablePoints()
    {
        if (Scene == null) return new List<ExaminationPoint>();

        return Scene.ExaminationPoints
            .Where(p => !p.IsHidden || IsRevealed(p))
            .Where(p => !ExaminedPoints.Contains(p)) // Object collection, not string IDs
            .Where(p => MeetsKnowledgeRequirements(p))
            .ToList();
    }

    public List<ExaminationPoint> GetAffordablePoints()
    {
        return GetAvailablePoints()
            .Where(p => CanAfford(p) && MeetsStatRequirements(p))
            .ToList();
    }

    public List<ExaminationPoint> GetBlockedPoints()
    {
        return GetAvailablePoints()
            .Where(p => !CanAfford(p) || !MeetsStatRequirements(p))
            .ToList();
    }

    public List<ExaminationPoint> GetExaminedPoints()
    {
        if (Scene == null) return new List<ExaminationPoint>();

        return Scene.ExaminationPoints
            .Where(p => ExaminedPoints.Contains(p)) // Object collection, not string IDs
            .ToList();
    }

    private bool IsRevealed(ExaminationPoint point)
    {
        if (!point.IsHidden) return true;

        // Check if any examined point reveals this one (using object references)
        return Scene.ExaminationPoints
            .Any(p => ExaminedPoints.Contains(p) &&
                     p.RevealsExaminationPoint == point); // Object reference, not RevealsExaminationPointId string
    }

    private bool CanAfford(ExaminationPoint point)
    {
        return CurrentFocus >= point.FocusCost;
    }

    private bool MeetsStatRequirements(ExaminationPoint point)
    {
        if (!point.RequiredStat.HasValue) return true;
        if (!point.RequiredStatLevel.HasValue) return true;

        // DOMAIN COLLECTION PRINCIPLE: Use explicit properties
        int currentLevel = GetStatLevel(point.RequiredStat.Value);
        return currentLevel >= point.RequiredStatLevel.Value;
    }

    private bool MeetsKnowledgeRequirements(ExaminationPoint point)
    {
        return point.RequiredKnowledge.All(k => PlayerKnowledge.Contains(k));
    }

    public string GetBlockReason(ExaminationPoint point)
    {
        if (!CanAfford(point))
            return $"Requires {point.FocusCost} Focus (you have {CurrentFocus})";

        if (!MeetsStatRequirements(point))
            return $"Requires {point.RequiredStat} level {point.RequiredStatLevel}";

        if (!MeetsKnowledgeRequirements(point))
        {
            List<string> missing = point.RequiredKnowledge
                .Where(k => !PlayerKnowledge.Contains(k))
                .ToList();
            return $"Missing knowledge: {string.Join(", ", missing)}";
        }

        return "";
    }

    public int GetTotalExaminations()
    {
        return ExaminedPoints.Count;
    }

    public int GetAvailableExaminations()
    {
        return GetAvailablePoints().Count;
    }

    public int GetRemainingFocus()
    {
        return CurrentFocus;
    }
}
