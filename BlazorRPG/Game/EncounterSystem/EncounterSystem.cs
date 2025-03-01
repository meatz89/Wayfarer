public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly ChoiceSystem choiceSystem;
    private readonly NarrativeSystem narrativeSystem;
    private readonly ChoiceExecutor choiceExecutor;

    public EncounterSystem(
        GameState gameState,
        ChoiceSystem choiceSystem,
        NarrativeSystem narrativeSystem,
        MessageSystem messageSystem,
        GameContentProvider contentProvider)
    {
        this.gameState = gameState;
        this.choiceSystem = choiceSystem;
        this.narrativeSystem = narrativeSystem;
        this.choiceExecutor = new ChoiceExecutor(gameState);
    }

    private EncounterStage GenerateStage(Encounter encounter, EncounterContext encounterContext, EncounterState encounterState)
    {
        // Get choice set from choice system
        //ChoiceSet choiceSet = choiceSystem.GenerateChoices(encounter, encounterContext, encounterStageContext);
        ChoicesModel choicesModel = choiceSystem.GenerateChoices(encounter, encounterContext, encounterState);

        if (choicesModel == null || choicesModel.Choices.Count == 0)
            return null;

        // Create stage with pre-calculated choices
        string newSituation = "situation";
        if (gameState.GameMode != Modes.Debug)
        {
            //ChoicesNarrativeResponse choicesNarrativeResponse = narrativeSystem.GetChoicesNarrative(encounterStageContext, choicesModel.Choices);
            //newSituation = GetStageNarrative(choicesNarrativeResponse);
            //List<ChoicesNarrative> choicesTexts = GetStageChoicesNarrative(choicesNarrativeResponse);
            //choicesModel.ApplyNarratives(choicesTexts);
        }

        return new EncounterStage
        {
            Situation = newSituation,
            EncounterState = choicesModel.EncounterState,
            Choices = choicesModel.Choices
        };
    }

    public bool GetNextStage(Encounter encounter, EncounterState newState)
    {
        EncounterStage newStage = GenerateStage(encounter, encounter.EncounterContext, newState);
        if (newStage == null)
            return false;

        encounter.AddStage(newStage);
        return true;
    }

    public List<UserEncounterChoiceOption> GetChoices(Encounter encounter)
    {
        EncounterStage stage = GetCurrentStage(encounter);
        List<Choice> choices = stage.Choices;

        List<UserEncounterChoiceOption> choiceOptions = new List<UserEncounterChoiceOption>();

        int i = 0;
        foreach (Choice choice in choices)
        {
            i++;
            
            LocationNames locationName = encounter.EncounterContext.Location.LocationName;
            string locationSpotName = encounter.EncounterContext.LocationSpot.Name;

            UserEncounterChoiceOption option = new UserEncounterChoiceOption(
                i,
                choice.ToString(),
                choice.Description,
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

    public EncounterResult ExecuteChoice(
        Encounter encounter,
        EncounterStage stage,
        Choice choice,
        LocationSpot locationSpot)
    {
        // Execute the choice with the actual modified values from the result
        //choiceExecutor.ExecuteChoice(encounter, choice, choice.CalculationResult);

        // Update last choice type
        encounter.LastStage = stage;
        encounter.LastChoice = choice;
        encounter.LastChoiceEffectType = choice.EffectType;
        encounter.LastChoiceApproach = choice.Approach;
        encounter.LastChoiceFocusType = choice.Focus;

        EncounterState encounterState = encounter.GetCurrentStage().EncounterState;

        EncounterState newState = choiceSystem.ApplyChoice(encounterState, choice);
        narrativeSystem.MakeChoice(encounter.EncounterContext, choice);

        GetNextStage(encounter, newState);
        EncounterStage newStage = encounter.AdvanceStage(newState);

        // Check for game over conditions
        return ProcessEncounterStageResult(encounter, stage, choice);
    }


    private EncounterResult ProcessEncounterStageResult(Encounter encounter, EncounterStage lastStage, Choice choice)
    {
        bool isEncounterEndingChoice = choice.IsEncounterFailingChoice || choice.IsEncounterWinningChoice;
        if (isEncounterEndingChoice)
        {
            if (choice.IsEncounterFailingChoice)
            {
                string narrative = "Failure";
                if (gameState.GameMode != Modes.Debug)
                {
                    narrative = narrativeSystem.GetEncounterFailureNarrative(lastStage.EncounterState);
                }
                EncounterResult failResult = new()
                {
                    encounter = encounter,
                    encounterResults = EncounterResults.EncounterFailure,
                    EncounterEndMessage = ""
                };
                return failResult;
            }

            if (choice.IsEncounterWinningChoice)
            {

                string narrative = "Success";
                if (gameState.GameMode != Modes.Debug)
                {
                    narrative = narrativeSystem.GetEncounterSuccessNarrative(lastStage.EncounterState);
                }

                EncounterResult successResult = new()
                {
                    encounter = encounter,
                    encounterResults = EncounterResults.EncounterSuccess,
                    EncounterEndMessage = narrative
                };
                return successResult;
            }
        }

        //GetNextStage(encounter, lastStage);
        EncounterResult ongoingResult = new()
        {
            encounter = encounter,
            encounterResults = EncounterResults.Ongoing,
            EncounterEndMessage = ""
        };
        return ongoingResult;
    }

    public Encounter GenerateEncounter(EncounterContext encounterContext, ActionImplementation actionImplementation)
    {
        narrativeSystem.NewEncounter(encounterContext, actionImplementation);

        // Create encounter with initial stage
        string situation = $"{actionImplementation.Name} ({actionImplementation.ActionType} Action)";

        Encounter encounter = new Encounter(situation);
        encounter.EncounterContext = encounterContext;

        EncounterState initialState = new EncounterState()
        {
            Momentum = 0,
            Pressure = 0,
            CurrentTurn = 1
        };

        EncounterStage initialStage = GenerateStage(encounter, encounterContext, initialState);
        encounter.AddStage(initialStage);

        return encounter;
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

    public Encounter GetEncounterForChoice(Choice choice)
    {
        return gameState.Actions.CurrentEncounter;
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

    internal EncounterStage GenerateInitialStage(Encounter encounter)
    {
        return new EncounterStage() { Situation = encounter.Situation };
    }

}
