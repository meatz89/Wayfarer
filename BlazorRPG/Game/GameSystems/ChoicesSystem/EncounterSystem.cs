public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly ChoiceSystem choiceSystem;

    public EncounterSystem(GameState gameState, ChoiceSystem choiceSystem)
    {
        this.gameState = gameState;
        this.choiceSystem = choiceSystem;
    }

    public Encounter GenerateEncounter(
      BasicActionTypes action,
      Location location,
      PlayerState playerState)
    {
        // Create context for generation
        EncounterActionContext context = new(
            action,
            location.LocationType,
            location.LocationArchetype,
            gameState.World.CurrentTimeSlot,
            location.LocationProperties,
            playerState,
            new EncounterStateValues(0, 0, 5, 0)
        );

        // Calculate encounter difficulty
        int encounterDifficulty = location.DifficultyLevel;

        // Calculate initial Advantage based on player level and encounter difficulty
        context.CurrentValues.Advantage = 5 + (gameState.Player.Level - encounterDifficulty);

        // Generate initial stage
        EncounterStage initialStage = GenerateStage(context);

        // Create encounter with initial stage
        return new Encounter
        {
            ActionType = action,
            LocationType = location.LocationType,
            LocationName = location.LocationName,
            TimeSlot = gameState.World.CurrentTimeSlot,
            Situation = GenerateSituation(context),
            Stages = new List<EncounterStage> { initialStage },
            InitialState = context.CurrentValues,
            EncounterDifficulty = encounterDifficulty
        };
    }

    private EncounterStage GenerateStage(EncounterActionContext context)
    {
        // Generate relevant choices based on context
        List<EncounterChoice> choices = choiceSystem.GenerateChoices(context);

        return new EncounterStage
        {
            Situation = GenerateStageSituation(context),
            Choices = choices
        };
    }

    public void SetActiveEncounter(Encounter encounter)
    {
        gameState.Actions.SetActiveEncounter(encounter);
    }

    public EncounterStage GetCurrentStage(Encounter encounter)
    {
        return encounter.Stages[encounter.currentStage];
    }

    public List<EncounterChoice> GetCurrentStageChoices(Encounter encounter)
    {
        return encounter.Stages[encounter.currentStage].Choices;
    }

    public void ExecuteChoice(Encounter encounter, EncounterChoice choice)
    {
        // Apply choice costs and rewards
        foreach (Outcome cost in choice.Costs)
        {
            cost.Apply(gameState.Player);
        }

        foreach (Outcome reward in choice.Rewards)
        {
            reward.Apply(gameState.Player);
        }

        // Apply choice encounter state changes
        encounter.InitialState.ApplyChanges(choice.EncounterStateChanges);
    }

    public bool GetNextStage(Encounter encounter)
    {
        // Don't proceed if we've hit our success condition
        if (encounter.InitialState.Advantage >= 10)
        {
            return false;
        }

        // Create context for new stage generation
        EncounterActionContext context = new(
          encounter.ActionType,
          encounter.LocationType,
          encounter.LocationArchetype,
          encounter.TimeSlot!.Value,
          gameState.World.CurrentLocation.LocationProperties,
          gameState.Player,
          encounter.InitialState
        );

        // Generate new stage and add it
        EncounterStage newStage = GenerateStage(context);
        encounter.Stages.Add(newStage);
        encounter.currentStage++;

        return true;
    }

    private string GenerateSituation(EncounterActionContext context)
    {
        return $"You attempt to {context.ActionType} at the {context.LocationArchetype} ({context.LocationType})...";
    }

    private string GenerateStageSituation(EncounterActionContext context)
    {
        // Generate situation based on narrative values and context
        if (context.CurrentValues.Tension >= 8)
            return "The situation is very tense...";
        if (context.CurrentValues.Understanding >= 8)
            return "You have a clear grasp of the situation...";

        return "You consider your options...";
    }
}