public class StrategicTag : IEncounterTag
{
    public string NarrativeName { get; }
    public IEnvironmentalProperty EnvironmentalProperty { get; }

    public StrategicTag(
        string name,
        IEnvironmentalProperty environmentalProperty,
        EnvironmentalPropertyEffect environmentalPropertyEffect)
    {
        NarrativeName = name;
        EnvironmentalProperty = environmentalProperty;
    }

    public string GetEffectDescription()
    {
        string baseDesc = EnvironmentalProperty.ToString();
        return baseDesc;
    }

    public string GetActivationDescription()
    {
        return "Always active"; // Strategic tags are always active
    }
}
