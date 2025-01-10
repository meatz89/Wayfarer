public static class ActionContent
{
    public static List<ActionTemplate> LoadActionTemplates()
    {
        List<ActionTemplate> actionTemplates = new List<ActionTemplate>();

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Serve Drinks")
            .WithActionType(BasicActionTypes.Labor)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Tavern)    // Mandatory
                .WithExposure(ExposureConditionTypes.Indoor) // Mandatory
                .WithLegality(LegalityTypes.Legal))          // Mandatory
            .Build());

        // Perform Music - Similar core requirements
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Perform Music")
            .WithActionType(BasicActionTypes.Perform)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Tavern)    // Mandatory
                .WithExposure(ExposureConditionTypes.Indoor) // Mandatory
                .WithLegality(LegalityTypes.Legal))          // Mandatory
            .Build());

        // Gossip - Same pattern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Gossip")
            .WithActionType(BasicActionTypes.Mingle)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Tavern)    // Mandatory
                .WithExposure(ExposureConditionTypes.Indoor) // Mandatory
                .WithLegality(LegalityTypes.Legal))          // Mandatory
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

        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Network with Smugglers")
            .WithActionType(BasicActionTypes.Mingle)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithLegality(LegalityTypes.Gray)        // Must be a legally ambiguous area
                .WithCrowdLevel(CrowdLevelTypes.Sparse)  // Not too many witnesses
                .WithPressure(PressureStateTypes.Alert)) // Some tension in the air
            .Build());

        // "Forest Meditation" - Now properly requires a natural setting
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Forest Meditation")
            .WithActionType(BasicActionTypes.Reflect)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Evening)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Forest)        // Must be in forest
                .WithExposure(ExposureConditionTypes.Outdoor)   // Must be outside
                .WithCrowdLevel(CrowdLevelTypes.Empty)          // Needs solitude
                .WithPressure(PressureStateTypes.Relaxed))      // Peaceful atmosphere
            .Build());

        // "Study Ancient Texts" should work in any quiet, complex location
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Study Ancient Texts")
            .WithActionType(BasicActionTypes.Study)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(properties => properties
                .WithComplexity(ComplexityTypes.Complex)       // Intellectual environment
                .WithCrowdLevel(CrowdLevelTypes.Sparse)       // Need quiet
                .WithPressure(PressureStateTypes.Relaxed)     // Peaceful atmosphere
                .WithExposure(ExposureConditionTypes.Indoor)) // Indoor activity
            .Build());

        // "Street Performance" - Now properly requires a market setting
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Street Performance")
            .WithActionType(BasicActionTypes.Perform)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Market)        // Must be in market
                .WithCrowdLevel(CrowdLevelTypes.Crowded)        // Need an audience
                .WithScale(ScaleVariationTypes.Large)           // Need space
                .WithLegality(LegalityTypes.Legal))             // Must be allowed
            .Build());

        // "Sort Fresh Catch" - Now properly requires a dock setting
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Sort Fresh Catch")
            .WithActionType(BasicActionTypes.Gather)
            .AddTimeSlot(TimeSlots.Morning)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Docks)         // Must be at docks
                .WithResource(ResourceTypes.Fish)                // Must have fish
                .WithExposure(ExposureConditionTypes.Outdoor)    // Fresh air needed
                .WithScale(ScaleVariationTypes.Large))           // Need work space
            .Build());

        return actionTemplates;
    }
}