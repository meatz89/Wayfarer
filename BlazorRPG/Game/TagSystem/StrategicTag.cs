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

    // Strategic tags are ALWAYS active
    public bool IsActive(EncounterTagSystem tagSystem)
    {
        return true;
    }

    public void ApplyEffect(EncounterState state)
    {
        // Strategic tags don't directly apply effects to the state
        // Their effects are calculated during projection and then applied through the projection
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
