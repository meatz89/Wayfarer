/// <summary>
/// Content definitions for locations with type-safe tag handling
/// </summary>
public class LocationContent
{
    /// <summary>
    /// Create Merchant Guild location properties with improved type safety
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
            // Positive player tags - now with type checking
            .WithRapportTags(
                TagRegistry.Rapport.SocialCurrency,
                TagRegistry.Rapport.NetworkLeverage,
                TagRegistry.Rapport.EmotionalInsight
            )
            .WithAnalysisTags(
                TagRegistry.Analysis.MarketInsight,
                TagRegistry.Analysis.NegotiationMastery,
                TagRegistry.Analysis.ResourceOptimization
            )
            // Negative location reaction tags - now with location-specific validation
            .WithLocationReactionTag(TagRegistry.LocationReaction.SocialFauxPas)
            .WithLocationReactionTag(TagRegistry.LocationReaction.MarketSuspicion)
            .Build();
    }

    /// <summary>
    /// Create Bandit Camp location properties with improved type safety
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
            // Positive player tags - now with type checking
            .WithDominanceTags(
                TagRegistry.Dominance.IntimidationTactics,
                TagRegistry.Dominance.OverwhelmingPresence,
                TagRegistry.Dominance.ForcefulBreakthrough
            )
            .WithConcealmentTags(
                TagRegistry.Concealment.UnseenThreat,
                TagRegistry.Concealment.PerfectAmbush,
                TagRegistry.Concealment.ShadowMovement
            )
            // Negative location reaction tags - now with location-specific validation
            .WithLocationReactionTag(TagRegistry.LocationReaction.HostileTerritory)
            .WithLocationReactionTag(TagRegistry.LocationReaction.UnstableGround)
            .Build();
    }
}