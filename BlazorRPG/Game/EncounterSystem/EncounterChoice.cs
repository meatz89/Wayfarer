public class EncounterChoice
{
    public string ChoiceID { get; set; }
    public string NarrativeText { get; set; }
    public int FocusCost { get; set; }
    public SkillOption SkillOption { get; set; }
    public bool IsDisabled { get; set; }
    public bool IsAffordable { get; set; }
    public ChoiceTemplate ChoiceTemplate { get; set; }
    public string TemplateUsed { get; set; }
    public string TemplatePurpose { get; set; }
    public object SkillCheck { get; internal set; }
    public string SuccessNarrative { get; internal set; }
    public string FailureNarrative { get; internal set; }
}

