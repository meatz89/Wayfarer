public class EncounterStage
{
    public string Situation { get; set; }
    public string CurrentChoiceSetName { get; set; }
    public List<EncounterChoice> Choices { get; set; }

    public EncounterStage()
    {
        Choices = new List<EncounterChoice>();
    }
}