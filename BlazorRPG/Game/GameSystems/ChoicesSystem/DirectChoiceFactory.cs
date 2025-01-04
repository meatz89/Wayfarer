public class DirectChoiceFactory
{
    // Creates direct, high-impact choices that cost more but provide immediate benefits
    public static NarrativeChoice Create(ActionContext context)
    {
        var builder = new ChoiceBuilder()
            .WithIndex(1)
            .WithChoiceType(ChoiceTypes.Direct)
            .WithName(GetDirectDescription(context))
            .WithNarrative(GetDirectNarrative(context))
            // Direct choices always have higher energy costs
            .ExpendsEnergy(GetContextEnergy(context), 2)
            // Standard reward for direct approach
            .WithMoneyOutcome(3)
            // Direct choices build momentum but increase tension
            .WithMomentumChange(2)
            .WithTensionChange(1);

        // Set high value thresholds for special options
        if (context.NarrativeState.Momentum >= 8)
        {
            builder.WhenMomentumAbove(8)
                   .WithMoneyOutcome(5); // Increased reward for high momentum
        }

        return builder.Build();
    }

    private static string GetDirectDescription(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "Put everything into your work",
            ActionTypes.Trade => "Make an aggressive deal",
            ActionTypes.Investigate => "Focus intensely on the details",
            ActionTypes.Mingle => "Take control of the conversation",
            _ => "Take decisive action"
        };
    }

    private static string GetDirectNarrative(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "You throw yourself into the physical work with complete focus, pushing your limits...",
            ActionTypes.Trade => "You step forward confidently with your best offer, taking control of the negotiation...",
            ActionTypes.Investigate => "You examine every detail with intense concentration, determined to uncover the truth...",
            ActionTypes.Mingle => "You take charge of the conversation, speaking your mind with clarity and purpose...",
            _ => "You commit fully to your chosen path, acting with determination..."
        };
    }

    private static EnergyTypes GetContextEnergy(ActionContext context)
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

public class CarefulChoiceFactory
{
    // Creates careful choices that minimize risk while building understanding
    public static NarrativeChoice Create(ActionContext context)
    {
        var builder = new ChoiceBuilder()
            .WithIndex(2)
            .WithChoiceType(ChoiceTypes.Careful)
            .WithName(GetCarefulDescription(context))
            .WithNarrative(GetCarefulNarrative(context))
            // Careful choices give smaller rewards but cost no energy
            .WithMoneyOutcome(1)
            // Build understanding and connection at cost of momentum
            .WithUnderstandingChange(1)
            .WithConnectionChange(1)
            .WithMomentumChange(-1);

        // Special options for high understanding
        if (context.NarrativeState.Understanding >= 8)
        {
            builder.WhenUnderstandingAbove(8)
                   .WithAdvantageChange(1);  // Extra advantage from careful analysis
        }

        return builder.Build();
    }

    private static string GetCarefulDescription(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "Take a methodical approach",
            ActionTypes.Trade => "Analyze the market carefully",
            ActionTypes.Investigate => "Document everything systematically",
            ActionTypes.Mingle => "Listen and observe attentively",
            _ => "Proceed with caution"
        };
    }

    private static string GetCarefulNarrative(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "You approach the work systematically, paying attention to every detail...",
            ActionTypes.Trade => "You take time to study the market conditions and current prices...",
            ActionTypes.Investigate => "You make careful notes of everything you observe, building a complete picture...",
            ActionTypes.Mingle => "You listen carefully to the conversation, picking up subtle details...",
            _ => "You move forward carefully, considering each step..."
        };
    }
}

public class TacticalChoiceFactory
{
    // Creates tactical choices that require skills but offer special opportunities
    public static NarrativeChoice Create(ActionContext context)
    {
        var builder = new ChoiceBuilder()
            .WithIndex(3)
            .WithChoiceType(ChoiceTypes.Tactical)
            .WithName(GetTacticalDescription(context))
            .WithNarrative(GetTacticalNarrative(context))
            // Tactical choices build advantage and understanding
            .WithAdvantageChange(2)
            .WithUnderstandingChange(1);

        // Add context-specific requirements and special outcomes
        AddContextualElements(builder, context);

        // Special options for high advantage
        if (context.NarrativeState.Advantage >= 8)
        {
            builder.WhenAdvantageAbove(8)
                   .WithConnectionChange(1);  // Leverage position for connections
        }

        return builder.Build();
    }

    private static void AddContextualElements(ChoiceBuilder builder, ActionContext context)
    {
        switch (context.ActionType)
        {
            case ActionTypes.Labor:
                builder.RequiresSkill(SkillTypes.PhysicalLabor, 2)
                      .WithMoneyOutcome(2);  // Bonus for skilled work
                break;
            case ActionTypes.Trade:
                builder.RequiresSkill(SkillTypes.Trading, 2)
                      .WithMoneyOutcome(4);  // Higher reward for market knowledge
                break;
            case ActionTypes.Investigate:
                builder.RequiresSkill(SkillTypes.Observation, 2)
                      .WithUnderstandingChange(2);  // Extra insight from expertise
                break;
            case ActionTypes.Mingle:
                builder.RequiresSkill(SkillTypes.Socializing, 2)
                      .WithConnectionChange(2);  // Better networking from social skill
                break;
        }
    }

    private static string GetTacticalDescription(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "Apply expert technique",
            ActionTypes.Trade => "Leverage market knowledge",
            ActionTypes.Investigate => "Use advanced observation methods",
            ActionTypes.Mingle => "Navigate social dynamics",
            _ => "Use tactical advantage"
        };
    }

    private static string GetTacticalNarrative(ActionContext context)
    {
        return context.ActionType switch
        {
            ActionTypes.Labor => "Drawing on your experience, you employ specialized techniques...",
            ActionTypes.Trade => "You spot an opportunity in the market that others might miss...",
            ActionTypes.Investigate => "Your trained eye picks up on subtle patterns and details...",
            ActionTypes.Mingle => "You carefully navigate the social dynamics of the situation...",
            _ => "You leverage your expertise to gain an advantage..."
        };
    }
}