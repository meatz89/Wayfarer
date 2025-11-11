/// <summary>
/// ObservationScene - Mental challenge system for scene investigation.
/// Represents a location-based investigation with multiple examination points.
/// Players spend Focus to examine points, gaining knowledge and triggering events.
/// Part of the Mental tactical layer (distinct from old equipment-based Scene system).
/// </summary>
public class ObservationScene
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // Placement context (NOT ownership)
    public string LocationId { get; set; }
    public Location Location { get; set; }

    // Access requirements
    public List<string> RequiredKnowledge { get; set; } = new List<string>();

    // Repeatability
    public bool IsRepeatable { get; set; }

    // Completion state
    public bool IsCompleted { get; set; }
    public List<string> ExaminedPointIds { get; set; } = new List<string>();

    // Examination content
    public List<ExaminationPoint> ExaminationPoints { get; set; } = new List<ExaminationPoint>();
}
