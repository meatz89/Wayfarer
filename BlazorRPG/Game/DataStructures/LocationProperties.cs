public class LocationProperties
{
    public ScaleVariationTypes? Scale { get; set; }
    public bool IsScaleSet { get; private set; } = false;
    public ExposureConditionTypes? Exposure { get; set; }
    public bool IsExposureSet { get; private set; } = false;
    public LegalityTypes? Legality { get; set; }
    public bool IsLegalitySet { get; private set; } = false;
    public PressureStateTypes? Pressure { get; set; }
    public bool IsPressureSet { get; private set; } = false;
    public ComplexityTypes? Complexity { get; set; }
    public bool IsComplexitySet { get; private set; } = false;
    public LocationArchetypes? Archetype { get; set; }
    public bool IsArchetypeSet { get; private set; } = false;
    public ResourceTypes? Resource { get; set; }
    public bool IsResourceSet { get; private set; } = false;
    public CrowdLevelTypes? CrowdLevel { get; set; }
    public bool IsCrowdLevelSet { get; private set; } = false;
    public ReputationTypes ReputationType { get; set; }
    public bool IsReputationTypeSet { get; private set; } = false;

    public object GetProperty(LocationPropertyTypes propertyType)
    {
        switch (propertyType)
        {
            case LocationPropertyTypes.Scale:
                return Scale;
            case LocationPropertyTypes.Exposure:
                return Exposure;
            case LocationPropertyTypes.Legality:
                return Legality;
            case LocationPropertyTypes.Pressure:
                return Pressure;
            case LocationPropertyTypes.Complexity:
                return Complexity;
            case LocationPropertyTypes.Archetype:
                return Archetype;
            case LocationPropertyTypes.Resource:
                return Resource;
            case LocationPropertyTypes.CrowdLevel:
                return CrowdLevel;
            case LocationPropertyTypes.ReputationType:
                return ReputationType;
            default:
                throw new ArgumentException($"Unknown property type: {propertyType}");
        }
    }

    // SetProperty now also sets the corresponding IsSet flag
    public void SetProperty(LocationPropertyTypes propertyType, object value)
    {
        switch (propertyType)
        {
            case LocationPropertyTypes.Scale:
                Scale = (ScaleVariationTypes)value;
                IsScaleSet = true;
                break;
            case LocationPropertyTypes.Exposure:
                Exposure = (ExposureConditionTypes)value;
                IsExposureSet = true;
                break;
            case LocationPropertyTypes.Legality:
                Legality = (LegalityTypes)value;
                IsLegalitySet = true;
                break;
            case LocationPropertyTypes.Pressure:
                Pressure = (PressureStateTypes)value;
                IsPressureSet = true;
                break;
            case LocationPropertyTypes.Complexity:
                Complexity = (ComplexityTypes)value;
                IsComplexitySet = true;
                break;
            case LocationPropertyTypes.Archetype:
                Archetype = (LocationArchetypes)value;
                IsArchetypeSet = true;
                break;
            case LocationPropertyTypes.Resource:
                Resource = (ResourceTypes?)value;
                IsResourceSet = true;
                break;
            case LocationPropertyTypes.CrowdLevel:
                CrowdLevel = (CrowdLevelTypes)value;
                IsCrowdLevelSet = true;
                break;
            case LocationPropertyTypes.ReputationType:
                ReputationType = (ReputationTypes)value;
                IsReputationTypeSet = true;
                break;
            default:
                throw new ArgumentException($"Unknown property type: {propertyType}");
        }
    }
}