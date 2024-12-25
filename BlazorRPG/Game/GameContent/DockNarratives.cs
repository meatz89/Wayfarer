public class DockNarratives
{
    public static Narrative DocksInvestigation => new NarrativeBuilder()
        .ForAction(BasicActionTypes.Investigate)
        .WithSituation("The docks are bustling with activity. Workers move cargo while ships come and go.")
        .AddChoice(choice => choice
            .WithIndex(1)
            .WithDescription("Observe the workers' routines")
            .RequiresResource(ResourceTypes.FocusEnergy, 1)
            .WithResourceOutcome(ResourceTypes.FocusEnergy, -1))
        .Build();

    public static Narrative DockWork => new NarrativeBuilder()
        .ForAction(BasicActionTypes.Labor)
        .WithSituation("The docks manager needs boxes moved quickly")
        .AddChoice(choice => choice
            .WithIndex(1)
            .WithDescription("Work steadily and methodically")
            .RequiresResource(ResourceTypes.PhysicalEnergy, 2)
            .RequiresSkill(SkillTypes.Strength, 1)
            .WithResourceOutcome(ResourceTypes.PhysicalEnergy, -1))
        .AddChoice(choice => choice
            .WithIndex(2)
            .WithDescription("Push yourself to work faster")
            .RequiresResource(ResourceTypes.PhysicalEnergy, 3)
            .WithResourceOutcome(ResourceTypes.PhysicalEnergy, -2)
            .WithResourceOutcome(ResourceTypes.Health, -1))
        .Build();

}
