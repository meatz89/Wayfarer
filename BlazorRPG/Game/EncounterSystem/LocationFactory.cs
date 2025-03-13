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
                10, // Duration
                9, 12, 15, // Higher momentum thresholds for hostile locations
                LocationInfo.HostilityLevels.Hostile,
                PresentationStyles.Physical);

            location.AddTag(new StrategicTag(
                "Growing Tension",
                StrategicEffectTypes.AddPressurePerTurn,
                null,  // Applies to all approaches
                null,  // Applies to all focuses
                null)); // No scaling

            location.AddTag(new NarrativeTag(
                "Drawn Weapons",
                new ApproachThresholdCondition(ApproachTags.Dominance, 4),
                ApproachTypes.Wit));

            location.AddTag(new NarrativeTag(
                "Hostile Territory",
                new ApproachThresholdCondition(ApproachTags.Concealment, 4),
                ApproachTypes.Charm));

            location.AddTag(new NarrativeTag(
                "Fight Started",
                new FocusThresholdCondition(FocusTags.Physical, 4),
                null)); // Special case - blocks all non-Force approaches

            // Strategic tags
            location.AddTag(new StrategicTag(
                "Combat Exhaustion",
                StrategicEffectTypes.ReduceHealthByPressure,
                ApproachTypes.Force,
                null,
                ApproachTags.Dominance)); // Scales with Dominance

            location.AddTag(new StrategicTag(
                "Poorly Coordinated",
                StrategicEffectTypes.AddMomentumToApproach,
                ApproachTypes.Force,
                null,
                ApproachTags.Precision)); // Scales with Precision

            location.AddTag(new StrategicTag(
                "Easily Distracted",
                StrategicEffectTypes.AddMomentumToApproach,
                ApproachTypes.Stealth,
                null,
                ApproachTags.Concealment)); // Scales with Concealment

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
                11, // Duration
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
                new ApproachThresholdCondition(ApproachTags.Concealment, 2),
                ApproachTypes.Charm));

            location.AddTag(new NarrativeTag(
                "Guard Presence",
                new ApproachThresholdCondition(ApproachTags.Dominance, 4),
                ApproachTypes.Stealth));

            return location;
        }
    }
}