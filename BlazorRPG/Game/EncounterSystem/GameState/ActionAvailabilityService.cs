public class ActionAvailabilityService
{
    // Define which properties are mandatory for action availability
    private static readonly HashSet<LocationPropertyTypes> MandatoryProperties = new()
    {
        LocationPropertyTypes.Archetype,  // The fundamental nature of the location
        LocationPropertyTypes.CrowdDensity,   
        LocationPropertyTypes.LocationScale,
    };

    public bool IsActionAvailable(ActionTemplate template, LocationProperties locationProperties)
    {
        // First check all mandatory properties - these must match exactly if specified
        IEnumerable<LocationPropertyCondition> mandatoryConditions = template.AvailabilityConditions
            .Where(c => MandatoryProperties.Contains(c.PropertyType));

        foreach (LocationPropertyCondition? condition in mandatoryConditions)
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
        IEnumerable<LocationPropertyCondition> contextualConditions = template.AvailabilityConditions
            .Where(c => !MandatoryProperties.Contains(c.PropertyType));

        foreach (LocationPropertyCondition? condition in contextualConditions)
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
            LocationPropertyTypes.Resource => properties.IsResourceSet,
            LocationPropertyTypes.CrowdDensity => properties.IsCrowdDensitySet,
            LocationPropertyTypes.LocationScale => properties.IsLocationScaleSet,

            LocationPropertyTypes.Accessibility => properties.IsAccessabilitySet,
            LocationPropertyTypes.Engagement => properties.IsEngagementSet,
            LocationPropertyTypes.Atmosphere => properties.IsAtmosphereSet,
            LocationPropertyTypes.RoomLayout => properties.IsRoomLayoutSet,
            LocationPropertyTypes.Temperature => properties.IsTemperatureSet,
            _ => throw new ArgumentException($"Unknown property type: {propertyType}")
        };
    }
}