namespace Wayfarer.Content.DTOs
{
    /// <summary>
    /// DTO for deserializing ObservationScene from JSON.
    /// ObservationScenes are investigation gameplay - players spend Focus to examine points.
    /// Part of Mental challenge system (NOT the old equipment-based Scene system).
    /// </summary>
    public class ObservationSceneDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LocationId { get; set; }
        public List<string> RequiredKnowledge { get; set; } = new List<string>();
        public bool IsRepeatable { get; set; }
        public List<ExaminationPointDTO> ExaminationPoints { get; set; } = new List<ExaminationPointDTO>();
    }

    /// <summary>
    /// DTO for deserializing ExaminationPoint from JSON.
    /// Points within an ObservationScene that can be examined for knowledge and rewards.
    /// </summary>
    public class ExaminationPointDTO
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int FocusCost { get; set; }
        public int TimeCost { get; set; }
        public string RequiredStat { get; set; }
        public int? RequiredStatLevel { get; set; }
        public List<string> RequiredKnowledge { get; set; } = new List<string>();
        public bool IsHidden { get; set; }
        public List<string> GrantedKnowledge { get; set; } = new List<string>();
        public string RevealsExaminationPointId { get; set; }
        public string FoundItemId { get; set; }
        public int FindItemChance { get; set; }
        public string SpawnedSituationId { get; set; }
        public string SpawnedConversationId { get; set; }
    }
}
