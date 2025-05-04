using System;

public class ActionProcessor
{
    private readonly LocationRepository locationRepository;

    public GameState gameState { get; }
    public PlayerState glayerState { get; }
    public WorldState worldState { get; }
    public PlayerProgression playerProgression { get; }
    public EnvironmentalPropertyManager environmentalPropertyManager { get; }
    public ChoiceRepository choiceRepository { get; }
    public MessageSystem messageSystem { get; }

    public ActionProcessor(
        GameState gameState,
        PlayerProgression playerProgression,
        EnvironmentalPropertyManager environmentalPropertyManager,
        ChoiceRepository choiceRepository,
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
        int turnAp = playerState.TurnActionPoints;

        int newEnergy = energy - turnAp;
        if (newEnergy >= 0)
        {
            playerState.SetNewEnergy(newEnergy);
        }
        else
        {
            int exhaustionPoints = Math.Abs(newEnergy);
            playerState.AddExhaustionPoints(exhaustionPoints);
        }

        gameState.TimeManager.StartNewDay();
        gameState.PlayerState.ModifyActionPoints(gameState.PlayerState.TurnActionPoints);
    }

    public void ProcessAction(ActionImplementation action)
    {
        PlayerState playerState = gameState.PlayerState;
        playerState.ApplyActionPointCost(action.ActionPointCost);
        
        ProcessActionCosts(action);
        ProcessActionOutcomes(action);

        ProcessAfflictions(action);
    }

    private void ProcessAfflictions(ActionImplementation action)
    {
        LocationSpot spot = locationRepository.GetCurrentLocationSpot();
        PlayerState playerState = gameState.PlayerState;

        // Apply standard point generation based on action characteristics
        ApplyNewAfflictions(action, playerState);
        ApplyRecovery(action);
        ApplyEscalation(playerState);
    }

    private void ApplyNewAfflictions(ActionImplementation action, PlayerState playerState)
    {
        string exertion = action.GetExertionType();
        if (!string.IsNullOrWhiteSpace(exertion))
        {
            int hungerPoints = MapExertion(exertion);
            playerState.AddHungerPoints(hungerPoints);
        }

        string mentalLoad = action.GetMentalLoadType();
        if (!string.IsNullOrWhiteSpace(mentalLoad))
        {
            int mentalLoadPoints = MapMentalLoad(exertion);
            playerState.AddMentalLoadPoints(mentalLoadPoints);
        }

        string socialImpact = action.GetSocialImpactType();
        if (!string.IsNullOrWhiteSpace(socialImpact))
        {
            int disconnectionPoints = MapSocialImpact(exertion);
            playerState.AddDisconnectPoints(disconnectionPoints);
        }
    }

    private static void ApplyEscalation(PlayerState playerState)
    {
        if (playerState.ExhaustionPoints > 0)
        {
            int scalingPoints = 1;
            playerState.AddExhaustionPoints(scalingPoints);
        }

        if (playerState.HungerPoints > 0)
        {
            int scalingPoints = 1;
            playerState.AddHungerPoints(scalingPoints);
        }
    }

    private void ApplyRecovery(ActionImplementation action)
    {
        string recoveryType = action.GetRecoveryType();
        ApplyRecovery(recoveryType);
    }

    private void ApplyRecovery(string recoveryType)
    {
    }

    private int MapExertion(string exertion)
    {
        return 0;
    }

    private int MapSocialImpact(string socialImpact)
    {
        return 0;
    }

    private int MapMentalLoad(string mentalLoad)
    {
        return 0;
    }


    private void ProcessActionOutcomes(ActionImplementation action)
    {
        // Apply Action Outcomes
        IncreaseSpotXp(action);
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

    private void IncreaseSpotXp(ActionImplementation action)
    {
        int spotXp = action.SpotXp;
        LocationSpot currentLocationSpot = gameState.WorldState.CurrentLocationSpot;
        if (spotXp > 0 && currentLocationSpot != null)
        {
            currentLocationSpot.IncreaseSpotXP(spotXp);
            messageSystem.AddSystemMessage($"Gained {spotXp} spotXp for {currentLocationSpot.Id}");
        }
    }

    private void IncreaseSkillXP(ActionImplementation action)
    {
        SkillTypes skill = DetermineSkillForAction(action);
        int skillXp = CalculateBasicActionSkillXP(action);
        playerProgression.AddSkillExp(skill, skillXp);
        messageSystem.AddSystemMessage($"Gained {skillXp} {skill} skill experience");
    }


    private void UnlockCards()
    {
        foreach (CardDefinition card in choiceRepository.GetAll())
        {
            if (IsCardUnlocked(card, glayerState.Skills))
            {
                glayerState.UnlockCard(card);
            }
        }
    }

    bool IsCardUnlocked(CardDefinition card, PlayerSkills playerSkills)
    {
        foreach (SkillRequirement requirement in card.UnlockRequirements)
        {
            int skillLevel = playerSkills.GetLevelForSkill(requirement.SkillType);
            if (skillLevel < requirement.RequiredLevel)
            {
                return false;
            }
        }
        return true;
    }
    private SkillTypes DetermineSkillForAction(ActionImplementation action)
    {
        // Map encounter type or action category to skill
        return action.EncounterType switch
        {
            EncounterApproaches.Force => SkillTypes.Endurance,
            EncounterApproaches.Rapport => SkillTypes.Diplomacy,
            EncounterApproaches.Precision => SkillTypes.Finesse,
            EncounterApproaches.Persuasion => SkillTypes.Charm,
            EncounterApproaches.Observation => SkillTypes.Insight,
            _ => SkillTypes.Insight,
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