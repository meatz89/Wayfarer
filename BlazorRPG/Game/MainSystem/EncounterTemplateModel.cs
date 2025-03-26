public class EncounterTemplateModel
{
    public int Duration { get; set; }
    public int MaxPressure { get; set; }
    public int PartialThreshold { get; set; }
    public int StandardThreshold { get; set; }
    public int ExceptionalThreshold { get; set; }
    public string Hostility { get; set; }
    public List<string> MomentumBoostApproaches { get; set; }
    public List<string> DangerousApproaches { get; set; }
    public List<string> PressureReducingFocuses { get; set; }
    public List<string> MomentumReducingFocuses { get; set; }
    public List<StrategicTagModel> StrategicTags { get; set; }
    public List<string> NarrativeTags { get; set; }
}
