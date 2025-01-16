public static class LocationPropertyChoiceEffects
{
    public static List<LocationPropertyChoiceEffect> AllEffects { get; set; } = new()
    {
        //    // Atmosphere effects
        //    new LocationPropertyChoiceEffect
        //    {
        //        LocationProperty = new AtmosphereValue { Atmosphere = AtmosphereTypes.Relaxed },
        //        ValueEffect = new ValueModification(ValueTypes.Pressure, -2, "Relaxed Atmosphere") // Add source here
        //        {
        //        },
        //        RuleDescription = "The relaxed atmosphere helps keep pressure low"
        //    },

        //    new LocationPropertyChoiceEffect
        //    {
        //        LocationProperty = new AtmosphereValue { Atmosphere = AtmosphereTypes.Tense },
        //        ValueEffect = new ValueModification(ValueTypes.Pressure, 2, "Tense Atmosphere") // Add source here
        //        {
        //        },
        //        RuleDescription = "The tense atmosphere increases pressure"
        //    },

        //    // Activity level effects
        //    new LocationPropertyChoiceEffect
        //    {
        //        LocationProperty = new ActivityLevelValue { ActivityLevel = ActivityLevelTypes.Quiet },
        //        ValueEffect = new ValueBonus
        //        {
        //            ChoiceArchetype = ChoiceArchetypes.Focus,
        //            ValueType = ValueTypes.Insight,
        //            BonusAmount = 2  // Quiet spaces enhance focus gains
        //        },
        //        RuleDescription = "The quiet space helps concentration"
        //    },

        //    // Example of a requirement modification
        //    new LocationPropertyChoiceEffect
        //    {
        //        LocationProperty = new SpaceValue { Space = SpaceTypes.Hazardous },
        //        ValueEffect = new RequirementModification
        //        {
        //            RequirementType = "SkillRequirement", // Specify the type of requirement
        //            Amount = 2, // The required skill level
        //            AdditionalInfo = "Strength" // Store the skill type as additional info
        //        },
        //        RuleDescription = "Navigating the hazardous space requires a Strength skill of 2"
        //    },

        //    // Example of a cost modification (using OutcomeModification)
        //    new LocationPropertyChoiceEffect
        //    {
        //        LocationProperty = new SpaceValue { Space = SpaceTypes.Hazardous },
        //        ValueEffect = new OutcomeModification
        //        {
        //            OutcomeType = "HealthOutcome", // Specify the type of outcome
        //            Amount = -1, // The health cost
        //            IsCost = true,
        //            AdditionalInfo = "" // No additional info needed for health outcomes (can be left empty)
        //        },
        //        RuleDescription = "Navigating the hazardous space results in a loss of 1 Health"
        //    },

        //    // Example of a reward modification (using OutcomeModification)
        //    new LocationPropertyChoiceEffect
        //    {
        //        LocationProperty = new ActivityLevelValue { ActivityLevel = ActivityLevelTypes.Bustling },
        //        ValueEffect = new OutcomeModification
        //        {
        //            OutcomeType = "ReputationOutcome", // Specify the type of outcome
        //            Amount = 1, // The reputation gain
        //            IsCost = false,
        //            AdditionalInfo = "Reliable" // Store the reputation type as additional info
        //        },
        //        RuleDescription = "Being in a bustling area increases your reputation for being Reliable by 1"
        //    },

        //    // Energy cost modifications
        //    new LocationPropertyChoiceEffect
        //    {
        //        LocationProperty = new SpaceValue { Space = SpaceTypes.Cramped },
        //        ValueEffect = new EnergyModification
        //        {
        //            TargetArchetype = ChoiceArchetypes.Physical,
        //            EnergyCostModifier = 1 // Cramped spaces increase energy cost for Physical choices
        //        },
        //        RuleDescription = "Cramped spaces make physical actions more tiring"
        //    },
        //    // SupervisionType
        //    new LocationPropertyChoiceEffect
        //    {
        //        LocationProperty = new SupervisionValue { Supervision = SupervisionTypes.Watched },
        //        ValueEffect = new OutcomeModification
        //        {
        //            OutcomeType = "EnergyOutcome",
        //            Amount = -1,
        //            IsCost = true,
        //            AdditionalInfo = "Social"
        //        },
        //        RuleDescription = "Being watched makes social interactions more tiring"
        //    },
    };
}