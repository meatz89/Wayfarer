public class PlayerSummary
{
    public string Name { get; set; }
    public string Background { get; set; }
    public int Level { get; set; }
    public List<string> TopSkills { get; set; }
    public List<ChoiceCard> TopCards { get; set; }
    public Dictionary<string, string> KeyRelationships { get; set; }
}
