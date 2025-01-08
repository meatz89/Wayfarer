public class ChoiceSystem
{
    private readonly Random random = new Random();

    // Main entry point for choice generation
    public List<EncounterChoice> GenerateChoices(EncounterActionContext context)
    {
        List<EncounterChoice> choices = new();

        // Calculate state-based modifiers that affect all choices
        int tensionModifier = CalculateTensionModifier(context.CurrentValues.Tension);
        int connectionBonus = CalculateConnectionBonus(context.CurrentValues.Connection);

        // Early stages (1-2): Focus on building resources and creating options
        if (context.StageNumber <= 2)
        {
            // Add a solid aggressive choice
            choices.Add(GenerateAggressiveChoice(context, tensionModifier, false));

            // Add a careful choice for building resources
            choices.Add(GenerateCarefulChoice(context, connectionBonus));

            // Add a tactical choice if we have low Understanding
            if (context.CurrentValues.Understanding < 5)
            {
                choices.Add(GenerateTacticalChoice(context));
            }
        }
        // Late stages (5+): Force a conclusion
        else
        {
            // Add a desperate aggressive choice
            choices.Add(GenerateDesperateChoice(context));

            // Add an escape choice if player has built up resources
            if (context.CurrentValues.Understanding >= 6)
            {
                choices.Add(GenerateEscapeChoice(context));
            }

            // Add a risky power move if conditions are right
            if (context.CurrentValues.Tension >= 7)
            {
                choices.Add(GeneratePowerMove(context));
            }
        }

        // Ensure we always have at least 2 choices
        while (choices.Count < 2)
        {
            choices.Add(GenerateFallbackChoice(context));
        }

        // Set proper indices
        for (int i = 0; i < choices.Count; i++)
        {
            choices[i] = UpdateChoiceIndex(choices[i], i);
        }

        return choices;
    }

    private EncounterChoice GenerateAggressiveChoice(
        EncounterActionContext context,
        int tensionModifier,
        bool isRisky)
    {
        // Base ranges for aggressive choice
        int minAdvantage = isRisky ? 3 : 2;
        int maxAdvantage = isRisky ? 5 : 4;
        int baseTensionGain = isRisky ? 2 : 1;
        int baseEnergyCost = isRisky ? 3 : 2;

        // Apply context modifiers
        int advantageGain = random.Next(minAdvantage, maxAdvantage + 1) + tensionModifier;
        int tensionGain = baseTensionGain + (isRisky ? 1 : 0);
        int energyCost = baseEnergyCost + (isRisky ? 1 : 0);

        string description = GenerateAggressiveDescription(context, isRisky);

        return new ChoiceBuilder()
            .WithIndex(0)
            .WithChoiceType(ChoiceTypes.Aggressive)
            .WithName(description)
            .WithValueChange(EncounterValues.Advantage, advantageGain)
            .WithValueChange(EncounterValues.Tension, tensionGain)
            .RequiresEnergy(DetermineEnergyType(context), energyCost)
            .Build();
    }

    private EncounterChoice GenerateCarefulChoice(
        EncounterActionContext context,
        int connectionBonus)
    {
        // Careful choices provide steady progress with optional bonuses
        int baseAdvantage = 1;
        int bonusAdvantage = connectionBonus;
        int understandingGain = random.Next(0, 2);  // 0-1 understanding gain
        int tensionChange = random.Next(-1, 1);     // -1 to 0 tension change
        int energyCost = 1;                         // Always low energy cost

        string description = GenerateCarefulDescription(context);

        ChoiceBuilder builder = new ChoiceBuilder()
            .WithIndex(0)
            .WithChoiceType(ChoiceTypes.Careful)
            .WithName(description)
            .WithValueChange(EncounterValues.Advantage, baseAdvantage + bonusAdvantage)
            .WithValueChange(EncounterValues.Tension, tensionChange)
            .RequiresEnergy(DetermineEnergyType(context), energyCost);

        if (understandingGain > 0)
        {
            builder.WithValueChange(EncounterValues.Understanding, understandingGain);
        }

        return builder.Build();
    }

    private EncounterChoice GenerateTacticalChoice(EncounterActionContext context)
    {
        // Tactical choices build resources at the cost of immediate progress
        int understandingGain = random.Next(2, 4);  // 2-3 understanding
        int connectionGain = random.Next(1, 3);     // 1-2 connection
        int tensionChange = -1;                     // Usually reduces tension
        int energyCost = 2;                         // Moderate energy cost

        string description = GenerateTacticalDescription(context);

        return new ChoiceBuilder()
            .WithIndex(0)
            .WithChoiceType(ChoiceTypes.Tactical)
            .WithName(description)
            .WithValueChange(EncounterValues.Understanding, understandingGain)
            .WithValueChange(EncounterValues.Connection, connectionGain)
            .WithValueChange(EncounterValues.Tension, tensionChange)
            .RequiresEnergy(DetermineEnergyType(context), energyCost)
            .Build();
    }

    private EncounterChoice GenerateDesperateChoice(EncounterActionContext context)
    {
        // Desperate choices offer high rewards but at great risk
        int advantageGain = random.Next(4, 7);      // 4-6 advantage
        int tensionGain = random.Next(3, 5);        // 3-4 tension
        int energyCost = random.Next(3, 5);         // 3-4 energy

        string description = GenerateDesperateDescription(context);

        return new ChoiceBuilder()
            .WithIndex(0)
            .WithChoiceType(ChoiceTypes.Aggressive)
            .WithName(description)
            .WithValueChange(EncounterValues.Advantage, advantageGain)
            .WithValueChange(EncounterValues.Tension, tensionGain)
            .RequiresEnergy(DetermineEnergyType(context), energyCost)
            .Build();
    }

    private EncounterChoice GenerateEscapeChoice(EncounterActionContext context)
    {
        // Escape choices use Understanding to reduce Tension
        int tensionReduction = -2;
        int advantageGain = 1;
        int energyCost = 1;

        string description = GenerateEscapeDescription(context);

        return new ChoiceBuilder()
            .WithIndex(0)
            .WithChoiceType(ChoiceTypes.Tactical)
            .WithName(description)
            .WithValueChange(EncounterValues.Tension, tensionReduction)
            .WithValueChange(EncounterValues.Advantage, advantageGain)
            .RequiresEnergy(DetermineEnergyType(context), energyCost)
            .AddRequirement(new UnderstandingRequirement(6))
            .Build();
    }

    private EncounterChoice GeneratePowerMove(EncounterActionContext context)
    {
        // Power moves are high-impact choices that require setup
        int advantageGain = random.Next(4, 6);      // 4-5 advantage
        int tensionReduction = -2;                  // Always reduces tension
        int energyCost = 3;                         // High energy cost

        string description = GeneratePowerMoveDescription(context);

        ChoiceBuilder builder = new ChoiceBuilder()
            .WithIndex(0)
            .WithChoiceType(ChoiceTypes.Aggressive)
            .WithName(description)
            .WithValueChange(EncounterValues.Advantage, advantageGain)
            .WithValueChange(EncounterValues.Tension, tensionReduction)
            .RequiresEnergy(DetermineEnergyType(context), energyCost);

        // Add appropriate skill requirement based on context
        AddContextualSkillRequirement(builder, context);

        return builder.Build();
    }

    private EncounterChoice GenerateFallbackChoice(EncounterActionContext context)
    {
        // Fallback choices are always available but not very effective
        int advantageGain = 1;
        int energyCost = 1;

        string description = GenerateFallbackDescription(context);

        return new ChoiceBuilder()
            .WithIndex(0)
            .WithChoiceType(ChoiceTypes.Careful)
            .WithName(description)
            .WithValueChange(EncounterValues.Advantage, advantageGain)
            .RequiresEnergy(DetermineEnergyType(context), energyCost)
            .Build();
    }

    private void AddContextualSkillRequirement(ChoiceBuilder builder, EncounterActionContext context)
    {
        // Add skill requirements based on action type
        switch (context.ActionType)
        {
            case BasicActionTypes.Labor:
                builder.AddRequirement(new SkillRequirement(SkillTypes.Strength, 3));
                break;
            case BasicActionTypes.Trade:
            case BasicActionTypes.Mingle:
                builder.AddRequirement(new SkillRequirement(SkillTypes.Charisma, 3));
                break;
            case BasicActionTypes.Investigate:
                builder.AddRequirement(new SkillRequirement(SkillTypes.Perception, 3));
                break;
        }
    }

    private bool CanOfferSpecialChoice(EncounterActionContext context)
    {
        // Check if player has built up resources for special moves
        return context.CurrentValues.Understanding >= 6 ||
               context.CurrentValues.Connection >= 7 ||
               context.CurrentValues.Tension >= 7;
    }

    private int CalculateTensionModifier(int tension)
    {
        // Higher tension increases potential gains
        if (tension >= 8) return 2;
        if (tension >= 6) return 1;
        return 0;
    }

    private int CalculateConnectionBonus(int connection)
    {
        // Connection improves effectiveness of careful choices
        if (connection >= 8) return 2;
        if (connection >= 5) return 1;
        return 0;
    }

    private EnergyTypes DetermineEnergyType(EncounterActionContext context)
    {
        return context.ActionType switch
        {
            BasicActionTypes.Labor => EnergyTypes.Physical,
            BasicActionTypes.Trade => EnergyTypes.Social,
            BasicActionTypes.Mingle => EnergyTypes.Social,
            BasicActionTypes.Investigate => EnergyTypes.Focus,
            _ => EnergyTypes.Physical
        };
    }

    private EncounterChoice UpdateChoiceIndex(EncounterChoice choice, int index)
    {
        return new ChoiceBuilder()
            .WithIndex(index)
            .WithChoiceType(choice.ChoiceType)
            .WithName(choice.Description)
            .WithValueChanges(choice.EncounterValueChanges)
            .WithRequirements(choice.ChoiceRequirements)
            .Build();
    }

    // Description generation methods
    private string GenerateAggressiveDescription(EncounterActionContext context, bool isRisky)
    {
        string intensity = isRisky ? "aggressively" : "directly";
        return context.ActionType switch
        {
            BasicActionTypes.Labor => $"Work {intensity} to complete the task",
            BasicActionTypes.Trade => $"Push {intensity} for better terms",
            BasicActionTypes.Mingle => $"Take {intensity} control of the conversation",
            BasicActionTypes.Investigate => $"Pursue the lead {intensity}",
            _ => $"Take {intensity} action"
        };
    }

    private string GenerateCarefulDescription(EncounterActionContext context) =>
        context.ActionType switch
        {
            BasicActionTypes.Labor => "Work at a steady pace",
            BasicActionTypes.Trade => "Negotiate carefully",
            BasicActionTypes.Mingle => "Keep the conversation light",
            BasicActionTypes.Investigate => "Observe carefully",
            _ => "Proceed carefully"
        };

    private string GenerateTacticalDescription(EncounterActionContext context) =>
        context.ActionType switch
        {
            BasicActionTypes.Labor => "Look for a better approach",
            BasicActionTypes.Trade => "Seek leverage",
            BasicActionTypes.Mingle => "Build rapport",
            BasicActionTypes.Investigate => "Analyze the situation",
            _ => "Take tactical action"
        };

    private string GenerateDesperateDescription(EncounterActionContext context) =>
        context.ActionType switch
        {
            BasicActionTypes.Labor => "Risk everything to finish",
            BasicActionTypes.Trade => "Make an all-or-nothing offer",
            BasicActionTypes.Mingle => "Take a dangerous gamble",
            BasicActionTypes.Investigate => "Chase the lead regardless of risk",
            _ => "Make a desperate move"
        };

    private string GenerateEscapeDescription(EncounterActionContext context) =>
        context.ActionType switch
        {
            BasicActionTypes.Labor => "Step back and reassess",
            BasicActionTypes.Trade => "Find common ground",
            BasicActionTypes.Mingle => "Defuse the situation",
            BasicActionTypes.Investigate => "Take a measured approach",
            _ => "Find a way out"
        };

    private string GeneratePowerMoveDescription(EncounterActionContext context) =>
        context.ActionType switch
        {
            BasicActionTypes.Labor => "Execute master technique",
            BasicActionTypes.Trade => "Play your trump card",
            BasicActionTypes.Mingle => "Demonstrate your influence",
            BasicActionTypes.Investigate => "Reveal critical insight",
            _ => "Make your power play"
        };

    private string GenerateFallbackDescription(EncounterActionContext context) =>
        context.ActionType switch
        {
            BasicActionTypes.Labor => "Keep working",
            BasicActionTypes.Trade => "Continue discussion",
            BasicActionTypes.Mingle => "Stay engaged",
            BasicActionTypes.Investigate => "Keep looking",
            _ => "Persist"
        };
}