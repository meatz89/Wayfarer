
/// <summary>
/// Content definitions for locations with reaction tags
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
            // Positive player tags
            .WithAvailableTag("precision_footwork")
            .WithAvailableTag("flawless_execution")
            .WithAvailableTag("efficient_movement")
            .WithAvailableTag("shadow_movement")
            .WithAvailableTag("perfect_stealth")
            .WithAvailableTag("hidden_observation")
            // Negative location reaction tags
            .WithLocationReactionTag("security_alert")
            .WithLocationReactionTag("narrow_passages")
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
            // Positive player tags
            .WithAvailableTag("social_currency")
            .WithAvailableTag("network_leverage")
            .WithAvailableTag("emotional_insight")
            .WithAvailableTag("market_insight")
            .WithAvailableTag("negotiation_mastery")
            .WithAvailableTag("resource_optimization")
            // Negative location reaction tags
            .WithLocationReactionTag("social_faux_pas")
            .WithLocationReactionTag("market_suspicion")
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
            // Positive player tags
            .WithAvailableTag("intimidation_tactics")
            .WithAvailableTag("overwhelming_presence")
            .WithAvailableTag("forceful_breakthrough")
            .WithAvailableTag("unseen_threat")
            .WithAvailableTag("perfect_ambush")
            .WithAvailableTag("shadow_movement")
            // Negative location reaction tags
            .WithLocationReactionTag("hostile_territory")
            .WithLocationReactionTag("unstable_ground")
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
            // Positive player tags
            .WithAvailableTag("social_currency")
            .WithAvailableTag("emotional_insight")
            .WithAvailableTag("network_leverage")
            .WithAvailableTag("meticulous_approach")
            .WithAvailableTag("smooth_talker")
            .WithAvailableTag("careful_allocation")
            // Negative location reaction tags
            .WithLocationReactionTag("court_etiquette")
            .WithLocationReactionTag("court_politics")
            .Build();
    }
}