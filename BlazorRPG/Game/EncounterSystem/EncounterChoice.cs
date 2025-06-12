public class EncounterChoice
{
    public string ChoiceID { get; set; }
    public string NarrativeText { get; set; }
    public int FocusCost { get; set; }
    public bool IsAffordable { get; set; }
    public string TemplateUsed { get; set; }
    public string TemplatePurpose { get; set; }
    public string SuccessNarrative { get; internal set; }
    public string FailureNarrative { get; internal set; }
    public SkillOption SkillOption { get; set; }

}

