namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Factory for creating common location types
    /// </summary>
    public static class LocationFactory
    {
        public static LocationInfo CreateBanditAmbush()
        {
            // Core properties - note the hostility level is Hostile
            LocationInfo location = new LocationInfo(
                "Bandit Ambush",
                new[] { ApproachTypes.Force, ApproachTypes.Stealth }.ToList(),
                new[] { ApproachTypes.Charm, ApproachTypes.Wit }.ToList(),
                new[] { FocusTags.Physical, FocusTags.Environment }.ToList(),
                new[] { FocusTags.Relationship }.ToList(),
                4, // Duration
                9, 12, 15, // Higher momentum thresholds for hostile locations
                LocationInfo.HostilityLevels.Hostile,
                PresentationStyles.Physical);

            // Narrative tags (restrictive)
            location.AddTag(new NarrativeTag(
                "Surrounded",
                new AlwaysActiveCondition(),
                ApproachTypes.Stealth));

            location.AddTag(new NarrativeTag(
                "Threat of Violence",
                new AlwaysActiveCondition(),
                ApproachTypes.Charm));

            location.AddTag(EncounterTagFactory.CreateApproachThresholdTag(
                "Drawn Weapons",
                ApproachTags.Dominance,
                EncounterTagFactory.MajorTagThreshold,
                ApproachTypes.Wit));

            location.AddTag(EncounterTagFactory.CreateApproachPressureIncrease(
                "Hostile Territory",
                ApproachTags.Concealment,
                EncounterTagFactory.MinorTagThreshold,
                ApproachTypes.Charm,
                2));

            location.AddTag(new StrategicTag(
                "Growing Tension",
                new AlwaysActiveCondition(),
                StrategicEffectTypes.AddPressurePerTurn,
                1));

            location.AddTag(new NarrativeTag(
                "Fight Started",
                new FocusThresholdCondition(FocusTags.Physical, EncounterTagFactory.MajorTagThreshold),
                null)); // Special case - blocks all non-Force approaches

            // Strategic tags (restrictive)
            location.AddTag(new StrategicTag(
                "Numerical Advantage",
                new AlwaysActiveCondition(),
                StrategicEffectTypes.AddPressurePerTurn,
                1));

            location.AddTag(new StrategicTag(
                "Territorial Knowledge",
                new AlwaysActiveCondition(),
                StrategicEffectTypes.ReduceMomentumFromFocus,
                1,
                null,
                FocusTags.Environment));

            location.AddTag(new StrategicTag(
                "Battle Ready",
                new ApproachThresholdCondition(ApproachTags.Dominance, EncounterTagFactory.MajorTagThreshold),
                StrategicEffectTypes.ReduceMomentumFromApproach,
                1,
                ApproachTypes.Force));

            // Strategic tags (beneficial - weaknesses)
            location.AddTag(new StrategicTag(
                "Superstitious",
                new ApproachThresholdCondition(ApproachTags.Analysis, EncounterTagFactory.MinorTagThreshold),
                StrategicEffectTypes.ReducePressurePerTurn,
                1));

            location.AddTag(new StrategicTag(
                "Poorly Coordinated",
                new ApproachThresholdCondition(ApproachTags.Precision, EncounterTagFactory.MajorTagThreshold),
                StrategicEffectTypes.AddMomentumToApproach,
                1,
                ApproachTypes.Force));

            location.AddTag(new StrategicTag(
                "Easily Distracted",
                new ApproachThresholdCondition(ApproachTags.Concealment, EncounterTagFactory.MinorTagThreshold),
                StrategicEffectTypes.AddMomentumToApproach,
                1,
                ApproachTypes.Stealth));

            return location;
        }

        public static LocationInfo CreateRoadsideInn()
        {
            // Core properties
            LocationInfo location = new LocationInfo(
                "Roadside Inn",
                new[] { ApproachTypes.Charm, ApproachTypes.Wit }.ToList(),
                new[] { ApproachTypes.Force }.ToList(),
                new[] { FocusTags.Relationship, FocusTags.Information }.ToList(),
                new[] { FocusTags.Physical }.ToList(),
                4, // Duration
                7, 10, 12, // Standard thresholds for friendly locations
                LocationInfo.HostilityLevels.Friendly,
                PresentationStyles.Social);

            // Narrative tags
            location.AddTag(new NarrativeTag(
                "Common Room",
                new AlwaysActiveCondition(),
                ApproachTypes.Stealth));

            location.AddTag(new NarrativeTag(
                "Tavern Regulations",
                new AlwaysActiveCondition(),
                ApproachTypes.Force));

            // Strategic tags
            location.AddTag(new StrategicTag(
                "Warm Hearth",
                new ApproachThresholdCondition(ApproachTags.Rapport, EncounterTagFactory.MinorTagThreshold),
                StrategicEffectTypes.AddMomentumToFocus,
                1,
                null,
                FocusTags.Relationship));

            location.AddTag(new StrategicTag(
                "Traveler's Rest",
                new FocusThresholdCondition(FocusTags.Information, EncounterTagFactory.MajorTagThreshold),
                StrategicEffectTypes.AddMomentumToFocus,
                1,
                null,
                FocusTags.Information));

            location.AddTag(new StrategicTag(
                "Innkeeper's Favor",
                new ApproachThresholdCondition(ApproachTags.Rapport, EncounterTagFactory.MajorTagThreshold),
                StrategicEffectTypes.ReducePressurePerTurn,
                1));

            location.AddTag(new StrategicTag(
                "Regular Patron",
                new FocusThresholdCondition(FocusTags.Relationship, EncounterTagFactory.MajorTagThreshold),
                StrategicEffectTypes.AddMomentumToApproach,
                1,
                ApproachTypes.Charm));

            return location;
        }

        public static LocationInfo CreateVillageMarket()
        {
            // Core properties
            LocationInfo location = new LocationInfo(
                "Village Market",
                new[] { ApproachTypes.Charm, ApproachTypes.Finesse }.ToList(),
                new[] { ApproachTypes.Force, ApproachTypes.Stealth }.ToList(),
                new[] { FocusTags.Relationship, FocusTags.Resource }.ToList(),
                new[] { FocusTags.Physical }.ToList(),
                5, // Duration
                8, 10, 12, // Momentum thresholds
                LocationInfo.HostilityLevels.Neutral,
                PresentationStyles.Social);

            // Narrative tags
            location.AddTag(new NarrativeTag(
                "Open Marketplace",
                new AlwaysActiveCondition(),
                ApproachTypes.Stealth));

            location.AddTag(new NarrativeTag(
                "Market Suspicion",
                new ApproachThresholdCondition(ApproachTags.Concealment, EncounterTagFactory.MinorTagThreshold),
                ApproachTypes.Charm));

            location.AddTag(new NarrativeTag(
                "Guard Presence",
                new ApproachThresholdCondition(ApproachTags.Dominance, EncounterTagFactory.MajorTagThreshold),
                ApproachTypes.Stealth));

            // Strategic tags
            location.AddTag(new StrategicTag(
                "Merchant's Respect",
                new ApproachThresholdCondition(ApproachTags.Rapport, EncounterTagFactory.MinorTagThreshold),
                StrategicEffectTypes.AddMomentumToFocus,
                1,
                null,
                FocusTags.Resource));

            location.AddTag(new StrategicTag(
                "Haggler's Eye",
                new ApproachThresholdCondition(ApproachTags.Precision, EncounterTagFactory.MinorTagThreshold),
                StrategicEffectTypes.ReducePressureFromFocus,
                1,
                null,
                FocusTags.Resource));

            location.AddTag(new StrategicTag(
                "Market Wisdom",
                new ApproachThresholdCondition(ApproachTags.Analysis, EncounterTagFactory.MajorTagThreshold),
                StrategicEffectTypes.AddMomentumToFocus,
                1,
                null,
                FocusTags.Information));

            location.AddTag(new StrategicTag(
                "Trading Network",
                new ApproachThresholdCondition(ApproachTags.Rapport, EncounterTagFactory.MajorTagThreshold),
                StrategicEffectTypes.ReducePressurePerTurn,
                1));

            return location;
        }
    }
}