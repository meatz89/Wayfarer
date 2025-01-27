public static class LocationPropertyChoiceEffects
{
    public static List<LocationPropertyChoiceEffect> AllEffects { get; set; } = new()
    {
        /*
        // Atmosphere effects
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new AtmosphereValue { Atmosphere = Atmosphere.Social },
            ValueEffect = new EncounterValueModification(ValueTypes.Pressure, -2, "Relaxed Atmosphere") // Add source here
            {
            },
            Description = "The relaxed atmosphere helps keep pressure low"
        },

        new LocationPropertyChoiceEffect
        {
            LocationProperty = new AtmosphereValue { Atmosphere = Atmosphere.Hostile },
            ValueEffect = new EncounterValueModification(ValueTypes.Pressure, 2, "Tense Atmosphere"), // Add source here
            Description = "The tense atmosphere increases pressure"
        },

        // Activity level effects
        new LocationPropertyChoiceEffect
        {
            LocationProperty = new CrowdDensityValue { CrowdDensity = CrowdDensity.Quiet },
            ValueEffect = new EncounterValueModification(ValueTypes.Insight, 2, "Quiet Atmosphere"),
            Description = "The quiet space helps concentration"
        },
        */

        //// Example of a requirement modification
        //new LocationPropertyChoiceEffect
        //{
        //    LocationProperty = new SpaceValue { Space = SpaceTypes.Hazardous },
        //    ValueEffect = new RequirementModification
        //    {
        //        RequirementType = new SkillRequirement(SkillTypes.Strength, 2),
        //    },
        //    Description = "Navigating the hazardous space requires a Strength skill of 2"
        //},

        //// Example of a cost modification (using OutcomeModification)
        //new LocationPropertyChoiceEffect
        //{
        //    LocationProperty = new SpaceValue { Space = SpaceTypes.Hazardous },
        //    ValueEffect = new OutcomeModification
        //    {
        //        OutcomeType = "HealthOutcome", // Specify the type of outcome
        //        Amount = -1, // The health cost
        //        IsCost = true,
        //    },
        //    Description = "Navigating the hazardous space results in a loss of 1 Health"
        //},

        //// Example of a reward modification (using OutcomeModification)
        //new LocationPropertyChoiceEffect
        //{
        //    LocationProperty = new ActivityLevelValue { ActivityLevel = ActivityLevelTypes.Bustling },
        //    ValueEffect = new OutcomeModification
        //    {
        //        OutcomeType = "ReputationOutcome", // Specify the type of outcome
        //        Amount = 1, // The reputation gain
        //        IsCost = false,
        //    },
        //    Description = "Being in a bustling area increases your reputation for being Reliable by 1"
        //},

        //// Energy cost modifications
        //new LocationPropertyChoiceEffect
        //{
        //    LocationProperty = new SpaceValue { Space = SpaceTypes.Cramped },
        //    ValueEffect = new EnergyModification
        //    {
        //        TargetArchetype = ChoiceArchetypes.Physical,
        //        EnergyType = EnergyTypes.Physical,
        //        EnergyCostModifier = 1 // Cramped spaces increase energy cost for Physical choices
        //    },
        //    Description = "Cramped spaces make physical actions more tiring"
        //}
    };
}