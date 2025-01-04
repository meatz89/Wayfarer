public class NarrativeChoiceGenerator
{
    private readonly List<ChoicePattern> _basePatterns;
    private readonly List<ValueStateRule> _valueRules;

    public NarrativeChoiceGenerator()
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

    public List<NarrativeChoice> GenerateChoices(ActionContext context, NarrativeState currentState)
    {
        var choices = new List<NarrativeChoice>();
        int choiceIndex = 1;

        // First, create our guaranteed viable core choices
        choices.Add(GenerateBaseChoice(context, choiceIndex++, currentState));
        choices.Add(GenerateTradeoffChoice(context, choiceIndex++, currentState));
        choices.Add(GenerateOpportunityChoice(context, choiceIndex++, currentState));

        // Add recovery choice if needed - this is a special case we always want to handle
        if (HasLowValues(currentState))
        {
            choices.Add(GenerateRecoveryChoice(context, choiceIndex++, currentState));
        }

        // Add high-value choices when appropriate
        choices.AddRange(GenerateHighValueChoices(context, choiceIndex, currentState));

        return choices;
    }

    private NarrativeChoice GenerateBaseChoice(ActionContext context, int index, NarrativeState currentState)
    {
        // Create a guaranteed viable basic choice that anyone can take
        return new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ChoiceTypes.Careful)
            .WithName(GetBaseActionDescription(context))
            .WithNarrative(GetBaseActionNarrative(context))
            .ExpendsEnergy(GetContextEnergy(context), 1)  // Low energy cost
            .WithMoneyOutcome(1)  // Small but guaranteed reward
            .WithMomentumChange(1)  // Simple positive value change
            .Build();
    }

    private NarrativeChoice GenerateTradeoffChoice(ActionContext context, int index, NarrativeState currentState)
    {
        // Create a choice with meaningful positive and negative effects
        var tradeoffValues = GenerateContextTradeoff(context);

        return new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ChoiceTypes.Direct)
            .WithName(GetTradeoffActionDescription(context))
            .WithNarrative(GetTradeoffActionNarrative(context))
            .ExpendsEnergy(GetContextEnergy(context), 2)
            .WithMoneyOutcome(2)
            .WithMomentumChange(tradeoffValues.positiveValue)
            .WithConnectionChange(tradeoffValues.negativeValue)
            .Build();
    }

    private NarrativeChoice GenerateOpportunityChoice(ActionContext context, int index, NarrativeState currentState)
    {
        // Create a choice that offers special rewards or unlocks
        var builder = new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ChoiceTypes.Tactical)
            .WithName(GetOpportunityActionDescription(context))
            .WithNarrative(GetOpportunityActionNarrative(context))
            .ExpendsEnergy(GetContextEnergy(context), 1)
            .WithUnderstandingChange(1)
            .WithAdvantageChange(1);

        // Add context-specific requirements and rewards
        AddContextualOpportunity(builder, context);

        return builder.Build();
    }

    private NarrativeChoice GenerateRecoveryChoice(ActionContext context, int index, NarrativeState currentState)
    {
        var lowestValue = GetLowestValue(currentState);

        return new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ChoiceTypes.Recovery)
            .WithName(GetRecoveryActionDescription(context, lowestValue))
            .WithNarrative(GetRecoveryActionNarrative(context, lowestValue))
            .WithValueChange(lowestValue, 2)  // Restore the lowest value
            .Build();
    }

    private IEnumerable<NarrativeChoice> GenerateHighValueChoices(ActionContext context, int startIndex, NarrativeState currentState)
    {
        var highValueChoices = new List<NarrativeChoice>();

        if (currentState.Momentum >= 8)
        {
            highValueChoices.Add(GenerateFlowStateChoice(context, startIndex++, currentState));
        }

        if (currentState.Understanding >= 8)
        {
            highValueChoices.Add(GenerateInsightChoice(context, startIndex++, currentState));
        }

        if (currentState.Connection >= 8)
        {
            highValueChoices.Add(GenerateTrustChoice(context, startIndex++, currentState));
        }

        return highValueChoices;
    }

    private (int positiveValue, int negativeValue) GenerateContextTradeoff(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => (2, -1),      // High momentum, lose connection
            ActionTypes.Trade => (2, -1),      // High advantage, lose connection
            ActionTypes.Investigate => (2, -1), // High understanding, lose momentum
            ActionTypes.Mingle => (2, -1),     // High connection, lose advantage
            _ => (1, -1)
        };
    }

    // Continuation of the NarrativeChoiceGenerator class...

    private void AddContextualOpportunity(ChoiceBuilder builder, ActionContext context)
    {
        switch (context.ActionType)
        {
            case ActionTypes.Labor:
                builder.RequiresSkill(SkillTypes.PhysicalLabor, 2)
                      .WithSkillOutcome(SkillTypes.PhysicalLabor, 1)
                      .WithName("Apply Expert Technique")
                      .WithNarrative("Draw upon your experience to find a more efficient approach to the work.");
                break;

            case ActionTypes.Trade:
                builder.RequiresSkill(SkillTypes.Trading, 2)
                      .WithReputationOutcome(ReputationTypes.Merchant, 1)
                      .WithName("Leverage Market Knowledge")
                      .WithNarrative("Use your understanding of trade patterns to spot a special opportunity.");
                break;

            case ActionTypes.Investigate:
                builder.RequiresSkill(SkillTypes.Observation, 2)
                      .WithSkillOutcome(SkillTypes.Observation, 1)
                      .WithName("Apply Careful Analysis")
                      .WithNarrative("Your trained eye allows you to notice subtle but important details.");
                break;

            case ActionTypes.Mingle:
                builder.RequiresSkill(SkillTypes.Socializing, 2)
                      .WithReputationOutcome(ReputationTypes.Social, 1)
                      .WithName("Read Social Dynamics")
                      .WithNarrative("Your social experience helps you navigate the complex web of relationships.");
                break;
        }
    }

    private string GetBaseActionDescription(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "Work at a steady pace",
            ActionTypes.Trade => "Make a fair offer",
            ActionTypes.Investigate => "Take a careful look",
            ActionTypes.Mingle => "Engage in small talk",
            _ => "Take measured action"
        };
    }

    private string GetBaseActionNarrative(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "You focus on maintaining a sustainable work rhythm, neither rushing nor dawdling.",
            ActionTypes.Trade => "You consider the fair market value and make a reasonable offer.",
            ActionTypes.Investigate => "You take your time to observe the obvious details first.",
            ActionTypes.Mingle => "You start with simple, friendly conversation to establish rapport.",
            _ => "You proceed with careful consideration of your actions."
        };
    }

    private string GetTradeoffActionDescription(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "Push yourself to the limit",
            ActionTypes.Trade => "Drive a hard bargain",
            ActionTypes.Investigate => "Focus intensely on details",
            ActionTypes.Mingle => "Dominate the conversation",
            _ => "Take aggressive action"
        };
    }

    private string GetTradeoffActionNarrative(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "You pour all your energy into working as quickly as possible, though it leaves little room for social niceties.",
            ActionTypes.Trade => "You press your advantage in negotiations, though it might strain relationships.",
            ActionTypes.Investigate => "You focus so intently on the details that you might miss the broader situation.",
            ActionTypes.Mingle => "You take control of the conversation, though some might find it overwhelming.",
            _ => "You commit fully to your chosen course, accepting the consequences."
        };
    }

    private string GetOpportunityActionDescription(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "Study efficient techniques",
            ActionTypes.Trade => "Build trade connections",
            ActionTypes.Investigate => "Search for patterns",
            ActionTypes.Mingle => "Cultivate relationships",
            _ => "Seek strategic advantage"
        };
    }

    private string GetOpportunityActionNarrative(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "While working, you pay special attention to the techniques of more experienced workers.",
            ActionTypes.Trade => "You focus on building long-term trading relationships rather than immediate profit.",
            ActionTypes.Investigate => "You look for recurring patterns that might reveal deeper truths.",
            ActionTypes.Mingle => "You invest time in developing meaningful connections with others.",
            _ => "You look for ways to create lasting advantages."
        };
    }

    private string GetRecoveryActionDescription(ActionContext context, ValueTypes lowestValue)
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

    private string GetRecoveryActionNarrative(ActionContext context, ValueTypes lowestValue)
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

    private NarrativeChoice GenerateFlowStateChoice(ActionContext context, int index, NarrativeState currentState)
    {
        // Flow state choices capitalize on high momentum for maximum efficiency
        return new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ChoiceTypes.Modified)
            .WithName(GetFlowStateDescription(context))
            .WithNarrative(GetFlowStateNarrative(context))
            .ExpendsEnergy(GetContextEnergy(context), 3)  // Higher energy cost
            .WithMoneyOutcome(4)  // Higher reward
            .WithMomentumChange(2)
            .WithTensionChange(1)
            .Build();
    }

    private string GetFlowStateDescription(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "Work with perfect efficiency",
            ActionTypes.Trade => "Negotiate with perfect timing",
            ActionTypes.Investigate => "Enter deep focus state",
            ActionTypes.Mingle => "Achieve perfect social flow",
            _ => "Enter flow state"
        };
    }

    private string GetFlowStateNarrative(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "Everything clicks into place as you work with machine-like efficiency.",
            ActionTypes.Trade => "You feel the perfect rhythm of negotiation, every word landing exactly right.",
            ActionTypes.Investigate => "Your mind enters a state of perfect clarity and focus.",
            ActionTypes.Mingle => "The social dynamics become crystal clear, every interaction flowing naturally.",
            _ => "You enter a state of perfect focus and efficiency."
        };
    }

    private NarrativeChoice GenerateInsightChoice(ActionContext context, int index, NarrativeState currentState)
    {
        // Insight choices leverage high understanding to reveal hidden opportunities
        return new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ChoiceTypes.Modified)
            .WithName(GetInsightDescription(context))
            .WithNarrative(GetInsightNarrative(context))
            .ExpendsEnergy(GetContextEnergy(context), 1)
            .WithMoneyOutcome(2)
            .WithUnderstandingChange(1)
            .WithConnectionChange(1)
            .Build();
    }

    private string GetInsightDescription(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "Apply advanced technique",
            ActionTypes.Trade => "Spot market pattern",
            ActionTypes.Investigate => "Make key connection",
            ActionTypes.Mingle => "Read social undercurrents",
            _ => "Use special insight"
        };
    }

    private string GetInsightNarrative(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "Your deep understanding reveals a better way to approach the task.",
            ActionTypes.Trade => "You recognize a subtle pattern in market behavior others have missed.",
            ActionTypes.Investigate => "The pieces suddenly click together in your mind.",
            ActionTypes.Mingle => "You perceive the hidden dynamics of the social situation.",
            _ => "Your understanding reveals a hidden opportunity."
        };
    }

    private NarrativeChoice GenerateTrustChoice(ActionContext context, int index, NarrativeState currentState)
    {
        // Trust choices leverage high connection for special cooperation opportunities
        return new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(ChoiceTypes.Modified)
            .WithName(GetTrustDescription(context))
            .WithNarrative(GetTrustNarrative(context))
            .WithMoneyOutcome(2)
            .WithConnectionChange(1)
            .WithAdvantageChange(1)
            .Build();
    }

    private string GetTrustDescription(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "Coordinate team effort",
            ActionTypes.Trade => "Call in a favor",
            ActionTypes.Investigate => "Share sensitive information",
            ActionTypes.Mingle => "Deepen relationship",
            _ => "Leverage trust"
        };
    }

    private string GetTrustNarrative(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "Your good relationships allow you to organize an effective team effort.",
            ActionTypes.Trade => "Your strong connections give you access to special opportunities.",
            ActionTypes.Investigate => "People trust you enough to share important information.",
            ActionTypes.Mingle => "The strong bonds you've built open new possibilities.",
            _ => "Your relationships create new opportunities."
        };
    }

    private ValueTypes GetLowestValue(NarrativeState state)
    {
        var values = new Dictionary<ValueTypes, int>
    {
        { ValueTypes.Momentum, state.Momentum },
        { ValueTypes.Advantage, state.Advantage },
        { ValueTypes.Understanding, state.Understanding },
        { ValueTypes.Connection, state.Connection }
    };

        return values.MinBy(x => x.Value).Key;
    }

    private bool HasLowValues(NarrativeState state)
    {
        const int lowThreshold = 2;
        return state.Momentum <= lowThreshold ||
               state.Advantage <= lowThreshold ||
               state.Understanding <= lowThreshold ||
               state.Connection <= lowThreshold;
    }

    private EnergyTypes GetContextEnergy(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => EnergyTypes.Physical,
            ActionTypes.Trade => EnergyTypes.Social,
            ActionTypes.Investigate => EnergyTypes.Focus,
            ActionTypes.Mingle => EnergyTypes.Social,
            _ => EnergyTypes.Physical
        };
    }
}
