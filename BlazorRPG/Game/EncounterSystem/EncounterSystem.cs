using System.Collections.Generic;

public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly ChoiceSystem choiceSystem;
    private readonly NarrativeSystem narrativeSystem;
    private readonly ChoiceExecutor choiceExecutor;

    private readonly List<EncounterChoiceSlot> encounterChoiceSlots;
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
        this.encounterChoiceSlots = contentProvider.GetChoiceSetTemplates();
    }

    public EncounterResult ExecuteChoice(Encounter encounter, EncounterChoice choice, LocationSpot locationSpot)
    {
        // Execute the choice with the actual modified values from the result
        choiceExecutor.ExecuteChoice(choice, choice.CalculationResult);

        // Update last choice type
        encounter.Context.CurrentValues.LastChoiceType = choice.Archetype;
        encounter.Context.CurrentValues.LastChoiceApproach = choice.Approach;

        narrativeSystem.MakeChoice(encounter.Context, choice);

        // Check for game over conditions
        return ProcessEncounterStageResult(encounter, choice);
    }

    private EncounterResult ProcessEncounterStageResult(Encounter encounter, EncounterChoice choice)
    {
        bool isEncounterEndingChoice = choice.IsEncounterFailingChoice || choice.IsEncounterWinningChoice;
        if (isEncounterEndingChoice)
        {
            if (choice.IsEncounterFailingChoice)
            {
                string narrative = "Failure";
                if (gameState.GameMode != Modes.Debug)
                {
                    narrative = narrativeSystem.GetEncounterFailureNarrative(encounter.Context);
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
                    narrative = narrativeSystem.GetEncounterSuccessNarrative(encounter.Context);
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

        GetNextStage(encounter);
        EncounterResult ongoingResult = new()
        {
            encounter = encounter,
            encounterResults = EncounterResults.Ongoing,
            EncounterEndMessage = ""
        };
        return ongoingResult;
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

        // Create encounter with initial stage
        string situation = $"{actionImplementation.Name} ({actionImplementation.ActionType} Action)";
        
        List<EncounterChoiceSlot> baseSlots = GetEnocunterBaseSlots(context);
        Encounter encounter = new Encounter(context, situation, baseSlots);
        encounter.AddStage(initialStage);
        return encounter;
    }

    private List<EncounterChoiceSlot> GetEnocunterBaseSlots(EncounterContext context)
    {
        List<EncounterChoiceSlot> baseSlots = new List<EncounterChoiceSlot>();

        foreach (EncounterChoiceSlot choiceSlot in encounterChoiceSlots)
        {
            if(choiceSlot.IsValidFor(context))
            {
                baseSlots.Add(choiceSlot);
            }
        }
        return baseSlots;
    }

    private EncounterStage GenerateStage(EncounterContext context)
    {
        // Get choice set from choice system
        ChoiceSet choiceSet = choiceSystem.GenerateChoices(context);

        if (choiceSet == null || choiceSet.Choices.Count == 0)
            return null;

        // Create stage with pre-calculated choices
        string newSituation = "situation";
        if (gameState.GameMode != Modes.Debug)
        {
            ChoicesNarrativeResponse choicesNarrativeResponse = narrativeSystem.GetChoicesNarrative(context, choiceSet.Choices);
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

    public List<UserEncounterChoiceOption> GetChoices(
        Encounter encounter)
    {
        EncounterStage stage = GetCurrentStage(encounter);
        List<EncounterChoice> choices = stage.Choices;

        List<UserEncounterChoiceOption> choiceOptions = new List<UserEncounterChoiceOption>();
        foreach (EncounterChoice choice in choices)
        {
            LocationNames locationName = encounter.Context.Location.LocationName;
            string locationSpotName = encounter.Context.LocationSpot.Name;

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

}
public class EncounterResult
{
    public Encounter encounter;
    public EncounterResults encounterResults;
    public string EncounterEndMessage;
}
