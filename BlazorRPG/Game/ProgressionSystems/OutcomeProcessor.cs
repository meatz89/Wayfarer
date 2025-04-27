public class OutcomeProcessor
{
    public GameState gameState { get; }
    public PlayerState glayerState { get; }
    public WorldState worldState { get; }
    public PlayerProgression playerProgression { get; }
    public EnvironmentalPropertyManager environmentalPropertyManager { get; }
    public ChoiceRepository choiceRepository { get; }
    public MessageSystem messageSystem { get; }

    public OutcomeProcessor(
        GameState gameState,
        PlayerProgression playerProgression,
        EnvironmentalPropertyManager environmentalPropertyManager,
        ChoiceRepository choiceRepository,
        MessageSystem messageSystem)
    {
        this.gameState = gameState;
        this.playerProgression = playerProgression;
        this.environmentalPropertyManager = environmentalPropertyManager;
        this.choiceRepository = choiceRepository;
        this.messageSystem = messageSystem;
        glayerState = gameState.PlayerState;
        worldState = gameState.WorldState;
    }

    public void ProcessActionYields(ActionImplementation action)
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

    private void UnlockCards()
    {
        foreach (CardDefinition card in choiceRepository.GetAll())
        {
            if (IsCardUnlocked(card, glayerState.PlayerSkills))
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
    private void IncreaseSkillXP(ActionImplementation action)
    {
        SkillTypes skill = DetermineSkillForAction(action);
        int skillXp = CalculateBasicActionSkillXP(action);
        playerProgression.AddSkillExp(skill, skillXp);
        messageSystem.AddSystemMessage($"Gained {skillXp} {skill} skill experience");
    }


    private SkillTypes DetermineSkillForAction(ActionImplementation action)
    {
        // Map encounter type or action category to skill
        return action.EncounterType switch
        {
            EncounterTypes.Combat => SkillTypes.Warfare,
            EncounterTypes.Social => SkillTypes.Diplomacy,
            EncounterTypes.Stealth => SkillTypes.Subterfuge,
            EncounterTypes.Exploration => SkillTypes.Wilderness,
            EncounterTypes.Lore => SkillTypes.Scholarship,
            _ => SkillTypes.Scholarship,
        };
    }

    private int CalculateBasicActionSkillXP(ActionImplementation action)
    {
        return action.Difficulty * 5;
    }

    private void UpdateTime()
    {
        Location currentLocation = worldState.CurrentLocation;
        environmentalPropertyManager.UpdateLocationForTime(currentLocation, worldState.TimeWindow);
    }

    private int CalculateDepletionAmount(float baseDepletion, PlayerSkills skills)
    {
        // More skilled players deplete resources less
        float skillFactor = 1.0f;

        // Apply skill-based reduction (example using Foraging)
        int foragingLevel = skills.GetLevelForSkill(SkillTypes.Warfare);
        if (foragingLevel > 0)
        {
            skillFactor = 1.0f - (0.05f * foragingLevel); // 5% reduction per level
            skillFactor = Math.Max(0.5f, skillFactor); // Cap at 50% reduction
        }

        return (int)(baseDepletion * skillFactor);
    }

    public void ProcessActionCosts(ActionImplementation action)
    {
        foreach (Outcome cost in action.Costs)
        {
            cost.Apply(gameState);


            if (cost is TimeOutcome timeCost)
            {
                UpdateTime();
            }

            messageSystem.AddOutcome(cost);
        }
    }

}