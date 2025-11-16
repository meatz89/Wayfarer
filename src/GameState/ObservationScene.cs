/// <summary>
/// ObservationScene - Mental challenge system for scene investigation.
/// Represents a location-based investigation with multiple examination points.
/// Players spend Focus to examine points, gaining knowledge and triggering events.
/// Part of the Mental tactical layer (distinct from old equipment-based Scene system).
/// </summary>
public class ObservationScene
{
    // HIGHLANDER: NO Id property - ObservationScene identified by object reference
    public string Name { get; set; }
    public string Description { get; set; }

    // HIGHLANDER: Object reference ONLY, no LocationId
    // Placement context (NOT ownership)
    public Location Location { get; set; }

    // Access requirements
    public List<string> RequiredKnowledge { get; set; } = new List<string>();

    // Repeatability
    public bool IsRepeatable { get; set; }

    // Completion state
    public bool IsCompleted { get; set; }
    // HIGHLANDER: Object references ONLY, no ExaminedPointIds
    public List<ExaminationPoint> ExaminedPoints { get; set; } = new List<ExaminationPoint>();

    // Examination content
    public List<ExaminationPoint> ExaminationPoints { get; set; } = new List<ExaminationPoint>();
}
