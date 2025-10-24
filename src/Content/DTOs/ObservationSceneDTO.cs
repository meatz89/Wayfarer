using System.Collections.Generic;

/// <summary>
/// DTO for ObservationScene - scene investigation with multiple examination points.
/// Player has limited resources and must prioritize what to examine.
/// </summary>
public class ObservationSceneDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string LocationId { get; set; }

    /// <summary>
    /// Knowledge tokens required to access this observation scene
    /// </summary>
    public List<string> RequiredKnowledge { get; set; } = new List<string>();

    /// <summary>
    /// Whether this observation scene can be repeated after completion
    /// </summary>
    public bool IsRepeatable { get; set; }

    /// <summary>
    /// Whether this observation scene has been completed
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Examination points within this scene
    /// </summary>
    public List<ExaminationPointDTO> ExaminationPoints { get; set; } = new List<ExaminationPointDTO>();

    /// <summary>
    /// IDs of examination points that have been examined (state tracking)
    /// </summary>
    public List<string> ExaminedPointIds { get; set; } = new List<string>();
}

/// <summary>
/// A specific point of interest within an observation scene that can be examined.
/// Costs resources, may have requirements, grants outcomes.
/// </summary>
public class ExaminationPointDTO
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    /// <summary>
    /// Focus cost to examine this point
    /// </summary>
    public int FocusCost { get; set; }

    /// <summary>
    /// Time cost in segments to examine this point
    /// </summary>
    public int TimeCost { get; set; }

    /// <summary>
    /// Required player stat to examine this point
    /// Values: "Insight", "Cunning", "Authority", etc.
    /// </summary>
    public string RequiredStat { get; set; }

    /// <summary>
    /// Required stat level to examine this point
    /// </summary>
    public int? RequiredStatLevel { get; set; }

    /// <summary>
    /// Knowledge tokens required to examine this point
    /// </summary>
    public List<string> RequiredKnowledge { get; set; } = new List<string>();

    /// <summary>
    /// Knowledge tokens granted by examining this point
    /// </summary>
    public List<string> GrantedKnowledge { get; set; } = new List<string>();

    /// <summary>
    /// Situation ID spawned by examining this point
    /// </summary>
    public string SpawnedSituationId { get; set; }

    /// <summary>
    /// Conversation tree ID spawned by examining this point
    /// </summary>
    public string SpawnedConversationId { get; set; }

    /// <summary>
    /// Item ID that can be found at this examination point
    /// </summary>
    public string FoundItemId { get; set; }

    /// <summary>
    /// Chance (0-100) to find the item when examining this point
    /// </summary>
    public int FindItemChance { get; set; }

    /// <summary>
    /// ID of another examination point that gets revealed when this point is examined
    /// Progressive revelation mechanic
    /// </summary>
    public string RevealsExaminationPointId { get; set; }

    /// <summary>
    /// Whether this examination point is hidden until revealed by another point
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>
    /// Whether this examination point has been examined (state tracking)
    /// </summary>
    public bool IsExamined { get; set; }
}
