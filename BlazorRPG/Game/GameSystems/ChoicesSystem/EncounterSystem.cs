


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
        // Let ChoiceSystem execute the choice
        choiceSystem.ExecuteChoice(choice);

        // Check for game over or victory conditions
        if (IsGameOver() || WinGame(encounter))
        {
            gameState.Actions.SetActiveEncounter(null);
        }
    }
    private bool WinGame(Encounter encounter)
    {
        return encounter.Context.ActionType switch
        {
            // Physical actions check Outcome
            BasicActionTypes.Labor => encounter.Context.CurrentValues.Outcome >= 10,
            BasicActionTypes.Gather => encounter.Context.CurrentValues.Outcome >= 10,
            BasicActionTypes.Travel => encounter.Context.CurrentValues.Outcome >= 10,

            // Social actions check Resonance
            BasicActionTypes.Mingle => encounter.Context.CurrentValues.Resonance >= 10,
            BasicActionTypes.Persuade => encounter.Context.CurrentValues.Resonance >= 10,
            BasicActionTypes.Perform => encounter.Context.CurrentValues.Resonance >= 10,

            // Mental actions check Insight
            BasicActionTypes.Investigate => encounter.Context.CurrentValues.Insight >= 10,
            BasicActionTypes.Study => encounter.Context.CurrentValues.Insight >= 10,
            BasicActionTypes.Reflect => encounter.Context.CurrentValues.Insight >= 10,

            _ => false
        };
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

    public void SetActiveEncounter(Encounter encounter)
    {
        gameState.Actions.SetActiveEncounter(encounter);
        gameState.Player.CurrentEncounter = encounter;
    }

    public bool GetNextStage(Encounter encounter)
    {
        // Don't proceed if we've hit our success condition (Outcome ≥ 20) or if the player is in a game over state
        if (encounter.Context.CurrentValues.Outcome >= 20 || IsGameOver())
        {
            return false;
        }

        // Generate new stage and add it
        EncounterStage newStage = GenerateStage(encounter.Context);
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

    internal Encounter GetEncounterForChoice(EncounterChoice choice)
    {
        return gameState.Actions.CurrentEncounter;
    }
}