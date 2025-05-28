public class BeatOutcome
{
    public int ProgressGained { get; set; }
    public string NarrativeDescription { get; set; }
    public string MechanicalDescription { get; set; }
    public bool IsEncounterComplete { get; set; }
    public BeatOutcomes Outcome { get; set; }
    public bool SkillCheckSuccess { get; set; }
    public SkillCheckResult CheckResult { get; set; }
    public string PayloadApplied { get; set; }
    public List<FlagStates> NewFlags { get; set; }
}