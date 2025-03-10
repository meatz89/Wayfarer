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
                new[] { ApproachTypes.Force, ApproachTypes.Concealment }.ToList(),
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
                tagSystem => true, // Always active
                ApproachTypes.Concealment));

            location.AddTag(new NarrativeTag(
                "Threat of Violence",
                tagSystem => true, // Always active
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
                tagSystem => true, // Always active
                state => state.AddEndOfTurnPressureReduction(-1), // Negative reduction = increase
                StrategicEffectTypes.AddPressurePerTurn,
                1));

            location.AddTag(new NarrativeTag(
                "Fight Started",
                tagSystem => tagSystem.GetFocusTagValue(FocusTags.Physical) >= EncounterTagFactory.MajorTagThreshold,
                null)); // Special case - blocks all non-Force approaches

            // Strategic tags (restrictive)
            location.AddTag(new StrategicTag(
                "Numerical Advantage",
                tagSystem => true, // Always active
                state => state.AddEndOfTurnPressureReduction(-1), // Adds pressure each turn
                StrategicEffectTypes.AddPressurePerTurn,
                1));

            location.AddTag(new StrategicTag(
                "Territorial Knowledge",
                tagSystem => true, // Always active
                state => { /* Implementation would reduce momentum */ },
                StrategicEffectTypes.ReduceMomentumFromFocus,
                1,
                null,
                FocusTags.Environment));

            location.AddTag(new StrategicTag(
                "Battle Ready",
                tagSystem => tagSystem.GetApproachTagValue(ApproachTags.Dominance) >= EncounterTagFactory.MajorTagThreshold,
                state => { /* Implementation would reduce momentum */ },
                StrategicEffectTypes.ReduceMomentumFromApproach,
                1,
                ApproachTypes.Force));

            // Strategic tags (beneficial - weaknesses)
            location.AddTag(new StrategicTag(
                "Superstitious",
                tagSystem => tagSystem.GetApproachTagValue(ApproachTags.Analysis) >= EncounterTagFactory.MinorTagThreshold,
                state => state.AddEndOfTurnPressureReduction(1),
                StrategicEffectTypes.ReducePressurePerTurn,
                1));

            location.AddTag(new StrategicTag(
                "Poorly Coordinated",
                tagSystem => tagSystem.GetApproachTagValue(ApproachTags.Precision) >= EncounterTagFactory.MajorTagThreshold,
                state => state.AddApproachMomentumBonus(ApproachTypes.Force, 1),
                StrategicEffectTypes.AddMomentumToApproach,
                1,
                ApproachTypes.Force));

            location.AddTag(new StrategicTag(
                "Easily Distracted",
                tagSystem => tagSystem.GetApproachTagValue(ApproachTags.Concealment) >= EncounterTagFactory.MinorTagThreshold,
                state => state.AddApproachMomentumBonus(ApproachTypes.Concealment, 1),
                StrategicEffectTypes.AddMomentumToApproach,
                1,
                ApproachTypes.Concealment));

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
                tagSystem => true, // Always active
                ApproachTypes.Concealment));

            location.AddTag(new NarrativeTag(
                "Tavern Regulations",
                tagSystem => true, // Always active
                ApproachTypes.Force));

            // Strategic tags
            location.AddTag(new StrategicTag(
                "Warm Hearth",
                tagSystem => tagSystem.GetApproachTagValue(ApproachTags.Rapport) >= EncounterTagFactory.MinorTagThreshold,
                state => state.AddFocusMomentumBonus(FocusTags.Relationship, 1),
                StrategicEffectTypes.AddMomentumToFocus,
                1,
                null,
                FocusTags.Relationship));

            location.AddTag(new StrategicTag(
                "Traveler's Rest",
                tagSystem => tagSystem.GetFocusTagValue(FocusTags.Information) >= EncounterTagFactory.MajorTagThreshold,
                state => state.AddFocusMomentumBonus(FocusTags.Information, 1),
                StrategicEffectTypes.AddMomentumToFocus,
                1,
                null,
                FocusTags.Information));

            location.AddTag(new StrategicTag(
                "Innkeeper's Favor",
                tagSystem => tagSystem.GetApproachTagValue(ApproachTags.Rapport) >= EncounterTagFactory.MajorTagThreshold,
                state => state.AddEndOfTurnPressureReduction(1),
                StrategicEffectTypes.ReducePressurePerTurn,
                1));

            location.AddTag(new StrategicTag(
                "Regular Patron",
                tagSystem => tagSystem.GetFocusTagValue(FocusTags.Relationship) >= EncounterTagFactory.MajorTagThreshold,
                state => state.AddApproachMomentumBonus(ApproachTypes.Charm, 1),
                StrategicEffectTypes.AddMomentumToApproach,
                1,
                ApproachTypes.Charm));

            return location;
        }
    }
}