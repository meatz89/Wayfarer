public class EncounterHistory
{
    public string InitialGoal { get; set; }
    public string InitialSituation { get; set; }
    public List<Narrative> Narratives = new();
    public string ResultSituation { get; set; }
    public Choice LastChoice { get; internal set; }
    public EffectTypes LastChoiceEffectType { get; internal set; }
    public ApproachTypes LastChoiceApproach { get; internal set; }
    public FocusTypes LastChoiceFocusType { get; internal set; }
}
