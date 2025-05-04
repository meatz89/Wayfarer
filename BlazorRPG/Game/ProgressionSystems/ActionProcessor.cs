using System;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    public void UpdateState()
    {
        Location currentLocation = worldState.CurrentLocation;
        List<Location> allLocs = locationRepository.GetAllLocations();

        foreach (Location loc in allLocs)
        {
            environmentalPropertyManager.UpdateLocationForTime(loc, worldState.TimeWindow);
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

    public void ProcessAction(ActionImplementation action)
    {
        ProcessDomains(action);

        ProcessActionCosts(action);
        ProcessActionOutcomes(action);
    }

    private void ProcessDomains(ActionImplementation action)
    {
        LocationSpot spot = locationRepository.GetCurrentLocationSpot();

        gameState.PlayerState.ApplyActionPointCost(action.ActionPointCost);

        // Calculate Vigor cost based on approach alignment
        int vigorCost = GetVigorCost(action);
        gameState.PlayerState.ApplyVigorCost(vigorCost);

        // Apply standard point generation based on action characteristics
        string exertion = action.GetExertionType();
        if (!string.IsNullOrWhiteSpace(exertion))
        {
            int hungerPoints = MapExertion(exertion);
            gameState.PlayerState.AddHungerPoints(hungerPoints);
        }

        string mentalLoad = action.GetMentalLoadType();
        if (!string.IsNullOrWhiteSpace(mentalLoad))
        {
            int mentalLoadPoints = MapMentalLoad(exertion);
            gameState.PlayerState.AddMentalLoadPoints(mentalLoadPoints);
        }

        string socialImpact = action.GetSocialImpactType();
        if (!string.IsNullOrWhiteSpace(socialImpact))
        {
            int disconnectionPoints = MapSocialImpact(exertion);
            gameState.PlayerState.AddDisconnectPoints(disconnectionPoints);
        }

        // Apply recovery effects if any
        string recoveryType = action.GetRecoveryType();
        if (!string.IsNullOrWhiteSpace(socialImpact))
        {
            ApplyRecovery(recoveryType);
        }

        // Apply affliction penalties if active
        if (gameState.PlayerState.GetHunger() > 0)
        {
            int scalingPoints = 1;
            gameState.PlayerState.AddHungerPoints(scalingPoints);
        }

        if (gameState.PlayerState.GetExhaustion() > 0)
        {
            int scalingPoints = 1;
            gameState.PlayerState.AddExhaustionPoints(scalingPoints);
        }
    }

    private void ApplyRecovery(string recoveryType)
    {
    }

    private int MapSocialImpact(string socialImpact)
    {
        return 1;
    }

    private int MapMentalLoad(string mentalLoad)
    {
        return 1;
    }

    private int MapExertion(string exertion)
    {
        return 1;
    }

    private int GetVigorCost(ActionImplementation action)
    {
        return 1;
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
        int hours = 3; // at least 3 hours;
        string timeWindowCost = "Half";

        foreach (Outcome cost in action.Costs)
        {
            if (cost is TimeOutcome timeCost)
            {
                timeWindowCost = timeCost.TimeWindow;
            }
            else
            {
                cost.Apply(gameState);
            }
            messageSystem.AddOutcome(cost);
        }

        if (!string.IsNullOrWhiteSpace(timeWindowCost) && timeWindowCost.ToLower() == "full")
        {
            hours = 6;
        }

        gameState.TimeManager.AdvanceTime(hours);
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
            EncounterTypes.Force => SkillTypes.Endurance,
            EncounterTypes.Rapport => SkillTypes.Diplomacy,
            EncounterTypes.Precision => SkillTypes.Finesse,
            EncounterTypes.Persuasion => SkillTypes.Charm,
            EncounterTypes.Observation => SkillTypes.Insight,
            _ => SkillTypes.Insight,
        };
    }

    private int CalculateBasicActionSkillXP(ActionImplementation action)
    {
        return action.Difficulty * 5;
    }
}