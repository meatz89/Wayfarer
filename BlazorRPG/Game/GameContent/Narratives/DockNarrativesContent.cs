public class DockNarrativesContent
{
    public static Narrative DocksInvestigation => new NarrativeBuilder()
        .ForAction(BasicActionTypes.Investigate)
        .AddStage(stage => stage
            .WithId(1)
            .WithSituation("The docks are bustling with activity. Workers move cargo while ships come and go.")
            .AddChoice(choice => choice
                .WithIndex(1)
                .WithDescription("Observe the workers' routines")
                .ExpendsEnergy(EnergyTypes.Focus, 1)))
        .Build();

    public static Narrative DockWork => new NarrativeBuilder()
        .ForAction(BasicActionTypes.Labor)
        .AddStage(stage => stage
            .WithId(1)
            .WithSituation("The docks manager needs boxes moved quickly")
            .AddChoice(choice => choice
                .WithIndex(1)
                .WithDescription("Work steadily and methodically")
                .ExpendsEnergy(EnergyTypes.Physical, 2)
                .RequiresSkill(SkillTypes.Strength, 1))
            .AddChoice(choice => choice
                .WithIndex(2)
                .WithDescription("Push yourself to work faster")
                .ExpendsEnergy(EnergyTypes.Physical, 3)
                .WithHealthOutcome(-1)))
        .Build();

}
