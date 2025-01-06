public class LocationProperties
{
    public ScaleVariations? Scale { get; set; }
    public bool IsScaleSet { get; private set; } = false;
    public ExposureConditions? Exposure { get; set; }
    public bool IsExposureSet { get; private set; } = false;
    public LegalityTypes? Legality { get; set; }
    public bool IsLegalitySet { get; private set; } = false;
    public TensionState? Tension { get; set; }
    public bool IsTensionSet { get; private set; } = false;
    public ComplexityTypes? Complexity { get; set; }
    public bool IsComplexitySet { get; private set; } = false;
    public LocationArchetype? Archetype { get; set; }
    public bool IsArchetypeSet { get; private set; } = false;
    public ResourceTypes? Resource { get; set; }
    public bool IsResourceSet { get; private set; } = false;
    public CrowdLevel? CrowdLevel { get; set; }
    public bool IsCrowdLevelSet { get; private set; } = false;
    public ReputationTypes ReputationType { get; set; }
    public bool IsReputationTypeSet { get; private set; } = false;

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
            case LocationPropertyType.ReputationType:
                return ReputationType;
            default:
                throw new ArgumentException($"Unknown property type: {propertyType}");
        }
    }

    // SetProperty now also sets the corresponding IsSet flag
    public void SetProperty(LocationPropertyType propertyType, object value)
    {
        switch (propertyType)
        {
            case LocationPropertyType.Scale:
                Scale = (ScaleVariations)value;
                IsScaleSet = true;
                break;
            case LocationPropertyType.Exposure:
                Exposure = (ExposureConditions)value;
                IsExposureSet = true;
                break;
            case LocationPropertyType.Legality:
                Legality = (LegalityTypes)value;
                IsLegalitySet = true;
                break;
            case LocationPropertyType.Tension:
                Tension = (TensionState)value;
                IsTensionSet = true;
                break;
            case LocationPropertyType.Complexity:
                Complexity = (ComplexityTypes)value;
                IsComplexitySet = true;
                break;
            case LocationPropertyType.Archetype:
                Archetype = (LocationArchetype)value;
                IsArchetypeSet = true;
                break;
            case LocationPropertyType.Resource:
                Resource = (ResourceTypes?)value;
                IsResourceSet = true;
                break;
            case LocationPropertyType.CrowdLevel:
                CrowdLevel = (CrowdLevel)value;
                IsCrowdLevelSet = true;
                break;
            case LocationPropertyType.ReputationType:
                ReputationType = (ReputationTypes)value;
                IsReputationTypeSet = true;
                break;
            default:
                throw new ArgumentException($"Unknown property type: {propertyType}");
        }
    }
}