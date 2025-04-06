public class EncounterDefinition
{
    public string Goal { get; set; } = "";
    public string Complication { get; set; } = "";
    public int Momentum { get; set; }
    public int Pressure { get; set; }
    public List<string> StrategicTags { get; set; } = new List<string>();
}
