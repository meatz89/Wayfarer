public class EncounterChoice
{
    public string ChoiceID { get; set; }
    public string NarrativeText { get; set; }
    public int FocusCost { get; set; }
    public SkillOption SkillOption { get; set; }
    public bool IsDisabled { get; set; }
    public bool IsAffordable { get; set; }
}

