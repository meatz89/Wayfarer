public class TravelEncounterContext
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int DangerLevel { get; private set; }
    public List<ChoiceTemplate> EncounterTemplates { get; private set; } = new List<ChoiceTemplate>();
}