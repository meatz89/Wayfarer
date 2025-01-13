using System.Diagnostics.Metrics;

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

    private EncounterStage GenerateStage(EncounterContext context)
    {
        // Use ChoiceSystem to generate choices
        ChoiceSet? choiceSet = choiceSystem.GenerateChoices(context);
        if (choiceSet == null || choiceSet.Choices.Count == 0) return null;

        // Create stage with generated choices
        return new EncounterStage
        {
            Situation = GenerateStageSituation(context),
            CurrentChoiceSetName = choiceSet.Name,
            Choices = choiceSet.Choices
        };
    }

    public void ExecuteChoice(Encounter encounter, EncounterChoice choice, LocationProperties locationProperties)
    {
        // Let ChoiceSystem execute the choice
        choiceSystem.ExecuteChoice(choice);

        // Check for game over or victory conditions
        if (IsGameOver(encounter) || IsGameWon(encounter))
        {
            gameState.Actions.SetActiveEncounter(null);
        }
    }

    public bool GetNextStage(Encounter encounter)
    {
        // Generate new stage and add it
        EncounterStage newStage = GenerateStage(encounter.Context);
        if (newStage == null) return false;

        encounter.AddStage(newStage);

        return true;
    }


    public void SetEncounterChoices(Encounter encounter, LocationNames location)
    {
        string locationSpot = "LocationSpot";

        EncounterStage stage = GetCurrentStage(encounter);
        List<EncounterChoice> choices = stage.Choices;

        List<UserEncounterChoiceOption> choiceOptions = new List<UserEncounterChoiceOption>();
        foreach (EncounterChoice choice in choices)
        {
            UserEncounterChoiceOption option = new UserEncounterChoiceOption(
                choice.Index, choice.Description, location,
                encounter, stage, choice);

            choiceOptions.Add(option);
        }

        gameState.Actions.SetEncounterChoiceOptions(choiceOptions);
    }

    public void SetActiveEncounter(Encounter encounter)
    {
        gameState.Actions.SetActiveEncounter(encounter);
        gameState.Player.CurrentEncounter = encounter;
    }

    private bool IsGameWon(Encounter encounter)
    {
        const int OUTCOME_WIN = 20;
        const int INSIGHT_WIN = 14;
        const int RESONANCE_WIN = 14;

        // Different win conditions based on action type
        return encounter.Context.ActionType switch
        {
            // Physical actions need high Outcome
            BasicActionTypes.Labor or
            BasicActionTypes.Gather or
            BasicActionTypes.Travel =>
                encounter.Context.CurrentValues.Outcome >= OUTCOME_WIN ||
                (encounter.Context.CurrentValues.Outcome >= 14 &&
                 encounter.Context.CurrentValues.Pressure <= 3),

            // Social actions can win through high Resonance
            BasicActionTypes.Mingle or
            BasicActionTypes.Persuade or
            BasicActionTypes.Perform =>
                encounter.Context.CurrentValues.Outcome >= OUTCOME_WIN ||
                (encounter.Context.CurrentValues.Resonance >= RESONANCE_WIN &&
                 encounter.Context.CurrentValues.Outcome >= 10),

            // Mental actions can win through high Insight
            BasicActionTypes.Investigate or
            BasicActionTypes.Study or
            BasicActionTypes.Reflect =>
                encounter.Context.CurrentValues.Outcome >= OUTCOME_WIN ||
                (encounter.Context.CurrentValues.Insight >= INSIGHT_WIN &&
                 encounter.Context.CurrentValues.Outcome >= 10),

            _ => false
        };
    }

    // New method to check for game over conditions
    private bool IsGameOver(Encounter encounter)
    {
        EncounterStateValues values = encounter.Context.CurrentValues;
        PlayerState player = gameState.Player;

        // Immediate loss if pressure maxes out
        if (values.Pressure >= 10)
            return true;

        // Loss if all energy types are depleted and can't pay permanent costs
        bool canPayPhysical = player.PhysicalEnergy > 0 || player.Health > 1;
        bool canPayFocus = player.FocusEnergy > 0 || player.Stress < player.MaxStress - 1;
        bool canPaySocial = player.SocialEnergy > 0 || player.Reputation > 1;

        return !canPayPhysical && !canPayFocus && !canPaySocial;
    }

    public EncounterStage GetCurrentStage(Encounter encounter)
    {
        return encounter.GetCurrentStage();
    }

    public Encounter GetEncounterForChoice(EncounterChoice choice)
    {
        return gameState.Actions.CurrentEncounter;
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
}