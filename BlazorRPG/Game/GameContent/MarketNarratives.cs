public class MarketNarratives
{
    public static Narrative MarketInvestigation => new NarrativeBuilder()
        .ForAction(BasicActionTypes.Investigate)
        .AddStage(stage => stage
            .WithId(1)
            .WithSituation("The market square is filled with merchants hawking their wares.")
            .AddChoice(choice => choice
                .WithIndex(1)
                .WithDescription("Study the market prices")
                .ExpendsFocusEnergy(1))
            .AddChoice(choice => choice
                .WithIndex(2)
                .WithDescription("Listen to merchant gossip")
                .ExpendsSocialEnergy(1)))
        .Build();
}