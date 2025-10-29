/// <summary>
/// ExaminationPoint - A point within an ObservationScene that can be examined.
/// Costs Focus and Time, grants Knowledge, may reveal other points or spawn content.
/// Part of the Mental challenge system (investigation/observation gameplay).
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

    // Visibility
    public bool IsHidden { get; set; }
    public bool IsExamined { get; set; }

    // Rewards
    public List<string> GrantedKnowledge { get; set; } = new List<string>();

    // Reveals other examination points
    public string RevealsExaminationPointId { get; set; }

    // Item finding
    public string FoundItemId { get; set; }
    public int FindItemChance { get; set; }

    // Spawn content
    public string SpawnedSituationId { get; set; }
    public string SpawnedConversationId { get; set; }
}
