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
                .WithArchetype(LocationArchetypes.Tavern)
                .WithCrowdLevel(CrowdLevelTypes.Crowded))
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
                .WithArchetype(LocationArchetypes.Tavern)
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
                .WithArchetype(LocationArchetypes.Market))
            .Build());

        // Example: Hunt
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Hunt")
            .WithActionType(BasicActionTypes.Gather)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Forest))
            .Build());

        // Example: Play Music at Tavern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Play Music")
            .WithActionType(BasicActionTypes.Perform)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Tavern))
            .Build());

        // Example: Gather Herbs in the Forest
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Gather Herbs")
            .WithActionType(BasicActionTypes.Gather)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Forest))
            .Build());

        // Example: Gossip in the Tavern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Gossip")
            .WithActionType(BasicActionTypes.Mingle)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Tavern))
            .Build());

        // Example: Investigate in the Tavern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Investigate")
            .WithActionType(BasicActionTypes.Investigate)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Tavern))
            .Build());

        // Example: Rest in the Tavern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Rest")
            .WithActionType(BasicActionTypes.Rest)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Tavern))
            .Build());


        // === MARKETPLACE ACTIONS ===
        // Haggling involves social skills and careful timing
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Haggle with Merchants")
            .WithActionType(BasicActionTypes.Persuade)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Market)
                .WithCrowdLevel(CrowdLevelTypes.Busy))
            .Build());

        // Market investigation requires focus during quieter times
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Study Market Prices")
            .WithActionType(BasicActionTypes.Investigate)
            .AddTimeSlot(TimeSlots.Morning)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Market)
                .WithCrowdLevel(CrowdLevelTypes.Sparse))
            .Build());

        // Performance takes advantage of crowds
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Street Performance")
            .WithActionType(BasicActionTypes.Perform)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Market)
                .WithCrowdLevel(CrowdLevelTypes.Crowded))
            .Build());

        // === BOOKSHOP ACTIONS ===
        // Quiet study benefits from the relaxed atmosphere
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Study Ancient Texts")
            .WithActionType(BasicActionTypes.Study)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Library)
                .WithPressure(PressureStateTypes.Relaxed))
            .Build());

        // Book restoration combines labor with focus
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Restore Books")
            .WithActionType(BasicActionTypes.Labor)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Library)
                .WithComplexity(ComplexityTypes.Complex))
            .Build());

        // === DOCKYARD ACTIONS ===
        // Physical labor with risk/reward balance
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Load Cargo")
            .WithActionType(BasicActionTypes.Labor)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Docks)
                .WithScale(ScaleVariationTypes.Large))
            .Build());

        // Social networking in gray area
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Network with Smugglers")
            .WithActionType(BasicActionTypes.Mingle)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Docks)
                .WithLegality(LegalityTypes.Gray))
            .Build());

        // Fish gathering depends on time
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Sort Fresh Catch")
            .WithActionType(BasicActionTypes.Gather)
            .AddTimeSlot(TimeSlots.Morning)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Docks)
                .WithResource(ResourceTypes.Fish))
            .Build());

        // === WORKSHOP ACTIONS ===
        // Crafting combines focus and physical labor
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Craft Clothing")
            .WithActionType(BasicActionTypes.Labor)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Workshop)
                .WithResource(ResourceTypes.Cloth))
            .Build());

        // Apprenticeship learning
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Study Crafting")
            .WithActionType(BasicActionTypes.Study)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Workshop)
                .WithComplexity(ComplexityTypes.Complex))
            .Build());

        // === FOREST ACTIONS ===
        // Herb gathering requires focus and timing
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Forage for Herbs")
            .WithActionType(BasicActionTypes.Gather)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Forest)
                .WithResource(ResourceTypes.Herbs))
            .Build());

        // Meditation benefits from isolation
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Forest Meditation")
            .WithActionType(BasicActionTypes.Reflect)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Evening)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Forest)
                .WithCrowdLevel(CrowdLevelTypes.Empty))
            .Build());

        // === GARDEN ACTIONS ===
        // Garden work combines physical labor with focus
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Tend Garden")
            .WithActionType(BasicActionTypes.Labor)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Garden)
                .WithResource(ResourceTypes.Food))
            .Build());

        // Peaceful reflection in nature
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Garden Contemplation")
            .WithActionType(BasicActionTypes.Reflect)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Evening)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Garden)
                .WithPressure(PressureStateTypes.Relaxed))
            .Build());

        return actionTemplates;
    }
}