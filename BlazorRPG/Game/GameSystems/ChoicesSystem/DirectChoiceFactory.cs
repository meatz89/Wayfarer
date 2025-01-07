public class DirectChoiceFactory
{
    // Creates direct, high-impact choices that cost more but provide immediate benefits
    public static EncounterChoice Create(EncounterActionContext context)
    {
        ChoiceBuilder builder = new ChoiceBuilder()
            .WithIndex(1)
            .WithChoiceType(ChoiceTypes.Direct)
            .WithName(GetDirectDescription(context))
            .WithEncounter(GetDirectEncounter(context))
            // Direct choices always have higher energy costs
            .ExpendsEnergy(GetContextEnergy(context), 2)
            // Standard reward for direct approach
            .WithMoneyOutcome(3)
            // Direct choices build momentum but increase tension
            .WithMomentumChange(2)
            .WithTensionChange(1);

        // Set high value thresholds for special options
        if (context.CurrentValues.Momentum >= 8)
        {
            builder.WhenMomentumAbove(8)
                   .WithMoneyOutcome(5); // Increased reward for high momentum
        }

        return builder.Build();
    }

    private static string GetDirectDescription(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "Put everything into your work",
            BasicActionTypes.Trade => "Make an aggressive deal",
            BasicActionTypes.Investigate => "Focus intensely on the details",
            BasicActionTypes.Mingle => "Take control of the conversation",
            _ => "Take decisive action"
        };
    }

    private static string GetDirectEncounter(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "You throw yourself into the physical work with complete focus, pushing your limits...",
            BasicActionTypes.Trade => "You step forward confidently with your best offer, taking control of the negotiation...",
            BasicActionTypes.Investigate => "You examine every detail with intense concentration, determined to uncover the truth...",
            BasicActionTypes.Mingle => "You take charge of the conversation, speaking your mind with clarity and purpose...",
            _ => "You commit fully to your chosen path, acting with determination..."
        };
    }

    private static EnergyTypes GetContextEnergy(EncounterActionContext context)
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

public class CarefulChoiceFactory
{
    // Creates careful choices that minimize risk while building understanding
    public static EncounterChoice Create(EncounterActionContext context)
    {
        ChoiceBuilder builder = new ChoiceBuilder()
            .WithIndex(2)
            .WithChoiceType(ChoiceTypes.Careful)
            .WithName(GetCarefulDescription(context))
            .WithEncounter(GetCarefulEncounter(context))
            // Careful choices give smaller rewards but cost no energy
            .WithMoneyOutcome(1)
            // Build understanding and connection at cost of momentum
            .WithUnderstandingChange(1)
            .WithConnectionChange(1)
            .WithMomentumChange(-1);

        // Special options for high understanding
        if (context.CurrentValues.Understanding >= 8)
        {
            builder.WhenUnderstandingAbove(8)
                   .WithAdvantageChange(1);  // Extra advantage from careful analysis
        }

        return builder.Build();
    }

    private static string GetCarefulDescription(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "Take a methodical approach",
            BasicActionTypes.Trade => "Analyze the market carefully",
            BasicActionTypes.Investigate => "Document everything systematically",
            BasicActionTypes.Mingle => "Listen and observe attentively",
            _ => "Proceed with caution"
        };
    }

    private static string GetCarefulEncounter(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "You approach the work systematically, paying attention to every detail...",
            BasicActionTypes.Trade => "You take time to study the market conditions and current prices...",
            BasicActionTypes.Investigate => "You make careful notes of everything you observe, building a complete picture...",
            BasicActionTypes.Mingle => "You listen carefully to the conversation, picking up subtle details...",
            _ => "You move forward carefully, considering each step..."
        };
    }
}

public class TacticalChoiceFactory
{
    // Creates tactical choices that require skills but offer special opportunities
    public static EncounterChoice Create(EncounterActionContext context)
    {
        ChoiceBuilder builder = new ChoiceBuilder()
            .WithIndex(3)
            .WithChoiceType(ChoiceTypes.Tactical)
            .WithName(GetTacticalDescription(context))
            .WithEncounter(GetTacticalEncounter(context))
            // Tactical choices build advantage and understanding
            .WithAdvantageChange(2)
            .WithUnderstandingChange(1);

        // Add context-specific requirements and special outcomes
        AddContextualElements(builder, context);

        // Special options for high advantage
        if (context.CurrentValues.Advantage >= 8)
        {
            builder.WhenAdvantageAbove(8)
                   .WithConnectionChange(1);  // Leverage position for connections
        }

        return builder.Build();
    }

    private static void AddContextualElements(ChoiceBuilder builder, EncounterActionContext context)
    {
        switch (context.ActionType)
        {
            case BasicActionTypes.Labor:
                builder.RequiresSkill(SkillTypes.PhysicalLabor, 2)
                      .WithMoneyOutcome(2);  // Bonus for skilled work
                break;
            case BasicActionTypes.Trade:
                builder.RequiresSkill(SkillTypes.Trading, 2)
                      .WithMoneyOutcome(4);  // Higher reward for market knowledge
                break;
            case BasicActionTypes.Investigate:
                builder.RequiresSkill(SkillTypes.Observation, 2)
                      .WithUnderstandingChange(2);  // Extra insight from expertise
                break;
            case BasicActionTypes.Mingle:
                builder.RequiresSkill(SkillTypes.Socializing, 2)
                      .WithConnectionChange(2);  // Better networking from social skill
                break;
        }
    }

    private static string GetTacticalDescription(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "Apply expert technique",
            BasicActionTypes.Trade => "Leverage market knowledge",
            BasicActionTypes.Investigate => "Use advanced observation methods",
            BasicActionTypes.Mingle => "Navigate social dynamics",
            _ => "Use tactical advantage"
        };
    }

    private static string GetTacticalEncounter(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => "Drawing on your experience, you employ specialized techniques...",
            BasicActionTypes.Trade => "You spot an opportunity in the market that others might miss...",
            BasicActionTypes.Investigate => "Your trained eye picks up on subtle patterns and details...",
            BasicActionTypes.Mingle => "You carefully navigate the social dynamics of the situation...",
            _ => "You leverage your expertise to gain an advantage..."
        };
    }
}