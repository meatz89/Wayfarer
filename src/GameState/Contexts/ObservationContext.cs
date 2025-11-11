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
    public Dictionary<PlayerStatType, int> PlayerStats { get; set; }
    public List<string> PlayerKnowledge { get; set; }

    // Examination tracking
    public List<string> ExaminedPointIds { get; set; }

    // Display info
    public string TimeDisplay { get; set; }

    public ObservationContext()
    {
        IsValid = true;
        ErrorMessage = string.Empty;
        PlayerStats = new Dictionary<PlayerStatType, int>();
        PlayerKnowledge = new List<string>();
        ExaminedPointIds = new List<string>();
    }

    // Helper methods for UI
    public List<ExaminationPoint> GetAvailablePoints()
    {
        if (Scene == null) return new List<ExaminationPoint>();

        return Scene.ExaminationPoints
            .Where(p => !p.IsHidden || IsRevealed(p))
            .Where(p => !ExaminedPointIds.Contains(p.Id))
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
            .Where(p => ExaminedPointIds.Contains(p.Id))
            .ToList();
    }

    private bool IsRevealed(ExaminationPoint point)
    {
        if (!point.IsHidden) return true;

        // Check if any examined point reveals this one
        return Scene.ExaminationPoints
            .Any(p => ExaminedPointIds.Contains(p.Id) &&
                     p.RevealsExaminationPointId == point.Id);
    }

    private bool CanAfford(ExaminationPoint point)
    {
        return CurrentFocus >= point.FocusCost;
    }

    private bool MeetsStatRequirements(ExaminationPoint point)
    {
        if (!point.RequiredStat.HasValue) return true;
        if (!point.RequiredStatLevel.HasValue) return true;

        if (!PlayerStats.ContainsKey(point.RequiredStat.Value))
            return false;

        return PlayerStats[point.RequiredStat.Value] >= point.RequiredStatLevel.Value;
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
        return ExaminedPointIds.Count;
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
