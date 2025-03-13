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
                new[] { ApproachTags.Force, ApproachTags.Stealth }.ToList(),
                new[] { ApproachTags.Charm, ApproachTags.Wit }.ToList(),
                new[] { FocusTags.Physical, FocusTags.Environment }.ToList(),
                new[] { FocusTags.Relationship }.ToList(),
                10, // Duration
                9, 12, 15, // Higher momentum thresholds for hostile locations
                LocationInfo.HostilityLevels.Hostile,
                PresentationStyles.Physical);

            location.AddTag(NarrativeTagRespository.DrawnWeapons);
            location.AddTag(NarrativeTagRespository.HostileTerritory);
            location.AddTag(NarrativeTagRespository.FightStarted);

            location.AddTag(StrategicTagRespository.GrowingTension);
            location.AddTag(StrategicTagRespository.CombatExhaustion);
            location.AddTag(StrategicTagRespository.PoorlyCoordinated);
            location.AddTag(StrategicTagRespository.EasilyDistracted);

            return location;
        }

        public static LocationInfo CreateVillageMarket()
        {
            // Core properties
            LocationInfo location = new LocationInfo(
                "Village Market",
                new[] { ApproachTags.Charm, ApproachTags.Finesse }.ToList(),
                new[] { ApproachTags.Force, ApproachTags.Stealth }.ToList(),
                new[] { FocusTags.Relationship, FocusTags.Resource }.ToList(),
                new[] { FocusTags.Physical }.ToList(),
                11, // Duration
                8, 10, 12, // Momentum thresholds
                LocationInfo.HostilityLevels.Neutral,
                PresentationStyles.Social);

            location.AddTag(NarrativeTagRespository.OpenMarketplace);
            location.AddTag(NarrativeTagRespository.MarketSuspicion);

            return location;
        }
    }
}