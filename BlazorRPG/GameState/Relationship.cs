public class Relationship
{
    public string Character { get; set; }
    public CharacterTypes Type { get; set; }
    public int Level { get; set; }
    public int Value { get; set; }
    public string Status { get; set; }
    public List<string> SharedHistory { get; set; } = new List<string>();
}
