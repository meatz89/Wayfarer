public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly ChoiceSystem choiceSystem;
    private readonly NarrativeSystem narrativeSystem;
    private readonly ChoiceCalculator choiceCalculator;
    private readonly ChoiceExecutor choiceExecutor;

    public EncounterSystem(
        GameState gameState,
        ChoiceSystem choiceSystem,
        NarrativeSystem narrativeSystem,
        MessageSystem messageSystemfabian)
    {
        this.gameState = gameState;
        this.choiceSystem = choiceSystem;
        this.narrativeSystem = narrativeSystem;
        this.choiceExecutor = new ChoiceExecutor(gameState);
        this.choiceCalculator = new ChoiceCalculator(gameState);
    }

    public EncounterResult ExecuteChoice(Encounter encounter, EncounterChoice choice, LocationProperties locationProperties)
    {
        // Retrieve the ChoiceCalculationResult
        ChoiceCalculationResult result = choiceCalculator.CalculateChoiceEffects(choice, encounter.Context);

        // Execute the choice with the actual modified values from the result
        choiceExecutor.ExecuteChoice(choice, result);

        // Update last choice type
        encounter.Context.CurrentValues.LastChoiceType = choice.Archetype;
        encounter.Context.CurrentValues.LastChoiceApproach = choice.Approach;
        narrativeSystem.MakeChoice(encounter.Context, choice);

        // Check for game over conditions
        if (IsEncounterWon(encounter))
        {
            string narrative = narrativeSystem.GetEncounterSuccessNarrative(encounter.Context);

            EncounterResult successResult = new()
            {
                encounter = encounter,
                encounterResults = EncounterResults.EncounterSuccess,
                EncounterEndMessage = narrative
            };
            return successResult;

        }
        else if (IsEncounterLost(encounter))
        {
            //string narrative = narrativeSystem.GetEncounterFailureNarrative(encounter.Context);
            EncounterResult failResult = new()
            {
                encounter = encounter,
                encounterResults = EncounterResults.EncounterFailure,
                EncounterEndMessage = ""
            };
            return failResult;
        }
        else
        {
            GetNextStage(encounter);
            EncounterResult ongoingResult = new()
            {
                encounter = encounter,
                encounterResults = EncounterResults.Ongoing,
                EncounterEndMessage = ""
            };
            return ongoingResult;
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

    public Encounter GenerateEncounter(EncounterContext context, ActionImplementation actionImplementation)
    {
        narrativeSystem.NewEncounter(context, actionImplementation);

        // Generate initial stage
        EncounterStage initialStage = GenerateStage(context);
        if (initialStage == null)
        {
            return null;
        }

        // Create encounter with initial stage
        string situation = GenerateSituation(context);

        Encounter encounter = new Encounter(context, situation);
        encounter.AddStage(initialStage);
        return encounter;
    }

    private EncounterStage GenerateStage(EncounterContext context)
    {
        // Get choice set from choice system
        ChoiceSet choiceSet = choiceSystem.GenerateChoices(context);

        if (choiceSet == null || choiceSet.Choices.Count == 0)
            return null;

        foreach (EncounterChoice choice in choiceSet.Choices)
        {
            choiceCalculator.CalculateChoiceEffects(choice, context);
        }

        // Pre-calculate all choices in the set
        foreach (EncounterChoice choice in choiceSet.Choices)
        {
            choiceCalculator.CalculateChoiceEffects(choice, context);
        }

        // Create stage with pre-calculated choices
        ChoicesNarrativeResponse choicesNarrativeResponse = narrativeSystem.GetChoicesNarrative(context, choiceSet.Choices);

        string newSituation = GetStageNarrative(choicesNarrativeResponse);
        List<ChoicesNarrative> choicesTexts = GetStageChoicesNarrative(choicesNarrativeResponse);

        choiceSet.ApplyNarratives(choicesTexts);

        return new EncounterStage
        {
            Situation = newSituation,
            CurrentChoiceSetName = choiceSet.Name,
            ChoiceSetName = choiceSet.Name,
            Choices = choiceSet.Choices
        };
    }


    public string GetStageNarrative(ChoicesNarrativeResponse choicesNarrativeResponse)
    {
        string sceneNarrative = choicesNarrativeResponse.introductory_narrative;
        return sceneNarrative;
    }

    public List<ChoicesNarrative> GetStageChoicesNarrative(ChoicesNarrativeResponse choicesNarrativeResponse)
    {
        List<ChoicesNarrative> choicesNarrative = choicesNarrativeResponse.choices.ToList();
        return choicesNarrative;
    }

    public List<UserEncounterChoiceOption> GetChoices(
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
                choice.Index,
                choice.Designation,
                choice.Narrative,
                locationName,
                locationSpotName,
                encounter,
                stage,
                choice);

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

        EncounterValues values = encounter.Context.CurrentValues;
        PlayerState player = gameState.Player;

        // Immediate loss if outcome is 0
        if (values.Outcome <= 0)
            return true;

        // Immediate loss if pressure maxes out
        if (values.Pressure >= PRESSURE_LOOSE)
            return true;

        return false;
    }

}
public class EncounterResult
{
    public Encounter encounter;
    public EncounterResults encounterResults;
    public string EncounterEndMessage;
}