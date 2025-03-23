public class Relationship
{
    public string CharacterId { get; set; }
    public int Value { get; set; }  // Numeric relationship value
    public string Status { get; set; }  // Predefined relationship status
    public List<string> SharedHistory { get; set; } = new List<string>();
}
