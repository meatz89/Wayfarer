public class CharacterGenerationRequest
{
    public string Archetype { get; set; } = "commoner";
    public string Region { get; set; } = "a small rural village";
    public string Gender { get; set; } = "";  // Empty string for any
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 60;
    public string AdditionalTraits { get; set; } = "";
}
