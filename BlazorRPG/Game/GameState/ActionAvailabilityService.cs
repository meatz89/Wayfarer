public class ActionAvailabilityService
{
    // Define which properties are mandatory for action availability
    private static readonly HashSet<LocationPropertyTypes> MandatoryProperties = new()
    {
        LocationPropertyTypes.Archetype,  // The fundamental nature of the location
        LocationPropertyTypes.Exposure,   // Whether it's indoor/outdoor
        LocationPropertyTypes.Legality    // Whether the action is legally allowed
    };

    public bool IsActionAvailable(ActionTemplate template, LocationProperties locationProperties)
    {
        // First check all mandatory properties - these must match exactly if specified
        var mandatoryConditions = template.AvailabilityConditions
            .Where(c => MandatoryProperties.Contains(c.PropertyType));

        foreach (var condition in mandatoryConditions)
        {
            // For mandatory properties, we require both that they're set and match
            if (!IsPropertySet(locationProperties, condition.PropertyType))
            {
                return false;
            }

            object propertyValue = locationProperties.GetProperty(condition.PropertyType);
            if (!propertyValue.Equals(condition.ExpectedValue))
            {
                return false;
            }
        }

        // Contextual properties (like crowd level, pressure, complexity) are stored
        // but don't affect availability - they'll modify the encounter instead
        var contextualConditions = template.AvailabilityConditions
            .Where(c => !MandatoryProperties.Contains(c.PropertyType));

        foreach (var condition in contextualConditions)
        {
            if (IsPropertySet(locationProperties, condition.PropertyType))
            {
                object propertyValue = locationProperties.GetProperty(condition.PropertyType);
                if (!propertyValue.Equals(condition.ExpectedValue))
                {
                    // Here we could store this mismatch for encounter generation
                    // For example, a crowded tavern might make serving drinks harder
                    // But it doesn't prevent the action
                    continue;
                }
            }
        }

        return true;
    }

    private bool IsPropertySet(LocationProperties properties, LocationPropertyTypes propertyType)
    {
        return propertyType switch
        {
            LocationPropertyTypes.Archetype => properties.IsArchetypeSet,
            LocationPropertyTypes.Scale => properties.IsScaleSet,
            LocationPropertyTypes.Exposure => properties.IsExposureSet,
            LocationPropertyTypes.Legality => properties.IsLegalitySet,
            LocationPropertyTypes.Pressure => properties.IsPressureSet,
            LocationPropertyTypes.Complexity => properties.IsComplexitySet,
            LocationPropertyTypes.Resource => properties.IsResourceSet,
            LocationPropertyTypes.CrowdLevel => properties.IsCrowdLevelSet,
            LocationPropertyTypes.ReputationType => properties.IsReputationTypeSet,
            _ => throw new ArgumentException($"Unknown property type: {propertyType}")
        };
    }
}