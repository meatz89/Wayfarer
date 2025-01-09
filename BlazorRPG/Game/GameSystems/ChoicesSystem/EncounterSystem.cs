public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly ChoiceSystem choiceSystem;
    private readonly LocationSystem locationSystem;
    private readonly MessageSystem messageSystem;

    public EncounterSystem(
        GameState gameState,
        ChoiceSystem choiceSystem,
        LocationSystem locationSystem,
        MessageSystem messageSystem)
    {
        this.gameState = gameState;
        this.choiceSystem = choiceSystem;
        this.locationSystem = locationSystem;
        this.messageSystem = messageSystem;
    }

    public void ExecuteChoice(Encounter encounter, EncounterChoice choice, LocationProperties locationProperties)
    {
        // First check if we can execute
        if (!ValidateChoice(choice))
        {
            messageSystem.AddSystemMessage("Cannot execute choice - requirements not met");
            return;
        }

        // Let ChoiceSystem execute the choice
        choiceSystem.ExecuteChoice(choice);

        // Apply any encounter-specific post-execution logic
        ApplyEncounterStateValueModifications(encounter);

        // Check for game over or victory conditions
        if (IsGameOver() || HasWon(encounter))
        {
            gameState.Actions.SetActiveEncounter(null);
        }

        // Record modifications in encounter history
        encounter.ChoiceValueModifications.Add(choice.Consequences);
    }

    private bool ValidateChoice(EncounterChoice choice)
    {
        // Check if all modified requirements are met
        return choice.Consequences.ModifiedRequirements.All(req => req.IsSatisfied(gameState.Player));
    }

    private bool HasWon(Encounter encounter)
    {
        return encounter.EncounterContext.CurrentValues.Outcome >= 20;
    }

    private EncounterStage GenerateStage(EncounterContext context)
    {
        // Use ChoiceSystem to generate choices
        List<EncounterChoice> choices = choiceSystem.GenerateChoices(context);
        if (choices == null || choices.Count == 0) return null;

        // Create stage with generated choices
        return new EncounterStage
        {
            Situation = GenerateStageSituation(context),
            Choices = choices
        };
    }

    public string GetChoicePreview(EncounterChoice choice)
    {
        return choiceSystem.GetChoicePreview(choice);
    }

    public Encounter GenerateEncounter(EncounterContext context)
    {
        // Generate initial stage
        EncounterStage initialStage = GenerateStage(context);
        if (initialStage == null)
        {
            return null;
        }

        // Create encounter with initial stage
        Encounter encounter = new Encounter(context, GenerateSituation(context));
        encounter.AddStage(initialStage);

        return encounter;
    }


    private void ApplyEncounterStateValueModifications(Encounter encounter)
    {
        EncounterContext context = encounter.EncounterContext;

        // **Resonance Modifiers**
        if (context.CurrentValues.Resonance >= 8)
        {
            context.CurrentValues.Outcome += 2;
        }
        else if (context.CurrentValues.Resonance >= 5)
        {
            context.CurrentValues.Outcome += 1;
        }

        // **Determine Base Outcome Based on Values**
        // This is where you'll implement logic to derive the outcome description
        // based on the combination of Outcome, Pressure, Insight, and Resonance values.
        // You can use a similar approach to what you had in `GetOutcomeType`, but
        // instead of returning a string, you might want to set properties on the
        // `Encounter` or `EncounterStage` that describe the outcome.

        // Clamp values after applying modifications
        context.CurrentValues.Outcome = Math.Clamp(context.CurrentValues.Outcome, 0, 10);
        context.CurrentValues.Insight = Math.Clamp(context.CurrentValues.Insight, 0, 10);
        context.CurrentValues.Resonance = Math.Clamp(context.CurrentValues.Resonance, 0, 10);
        context.CurrentValues.Pressure = Math.Clamp(context.CurrentValues.Pressure, 0, 10);
    }

    public void SetActiveEncounter(Encounter encounter)
    {
        gameState.Actions.SetActiveEncounter(encounter);
        gameState.Player.CurrentEncounter = encounter;
    }

    public bool GetNextStage(Encounter encounter)
    {
        // Don't proceed if we've hit our success condition (Outcome ≥ 20) or if the player is in a game over state
        if (encounter.EncounterContext.CurrentValues.Outcome >= 20 || IsGameOver())
        {
            return false;
        }

        // Generate new stage and add it
        EncounterStage newStage = GenerateStage(encounter.EncounterContext);
        if (newStage == null) return false;

        encounter.AddStage(newStage);

        return true;
    }

    // New method to check for game over conditions
    private bool IsGameOver()
    {
        return gameState.Player.Health <= 0;
    }

    private string GenerateSituation(EncounterContext context)
    {
        // Improved situation generation
        List<string> situationElements = new List<string>();

        situationElements.Add($"You are trying to {context.ActionType} at the {context.LocationArchetype} ({context.LocationType}).");

        if (context.CurrentValues.Pressure >= 6)
        {
            situationElements.Add("The situation is tense.");
        }
        if (context.CurrentValues.Insight >= 7)
        {
            situationElements.Add("You have a good insight of what's going on.");
        }
        else if (context.CurrentValues.Insight <= 2)
        {
            situationElements.Add("You're not quite sure what to do.");
        }

        return string.Join(" ", situationElements);
    }

    private string GenerateStageSituation(EncounterContext context)
    {
        // Generate situation based on narrative values and context
        if (context.CurrentValues.Pressure >= 8)
            return "The situation is very tense...";
        if (context.CurrentValues.Insight >= 8)
            return "You have a clear grasp of the situation...";

        return "You consider your options...";
    }

    public EncounterStage GetCurrentStage(Encounter encounter)
    {
        return encounter.GetCurrentStage();
    }
}