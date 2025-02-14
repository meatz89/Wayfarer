
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

    private EncounterStage GenerateStage(Encounter encounter, EncounterContext encounterContext, EncounterStageContext lastEncounterStageContext)
    {
        EncounterStageContext encounterStageContext = new EncounterStageContext()
        {
            LocationPropertyChoiceEffects = lastEncounterStageContext.LocationPropertyChoiceEffects,
            StageValues = lastEncounterStageContext.StageValues,
        };

        // Get choice set from choice system
        ChoiceSet choiceSet = choiceSystem.GenerateChoices(encounter, encounterContext, encounterStageContext);

        if (choiceSet == null || choiceSet.Choices.Count == 0)
            return null;

        // Create stage with pre-calculated choices
        string newSituation = "situation";
        if (gameState.GameMode != Modes.Debug)
        {
            ChoicesNarrativeResponse choicesNarrativeResponse = narrativeSystem.GetChoicesNarrative(encounterStageContext, choiceSet.Choices);
            newSituation = GetStageNarrative(choicesNarrativeResponse);
            List<ChoicesNarrative> choicesTexts = GetStageChoicesNarrative(choicesNarrativeResponse);
            choiceSet.ApplyNarratives(choicesTexts);
        }

        return new EncounterStage
        {
            Situation = newSituation,
            CurrentChoiceSetName = choiceSet.Name,
            ChoiceSetName = choiceSet.Name,
            Choices = choiceSet.Choices
        };
    }

    public List<UserEncounterChoiceOption> GetChoices(Encounter encounter)
    {
        EncounterStage stage = GetCurrentStage(encounter);
        List<EncounterChoice> choices = stage.Choices;

        List<UserEncounterChoiceOption> choiceOptions = new List<UserEncounterChoiceOption>();
        foreach (EncounterChoice choice in choices)
        {
            LocationNames locationName = encounter.EncounterContext.Location.LocationName;
            string locationSpotName = encounter.EncounterContext.LocationSpot.Name;

            UserEncounterChoiceOption option = new UserEncounterChoiceOption(
                choice.Index,
                choice.ChoiceType,
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

    public EncounterResult ExecuteChoice(
        Encounter encounter,
        EncounterStage stage,
        EncounterChoice choice, 
        LocationSpot locationSpot)
    {
        // Execute the choice with the actual modified values from the result
        choiceExecutor.ExecuteChoice(encounter, choice, choice.CalculationResult);

        // Update last choice type
        encounter.LastStage = stage;
        encounter.LastChoice = choice;
        encounter.LastChoiceType = choice.Archetype;
        encounter.LastChoiceApproach = choice.Approach;

        narrativeSystem.MakeChoice(encounter.EncounterContext, choice);

        // Check for game over conditions
        return ProcessEncounterStageResult(encounter, stage, choice);
    }

    private EncounterResult ProcessEncounterStageResult(Encounter encounter, EncounterStage lastStage, EncounterChoice choice)
    {
        bool isEncounterEndingChoice = choice.IsEncounterFailingChoice || choice.IsEncounterWinningChoice;
        if (isEncounterEndingChoice)
        {
            if (choice.IsEncounterFailingChoice)
            {
                string narrative = "Failure";
                if (gameState.GameMode != Modes.Debug)
                {
                    narrative = narrativeSystem.GetEncounterFailureNarrative(lastStage.EncounterStageContext);
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
                    narrative = narrativeSystem.GetEncounterSuccessNarrative(lastStage.EncounterStageContext);
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

        GetNextStage(encounter, lastStage);
        EncounterResult ongoingResult = new()
        {
            encounter = encounter,
            encounterResults = EncounterResults.Ongoing,
            EncounterEndMessage = ""
        };
        return ongoingResult;
    }

    private bool GetNextStage(Encounter encounter, EncounterStage lastStage)
    {
        EncounterStage newStage = GenerateStage(encounter, encounter.EncounterContext, lastStage.EncounterStageContext);
        if (newStage == null)
            return false;

        encounter.AddStage(newStage);
        return true;
    }

    public Encounter GenerateEncounter(EncounterContext encounterContext, ActionImplementation actionImplementation)
    {
        narrativeSystem.NewEncounter(encounterContext, actionImplementation);

        // Create encounter with initial stage
        string situation = $"{actionImplementation.Name} ({actionImplementation.ActionType} Action)";

        Encounter encounter = new Encounter(situation);
        EncounterStageContext initialStageContext = new EncounterStageContext();

        EncounterStage initialStage = GenerateStage(encounter, encounterContext, initialStageContext);
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

    public Encounter GetEncounterForChoice(EncounterChoice choice)
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
public class EncounterResult
{
    public Encounter encounter;
    public EncounterResults encounterResults;
    public string EncounterEndMessage;
}
