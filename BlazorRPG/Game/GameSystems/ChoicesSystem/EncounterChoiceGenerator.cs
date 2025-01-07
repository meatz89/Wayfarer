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
        choices.Add(GenerateTRADEoffChoice(context, choiceIndex++));
        choices.Add(GenerateOpportunityChoice(context, choiceIndex++));

        // Add high-value choices when appropriate
        choices.AddRange(GenerateHighValueChoices(context, choiceIndex));

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

    private EncounterChoice GenerateTRADEoffChoice(EncounterActionContext context, int index)
    {
        // Create a choice with meaningful positive and negative effects
        (int positiveValue, int negativeValue) tradeoffValues = GenerateContextTRADEoff(context);

        return new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ChoiceTypes.Direct)
            .WithName(GetTRADEoffActionDescription(context))
            .WithEncounter(GetTRADEoffActionEncounter(context))
            .ExpendsEnergy(GetContextEnergy(context), 2)
            .WithMoneyOutcome(2)
            .WithMomentumChange(tradeoffValues.positiveValue)
            .WithConnectionChange(tradeoffValues.negativeValue)
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


    private List<EncounterChoice> GenerateHighValueChoices(EncounterActionContext context, int startIndex)
    {
        List<EncounterChoice> highValueChoices = new List<EncounterChoice>();

        if (context.CurrentValues.Momentum >= 8)
        {
            highValueChoices.Add(GenerateFlowStateChoice(context, startIndex++));
        }

        if (context.CurrentValues.Advantage >= 8)
        {
            //highValueChoices.Add(GenerateAdvantageChoice(context, startIndex++));
        }

        if (context.CurrentValues.Understanding >= 8)
        {
            highValueChoices.Add(GenerateInsightChoice(context, startIndex++));
        }

        if (context.CurrentValues.Connection >= 8)
        {
            highValueChoices.Add(GenerateTrustChoice(context, startIndex++));
        }

        if (context.CurrentValues.Tension >= 8)
        {
            //highValueChoices.Add(GenerateTensionChoice(context, startIndex++));
        }

        return highValueChoices;
    }

    private (int positiveValue, int negativeValue) GenerateContextTRADEoff(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => (2, -1),      // High momentum, lose connection
            BasicActionTypes.Trade => (2, -1),      // High advantage, lose connection
            BasicActionTypes.Investigate => (2, -1), // High understanding, lose momentum
            BasicActionTypes.Mingle => (2, -1),     // High connection, lose advantage
            _ => (1, -1)
        };
    }

    // Continuation of the EncounterChoiceGenerator class...

    private void AddContextualOpportunity(ChoiceBuilder builder, EncounterActionContext context)
    {
        switch (context.ActionType)
        {
            case BasicActionTypes.Labor:
                builder.RequiresSkill(SkillTypes.PhysicalLabor, 2)
                      .WithSkillOutcome(SkillTypes.PhysicalLabor, 1)
                      .WithName("Apply Expert Technique")
                      .WithEncounter("Draw upon your experience to find a more efficient approach to the work.");
                break;

            case BasicActionTypes.Trade:
                builder.RequiresSkill(SkillTypes.Trading, 2)
                      .WithReputationOutcome(ReputationTypes.Merchant, 1)
                      .WithName("Leverage Market Knowledge")
                      .WithEncounter("Use your understanding of trade patterns to spot a special opportunity.");
                break;

            case BasicActionTypes.Investigate:
                builder.RequiresSkill(SkillTypes.Observation, 2)
                      .WithSkillOutcome(SkillTypes.Observation, 1)
                      .WithName("Apply Careful Analysis")
                      .WithEncounter("Your trained eye allows you to notice subtle but important details.");
                break;

            case BasicActionTypes.Mingle:
                builder.RequiresSkill(SkillTypes.Socializing, 2)
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

    private string GetTRADEoffActionEncounter(EncounterActionContext context)
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

    private string GetRecoveryActionDescription(EncounterActionContext context, ValueTypes lowestValue)
    {
        return lowestValue switch
        {
            ValueTypes.Momentum => "Take a moment to recover",
            ValueTypes.Advantage => "Regroup and reassess",
            ValueTypes.Understanding => "Clear your mind",
            ValueTypes.Connection => "Reset social dynamics",
            _ => "Take time to recover"
        };
    }

    private string GetRecoveryActionEncounter(EncounterActionContext context, ValueTypes lowestValue)
    {
        return lowestValue switch
        {
            ValueTypes.Momentum => "You pause to catch your breath and restore your energy.",
            ValueTypes.Advantage => "You step back to regain your strategic position.",
            ValueTypes.Understanding => "You take time to organize your thoughts and observations.",
            ValueTypes.Connection => "You reset your approach to social interactions.",
            _ => "You take time to recover your strength."
        };
    }

    private EncounterChoice GenerateFlowStateChoice(EncounterActionContext context, int index)
    {
        // Flow state choices capitalize on high momentum for maximum efficiency
        return new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ChoiceTypes.Modified)
            .WithName(GetFlowStateDescription(context))
            .WithEncounter(GetFlowStateEncounter(context))
            .ExpendsEnergy(GetContextEnergy(context), 3)  // Higher energy cost
            .WithMoneyOutcome(4)  // Higher reward
            .WithMomentumChange(2)
            .WithTensionChange(1)
            .Build();
    }

    private string GetFlowStateDescription(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "Work with perfect efficiency",
            BasicActionTypes.Trade => "Negotiate with perfect timing",
            BasicActionTypes.Investigate => "Enter deep focus state",
            BasicActionTypes.Mingle => "Achieve perfect social flow",
            _ => "Enter flow state"
        };
    }

    private string GetFlowStateEncounter(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "Everything clicks into place as you work with machine-like efficiency.",
            BasicActionTypes.Trade => "You feel the perfect rhythm of negotiation, every word landing exactly right.",
            BasicActionTypes.Investigate => "Your mind enters a state of perfect clarity and focus.",
            BasicActionTypes.Mingle => "The social dynamics become crystal clear, every interaction flowing naturally.",
            _ => "You enter a state of perfect focus and efficiency."
        };
    }

    private EncounterChoice GenerateInsightChoice(EncounterActionContext context, int index)
    {
        // Insight choices leverage high understanding to reveal hidden opportunities
        return new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ChoiceTypes.Modified)
            .WithName(GetInsightDescription(context))
            .WithEncounter(GetInsightEncounter(context))
            .ExpendsEnergy(GetContextEnergy(context), 1)
            .WithMoneyOutcome(2)
            .WithUnderstandingChange(1)
            .WithConnectionChange(1)
            .Build();
    }

    private string GetInsightDescription(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "Apply advanced technique",
            BasicActionTypes.Trade => "Spot market pattern",
            BasicActionTypes.Investigate => "Make key connection",
            BasicActionTypes.Mingle => "Read social undercurrents",
            _ => "Use special insight"
        };
    }

    private string GetInsightEncounter(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "Your deep understanding reveals a better way to approach the task.",
            BasicActionTypes.Trade => "You recognize a subtle pattern in market behavior others have missed.",
            BasicActionTypes.Investigate => "The pieces suddenly click together in your mind.",
            BasicActionTypes.Mingle => "You perceive the hidden dynamics of the social situation.",
            _ => "Your understanding reveals a hidden opportunity."
        };
    }

    private EncounterChoice GenerateTrustChoice(EncounterActionContext context, int index)
    {
        // Trust choices leverage high connection for special cooperation opportunities
        return new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ChoiceTypes.Modified)
            .WithName(GetTrustDescription(context))
            .WithEncounter(GetTrustEncounter(context))
            .WithMoneyOutcome(2)
            .WithConnectionChange(1)
            .WithAdvantageChange(1)
            .Build();
    }

    private string GetTrustDescription(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "Coordinate team effort",
            BasicActionTypes.Trade => "Call in a favor",
            BasicActionTypes.Investigate => "Share sensitive information",
            BasicActionTypes.Mingle => "Deepen relationship",
            _ => "Leverage trust"
        };
    }

    private string GetTrustEncounter(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "Your good relationships allow you to organize an effective team effort.",
            BasicActionTypes.Trade => "Your strong connections give you access to special opportunities.",
            BasicActionTypes.Investigate => "People trust you enough to share important information.",
            BasicActionTypes.Mingle => "The strong bonds you've built open new possibilities.",
            _ => "Your relationships create new opportunities."
        };
    }

    private ValueTypes GetLowestValue(EncounterStateValues state)
    {
        Dictionary<ValueTypes, int> values = new Dictionary<ValueTypes, int>
    {
        { ValueTypes.Momentum, state.Momentum },
        { ValueTypes.Advantage, state.Advantage },
        { ValueTypes.Understanding, state.Understanding },
        { ValueTypes.Connection, state.Connection }
    };

        return values.MinBy(x => x.Value).Key;
    }

    private bool HasLowValues(EncounterStateValues state)
    {
        const int lowThreshold = 2;
        return state.Momentum <= lowThreshold ||
               state.Advantage <= lowThreshold ||
               state.Understanding <= lowThreshold ||
               state.Connection <= lowThreshold;
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
