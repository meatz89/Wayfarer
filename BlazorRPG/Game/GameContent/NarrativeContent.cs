public class NarrativeContent
{
    public static Narrative TavernServeDrinks => new NarrativeBuilder()
        .ForAction(BasicActionTypes.Mingle)
        .ForLocationType(LocationTypes.Social)
        .ForLocation(LocationNames.LionsHeadTavern)
        .ForLocationSpot(LocationSpotNames.TavernKitchen)
        .WithCharacter(CharacterNames.Bartender)
        .InCharacterRole(CharacterRoles.WorkFor)
        .WithTimeSlot(TimeSlots.Night)
        .WithSituation("Serve Drinks in the Tavern at night.")
        .WithMomentum(3) // normal service pace
        .WithAdvantage(4) // decent positioning
        .WithUnderstanding(5) // familiar with job
        .WithConnection(4) // regular staff
        .WithTension(5) // busy night
        .AddStage(stage => stage
            .WithId(1)
            .WithSituation("Multiple patrons are calling for drinks. The bar is crowded.")

            .AddChoice(choice => choice
                .WithIndex(1)
                .WithName("Rush Multiple Orders")
                .WithChoiceType(ChoiceTypes.Aggressive)
                .WithNarrative("Grab several mugs at once, aiming to serve multiple customers quickly.")
                .ExpendsEnergy(EnergyTypes.Physical, 2)
                .WithMomentumChange(2) // faster Service
                .WithTensionChange(1)
                .WithConnectionChange(-1) // less personal attention
            )

            .AddChoice(choice => choice
                .WithIndex(2)
                .WithName("Organize Your Section")
                .WithChoiceType(ChoiceTypes.Careful)
                .WithNarrative("Take a moment to plan your route and prioritize orders.")
                .ExpendsEnergy(EnergyTypes.Physical, 1)
                .WithMomentumChange(-1) // takes time
                .WithAdvantageChange(1)
                .WithUnderstandingChange(2) // better grasp
            )

            .AddChoice(choice => choice
                .WithIndex(3)
                .WithName("Work With Other Staff")
                .WithChoiceType(ChoiceTypes.Tactical)
                .WithNarrative("Coordinate with colleagues to establish serving zones.")
                .ExpendsEnergy(EnergyTypes.Physical, 1)
                .WithAdvantageChange(-1) // relying on others
                .WithUnderstandingChange(1)
                .WithConnectionChange(2) // team synergy
            )

            .AddChoice(choice => choice
                .WithIndex(4)
                .WithName("Expert Pour Technique")
                .WithChoiceType(ChoiceTypes.Modified)
                .WithNarrative("Use your practiced technique to pour multiple drinks efficiently.")
                .ExpendsEnergy(EnergyTypes.Physical, 1)
                .RequiresSkill(SkillTypes.Agility, 1)
                .WithMomentumChange(2)
            ))

        .AddStage(stage => stage
            .WithId(2)
            .WithSituation("As you're balancing multiple drinks, a rowdy patron bumps into you. The drinks are wobbling...")

            .AddChoice(choice => choice
                .WithIndex(1)
                .WithName("Quick Recovery")
                .WithChoiceType(ChoiceTypes.Aggressive)
                .WithNarrative("Make a rapid move to stabilize the drinks.")
                .WithAdvantageChange(-1)
                .WithMomentumChange(2)
            )

            .AddChoice(choice => choice
                .WithIndex(2)
                .WithName("Steady Hands")
                .WithChoiceType(ChoiceTypes.Careful)
                .WithNarrative("Carefully adjust your grip and stance.")
                .WithMomentumChange(-1)
                .WithAdvantageChange(+2)
            )

            .AddChoice(choice => choice
                .WithIndex(3)
                .WithName("Use The Crowd")
                .WithChoiceType(ChoiceTypes.Tactical)
                .WithNarrative("Shift through nearby patrons to regain balance.")
                .WithAdvantageChange(2)
                .WithConnectionChange(-1)
            )

            .AddChoice(choice => choice
                .WithIndex(4)
                .WithName("Ask Patron for Space")
                .WithChoiceType(ChoiceTypes.Tactical)
                .WithNarrative("Politely but firmly request more room to work.")
                .WhenTensionAbove(5)
                .WithTensionChange(-2)
                .WithConnectionChange(1)
            ))

        .AddStage(stage => stage
            .WithId(3)
            .WithSituation("You've stabilized the drinks, but now some patrons are impatient from the delay.")

            .AddChoice(choice => choice
                .WithIndex(1)
                .WithName("Quick Distribution")
                .WithChoiceType(ChoiceTypes.Aggressive)
                .WithNarrative("Make a rapid move to stabilize the drinks.")
                .ExpendsEnergy(EnergyTypes.Physical, 2)
                .WithUnderstandingChange(-1)
                .WithMomentumChange(2)
            )

            .AddChoice(choice => choice
                .WithIndex(2)
                .WithName("Personal Touch")
                .WithChoiceType(ChoiceTypes.Careful)
                .WithNarrative("Carefully adjust your grip and stance.")
                .ExpendsEnergy(EnergyTypes.Physical, 1)
                .WithMomentumChange(-1)
                .WithConnectionChange(+2)
            ))
        .Build();

}
