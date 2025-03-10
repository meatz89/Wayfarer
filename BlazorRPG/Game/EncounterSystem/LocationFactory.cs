namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Factory for creating common location types
    /// </summary>
    public static class LocationFactory
    {
        // Create the Village Market location
        public static LocationInfo CreateVillageMarket()
        {
            // Favored/disfavored approaches and focuses
            List<ApproachTypes> favoredApproaches = new List<ApproachTypes> { ApproachTypes.Charm, ApproachTypes.Finesse };
            List<ApproachTypes> disfavoredApproaches = new List<ApproachTypes> { ApproachTypes.Force, ApproachTypes.Stealth };
            List<FocusTags> favoredFocuses = new List<FocusTags> { FocusTags.Relationship, FocusTags.Resource };
            List<FocusTags> disfavoredFocuses = new List<FocusTags> { FocusTags.Physical };

            // Encounter tags
            List<IEncounterTag> tags = new List<IEncounterTag>();

            // Narrative tags
            tags.Add(new NarrativeTag(
                "Open Marketplace",
                _ => true, // Always active
                ApproachTypes.Stealth // Blocks stealth approaches
            ));

            tags.Add(EncounterTagFactory.CreateApproachThresholdTag(
                "Market Suspicion",
                ApproachTags.Concealment,
                EncounterTagFactory.MinorTagThreshold,
                ApproachTypes.Charm // Blocks charm when concealment 2+
            ));

            tags.Add(EncounterTagFactory.CreateApproachThresholdTag(
                "Guard Presence",
                ApproachTags.Dominance,
                EncounterTagFactory.MajorTagThreshold,
                ApproachTypes.Stealth // Blocks stealth when dominance 4+
            ));

            // Strategic tags
            tags.Add(new StrategicTag(
                "Merchant's Respect",
                tagSystem => tagSystem.GetApproachTagValue(ApproachTags.Rapport) >= EncounterTagFactory.MinorTagThreshold,
                state => state.AddFocusMomentumBonus(FocusTags.Resource, 1)
            ));

            tags.Add(new StrategicTag(
                "Haggler's Eye",
                tagSystem => tagSystem.GetApproachTagValue(ApproachTags.Precision) >= EncounterTagFactory.MinorTagThreshold,
                state => state.AddEndOfTurnPressureReduction(1)
            ));

            tags.Add(new StrategicTag(
                "Market Wisdom",
                tagSystem => tagSystem.GetApproachTagValue(ApproachTags.Analysis) >= EncounterTagFactory.MajorTagThreshold,
                state => state.AddFocusMomentumBonus(FocusTags.Information, 1)
            ));

            tags.Add(new StrategicTag(
                "Trading Network",
                tagSystem => tagSystem.GetFocusTagValue(FocusTags.Relationship) >= EncounterTagFactory.MajorTagThreshold,
                state => state.AddEndOfTurnPressureReduction(1)
            ));

            return new LocationInfo(
                "Village Market",
                favoredApproaches,
                disfavoredApproaches,
                favoredFocuses,
                disfavoredFocuses,
                tags,
                8, // Partial threshold
                10, // Standard threshold
                12, // Exceptional threshold
                5, // Duration in turns
                LocationInfo.HostilityLevels.Friendly,
                PresentationStyles.Social
            );
        }

        // Create the Bandit Ambush location
        public static LocationInfo CreateBanditAmbush()
        {
            // Favored/disfavored approaches and focuses
            List<ApproachTypes> favoredApproaches = new List<ApproachTypes> { ApproachTypes.Force, ApproachTypes.Stealth };
            List<ApproachTypes> disfavoredApproaches = new List<ApproachTypes> { ApproachTypes.Charm, ApproachTypes.Wit };
            List<FocusTags> favoredFocuses = new List<FocusTags> { FocusTags.Physical, FocusTags.Environment };
            List<FocusTags> disfavoredFocuses = new List<FocusTags> { FocusTags.Relationship };

            // Encounter tags
            List<IEncounterTag> tags = new List<IEncounterTag>();

            // Narrative tags (restrictive)
            tags.Add(new NarrativeTag(
                "Surrounded",
                _ => true, // Always active
                ApproachTypes.Stealth // Blocks stealth approaches
            ));

            tags.Add(new NarrativeTag(
                "Threat of Violence",
                _ => true, // Always active
                ApproachTypes.Charm // Blocks charm approaches
            ));

            tags.Add(EncounterTagFactory.CreateApproachThresholdTag(
                "Drawn Weapons",
                ApproachTags.Dominance,
                EncounterTagFactory.MajorTagThreshold,
                ApproachTypes.Wit // Blocks wit when dominance 4+
            ));

            tags.Add(new NarrativeTag(
                "Fight Started",
                tagSystem => tagSystem.GetFocusTagValue(FocusTags.Physical) >= EncounterTagFactory.MajorTagThreshold,
                ApproachTypes.Finesse // Blocks finesse when physical 4+
            ));

            // Strategic tags (negative)
            tags.Add(new StrategicTag(
                "Numerical Advantage",
                _ => true, // Always active
                state => state.BuildPressure(1) // +1 pressure each turn
            ));

            // Strategic tags (weaknesses)
            tags.Add(new StrategicTag(
                "Superstitious",
                tagSystem => tagSystem.GetApproachTagValue(ApproachTags.Analysis) >= EncounterTagFactory.MinorTagThreshold,
                state => state.AddEndOfTurnPressureReduction(1)
            ));

            tags.Add(new StrategicTag(
                "Poorly Coordinated",
                tagSystem => tagSystem.GetApproachTagValue(ApproachTags.Precision) >= EncounterTagFactory.MajorTagThreshold,
                state => state.AddApproachMomentumBonus(ApproachTypes.Force, 1)
            ));

            tags.Add(new StrategicTag(
                "Easily Distracted",
                tagSystem => tagSystem.GetApproachTagValue(ApproachTags.Concealment) >= EncounterTagFactory.MinorTagThreshold,
                state => state.AddApproachMomentumBonus(ApproachTypes.Stealth, 1)
            ));

            return new LocationInfo(
                "Bandit Ambush",
                favoredApproaches,
                disfavoredApproaches,
                favoredFocuses,
                disfavoredFocuses,
                tags,
                9, // Partial threshold
                12, // Standard threshold
                15, // Exceptional threshold
                4, // Duration in turns
                LocationInfo.HostilityLevels.Hostile,
                PresentationStyles.Physical
            );
        }
    }
}