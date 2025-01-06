public class LocationProperties
{
    // Existing properties
    public ScaleVariations Scale { get; set; }
    public ExposureConditions Exposure { get; set; }
    public LegalityTypes Legality { get; set; }
    public TensionState Tension { get; set; }
    public ComplexityTypes Complexity { get; set; }

    // New properties
    public LocationArchetype Archetype { get; set; }
    public ResourceTypes? Resource { get; set; }
    public CrowdLevel CrowdLevel { get; set; }

    public object GetProperty(LocationPropertyType propertyType)
    {
        switch (propertyType)
        {
            case LocationPropertyType.Scale:
                return Scale;
            case LocationPropertyType.Exposure:
                return Exposure;
            case LocationPropertyType.Legality:
                return Legality;
            case LocationPropertyType.Tension:
                return Tension;
            case LocationPropertyType.Complexity:
                return Complexity;
            case LocationPropertyType.Archetype:
                return Archetype;
            case LocationPropertyType.Resource:
                return Resource;
            case LocationPropertyType.CrowdLevel:
                return CrowdLevel;
            default:
                throw new ArgumentException($"Unknown property type: {propertyType}");
        }
    }
}