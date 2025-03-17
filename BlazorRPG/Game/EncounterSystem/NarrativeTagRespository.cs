public class NarrativeTagRespository
{
    public static NarrativeTag DrawnWeapons = new NarrativeTag(
            "Drawn Weapons",
            new FocusThresholdCondition(ApproachTags.Dominance, 1),
            FocusTags.Information);

    public static NarrativeTag HostileTerritory = new NarrativeTag(
            "Hostile Territory",
            new FocusThresholdCondition(ApproachTags.Concealment, 1),
            FocusTags.Charm);

    public static NarrativeTag FightStarted = new NarrativeTag(
            "Fight Started",
            new FocusThresholdCondition(FocusTags.Physical, 2),
            null);

    public static NarrativeTag OpenMarketplace = new NarrativeTag(
            "Open Marketplace",
            new AlwaysActiveCondition(),
            FocusTags.Stealth);

    public static NarrativeTag MarketSuspicion = new NarrativeTag(
            "Market Suspicion",
            new FocusThresholdCondition(ApproachTags.Concealment, 2),
            FocusTags.Charm);

}
