public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly ChoiceSystem choiceSystem;
    private readonly ChoiceCalculator choiceCalculator;
    private readonly ChoiceExecutor choiceExecutor;

    public EncounterSystem(
        GameState gameState,
        ChoiceSystem choiceSystem,
        MessageSystem messageSystemfabian)
    {
        this.gameState = gameState;
        this.choiceSystem = choiceSystem;
        this.choiceExecutor = new ChoiceExecutor(gameState);
        this.choiceCalculator = new ChoiceCalculator(gameState);
    }

    public EncounterResults ExecuteChoice(Encounter encounter, EncounterChoice choice, LocationProperties locationProperties)
    {
        // Retrieve the ChoiceCalculationResult
        ChoiceCalculationResult result = choiceCalculator.CalculateChoiceEffects(choice, encounter.Context);

        // Execute the choice with the actual modified values from the result
        choiceExecutor.ExecuteChoice(choice, result);

        // Update last choice type
        encounter.Context.CurrentValues.LastChoiceType = choice.Archetype;
        encounter.Context.CurrentValues.LastChoiceApproach = choice.Approach;

        // Check for game over conditions
        if (IsEncounterWon(encounter))
        {
            return EncounterResults.EncounterSuccess;
        }
        else if (IsEncounterLost(encounter))
        {
            return EncounterResults.EncounterFailure;
        }
        else
        {
            GetNextStage(encounter);
            return EncounterResults.Ongoing;
        }
    }

    private bool GetNextStage(Encounter encounter)
    {
        EncounterStage newStage = GenerateStage(encounter.Context);
        if (newStage == null)
            return false;

        encounter.AddStage(newStage);
        return true;
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
        // Get choice set from choice system
        ChoiceSet choiceSet = choiceSystem.GenerateChoices(context);
        if (choiceSet == null || choiceSet.Choices.Count == 0)
            return null;

        // Pre-calculate all choices in the set
        foreach (EncounterChoice choice in choiceSet.Choices)
        {
            choiceCalculator.CalculateChoiceEffects(choice, context);
        }

        // Create stage with pre-calculated choices
        return new EncounterStage
        {
            Situation = GenerateStageSituation(context),
            CurrentChoiceSetName = choiceSet.Name,
            Choices = choiceSet.Choices
        };
    }

    public List<UserEncounterChoiceOption> GetChoiceOptions(
        Encounter encounter)
    {
        EncounterStage stage = GetCurrentStage(encounter);
        List<EncounterChoice> choices = stage.Choices;

        List<UserEncounterChoiceOption> choiceOptions = new List<UserEncounterChoiceOption>();
        foreach (EncounterChoice choice in choices)
        {
            LocationNames locationName = encounter.Context.LocationName;
            string locationSpotName = encounter.Context.LocationSpotName;

            UserEncounterChoiceOption option = new UserEncounterChoiceOption(
                choice.Index, choice.Description, locationName, locationSpotName,
                encounter, stage, choice);

            choiceOptions.Add(option);
        }

        return choiceOptions;
    }

    public void SetActiveEncounter(Encounter encounter)
    {
        gameState.Actions.SetActiveEncounter(encounter);
        gameState.Player.CurrentEncounter = encounter;
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

    private bool IsEncounterWon(Encounter encounter)
    {
        const int WIN_BASE = 10;
        int OUTCOME_WIN = encounter.Context.LocationDifficulty + WIN_BASE;

        return encounter.Context.CurrentValues.Outcome >= OUTCOME_WIN;
    }

    private bool IsEncounterLost(Encounter encounter)
    {
        const int LOSE_BASE = 40;
        int PRESSURE_LOOSE = LOSE_BASE - encounter.Context.LocationDifficulty;

        EncounterStateValues values = encounter.Context.CurrentValues;
        PlayerState player = gameState.Player;

        // Immediate loss if pressure maxes out
        if (values.Pressure >= PRESSURE_LOOSE)
            return true;

        return false;
    }

}