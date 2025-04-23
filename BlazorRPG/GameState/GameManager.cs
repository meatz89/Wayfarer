using System.Data;
public class GameManager
{
    public PlayerState playerState => gameState.PlayerState;
    public WorldState worldState => gameState.WorldState;

    public GameState gameState;
    public PostEncounterEvolutionSystem evolutionSystem { get; }
    public LocationCreationSystem locationCreationSystem { get; }
    public LocationSystem LocationSystem { get; }
    public ItemSystem ItemSystem { get; }
    public NarrativeService NarrativeService { get; }
    public MessageSystem MessageSystem { get; }
    public ActionFactory ActionFactory { get; }
    public ActionRepository ActionRepository { get; }
    public TravelManager TravelManager { get; }
    public ActionGenerator ActionGenerator { get; }
    public CharacterSystem CharacterSystem { get; }
    public OpportunitySystem OpportunitySystem { get; }
    public PlayerProgression PlayerProgression { get; }
    public CardRepository ChoiceRepository { get; }
    public YieldProcessor YieldProcessor { get; }
    public EncounterSystem EncounterSystem { get; }

    private bool _useMemory = false;
    private bool _processStateChanges = false;

    public GameManager(
        GameState gameState,
        PostEncounterEvolutionSystem postEncounterEvolutionService,
        LocationCreationSystem locationCreationSystem,
        EncounterSystem EncounterSystem,
        LocationSystem locationSystem,
        ItemSystem itemSystem,
        NarrativeService narrativeService,
        MessageSystem messageSystem,
        ActionFactory actionFactory,
        ActionRepository actionRepository,
        TravelManager travelManager,
        ActionGenerator actionGenerator,
        CharacterSystem characterSystem,
        OpportunitySystem opportunitySystem,
        PlayerProgression playerProgression,
        CardRepository choiceRepository,
        YieldProcessor yieldProcessor,
        IConfiguration configuration
        )
    {
        this.gameState = gameState;
        this.EncounterSystem = EncounterSystem;
        this.LocationSystem = locationSystem;
        this.ItemSystem = itemSystem;
        this.NarrativeService = narrativeService;
        this.MessageSystem = messageSystem;
        evolutionSystem = postEncounterEvolutionService;
        this.locationCreationSystem = locationCreationSystem;
        ActionFactory = actionFactory;
        ActionRepository = actionRepository;
        TravelManager = travelManager;
        ActionGenerator = actionGenerator;
        CharacterSystem = characterSystem;
        OpportunitySystem = opportunitySystem;
        PlayerProgression = playerProgression;
        ChoiceRepository = choiceRepository;
        YieldProcessor = yieldProcessor;
        _processStateChanges = configuration.GetValue<bool>("processStateChanges");
        _useMemory = configuration.GetValue<bool>("useMemory");
    }

    public async Task StartGame()
    {
        ProcessPlayerArchetype();

        Location startingLocation = await LocationSystem.Initialize(LocationSystem.StartingLocation);
        string startingLocationName = LocationSystem.StartingLocation;

        worldState.RecordLocationVisit(startingLocationName);
        TravelManager.TravelToLocation(startingLocationName, TravelMethods.Walking);
        await OnLocationArrival(startingLocationName);

        if (worldState.CurrentLocationSpot == null && worldState.CurrentLocation?.LocationSpots?.Any() == true)
        {
            Console.WriteLine("Current location spot is null despite spots existing - manually setting");
            worldState.SetCurrentLocationSpot(worldState.CurrentLocation.LocationSpots.First());
        }

        await UpdateState();

        Location? currentLoc = worldState.CurrentLocation;
        Console.WriteLine($"Game started at: {currentLoc?.Name}, Current spot: {worldState.CurrentLocationSpot?.Name}");
    }

    private void ProcessPlayerArchetype()
    {
        ArchetypeTypes archetype = playerState.Archetype;
        int XpBonusForArchetype = 300;

        switch (archetype)
        {
            case ArchetypeTypes.Knight:
                PlayerProgression.AddSkillExp(SkillTypes.Warfare, XpBonusForArchetype);
                break;
            case ArchetypeTypes.Courtier:
                PlayerProgression.AddSkillExp(SkillTypes.Diplomacy, XpBonusForArchetype);
                break;
            case ArchetypeTypes.Sage:
                PlayerProgression.AddSkillExp(SkillTypes.Scholarship, XpBonusForArchetype);
                break;
            case ArchetypeTypes.Forester:
                PlayerProgression.AddSkillExp(SkillTypes.Wilderness, XpBonusForArchetype);
                break;
            case ArchetypeTypes.Shadow:
                PlayerProgression.AddSkillExp(SkillTypes.Subterfuge, XpBonusForArchetype);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(archetype));
        }
    }

    public async Task<ActionImplementation> ExecuteAction(UserActionOption action)
    {
        ActionImplementation actionImplementation = action.ActionImplementation;

        // Track action count for progressive action yields
        worldState.IncrementActionCount(actionImplementation.Id);

        Location location = LocationSystem.GetLocation(action.Location);
        string locationSpot = action.LocationSpot;

        // Set current action in game state
        gameState.ActionStateTracker.SetCurrentUserAction(action);

        // Use our action classification system to determine execution path
        ActionExecutionType executionType = GetExecutionType(action.ActionImplementation);

        switch (executionType)
        {
            case ActionExecutionType.Encounter:
                EncounterManager encounterManager = await PrepareEncounter(actionImplementation);
                gameState.ActionStateTracker.SetActiveEncounter(encounterManager);

                break;

            case ActionExecutionType.Basic:
            default:
                await ProcessActionCompletion(actionImplementation);
                await UpdateState();
                break;
        }

        return actionImplementation;
    }

    public async Task ProcessActionCompletion(ActionImplementation actionImplementation)
    {
        gameState.ActionStateTracker.CompleteAction();

        ApplyActionOutcomes(actionImplementation);

        var skill = DetermineSkillForAction(actionImplementation);
        int skillXp = CalculateBasicActionSkillXP(actionImplementation);

        PlayerProgression.AddSkillExp(skill, skillXp);
        MessageSystem.AddSystemMessage($"Gained {skillXp} {skill} skill experience");

        await HandlePlayerMoving(actionImplementation);
        UpdateTime(actionImplementation.TimeCostHours);
        await UpdateState();
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

    private void GainExperience(EncounterResult result)
    {
        int xpAward;
        switch (result.NarrativeResult?.Outcome)
        {
            case EncounterOutcomes.Exceptional: xpAward = 50; break;
            case EncounterOutcomes.Standard: xpAward = 30; break;
            case EncounterOutcomes.Partial: xpAward = 15; break;
            default: xpAward = 5; break;
        }
        xpAward += 10;

        // Grant player XP level
        PlayerProgression.AddPlayerExp(xpAward);
        MessageSystem.AddSystemMessage($"Gained {xpAward} experience points");

        // Grant skill XP based on encounter type
        var skill = DetermineSkillForAction(result.ActionImplementation);
        int skillXp = xpAward; // or a fraction thereof
        PlayerProgression.AddSkillExp(skill, skillXp);
        MessageSystem.AddSystemMessage($"Gained {skillXp} {skill} skill experience");
    }

    private int CalculateBasicActionSkillXP(ActionImplementation action)
    {
        return action.Difficulty * 5;
    }


    private async Task HandlePlayerMoving(ActionImplementation actionImplementation)
    {
        string location = actionImplementation.DestinationLocation;
        if (!string.IsNullOrWhiteSpace(location))
        {
            await OnLocationArrival(location);
        }
        string locationSpot = actionImplementation.DestinationLocationSpot;
        if (!string.IsNullOrWhiteSpace(locationSpot))
        {
            await MoveToLocationSpot(locationSpot);
        }
    }

    private void UpdateTime(int timeCostHours)
    {
        int hours = timeCostHours;
        for (int i = 0; i < hours; i++)
        {
            gameState.TimeManager.AdvanceTime(1);
        }

        Location currentLocation = worldState.CurrentLocation;
        currentLocation.OnTimeChanged(worldState.WorldTime);

        foreach (Location location in worldState.Locations)
        {
            if (worldState.GetLocationVisitCount(location.Name) > 0)
            {
                EnvironmentalPropertyManager.UpdateLocationForTime(location, worldState.WorldTime);
            }
        }
    }

    public void CreateGlobalActions()
    {
        List<UserActionOption> userActions = new List<UserActionOption>();

        // Only add food consumption if player has food
        if (gameState.PlayerState.Food > 0)
        {
            ActionDefinition foodActionTemplate = ActionRepository.GetAction(ActionNames.ConsumeFood.ToString());
            if (foodActionTemplate != null)
            {
                ActionImplementation consumeFoodAction = ActionFactory.CreateActionFromTemplate(foodActionTemplate);

                // Check if requirements are met
                bool requirementsMet = consumeFoodAction.CanExecute(gameState);

                UserActionOption consumeFoodOption = new UserActionOption(
                    consumeFoodAction.Id.ToString(),
                    !requirementsMet,
                    consumeFoodAction,
                    gameState.WorldState.CurrentLocation?.Name ?? "Global",
                    gameState.WorldState.CurrentLocationSpot?.Name ?? "Global",
                    null,
                    0,
                    string.Empty);

                userActions.Add(consumeFoodOption);
            }
        }

        // Only add medicinal herbs consumption if player has herbs
        if (gameState.PlayerState.MedicinalHerbs > 0)
        {
            ActionDefinition herbsActionTemplate = ActionRepository.GetAction(ActionNames.ConsumeMedicinalHerbs.ToString());
            if (herbsActionTemplate != null)
            {
                ActionImplementation consumeHerbsAction = ActionFactory.CreateActionFromTemplate(herbsActionTemplate);

                // Check if requirements are met
                bool requirementsMet = consumeHerbsAction.CanExecute(gameState);

                UserActionOption consumeHerbsOption = new UserActionOption(
                    consumeHerbsAction.Id.ToString(),
                    !requirementsMet, // Disabled if requirements aren't met
                    consumeHerbsAction,
                    gameState.WorldState.CurrentLocation?.Name ?? "Global",
                    gameState.WorldState.CurrentLocationSpot?.Name ?? "Global",
                    null,
                    0,
                    string.Empty);

                userActions.Add(consumeHerbsOption);
            }
        }

        gameState.ActionStateTracker.SetGlobalActions(userActions);
    }

    public async Task InitiateTravelToLocation(string locationName)
    {
        // Create travel action using TravelManager
        ActionImplementation travelAction = TravelManager.TravelToLocation(locationName, TravelMethods.Walking);

        // Create option with consistent structure
        UserActionOption travelOption = new UserActionOption(
            "Travel to " + locationName, false, travelAction,
            worldState.CurrentLocation.Name, worldState.CurrentLocationSpot.Name,
            null, worldState.CurrentLocation.Difficulty, null);

        // Use unified action execution
        await ExecuteAction(travelOption);
    }

    private async Task<List<UserActionOption>> CreateActionsForLocationSpot(Location location, LocationSpot locationSpot)
    {
        string? currentLocation = worldState.CurrentLocation?.Name;
        if (string.IsNullOrWhiteSpace(currentLocation)) return new List<UserActionOption>();

        List<string> locationSpotActions = locationSpot.BaseActionIds.ToList();

        List<UserActionOption> options = new List<UserActionOption>();

        foreach (string locationSpotAction in locationSpotActions)
        {
            ActionDefinition actionTemplate = ActionRepository.GetAction(locationSpotAction);
            if (actionTemplate == null)
            {
                string actionId =
                    await ActionGenerator.GenerateActionAndEncounter(
                    locationSpotAction,
                    locationSpot.Name,
                    location.Name);

                actionTemplate = ActionRepository.GetAction(locationSpotAction);
            }

            ActionImplementation actionImplementation = ActionFactory.CreateActionFromTemplate(actionTemplate);

            UserActionOption userActionOption =
                new UserActionOption(
                    actionImplementation.Id.ToString(),
                    false,
                    actionImplementation,
                    locationSpot.LocationName,
                    locationSpot.Name,
                    default,
                    location.Difficulty,
                    string.Empty);

            bool requirementsMet = AreRequirementsMet(userActionOption);
            userActionOption = userActionOption with { IsDisabled = !requirementsMet };
            options.Add(userActionOption);
        }

        return options;
    }

    private async Task<EncounterManager> PrepareEncounter(ActionImplementation actionImplementation)
    {
        Location location = worldState.CurrentLocation;
        string locationId = location.Name;
        string locationName = location.Name;

        // Get time of day
        string timeOfDay = GetTimeOfDay(worldState.CurrentTimeMinutes);

        // Find characters at this location
        List<Character> presentCharacters = worldState.GetCharacters()
            .Where(c => c.Location == locationId)
            .ToList();

        // Find available opportunities
        List<Opportunity> opportunities = worldState.GetOpportunities()
            .Where(o => o.Location == locationId && o.Status == "Available")
            .ToList();

        // Get player's history with this location
        List<string> previousInteractions = new();

        LocationSpot? locationSpot = LocationSystem.GetLocationSpotForLocation(
            location.Name, worldState.CurrentLocationSpot.Name);

        // Create initial context with our new value system
        int playerLevel = playerState.Level;

        EncounterContext context = new EncounterContext()
        {
            ActionImplementation = actionImplementation,
            BasicActionType = actionImplementation.EncounterType,
            Location = location,
            LocationSpot = locationSpot,
        };

        EncounterManager encounterManager = await EncounterSystem
            .GenerateEncounter(actionImplementation.Id, location, locationSpot.Name, context, worldState, playerState, actionImplementation);

        List<UserEncounterChoiceOption> choiceOptions = GetUserEncounterChoiceOptions(encounterManager.EncounterResult);
        gameState.ActionStateTracker.SetEncounterChoiceOptions(choiceOptions);

        return encounterManager;
    }

    public async Task<EncounterResult> ExecuteEncounterChoice(UserEncounterChoiceOption choiceOption)
    {
        Location location = LocationSystem.GetLocation(choiceOption.LocationName);

        string? currentLocation = worldState.CurrentLocation?.Name;
        if (string.IsNullOrWhiteSpace(currentLocation)) return null;

        EncounterResult encounterResult = await EncounterSystem.ExecuteChoice(
            choiceOption.NarrativeResult,
            choiceOption.Choice);

        ActionResults currentEncounterResult = encounterResult.ActionResult;
        if (currentEncounterResult == ActionResults.Ongoing)
        {
            ProcessOngoingEncounter(encounterResult);
        }
        else
        {
            await ProcessEncounterOutcome(encounterResult);
        }

        return encounterResult;
    }

    private void ProcessOngoingEncounter(EncounterResult encounterResult)
    {
        if (!IsGameOver(gameState.PlayerState))
        {
            gameState.ActionStateTracker.EncounterResult = encounterResult;

            List<UserEncounterChoiceOption> choiceOptions = GetUserEncounterChoiceOptions(encounterResult);
            gameState.ActionStateTracker.SetEncounterChoiceOptions(choiceOptions);
        }
        else
        {
            gameState.ActionStateTracker.CompleteAction();
            return;
        }
    }

    private async Task OnLocationArrival(string targetLocationName)
    {
        string? currentLocation = worldState.CurrentLocation?.Name;
        Location targetLocation = gameState.WorldState.GetLocation(targetLocationName);

        if (targetLocation == null)
        {
            await UpdateState();
            return;
        }

        if (targetLocation.LocationSpots.Count > 0)
        {
            worldState.CurrentLocationSpot = targetLocation.LocationSpots[0];
        }

        if (string.IsNullOrWhiteSpace(currentLocation))
        {
            worldState.SetCurrentLocation(targetLocation);
            currentLocation = worldState.CurrentLocation?.Name;
        }

        bool isFirstVisit = worldState.IsFirstVisit(targetLocation.Name);
        if (isFirstVisit)
        {
            Location originLocation = worldState.GetLocation(currentLocation);
            int newLocationDepth = originLocation.Depth + 1;

            targetLocation =
                await locationCreationSystem.PopulateLocation(
                targetLocationName,
                originLocation.Name,
                newLocationDepth);

            worldState.RecordLocationVisit(targetLocation.Name);
        }

        await UpdateState();
    }

    /// <summary>
    /// ONLY Encounter related outcome that is NOT ALSO included in basic actions
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public async Task ProcessEncounterOutcome(EncounterResult result)
    {
        worldState.MarkEncounterCompleted(result.ActionImplementation.Id);

        gameState.ActionStateTracker.EncounterResult = result;

        NarrativeResult narrativeResult = result.NarrativeResult;
        string narrative = narrativeResult?.SceneNarrative;
        string outcome = narrativeResult?.Outcome.ToString();

        if (result.ActionResult == ActionResults.EncounterSuccess)
        {
            GainExperience(result);
        }

        if (_processStateChanges)
        {
            await ProcessPostEncounterEvolution(result, narrative, outcome);
        }
    }

    private async Task ProcessPostEncounterEvolution(EncounterResult result, string narrative, string outcome)
    {
        // If not a travel encounter, evolve the current location
        EncounterTypes basicActionType = result.ActionImplementation.EncounterType;

        Location currentLocation = worldState.GetLocation(result.NarrativeContext.LocationName);
        if (_useMemory)
        {
            await CreateMemoryRecord(result);
        }

        // Prepare the input
        PostEncounterEvolutionInput input = evolutionSystem.PreparePostEncounterEvolutionInput(narrative, outcome);

        // Process world evolution
        PostEncounterEvolutionResult evolutionResponse = await evolutionSystem.ProcessEncounterOutcome(result.NarrativeContext, input, result);

        // Store the evolution response in the result
        result.PostEncounterEvolution = evolutionResponse;

        // Update world state
        await evolutionSystem.IntegrateEncounterOutcome(evolutionResponse, worldState, LocationSystem, playerState);
    }

    private async Task CreateMemoryRecord(EncounterResult encounterResult)
    {
        string oldMemory = await MemoryFileAccess.ReadFromMemoryFile();

        // Create memory entry
        MemoryConsolidationInput memoryInput = new MemoryConsolidationInput { OldMemory = oldMemory };

        string currentLocation = worldState.CurrentLocation.Name;
        if (string.IsNullOrWhiteSpace(currentLocation)) return;

        string memoryEntry = await evolutionSystem.ConsolidateMemory(encounterResult.NarrativeContext, memoryInput);

        string location = encounterResult.NarrativeContext.LocationName;
        string locationSpot = encounterResult.NarrativeContext.locationSpotName;
        string actionName = encounterResult.ActionImplementation.Id;
        string goal = encounterResult.ActionImplementation.Goal;

        string title = $"{location} - {locationSpot}, {actionName} - {goal}" + Environment.NewLine;

        string memoryEntryToWrite = title + memoryEntry;

        await MemoryFileAccess.WriteToMemoryFile(memoryEntryToWrite);
    }

    public List<UserEncounterChoiceOption> GetUserEncounterChoiceOptions(EncounterResult encounterResult)
    {
        NarrativeResult narrativeResult = encounterResult.NarrativeResult;
        List<CardDefinition> choices = EncounterSystem.GetChoices();
        List<UserEncounterChoiceOption> choiceOptions = new List<UserEncounterChoiceOption>();

        int i = 0;
        foreach (CardDefinition choice in choices)
        {
            i++;
            NarrativeContext narrativeContext = encounterResult.NarrativeContext;

            UserEncounterChoiceOption option = new UserEncounterChoiceOption(
                i,
                choice.GetDetails(),
                "Narrative",
                narrativeContext.LocationName,
                "locationSpotName",
                encounterResult,
                narrativeResult,
                choice);

            choiceOptions.Add(option);
        }

        return choiceOptions;
    }

    public ChoiceProjection GetChoicePreview(UserEncounterChoiceOption choiceOption)
    {
        Location location = LocationSystem.GetLocation(choiceOption.LocationName);

        // Execute the choice
        EncounterManager encounter = gameState.ActionStateTracker.GetCurrentEncounter();
        ChoiceProjection choiceProjection = EncounterSystem.GetChoiceProjection(encounter, choiceOption.Choice);
        return choiceProjection;
    }

    public async Task MoveToLocationSpot(string locationSpotName)
    {
        string locationName = gameState.WorldState.CurrentLocation.Name;

        LocationSpot locationSpot = LocationSystem.GetLocationSpotForLocation(locationName, locationSpotName);
        gameState.WorldState.SetCurrentLocationSpot(locationSpot);

        await UpdateState();
    }

    private bool IsGameOver(PlayerState player)
    {
        if (player.Health <= 0) return true;
        if (player.Concentration <= 0) return true;
        if (player.Confidence <= 0) return true;

        return false;
    }

    public List<Location> GetPlayerKnownLocations()
    {
        return LocationSystem.GetKnownLocations();
    }

    public List<string> GetConnectedLocations()
    {
        List<string> loc =
            LocationSystem.GetKnownLocations()
            .Where(x => x != gameState.WorldState.CurrentLocation)
            .Select(x => x.Name)
            .ToList();

        return loc;
    }

    public bool CanTravelTo(string destinationName)
    {
        if (worldState.CurrentLocation == null)
            return false;

        // Check if locations are directly connected
        return worldState.CurrentLocation.ConnectedTo?.Contains(destinationName) ?? false;
    }

    public bool CanMoveToSpot(string locationSpotName)
    {
        return true;
    }

    public bool AreRequirementsMet(UserActionOption action)
    {
        // Check time window constraints
        List<TimeWindows> timeWindows = action.ActionImplementation.TimeWindows;
        bool anyTimeWindow = timeWindows != null && timeWindows.Any();
        if (anyTimeWindow && !timeWindows.Contains(gameState.WorldState.WorldTime))
        {
            return false; // Not available during current time window
        }

        // Check if the action has been completed and is non-repeatable
        if (!action.ActionImplementation.IsRepeatable)
        {
            string encounterId = action.ActionId;
            if (gameState.WorldState.IsEncounterCompleted(encounterId))
            {
                return false; // Already completed this non-repeatable action
            }
        }

        // Continue with existing requirement checks
        return action.ActionImplementation.CanExecute(gameState);
    }

    public bool AdvanceTime(int inHours)
    {
        bool daySkip = false;
        // Advance the current time
        if (gameState.WorldState.CurrentTimeInHours + inHours > 24)
        {
            daySkip = true;
        }

        gameState.WorldState.CurrentTimeInHours = (gameState.WorldState.CurrentTimeInHours + inHours) % 24;

        const int timeWindowsPerDay = 4;
        const int hoursPerTimeWindow = 6;
        int timeWindow = (gameState.WorldState.CurrentTimeInHours / hoursPerTimeWindow) % timeWindowsPerDay;

        gameState.WorldState.DetermineCurrentTimeWindow(timeWindow);

        if (daySkip)
        {
        }

        return true;
    }


    public string GetTimeOfDay(int totalMinutes)
    {
        // Calculate hours (24-hour clock)
        int totalHours = (totalMinutes / 60) % 24;

        if (totalHours >= 5 && totalHours < 12) return "Morning";
        if (totalHours >= 12 && totalHours < 17) return "Afternoon";
        if (totalHours >= 17 && totalHours < 21) return "Evening";
        return "Night";
    }

    private async Task CreateLocationActions(Location currentLocation)
    {
        worldState.SetCurrentLocation(currentLocation);

        LocationSpot currentLocationSpot = worldState.CurrentLocationSpot;
        List<UserActionOption> locationSpotActionOptions = await CreateActionsForLocationSpot(currentLocation, currentLocationSpot);
        gameState.ActionStateTracker.SetLocationSpotActions(locationSpotActionOptions);
    }

    public List<UserEncounterChoiceOption> GetChoices()
    {
        List<UserEncounterChoiceOption> userEncounterChoiceOptions = EncounterSystem.GetUserEncounterChoiceOptions();
        return userEncounterChoiceOptions;
    }

    public EncounterViewModel? GetEncounterViewModel()
    {
        EncounterManager encounterManager = EncounterSystem.GetCurrentEncounter();
        List<UserEncounterChoiceOption> userEncounterChoiceOptions = GetChoices();

        if (encounterManager == null)
        {
            EncounterViewModel encounterViewModel = new();
            encounterViewModel.CurrentEncounter = encounterManager;
            encounterViewModel.CurrentChoices = userEncounterChoiceOptions;
            encounterViewModel.ChoiceSetName = "Current Situation";
        }
        else
        {
            EncounterState state = encounterManager.EncounterState;

            EncounterViewModel model = new EncounterViewModel();
            model.CurrentEncounter = encounterManager;
            model.CurrentChoices = userEncounterChoiceOptions;
            model.ChoiceSetName = "Current Situation";
            model.State = state;
            model.EncounterResult = encounterManager.EncounterResult;
            return model;
        }

        return null;
    }

    public ActionExecutionType GetExecutionType(ActionImplementation action)
    {
        if (action.ActionType == ActionTypes.Encounter)
        {
            return ActionExecutionType.Encounter;
        }

        // Basic action
        return ActionExecutionType.Basic;
    }

    public async Task UpdateState()
    {
        gameState.ActionStateTracker.ClearCurrentUserAction();
        CreateGlobalActions();
        await CreateLocationActions(worldState.CurrentLocation);
    }

    public EncounterResult GetEncounterResultFor(ActionImplementation actionImplementation)
    {
        return new EncounterResult()
        {
            ActionImplementation = actionImplementation,
            ActionResult = ActionResults.EncounterSuccess,
            EncounterEndMessage = "Success",
            NarrativeResult = null,
            NarrativeContext = null
        };
    }

    public void ApplyActionOutcomes(ActionImplementation action)
    {
        ProcessYields(action);

        UnlockCards();

        foreach (Outcome cost in action.Costs)
        {
            cost.Apply(gameState);
            MessageSystem.AddOutcome(cost);
        }

        foreach (Outcome reward in action.Rewards)
        {
            reward.Apply(gameState);
            MessageSystem.AddOutcome(reward);
        }
    }

    private void ProcessYields(ActionImplementation actionImplementation)
    {
        YieldProcessor.ProcessActionYields(actionImplementation, gameState);
    }

    private void UnlockCards()
    {
        foreach (CardDefinition card in ChoiceRepository.GetAll())
        {
            if (IsCardUnlocked(card, playerState.PlayerSkills))
            {
                playerState.UnlockCard(card);
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

}