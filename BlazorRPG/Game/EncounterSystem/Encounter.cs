public class Encounter
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string LocationName { get; set; }
    public string LocationSpotName { get; set; }
    public ActionTypes SkillCategory { get; set; }
    public List<EncounterStage> Stages { get; set; }
    public int TotalProgress { get; set; }
    public int EncounterDifficulty { get; set; }
    public int SuccessThreshold { get; set; }
    public string CommissionId { get; set; }
    public ApproachDefinition Approach { get; set; }
}
