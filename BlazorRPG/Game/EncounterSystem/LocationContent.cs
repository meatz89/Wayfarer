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
            .Build();
    }
}