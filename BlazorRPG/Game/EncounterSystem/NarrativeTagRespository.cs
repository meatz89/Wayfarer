namespace BlazorRPG.Game.EncounterManager
{
    public class NarrativeTagRespository
    {
        public static NarrativeTag DrawnWeapons = new NarrativeTag(
                "Drawn Weapons",
                new ApproachThresholdCondition(EncounterStateTags.Dominance, 1),
                ApproachTypes.Wit);

        public static NarrativeTag HostileTerritory = new NarrativeTag(
                "Hostile Territory",
                new ApproachThresholdCondition(EncounterStateTags.Concealment, 1),
                ApproachTypes.Charm);

        public static NarrativeTag FightStarted = new NarrativeTag(
                "Fight Started",
                new FocusThresholdCondition(FocusTags.Physical, 2),
                null);

        public static NarrativeTag OpenMarketplace = new NarrativeTag(
                "Open Marketplace",
                new AlwaysActiveCondition(),
                ApproachTypes.Stealth);

        public static NarrativeTag MarketSuspicion = new NarrativeTag(
                "Market Suspicion",
                new ApproachThresholdCondition(EncounterStateTags.Concealment, 2),
                ApproachTypes.Charm);

    }
}