public static class ActionContent
{
    public static List<ActionTemplate> LoadActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        // Example: Serve Drinks
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Serve Drinks")
            .WithActionType(BasicActionTypes.Labor)
            .AddCost(new EnergyOutcome(EnergyTypes.Physical, -1))
            .AddReward(new CoinsOutcome(3))
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(new LocationPropertyCondition(LocationPropertyType.Archetype, LocationArchetype.Tavern))
            .AddAvailabilityCondition(new LocationPropertyCondition(LocationPropertyType.CrowdLevel, CrowdLevel.Crowded))
            .Build());

        // Example: Clean Tables
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Clean Tables")
            .WithActionType(BasicActionTypes.Labor)
            .AddCost(new EnergyOutcome(EnergyTypes.Physical, -1))
            .AddReward(new CoinsOutcome(1))
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(new LocationPropertyCondition(LocationPropertyType.Archetype, LocationArchetype.Tavern))
            .AddAvailabilityCondition(new LocationPropertyCondition(LocationPropertyType.CrowdLevel, CrowdLevel.Empty))
            .Build());

        // Example: Browse Market
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Browse Market")
            .WithActionType(BasicActionTypes.Gather)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddAvailabilityCondition(new LocationPropertyCondition(LocationPropertyType.Archetype, LocationArchetype.Market))
            .Build());

        // Example: Hunt
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Hunt")
            .WithActionType(BasicActionTypes.Gather)
            .AddCost(new EnergyOutcome(EnergyTypes.Physical, -2))
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddAvailabilityCondition(new LocationPropertyCondition(LocationPropertyType.Archetype, LocationArchetype.Forest))
            .Build());

        // Example: Barter at Tavern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Barter")
            .WithActionType(BasicActionTypes.Trade)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(new LocationPropertyCondition(LocationPropertyType.Archetype, LocationArchetype.Tavern))
            .Build());

        // Example: Play Music at Tavern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Play Music")
            .WithActionType(BasicActionTypes.Perform)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(new LocationPropertyCondition(LocationPropertyType.Archetype, LocationArchetype.Tavern))
            .Build());

        // Example: Gather Herbs in the Forest
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Gather Herbs")
            .WithActionType(BasicActionTypes.Gather)
            .AddReward(new ResourceOutcome(ResourceTypes.Herbs, 2))
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(new LocationPropertyCondition(LocationPropertyType.Archetype, LocationArchetype.Forest))
            .Build());

        // Example: Gossip in the Tavern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Gossip")
            .WithActionType(BasicActionTypes.Mingle)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(new LocationPropertyCondition(LocationPropertyType.Archetype, LocationArchetype.Tavern))
            .Build());

        // Example: Investigate in the Tavern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Investigate")
            .WithActionType(BasicActionTypes.Investigate)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(new LocationPropertyCondition(LocationPropertyType.Archetype, LocationArchetype.Tavern))
            .Build());

        // Example: Rest in the Tavern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Rest")
            .WithActionType(BasicActionTypes.Rest)
            .AddReward(new HealthOutcome(5)) // Assuming Rest restores health
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(new LocationPropertyCondition(LocationPropertyType.Archetype, LocationArchetype.Tavern))
            .Build());

        // ... Add more action templates for different actions and location types

        return actionTemplates;
    }
}