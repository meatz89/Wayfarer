/// <summary>
/// ObservationScene - Immutable template for mental challenge investigation.
/// Represents a location-based investigation with multiple examination points.
/// Players spend Focus to examine points, gaining knowledge and triggering events.
/// Part of the Mental tactical layer (distinct from old equipment-based Scene system).
/// HIGHLANDER: Template IDs are allowed (immutable archetypes).
/// </summary>
public class ObservationScene
{
    // ADR-007: Id property - Templates (immutable archetypes) ARE allowed to have IDs
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // HIGHLANDER: Object reference ONLY, no LocationId
    // Placement context (NOT ownership)
    public Location Location { get; set; }

    // Access requirements
    public List<string> RequiredKnowledge { get; set; } = new List<string>();

    // Repeatability
    public bool IsRepeatable { get; set; }

    // Examination content (template data)
    public List<ExaminationPoint> ExaminationPoints { get; set; } = new List<ExaminationPoint>();
}

/// <summary>
/// Mutable runtime state for an active observation scene investigation.
/// HIGHLANDER: NO Id property - identified by object reference only.
/// References immutable ObservationScene template.
/// </summary>
public class ObservationSceneState
{
    // HIGHLANDER: Object reference to template, not template ID
    public ObservationScene Template { get; set; }

    // Mutable state
    public bool IsCompleted { get; set; }
    // HIGHLANDER: Object references ONLY, no ExaminedPointIds
    public List<ExaminationPoint> ExaminedPoints { get; set; } = new List<ExaminationPoint>();
}
