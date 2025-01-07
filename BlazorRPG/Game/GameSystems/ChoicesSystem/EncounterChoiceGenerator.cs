//SPECIAL CHOICE GENERATION:
//TENSION - BASED(Power Moves):
//CopyRequirements:
//-Tension ≥ 7
//- Physical Skill ≥ relevant threshold
//- Equipped item matches narrative type

public enum ItemTypes { Weapon, Valuable, Tool, Equipment, Gift }

//Item Types per Context:
//-Combat: WEAPON
//- Negotiation: VALUABLE
//- Investigation: TOOL
//- Labor: EQUIPMENT
//- Social: GIFT

//Effects:
//-Large Advantage gain(+3)
//-Tension reduction
//- Possible permanent item effects
//UNDERSTANDING-BASED (Expert Moves):
//CopyRequirements:
//-Understanding ≥ 7
//- Focus Skill ≥ relevant threshold
//- Has relevant knowledge flag

public enum KnowledgeTypes { Weakness, Leverage, Clue, Technique, Secret }

//Knowledge Types per Context:
//-Combat: WEAKNESS
//- Negotiation: LEVERAGE
//- Investigation: CLUE
//- Labor: TECHNIQUE
//- Social: SECRET

//Effects:
//-Significant Advantage gain(+2)
//-Understanding increase
//- May unlock new knowledge
//CONNECTION - BASED(Social Moves):
//CopyRequirements:
//-Connection ≥ 7
//- Social Skill ≥ relevant threshold
//- Has relevant reputation type

public enum ReputationTypes { Unbreakable, Honest, Sharp, Reliable, Trusted }

//Reputation Types per Context:
//-Combat: UNBREAKABLE
//- Negotiation: HONEST
//- Investigation: SHARP
//- Labor: RELIABLE
//- Social: TRUSTED

//Effects:
//-Reliable Advantage gain(+2)
//-Connection increase
//- Reputation reinforcement
//CHOICE GENERATION RULES:
//CopyEach stage generates 3-4 choices:
//1.Always include 1-2 basic progression choices
//2. Include contextual special choice if available
//3. Include strategic value-building choice

//Basic Choice Properties:
//-Energy cost(type based on context)
//- Value changes
//- Simple effects

//Special Choice Properties:
//-Higher energy cost
//- Significant advantage gain
//- Secondary benefits
//- May consume resources

//Strategic Choice Properties:
//-Lower energy cost
//- Build helpful values
//- Position improvement

public class EncounterChoiceGenerator
{
    private readonly List<ChoicePattern> _basePatterns;
    private readonly List<ValueStateRule> _valueRules;

    public EncounterChoiceGenerator()
    {
        // We'll use these patterns as templates, modifying them based on context
        _basePatterns = new List<ChoicePattern>
        {
            StandardPatterns.DirectProgressImmediate,
            StandardPatterns.CarefulPositionInvested,
            StandardPatterns.TacticalOpportunityStrategic
        };

        _valueRules = HighValueRules.AllRules;
    }

    public List<EncounterChoice> GenerateChoices(EncounterActionContext context)
    {
        List<EncounterChoice> choices = new List<EncounterChoice>();
        int choiceIndex = 1;

        // First, create our guaranteed viable core choices
        choices.Add(GenerateBaseChoice(context, choiceIndex++));
        choices.Add(GenerateTradeoffChoice(context, choiceIndex++));
        choices.Add(GenerateOpportunityChoice(context, choiceIndex++));

        return choices;
    }

    private EncounterChoice GenerateBaseChoice(EncounterActionContext context, int index)
    {
        // Create a guaranteed viable basic choice that anyone can take
        return new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ChoiceTypes.Careful)
            .WithName(GetBaseActionDescription(context))
            .WithEncounter(GetBaseActionEncounter(context))
            .ExpendsEnergy(GetContextEnergy(context), 1)  // Low energy cost
            .WithMoneyOutcome(1)  // Small but guaranteed reward
            .WithMomentumChange(1)  // Simple positive value change
            .Build();
    }

    private EncounterChoice GenerateTradeoffChoice(EncounterActionContext context, int index)
    {
        return new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ChoiceTypes.Direct)
            .WithName(GetTRADEoffActionDescription(context))
            .WithEncounter(GetTradeoffActionEncounter(context))
            .ExpendsEnergy(GetContextEnergy(context), 2)
            .WithMoneyOutcome(2)
            .WithMomentumChange(1)
            .WithConnectionChange(-1)
            .Build();
    }

    private EncounterChoice GenerateOpportunityChoice(EncounterActionContext context, int index)
    {
        // Create a choice that offers special rewards or unlocks
        ChoiceBuilder builder = new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ChoiceTypes.Tactical)
            .WithName(GetOpportunityActionDescription(context))
            .WithEncounter(GetOpportunityActionEncounter(context))
            .ExpendsEnergy(GetContextEnergy(context), 1)
            .WithUnderstandingChange(1)
            .WithAdvantageChange(1);

        // Add context-specific requirements and rewards
        AddContextualOpportunity(builder, context);

        return builder.Build();
    }


    private void AddContextualOpportunity(ChoiceBuilder builder, EncounterActionContext context)
    {
        switch (context.ActionType)
        {
            case BasicActionTypes.Labor:
                builder.RequiresSkill(SkillTypes.Strength, 2)
                      .WithSkillOutcome(SkillTypes.Strength, 1)
                      .WithName("Apply Expert Technique")
                      .WithEncounter("Draw upon your experience to find a more efficient approach to the work.");
                break;

            case BasicActionTypes.Trade:
                builder.RequiresSkill(SkillTypes.Charisma, 2)
                      .WithReputationOutcome(ReputationTypes.Merchant, 1)
                      .WithName("Leverage Market Knowledge")
                      .WithEncounter("Use your understanding of trade patterns to spot a special opportunity.");
                break;

            case BasicActionTypes.Investigate:
                builder.RequiresSkill(SkillTypes.Perception, 2)
                      .WithSkillOutcome(SkillTypes.Perception, 1)
                      .WithName("Apply Careful Analysis")
                      .WithEncounter("Your trained eye allows you to notice subtle but important details.");
                break;

            case BasicActionTypes.Mingle:
                builder.RequiresSkill(SkillTypes.Charisma, 2)
                      .WithReputationOutcome(ReputationTypes.Social, 1)
                      .WithName("Read Social Dynamics")
                      .WithEncounter("Your social experience helps you navigate the complex web of relationships.");
                break;
        }
    }

    private string GetBaseActionDescription(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "Work at a steady pace",
            BasicActionTypes.Trade => "Make a fair offer",
            BasicActionTypes.Investigate => "Take a careful look",
            BasicActionTypes.Mingle => "Engage in small talk",
            _ => "Take measured action"
        };
    }

    private string GetBaseActionEncounter(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "You focus on maintaining a sustainable work rhythm, neither rushing nor dawdling.",
            BasicActionTypes.Trade => "You consider the fair market value and make a reasonable offer.",
            BasicActionTypes.Investigate => "You take your time to observe the obvious details first.",
            BasicActionTypes.Mingle => "You start with simple, friendly conversation to establish rapport.",
            _ => "You proceed with careful consideration of your actions."
        };
    }

    private string GetTRADEoffActionDescription(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "Push yourself to the limit",
            BasicActionTypes.Trade => "Drive a hard bargain",
            BasicActionTypes.Investigate => "Focus intensely on details",
            BasicActionTypes.Mingle => "Dominate the conversation",
            _ => "Take aggressive action"
        };
    }

    private string GetTradeoffActionEncounter(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "You pour all your energy into working as quickly as possible, though it leaves little room for social niceties.",
            BasicActionTypes.Trade => "You press your advantage in negotiations, though it might strain relationships.",
            BasicActionTypes.Investigate => "You focus so intently on the details that you might miss the broader situation.",
            BasicActionTypes.Mingle => "You take control of the conversation, though some might find it overwhelming.",
            _ => "You commit fully to your chosen course, accepting the consequences."
        };
    }

    private string GetOpportunityActionDescription(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "Study efficient techniques",
            BasicActionTypes.Trade => "Build trade connections",
            BasicActionTypes.Investigate => "Search for patterns",
            BasicActionTypes.Mingle => "Cultivate relationships",
            _ => "Seek strategic advantage"
        };
    }

    private string GetOpportunityActionEncounter(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "While working, you pay special attention to the techniques of more experienced workers.",
            BasicActionTypes.Trade => "You focus on building long-term trading relationships rather than immediate profit.",
            BasicActionTypes.Investigate => "You look for recurring patterns that might reveal deeper truths.",
            BasicActionTypes.Mingle => "You invest time in developing meaningful connections with others.",
            _ => "You look for ways to create lasting advantages."
        };
    }


    private EnergyTypes GetContextEnergy(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => EnergyTypes.Physical,
            BasicActionTypes.Trade => EnergyTypes.Social,
            BasicActionTypes.Investigate => EnergyTypes.Focus,
            BasicActionTypes.Mingle => EnergyTypes.Social,
            _ => EnergyTypes.Physical
        };
    }
}
