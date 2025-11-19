/// <summary>
/// ExaminationPoint - A point within an ObservationScene that can be examined.
/// Costs Focus and Time, grants Knowledge, may reveal other points or spawn content.
/// Part of the Mental challenge system (investigation/observation gameplay).
/// </summary>
public class ExaminationPoint
{
    // HIGHLANDER: NO Id property - ExaminationPoint identified by object reference
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

    // HIGHLANDER: Object reference ONLY, no RevealsExaminationPointId
    // Reveals other examination points
    public ExaminationPoint RevealsExaminationPoint { get; set; }

    // HIGHLANDER: Object reference ONLY, no FoundItemId
    // Item finding
    public Item FoundItem { get; set; }
    public int FindItemChance { get; set; }

    // HIGHLANDER: Object references ONLY, no SpawnedSituationId or SpawnedConversationId
    // Spawn content
    public Situation SpawnedSituation { get; set; }
    public ConversationTree SpawnedConversation { get; set; }
}
