public class MarketNarrativesContent
{
    public static Narrative MarketInvestigation => new NarrativeBuilder()
        .ForAction(BasicActionTypes.Investigate)
        .AddStage(stage => stage
            .WithId(1)
            .WithSituation("The market square is filled with merchants hawking their wares.")
            .AddChoice(choice => choice
                .WithIndex(1)
                .WithDescription("Study the market prices")
                .ExpendsEnergy(EnergyTypes.Focus, 1))
            .AddChoice(choice => choice
                .WithIndex(2)
                .WithDescription("Listen to merchant gossip")
                .ExpendsEnergy(EnergyTypes.Social, 1)))
        .Build();
}