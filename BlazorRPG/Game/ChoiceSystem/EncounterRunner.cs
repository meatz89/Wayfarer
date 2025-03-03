/// <summary>
/// Runs example encounters and displays the results
/// </summary>
public class EncounterRunner
{
    private readonly EncounterFactory _factory;

    public EncounterRunner()
    {
        _factory = new EncounterFactory();
    }

    /// <summary>
    /// Run a complete example showing how the system works
    /// </summary>
    public void RunExample()
    {
        // Create the Harbor Warehouse encounter
        Console.WriteLine("=== HARBOR WAREHOUSE ENCOUNTER EXAMPLE ===");
        EncounterProcessor harborEncounter = _factory.CreateHarborWarehouseEncounter();
        RunHarborWarehouseEncounter(harborEncounter);
    }

    /// <summary>
    /// Run through a Harbor Warehouse encounter step by step
    /// </summary>
    private void RunHarborWarehouseEncounter(EncounterProcessor encounter)
    {
        EncounterState state = encounter.GetState();

        // Display initial state
        Console.WriteLine("Initial State:");
        Console.WriteLine($"Momentum: {state.Momentum} | Pressure: {state.Pressure}");
        Console.WriteLine($"Signature: All elements at 0");
        Console.WriteLine("No active tags");
        Console.WriteLine();

        // Turn 1: Use a Finesse choice to build Precision element
        Choice choice1 = FindChoiceByType(state.CurrentChoices, ApproachTypes.Finesse, null, EffectTypes.Momentum);
        Console.WriteLine($"TURN 1: Selected {choice1.Name} ({choice1.ApproachType} + {choice1.FocusType} + {choice1.EffectType})");
        encounter.ProcessChoice(choice1);
        DisplayState(encounter);

        // Turn 2: Use another Finesse choice to continue building Precision
        choice1 = FindChoiceByType(state.CurrentChoices, ApproachTypes.Finesse, null, EffectTypes.Momentum);
        Console.WriteLine($"TURN 2: Selected {choice1.Name} ({choice1.ApproachType} + {choice1.FocusType} + {choice1.EffectType})");
        encounter.ProcessChoice(choice1);
        DisplayState(encounter);

        // Turn 3: Use a third Finesse choice to reach Precision 3 and activate tags
        choice1 = FindChoiceByType(state.CurrentChoices, ApproachTypes.Finesse, null, EffectTypes.Momentum);
        Console.WriteLine($"TURN 3: Selected {choice1.Name} ({choice1.ApproachType} + {choice1.FocusType} + {choice1.EffectType})");
        encounter.ProcessChoice(choice1);
        DisplayState(encounter);

        // Turn 4: Test a tag effect - Using environment-focused choice with Precision 3 (precision_footwork active)
        choice1 = FindChoiceByType(state.CurrentChoices, null, FocusTypes.Environment, EffectTypes.Pressure);
        Console.WriteLine($"TURN 4: Selected {choice1.Name} ({choice1.ApproachType} + {choice1.FocusType} + {choice1.EffectType})");
        Console.WriteLine("   Testing precision_footwork tag - should reduce pressure from environment choices");
        encounter.ProcessChoice(choice1);
        DisplayState(encounter);

        // Turn 5: Use a Stealth choice to build Concealment and finish the encounter
        choice1 = FindChoiceByType(state.CurrentChoices, ApproachTypes.Stealth, null, EffectTypes.Momentum);
        Console.WriteLine($"TURN 5: Selected {choice1.Name} ({choice1.ApproachType} + {choice1.FocusType} + {choice1.EffectType})");
        encounter.ProcessChoice(choice1);
        DisplayState(encounter);

        // Final results
        Console.WriteLine("=== FINAL RESULTS ===");
        Console.WriteLine($"Encounter Status: {state.EncounterStatus}");
        StrategicSignature signature = encounter.GetSignature();
        Console.WriteLine($"Final Signature: D{signature.Dominance} R{signature.Rapport} A{signature.Analysis} P{signature.Precision} C{signature.Concealment}");

        List<EncounterTag> activeTags = encounter.GetActiveTags();
        Console.WriteLine("\nActive Tags at end of encounter:");
        foreach (EncounterTag tag in activeTags)
        {
            Console.WriteLine($"- {tag.Name}: {tag.Description} (from {tag.SourceElement} level {tag.ThresholdValue})");
        }
    }

    /// <summary>
    /// Helper method to display the state after a turn
    /// </summary>
    private void DisplayState(EncounterProcessor encounter)
    {
        EncounterState state = encounter.GetState();
        StrategicSignature signature = encounter.GetSignature();

        Console.WriteLine($"Momentum: {state.Momentum} | Pressure: {state.Pressure} | Turn: {state.CurrentTurn}");
        Console.WriteLine($"Signature: D{signature.Dominance} R{signature.Rapport} A{signature.Analysis} P{signature.Precision} C{signature.Concealment}");

        List<EncounterTag> activeTags = encounter.GetActiveTags();
        if (activeTags.Count > 0)
        {
            Console.WriteLine("Active Tags:");
            foreach (EncounterTag tag in activeTags)
            {
                Console.WriteLine($"- {tag.Name}: {tag.Description}");
            }
        }
        else
        {
            Console.WriteLine("No active tags");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Find a choice by its characteristics, with fallbacks if exact match not found
    /// </summary>
    private Choice FindChoiceByType(List<Choice> choices, ApproachTypes? approach, FocusTypes? focus, EffectTypes? effect)
    {
        // Try to find exact match
        Choice matchingChoice = choices.FirstOrDefault(c =>
            (approach == null || c.ApproachType == approach.Value) &&
            (focus == null || c.FocusType == focus.Value) &&
            (effect == null || c.EffectType == effect.Value));

        if (matchingChoice != null)
            return matchingChoice;

        // Try with just approach
        if (approach.HasValue)
        {
            matchingChoice = choices.FirstOrDefault(c => c.ApproachType == approach.Value);
            if (matchingChoice != null)
                return matchingChoice;
        }

        // Try with just focus
        if (focus.HasValue)
        {
            matchingChoice = choices.FirstOrDefault(c => c.FocusType == focus.Value);
            if (matchingChoice != null)
                return matchingChoice;
        }

        // Fallback to any choice
        return choices.First();
    }

    /// <summary>
    /// Main entry point to run the example
    /// </summary>
    public static void Main()
    {
        EncounterRunner runner = new EncounterRunner();
        runner.RunExample();
    }
}