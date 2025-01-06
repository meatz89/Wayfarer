public static class ActionFactory
{
    public static List<ActionTemplate> CreateActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        // Example: Serve Drinks
        actionTemplates.Add(new ActionTemplate(
            name: "Serve Drinks",
            actionType: BasicActionTypes.Labor,
            baseRequirements: new List<Requirement>(), // Add base requirements if any
            baseCosts: new List<Outcome>
            {
                new EnergyOutcome(EnergyTypes.Physical, -1) // Example cost
            },
            baseRewards: new List<Outcome>
            {
                new CoinsOutcome(3) // Example reward
            },
            timeSlots: new List<TimeSlots> { TimeSlots.Morning, TimeSlots.Afternoon, TimeSlots.Evening, TimeSlots.Night },
            availabilityConditions: new List<LocationPropertyCondition>
            {
                new LocationPropertyCondition(LocationPropertyType.Archetype, LocationArchetype.Tavern),
                new LocationPropertyCondition(LocationPropertyType.CrowdLevel, CrowdLevel.Crowded)
            }
        ));

        // ... Add more action templates with their specific availability conditions

        return actionTemplates;
    }
}

public class ActionNameGenerator
{
    public string GenerateName(string baseName, LocationProperties properties)
    {
        // Modify the baseName based on location properties if needed
        // For example, if the location has a specific resource, you can add it to the name:

        if (properties.Resource.HasValue)
        {
            return $"{baseName} ({properties.Resource})";
        }

        return baseName;
    }
}