public class ValueModifier
{
    public string Source { get; }  // e.g. "Location Danger", "Player Skill"
    public float Multiplier { get; }
    public int FlatBonus { get; }

    public float Apply(float value)
    {
        return (value * Multiplier) + FlatBonus;
    }
}
