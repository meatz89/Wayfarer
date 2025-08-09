public class ConversationBeatOutcome
{
    // Outcome removed - all consequences flow from choices, not endings
    public int ProgressGained { get; set; }
    public string NarrativeDescription { get; set; }
    public string MechanicalDescription { get; set; }
    public bool IsConversationComplete { get; set; }
    public bool SkillCheckSuccess { get; set; }
    public SkillCheckResult CheckResult { get; set; }
    public string EffectApplied { get; set; }
}