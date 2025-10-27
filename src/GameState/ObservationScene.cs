/// <summary>
/// Scene investigation with multiple examination points.
/// Player has limited resources and must prioritize what to examine.
/// </summary>
public class ObservationScene
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Location Location { get; set; }  // Resolved during parsing

    // Availability
    public List<string> RequiredKnowledge { get; set; } = new List<string>();
    public bool IsRepeatable { get; set; }
    public bool IsCompleted { get; set; }

    // Examination points
    public List<ExaminationPoint> ExaminationPoints { get; set; } = new List<ExaminationPoint>();

    // Tracking
    public List<string> ExaminedPointIds { get; set; } = new List<string>();
}

/// <summary>
/// A specific point of interest within an observation scene that can be examined.
/// Costs resources, may have requirements, grants outcomes.
/// </summary>
public class ExaminationPoint
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    // Costs
    public int FocusCost { get; set; }
    public int TimeCost { get; set; }

    // Requirements
    public PlayerStatType? RequiredStat { get; set; }
    public int? RequiredStatLevel { get; set; }
    public List<string> RequiredKnowledge { get; set; } = new List<string>();

    // Outcomes
    public List<string> GrantedKnowledge { get; set; } = new List<string>();
    public string SpawnedSituationId { get; set; }
    public string SpawnedConversationId { get; set; }
    public string FoundItemId { get; set; }
    public int FindItemChance { get; set; }  // 0-100 percentage

    // Discovery progression (examining one point reveals another)
    public string RevealsExaminationPointId { get; set; }

    // State
    public bool IsHidden { get; set; }
    public bool IsExamined { get; set; }
}
