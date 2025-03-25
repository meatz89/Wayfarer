public class EncounterManager
{
    public ActionImplementation ActionImplementation;

    private CardSelectionAlgorithm cardSelectionAlgorithm;
    private DiscoveryManager discoveryManager;
    public EncounterState encounterState;

    private NarrativeService narrativeService;
    private NarrativeContext narrativeContext;

    private ResourceManager resourceManager;
    private RelationshipManager relationshipManager;

    public List<IChoice> CurrentChoices = new List<IChoice>();

    public PlayerState playerState;
    public WorldState worldState;
    private EncounterInfo encounterInfo;

    private bool _useAiNarrative = false;
    private bool _useMemory = false;
    private bool _processStateChanges = false;

    public EncounterManager(
        ActionImplementation actionImplementation,
        CardSelectionAlgorithm cardSelector,
        DiscoveryManager discoveryManager,
        NarrativeService narrativeService,
        ResourceManager resourceManager,
        RelationshipManager relationshipManager,
        IConfiguration configuration,
        ILogger<EncounterSystem> logger)
    {
        ActionImplementation = actionImplementation;
        cardSelectionAlgorithm = cardSelector;
        this.discoveryManager = discoveryManager;
        this.narrativeService = narrativeService;
        this.resourceManager = resourceManager;
        this.relationshipManager = relationshipManager;
        _useAiNarrative = configuration.GetValue<bool>("useAiNarrative");
        _useMemory = configuration.GetValue<bool>("useMemory");
        _processStateChanges = configuration.GetValue<bool>("processStateChanges");
    }

    private void StartEncounter(WorldState worldState, PlayerState playerState, EncounterInfo encounterInfo)
    {
        this.worldState = worldState;
        this.playerState = playerState;

        encounterState = new EncounterState(encounterInfo, playerState, this.resourceManager);
        encounterState.UpdateActiveTags(encounterInfo.AvailableTags);
    }

    public async Task<NarrativeResult> StartEncounterWithNarrativeAsync(
        Location location,
        EncounterInfo encounterInfo,
        WorldState worldState,
        PlayerState playerState,
        ActionImplementation actionImplementation,
        AIProviderType providerType)
    {
        this.playerState = playerState;
        this.encounterInfo = encounterInfo;

        narrativeService.SwitchProvider(providerType);

        // Start the encounter mechanically
        StartEncounter(worldState, playerState, encounterInfo);

        // Create narrative context
        narrativeContext =
            new NarrativeContext(
                encounterInfo.LocationName.ToString(),
                encounterInfo.LocationSpotName.ToString(),
                encounterInfo.EncounterType,
                actionImplementation);

        // Generate introduction
        EncounterStatusModel status = GetEncounterStatusModel();

        string introduction = "introduction";

        if (_useAiNarrative)
        {
            string memoryContent = string.Empty;

            if (_useMemory)
            {
                memoryContent = await MemoryFileAccess.ReadFromMemoryFile();
            }

            introduction = await narrativeService.GenerateIntroductionAsync(
                narrativeContext,
                status,
                memoryContent);
        }

        // Get available choices
        GenerateChoices();
        List<IChoice> choices = GetCurrentChoices();
        List<ChoiceProjection> projections = choices.Select(ProjectChoice).ToList();

        // Create first narrative event
        NarrativeEvent firstNarrative = new NarrativeEvent(
            encounterState.CurrentTurn,
            introduction);

        narrativeContext.AddEvent(firstNarrative);

        // Generate choice descriptions
        Dictionary<IChoice, ChoiceNarrative> choiceDescriptions = null;
        if (_useAiNarrative)
        {
            choiceDescriptions = await narrativeService.GenerateChoiceDescriptionsAsync(
                narrativeContext,
                choices,
                projections,
                status);

            firstNarrative.SetAvailableChoiceDescriptions(choiceDescriptions);
        }

        // Return the narrative result
        return new NarrativeResult(
            introduction,
            choices,
            projections,
            choiceDescriptions,
            firstNarrative.ChoiceNarrative);
    }

    public async Task<NarrativeResult> ApplyChoiceWithNarrativeAsync(
        IChoice choice,
        ChoiceNarrative choiceDescription)
    {
        // Apply the choice
        ChoiceOutcome outcome = ApplyChoiceProjection(playerState, encounterInfo, choice);

        // Get status after the choice
        EncounterStatusModel newStatus = GetEncounterStatusModel();

        // Generate narrative for the reaction and new scene
        string narrative = "Continued Narrative";

        // If the encounter is over, return the outcome
        if (outcome.IsEncounterOver)
        {
            if (_useAiNarrative)
            {
                narrative = await narrativeService.GenerateEndingAsync(
                    narrativeContext,
                    choice,
                    choiceDescription,
                    outcome,
                    newStatus);

                if (_processStateChanges)
                {
                    await ProcessEncounterOutcome(narrative, outcome.Description, encounterState);
                }

                if (_useMemory)
                {
                    await UpdateMemoryFile(outcome, newStatus);
                }
            }

            NarrativeEvent narrativeEvent = GetNarrativeEvent(choice, choiceDescription, outcome, narrative);
            narrativeContext.AddEvent(narrativeEvent);

            NarrativeResult narrativeResultFinished = new(
                narrative,
                new List<IChoice>(),
                new List<ChoiceProjection>(),
                new Dictionary<IChoice, ChoiceNarrative>(),
                narrativeEvent.ChoiceNarrative);

            narrativeResultFinished.SetOutcome(outcome.Outcome);
            narrativeResultFinished.SetIsEncounterOver(outcome.IsEncounterOver);

            return narrativeResultFinished;
        }
        else
        {
            if (_useAiNarrative)
            {
                narrative = await narrativeService.GenerateEncounterNarrative(
                    narrativeContext,
                    choice,
                    choiceDescription,
                    outcome,
                    newStatus);
            }

            NarrativeEvent narrativeEvent = GetNarrativeEvent(choice, choiceDescription, outcome, narrative);
            narrativeContext.AddEvent(narrativeEvent);

            // Get the new choices and projections
            GenerateChoices();
            List<IChoice> newChoices = GetCurrentChoices();
            List<ChoiceProjection> newProjections = newChoices.Select(ProjectChoice).ToList();

            // Generate descriptive narratives for each choice
            Dictionary<IChoice, ChoiceNarrative> newChoiceDescriptions = null;

            if (_useAiNarrative)
            {
                newChoiceDescriptions = await narrativeService.GenerateChoiceDescriptionsAsync(
                    narrativeContext,
                    newChoices,
                    newProjections,
                    newStatus);
            }

            // Add the choice descriptions to the latest event
            narrativeEvent.ChoiceDescriptions.Clear();
            if (newChoiceDescriptions != null)
            {
                foreach (KeyValuePair<IChoice, ChoiceNarrative> kvp in newChoiceDescriptions)
                {
                    narrativeEvent.ChoiceDescriptions[kvp.Key] = kvp.Value;
                }
            }

            ChoiceNarrative lastChoiceNarrative = narrativeEvent.ChoiceNarrative;

            // Return the narrative result
            NarrativeResult narrativeResultOngoing = new(
                narrative,
                newChoices,
                newProjections,
                newChoiceDescriptions,
                narrativeEvent.ChoiceNarrative);

            return narrativeResultOngoing;
        }
    }

    private ChoiceOutcome ApplyChoiceProjection(PlayerState playerState, EncounterInfo encounterInfo, IChoice choice)
    {
        this.playerState = playerState;

        ChoiceProjection projection = encounterState.ApplyChoice(playerState, encounterInfo, choice);

        ChoiceOutcome outcome = new ChoiceOutcome(
            projection.MomentumGained,
            projection.PressureBuilt,
            projection.NarrativeDescription,
            projection.EncounterWillEnd,
            projection.ProjectedOutcome,
            projection.HealthChange,
            projection.ConcentrationChange,
            projection.ConfidenceChange);

        foreach (KeyValuePair<FocusTags, int> kvp in projection.FocusTagChanges)
        {
            outcome.FocusTagChanges[kvp.Key] = kvp.Value;
        }

        foreach (KeyValuePair<ApproachTags, int> kvp in projection.EncounterStateTagChanges)
        {
            outcome.EncounterStateTagChanges[kvp.Key] = kvp.Value;
        }

        outcome.NewlyActivatedTags.AddRange(projection.NewlyActivatedTags);
        outcome.DeactivatedTags.AddRange(projection.DeactivatedTags);

        return outcome;
    }


    private async Task UpdateMemoryFile(ChoiceOutcome outcome, EncounterStatusModel newStatus)
    {
        string oldMemory = await MemoryFileAccess.ReadFromMemoryFile();

        string memoryContent = await narrativeService.GenerateMemoryFileAsync(
            narrativeContext,
            outcome,
            newStatus,
            oldMemory
            );

        await MemoryFileAccess.WriteToMemoryFile(outcome, newStatus, memoryContent);
    }


    private NarrativeEvent GetNarrativeEvent(
        IChoice choice,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        string narrative)
    {
        NarrativeEvent narrativeEvent = new NarrativeEvent(
            encounterState.CurrentTurn - 1, // The turn counter increases after application
            narrative);

        narrativeEvent.SetChosenOption(choice);
        narrativeEvent.SetChoiceNarrative(choiceDescription);
        narrativeEvent.SetOutcome(outcome.Description);
        return narrativeEvent;
    }


    public EncounterStatusModel GetEncounterStatusModel()
    {
        return new EncounterStatusModel(
            currentTurn: encounterState.CurrentTurn,
            maxMomentum: encounterState.Location.ExceptionalThreshold,
            maxPressure: encounterState.Location.MaxPressure,
            successThreshold: encounterState.Location.StandardThreshold,
            maxTurns: encounterState.Location.TurnDuration,
            momentum: encounterState.Momentum,
            pressure: encounterState.Pressure,
            health: encounterState.PlayerState.Health,
            maxHealth: encounterState.PlayerState.MaxHealth,
            concentration: encounterState.PlayerState.Concentration,
            maxConcentration: encounterState.PlayerState.MaxConcentration,
            confidence: encounterState.PlayerState.Confidence,
            maxConfidence: encounterState.PlayerState.MaxConfidence,
            approachTags: encounterState.TagSystem.GetAllApproachTags(),
            focusTags: encounterState.TagSystem.GetAllFocusTags(),
            activeTagNames: encounterState.GetActiveTagsNames()
        );
    }

    public NarrativeContext GetNarrativeContext()
    {
        return narrativeContext;
    }

    public ChoiceProjection ProjectChoice(IChoice choice)
    {
        ChoiceProjection projection = encounterState.CreateChoiceProjection(choice);
        projection.NarrativeDescription = choice.Name + " " + choice.Description;
        return projection;
    }

    public void SwitchAIProvider(AIProviderType providerType)
    {
        if (narrativeService != null)
        {
            narrativeService.SwitchProvider(providerType);
        }
    }

    public AIProviderType GetCurrentAIProvider()
    {
        return narrativeService?.CurrentProvider ?? AIProviderType.OpenAI;
    }

    public string GetCurrentAIProviderName()
    {
        return narrativeService?.GetCurrentProviderName() ?? "None";
    }

    // Existing methods remain the same
    public List<IChoice> GetCurrentChoices()
    {
        return CurrentChoices;
    }

    public void GenerateChoices()
    {
        CurrentChoices = cardSelectionAlgorithm.SelectChoices(encounterState);
    }


    public async Task ProcessEncounterOutcome(string encounterNarrative, string encounterOutcome, EncounterState finalState)
    {
        // First, discover new entities
        await discoveryManager.ProcessEncounterForDiscoveries(encounterNarrative + "\n" + encounterOutcome);

        // Create encounter context
        EncounterContext context = new EncounterContext { /* populate from encounter state */ };

        // Get state change recommendations
        StateChangeRecommendations recommendations = await this.narrativeService.GenerateStateChanges(encounterOutcome, context);

        // Apply resource changes
        foreach (KeyValuePair<string, int> resource in recommendations.ResourceChanges)
        {
            resourceManager.ApplyResourceChanges(resource.Key, resource.Value);
        }

        // Apply relationship changes
        foreach (KeyValuePair<string, int> relationship in recommendations.RelationshipChanges)
        {
            relationshipManager.ApplyRelationshipChange(relationship.Key, relationship.Value);
        }

        // Apply skill experience
        foreach (string skill in recommendations.SkillExperienceGained)
        {
            playerState.ApplySkillExperience(skill);
        }

        // Record world events
        foreach (string worldEvent in recommendations.SuggestedWorldEvents)
        {
            worldState.WorldEvents.Add(worldEvent);
        }

        // Record interaction with location and characters
        discoveryManager.RecordLocationInteraction(context.Location.Name, encounterOutcome);
        foreach (Character character in context.PresentCharacters)
        {
            discoveryManager.RecordCharacterInteraction(character.Name, encounterOutcome);
        }
    }
}