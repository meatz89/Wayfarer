public class MarketNarratives
{
    public static Narrative MarketInvestigation => new NarrativeBuilder()
        .ForAction(BasicActionTypes.Investigate)
        .WithSituation("The market square is filled with merchants hawking their wares.")
        .AddChoice(choice => choice
            .WithIndex(1)
            .WithDescription("Study the market prices")
            .RequiresResource(ResourceTypes.FocusEnergy, 1)
            .WithResourceOutcome(ResourceTypes.FocusEnergy, -1))
        .AddChoice(choice => choice
            .WithIndex(2)
            .WithDescription("Listen to merchant gossip")
            .RequiresResource(ResourceTypes.SocialEnergy, 1)
            .WithResourceOutcome(ResourceTypes.SocialEnergy, -1))
        .Build();
}