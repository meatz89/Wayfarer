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
                .WithActivityLevel(ActivityLevelTypes.Bustling)     // Mandatory
                .WithAccessibility(AccessibilityTypes.Public)       // Mandatory
                .WithSupervision(SupervisionTypes.Unsupervised))    // Mandatory
            .Build());

        // Perform Music - Similar core requirements
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Perform Music")
            .WithActionType(BasicActionTypes.Perform)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithActivityLevel(ActivityLevelTypes.Bustling)     // Mandatory
                .WithAccessibility(AccessibilityTypes.Public)       // Mandatory
                .WithSupervision(SupervisionTypes.Unsupervised))    // Mandatory
            .Build());

        // Gossip - Same pattern
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Gossip")
            .WithActionType(BasicActionTypes.Mingle)
            .AddTimeSlot(TimeSlots.Evening)
            .AddTimeSlot(TimeSlots.Night)
            .AddAvailabilityCondition(properties => properties
                .WithActivityLevel(ActivityLevelTypes.Bustling)     // Mandatory
                .WithAccessibility(AccessibilityTypes.Public)       // Mandatory
                .WithSupervision(SupervisionTypes.Unsupervised))    // Mandatory
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
                .WithActivityLevel(ActivityLevelTypes.Deserted)     // Mandatory
                .WithAccessibility(AccessibilityTypes.Public)       // Mandatory
                .WithSupervision(SupervisionTypes.Unsupervised))    // Mandatory
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
                .WithActivityLevel(ActivityLevelTypes.Bustling))
            .Build());

        // Market investigation requires focus during quieter times
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Study Market Prices")
            .WithActionType(BasicActionTypes.Investigate)
            .AddTimeSlot(TimeSlots.Morning)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Market)
                .WithActivityLevel(ActivityLevelTypes.Bustling))
            .Build());

        // Performance takes advantage of crowds
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Street Performance")
            .WithActionType(BasicActionTypes.Perform)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Market)
                .WithActivityLevel(ActivityLevelTypes.Bustling))
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
                .WithSupervision(SupervisionTypes.Unsupervised))
            .Build());

        // Book restoration combines labor with focus
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Restore Books")
            .WithActionType(BasicActionTypes.Labor)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Library)
                .WithAtmosphere(AtmosphereTypes.Tense))
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
                .WithAccessibility(AccessibilityTypes.Public))
            .Build());

        // "Forest Meditation" - Now properly requires a natural setting
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Forest Meditation")
            .WithActionType(BasicActionTypes.Reflect)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Evening)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Forest)        // Must be in forest
                .WithExposure(ExposureTypes.Outdoor)   // Must be outside
                .WithActivityLevel(ActivityLevelTypes.Deserted)          // Needs solitude
                .WithSupervision(SupervisionTypes.Unsupervised))      // Peaceful atmosphere
            .Build());

        // "Study Ancient Texts" should work in any quiet, complex location
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Study Ancient Texts")
            .WithActionType(BasicActionTypes.Study)
            .AddTimeSlot(TimeSlots.Morning)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddAvailabilityCondition(properties => properties
                .WithAtmosphere(AtmosphereTypes.Tense)       // Intellectual environment
                .WithActivityLevel(ActivityLevelTypes.Quiet)       // Need quiet
                .WithSupervision(SupervisionTypes.Unsupervised)     // Peaceful atmosphere
                .WithExposure(ExposureTypes.Indoor)) // Indoor activity
            .Build());

        // "Street Performance" - Now properly requires a market setting
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Street Performance")
            .WithActionType(BasicActionTypes.Perform)
            .AddTimeSlot(TimeSlots.Afternoon)
            .AddTimeSlot(TimeSlots.Evening)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Market)        // Must be in market
                .WithActivityLevel(ActivityLevelTypes.Bustling)        // Need an audience
                .WithAccessibility(AccessibilityTypes.Public)           // Need space
                .WithSupervision(SupervisionTypes.Unsupervised))             // Must be allowed
            .Build());

        // "Sort Fresh Catch" - Now properly requires a dock setting
        actionTemplates.Add(new ActionTemplateBuilder()
            .WithName("Sort Fresh Catch")
            .WithActionType(BasicActionTypes.Gather)
            .AddTimeSlot(TimeSlots.Morning)
            .AddAvailabilityCondition(properties => properties
                .WithArchetype(LocationArchetypes.Docks)         // Must be at docks
                .WithResource(ResourceTypes.Fish)                // Must have fish
                .WithExposure(ExposureTypes.Outdoor)    // Fresh air needed
                .WithAccessibility(AccessibilityTypes.Public))           // Need work space
            .Build());

        return actionTemplates;
    }
}