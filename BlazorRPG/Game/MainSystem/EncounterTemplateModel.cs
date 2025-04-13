public class EncounterTemplateModel
{
    public string Name { get; set; }
    public int Duration { get; set; }
    public int MaxPressure { get; set; }
    public int PartialThreshold { get; set; }
    public int StandardThreshold { get; set; }
    public int ExceptionalThreshold { get; set; }
    public string Hostility { get; set; }
    public List<StrategicTagModel> StrategicTags { get; set; }
    public List<string> NarrativeTags { get; set; }
}
