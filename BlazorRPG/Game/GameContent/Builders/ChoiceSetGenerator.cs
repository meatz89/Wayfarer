public class ChoiceSetGenerator
{
    private readonly ChoiceCalculator calculator;
    private readonly EncounterContext context;

    public ChoiceSetGenerator(EncounterContext context)
    {
        this.context = context;
        this.calculator = new ChoiceCalculator(context.LocationPropertyChoiceEffects);
    }

    public ChoiceSet GenerateChoiceSet(ChoiceSetTemplate template)
    {
        // Create base choices using the builder
        List<EncounterChoice> choices = GenerateBaseChoices(template);

        // Apply location property effects
        foreach (EncounterChoice choice in choices)
        {
            calculator.CalculateChoice(choice, context);
        }

        // Validate and ensure minimum choices
        choices = ValidateAndRefineChoices(choices);

        while (choices.Count < GetMinimumChoiceCount(template))
        {
            AddImprovisedChoice(choices);
        }

        return new ChoiceSet(template.Name, choices);
    }

    private List<EncounterChoice> GenerateBaseChoices(ChoiceSetTemplate template)
    {
        List<EncounterChoice> choices = new();
        int index = 0;

        foreach (ChoiceTemplate pattern in template.ChoiceTemplates)
        {
            // Use the builder to create each choice
            EncounterChoice choice = new ChoiceBuilder()
                .WithIndex(index++)
                .WithArchetype(pattern.ChoiceArchetype)
                .WithApproach(pattern.ChoiceApproach)
                .WithDescription(GenerateDescription(pattern))
                .Build();

            choices.Add(choice);
        }

        return choices;
    }

    private void AddImprovisedChoice(List<EncounterChoice> choices)
    {
        ChoiceArchetypes archetype = DetermineImprovisedArchetype(choices);

        // Create improvised choice using the builder
        EncounterChoice improvisedChoice = new ChoiceBuilder()
            .WithIndex(choices.Count)
            .WithArchetype(archetype)
            .WithApproach(ChoiceApproaches.Improvised)
            .WithDescription(GenerateImprovisedDescription(archetype))
            .Build();

        // Apply location property effects
        calculator.CalculateChoice(improvisedChoice, context);

        choices.Add(improvisedChoice);
    }

    private List<EncounterChoice> ValidateAndRefineChoices(List<EncounterChoice> choices)
    {
        List<EncounterChoice> validChoices = new();

        foreach (EncounterChoice choice in choices)
        {
            // Check if the choice's approach is valid for current encounter state
            if (IsApproachValid(choice.Approach, choice.Archetype))
            {
                validChoices.Add(choice);
            }
        }

        return validChoices;
    }

    private bool IsApproachValid(ChoiceApproaches approach, ChoiceArchetypes archetype)
    {
        // Check if the approach is valid based on current encounter values
        switch (approach)
        {
            case ChoiceApproaches.Direct:
                // Direct requires sufficient energy
                return HasSufficientEnergy(archetype, 2);

            case ChoiceApproaches.Pragmatic:
                // Pragmatic requires low pressure
                return context.CurrentValues.Pressure < 5;

            case ChoiceApproaches.Tactical:
                // Tactical requires high secondary value for that archetype
                return archetype switch
                {
                    ChoiceArchetypes.Focus => context.CurrentValues.Insight >= 5,
                    ChoiceArchetypes.Social => context.CurrentValues.Resonance >= 5,
                    _ => true // Physical tactical just needs the item
                };

            case ChoiceApproaches.Improvised:
                return true; // Always valid

            default:
                return false;
        }
    }

    private bool HasSufficientEnergy(ChoiceArchetypes archetype, int amount)
    {
        // Check if player has enough energy based on archetype
        return archetype switch
        {
            ChoiceArchetypes.Physical => context.PlayerState.PhysicalEnergy >= amount,
            ChoiceArchetypes.Focus => context.PlayerState.FocusEnergy >= amount,
            ChoiceArchetypes.Social => context.PlayerState.SocialEnergy >= amount,
            _ => false
        };
    }

    private ChoiceArchetypes DetermineImprovisedArchetype(List<EncounterChoice> existingChoices)
    {
        // Get counts of each archetype
        int physicalCount = existingChoices.Count(c => c.Archetype == ChoiceArchetypes.Physical);
        int focusCount = existingChoices.Count(c => c.Archetype == ChoiceArchetypes.Focus);
        int socialCount = existingChoices.Count(c => c.Archetype == ChoiceArchetypes.Social);

        // Determine primary archetype for current action type
        ChoiceArchetypes primaryArchetype = GetPrimaryArchetypeForAction(context.ActionType);

        // If primary archetype isn't overrepresented, use it
        int primaryCount = primaryArchetype switch
        {
            ChoiceArchetypes.Physical => physicalCount,
            ChoiceArchetypes.Focus => focusCount,
            ChoiceArchetypes.Social => socialCount,
            _ => throw new ArgumentException("Invalid archetype")
        };

        if (primaryCount < 2)
        {
            return primaryArchetype;
        }

        // Otherwise pick the least represented archetype
        if (physicalCount <= focusCount && physicalCount <= socialCount)
            return ChoiceArchetypes.Physical;
        if (focusCount <= physicalCount && focusCount <= socialCount)
            return ChoiceArchetypes.Focus;
        return ChoiceArchetypes.Social;
    }

    private ChoiceArchetypes GetPrimaryArchetypeForAction(BasicActionTypes actionType)
    {
        return actionType switch
        {
            BasicActionTypes.Labor => ChoiceArchetypes.Physical,
            BasicActionTypes.Gather => ChoiceArchetypes.Physical,
            BasicActionTypes.Travel => ChoiceArchetypes.Physical,

            BasicActionTypes.Investigate => ChoiceArchetypes.Focus,
            BasicActionTypes.Study => ChoiceArchetypes.Focus,
            BasicActionTypes.Reflect => ChoiceArchetypes.Focus,

            BasicActionTypes.Mingle => ChoiceArchetypes.Social,
            BasicActionTypes.Persuade => ChoiceArchetypes.Social,
            BasicActionTypes.Perform => ChoiceArchetypes.Social,

            _ => ChoiceArchetypes.Physical // Default fallback
        };
    }

    private int GetMinimumChoiceCount(ChoiceSetTemplate template)
    {
        // Each set should have at least 3 choices
        return Math.Max(3, template.ChoiceTemplates.Count / 2);
    }

    private string GenerateDescription(ChoiceTemplate pattern)
    {
        // Logic for generating appropriate description based on the pattern
        // This could consider the archetype, approach, and context
        return "Description placeholder"; // You'll want to implement proper description generation
    }

    private string GenerateImprovisedDescription(ChoiceArchetypes archetype)
    {
        // Generate appropriate description for an improvised choice
        return $"Desperate {archetype} action"; // You'll want more sophisticated description generation
    }
}