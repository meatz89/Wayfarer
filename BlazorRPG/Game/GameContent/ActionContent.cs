public static class ActionContent
{
    public static List<ActionTemplate> LoadActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        // Example: Serve Drinks
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Serve Drinks")
            .WithActionType(BasicActionTypes.Labor)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetype.Tavern)
                .WithCrowdLevel(CrowdLevelTypes.Busy))
            .Build());

        // Example: Clean Tables
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Clean Tables")
            .WithActionType(BasicActionTypes.Labor)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetype.Tavern)
                .WithCrowdLevel(CrowdLevelTypes.Empty))
            .Build());

        // Example: Browse Market
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Browse Market")
            .WithActionType(BasicActionTypes.Gather)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetype.Market))
            .Build());

        // Example: Hunt
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Hunt")
            .WithActionType(BasicActionTypes.Gather)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetype.Forest))
            .Build());

        // Example: Barter at Tavern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Barter")
            .WithActionType(BasicActionTypes.Trade)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetype.Tavern))
            .Build());

        // Example: Play Music at Tavern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Play Music")
            .WithActionType(BasicActionTypes.Perform)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetype.Tavern))
            .Build());

        // Example: Gather Herbs in the Forest
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Gather Herbs")
            .WithActionType(BasicActionTypes.Gather)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetype.Forest))
            .Build());

        // Example: Gossip in the Tavern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Gossip")
            .WithActionType(BasicActionTypes.Mingle)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetype.Tavern))
            .Build());

        // Example: Investigate in the Tavern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Investigate")
            .WithActionType(BasicActionTypes.Investigate)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetype.Tavern))
            .Build());

        // Example: Rest in the Tavern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Rest")
            .WithActionType(BasicActionTypes.Rest)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetype.Tavern))
            .Build());

        // ... Add more action templates for different actions and location types

        return actionTemplates;
    }
}