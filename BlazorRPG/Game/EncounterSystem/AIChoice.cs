public class AiChoice
{
    public string ChoiceID { get; set; }
    public string NarrativeText { get; set; }
    public int FocusCost { get; set; }
    public List<SkillOption> SkillOptions { get; set; } = new List<SkillOption>();
    public bool IsDisabled { get; internal set; }
}

