public class StrategicTag : IConversationTag
{
    public string NarrativeName { get; }
    public ILocationProperty EnvironmentalProperty { get; }

    public StrategicTag(
        string name,
        ILocationProperty environmentalProperty)
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
