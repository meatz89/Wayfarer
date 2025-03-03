
/// <summary>
/// Content definitions for locations
/// </summary>
public class LocationContent
{
    /// <summary>
    /// Create Harbor Warehouse location properties
    /// </summary>
    public static LocationStrategicProperties CreateHarborWarehouse()
    {
        return new LocationStrategicBuilder()
            .WithId("harbor_warehouse")
            .WithName("Harbor Warehouse")
            .WithFavoredElement(SignatureElementTypes.Precision)
            .WithFavoredElement(SignatureElementTypes.Concealment)
            .WithDisfavoredElement(SignatureElementTypes.Dominance)
            .WithDisfavoredElement(SignatureElementTypes.Rapport)
            .WithAvailableTag("precision_footwork")
            .WithAvailableTag("flawless_execution")
            .WithAvailableTag("efficient_movement")
            .WithAvailableTag("shadow_movement")
            .WithAvailableTag("perfect_stealth")
            .WithAvailableTag("hidden_observation")
            .Build();
    }

    /// <summary>
    /// Create Merchant Guild location properties
    /// </summary>
    public static LocationStrategicProperties CreateMerchantGuild()
    {
        return new LocationStrategicBuilder()
            .WithId("merchant_guild")
            .WithName("Merchant Guild")
            .WithFavoredElement(SignatureElementTypes.Rapport)
            .WithFavoredElement(SignatureElementTypes.Analysis)
            .WithDisfavoredElement(SignatureElementTypes.Dominance)
            .WithDisfavoredElement(SignatureElementTypes.Concealment)
            .WithAvailableTag("social_currency")
            .WithAvailableTag("network_leverage")
            .WithAvailableTag("emotional_insight")
            .WithAvailableTag("market_insight")
            .WithAvailableTag("negotiation_mastery")
            .WithAvailableTag("resource_optimization")
            .Build();
    }

    /// <summary>
    /// Create Bandit Camp location properties
    /// </summary>
    public static LocationStrategicProperties CreateBanditCamp()
    {
        return new LocationStrategicBuilder()
            .WithId("bandit_camp")
            .WithName("Bandit Camp")
            .WithFavoredElement(SignatureElementTypes.Dominance)
            .WithFavoredElement(SignatureElementTypes.Concealment)
            .WithDisfavoredElement(SignatureElementTypes.Rapport)
            .WithDisfavoredElement(SignatureElementTypes.Precision)
            .WithAvailableTag("intimidation_tactics")
            .WithAvailableTag("overwhelming_presence")
            .WithAvailableTag("forceful_breakthrough")
            .WithAvailableTag("unseen_threat")
            .WithAvailableTag("perfect_ambush")
            .WithAvailableTag("shadow_movement")
            .Build();
    }

    /// <summary>
    /// Create Royal Court location properties
    /// </summary>
    public static LocationStrategicProperties CreateRoyalCourt()
    {
        return new LocationStrategicBuilder()
            .WithId("royal_court")
            .WithName("Royal Court")
            .WithFavoredElement(SignatureElementTypes.Rapport)
            .WithFavoredElement(SignatureElementTypes.Precision)
            .WithDisfavoredElement(SignatureElementTypes.Dominance)
            .WithDisfavoredElement(SignatureElementTypes.Concealment)
            .WithAvailableTag("social_currency")
            .WithAvailableTag("emotional_insight")
            .WithAvailableTag("network_leverage")
            .WithAvailableTag("meticulous_approach")
            .WithAvailableTag("smooth_talker")
            .WithAvailableTag("careful_allocation")
            .Build();
    }

    /// <summary>
    /// Create Wilderness location properties
    /// </summary>
    public static LocationStrategicProperties CreateWilderness()
    {
        return new LocationStrategicBuilder()
            .WithId("wilderness")
            .WithName("Wilderness")
            .WithFavoredElement(SignatureElementTypes.Dominance)
            .WithFavoredElement(SignatureElementTypes.Precision)
            .WithDisfavoredElement(SignatureElementTypes.Rapport)
            .WithDisfavoredElement(SignatureElementTypes.Analysis)
            .WithAvailableTag("direct_approach")
            .WithAvailableTag("forceful_breakthrough")
            .WithAvailableTag("precision_footwork")
            .WithAvailableTag("efficient_movement")
            .WithAvailableTag("perfect_technique")
            .WithAvailableTag("forceful_authority")
            .Build();
    }
}
