using System;

public class ActionProcessor
{
    private readonly LocationRepository locationRepository;

    public GameState gameState { get; }
    public PlayerState glayerState { get; }
    public WorldState worldState { get; }
    public PlayerProgression playerProgression { get; }
    public EnvironmentalPropertyManager environmentalPropertyManager { get; }
    public NarrativeChoiceRepository choiceRepository { get; }
    public MessageSystem messageSystem { get; }

    public ActionProcessor(
        GameState gameState,
        PlayerProgression playerProgression,
        EnvironmentalPropertyManager environmentalPropertyManager,
        NarrativeChoiceRepository choiceRepository,
        LocationRepository locationRepository,
        MessageSystem messageSystem)
    {
        this.gameState = gameState;
        this.playerProgression = playerProgression;
        this.environmentalPropertyManager = environmentalPropertyManager;
        this.choiceRepository = choiceRepository;
        this.locationRepository = locationRepository;
        this.messageSystem = messageSystem;
        glayerState = gameState.PlayerState;
        worldState = gameState.WorldState;
    }

    public void ProcessTurnChange()
    {
        PlayerState playerState = gameState.PlayerState;

        int energy = playerState.CurrentEnergy();
        int turnAp = playerState.MaxActionPoints;

        int newEnergy = energy - turnAp;
        if (newEnergy >= 0)
        {
            playerState.SetNewEnergy(newEnergy);
        }

        gameState.TimeManager.StartNewDay();
        gameState.PlayerState.ModifyActionPoints(gameState.PlayerState.MaxActionPoints);
    }

    public void ProcessAction(ActionImplementation action)
    {
        PlayerState playerState = gameState.PlayerState;
        playerState.ApplyActionPointCost(action.ActionPointCost);

        ProcessActionCosts(action);
        ProcessActionOutcomes(action);
    }

    private void ApplyRecovery(ActionImplementation action)
    {
        //string recoveryType = action.GetRecoveryType();
        //ApplyRecovery(recoveryType);
    }

    private void ProcessActionOutcomes(ActionImplementation action)
    {
        // Apply Action Outcomes
        IncreaseSkillXP(action);
        UnlockCards();

        foreach (Outcome reward in action.Yields)
        {
            reward.Apply(gameState);
            messageSystem.AddOutcome(reward);
        }
    }

    private void ProcessActionCosts(ActionImplementation action)
    {
        int actionPointCost = 1; // Default cost of 1 action point

        foreach (Outcome cost in action.Costs)
        {
            if (cost is ActionPointOutcome apCost)
            {
                actionPointCost = apCost.Amount;
            }
            else
            {
                cost.Apply(gameState);
            }
            messageSystem.AddOutcome(cost);
        }

        gameState.PlayerState.ModifyActionPoints(actionPointCost);
        gameState.TimeManager.UpdateTimeWindow();

        UpdateState();
    }

    private void IncreaseSkillXP(ActionImplementation action)
    {
        Skills skill = DetermineSkillForAction(action);
        int skillXp = CalculateBasicActionSkillXP(action);
        playerProgression.AddSkillExp(skill, skillXp);
        messageSystem.AddSystemMessage($"Gained {skillXp} {skill} skill experience");
    }


    private void UnlockCards()
    {
    }

    private Skills DetermineSkillForAction(ActionImplementation action)
    {
        // Map encounter type or action category to skill
        return action.EncounterType switch
        {
            EncounterCategories.Force => Skills.Endurance,
            EncounterCategories.Persuasion => Skills.Charm,
        };
    }

    private int CalculateBasicActionSkillXP(ActionImplementation action)
    {
        return action.Difficulty * 5;
    }


    public void UpdateState()
    {
        Location currentLocation = worldState.CurrentLocation;
        List<Location> allLocs = locationRepository.GetAllLocations();

        foreach (Location loc in allLocs)
        {
            environmentalPropertyManager.UpdateLocationForTime(loc, worldState.CurrentTimeWindow);
        }
    }

    public bool CanExecute(ActionImplementation action)
    {
        foreach (IRequirement requirement in action.Requirements)
        {
            if (!requirement.IsMet(gameState))
            {
                return false; // Requirement not met
            }
        }

        // Check if the action has been completed and is non-repeatable
        if (action.ActionType == ActionTypes.Encounter)
        {
            string encounterId = action.Id;
            if (gameState.WorldState.IsEncounterCompleted(encounterId))
            {
                return false; // Encounter already completed
            }
        }
        return true; // All requirements are met
    }

}