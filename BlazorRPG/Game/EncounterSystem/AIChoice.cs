public class AiChoice
{
    public string ChoiceID { get; set; }
    public string NarrativeText { get; set; }
    public int FocusCost { get; set; }
    public SkillOption SkillOption { get; set; }
    public bool IsDisabled { get; internal set; }
}

