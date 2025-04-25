public class GameManager(
    GameState gameState,
    EncounterSystem encounterSystem,
    PostEncounterEvolutionSystem evolutionSystem,
    LocationSystem locationSystem,
    MessageSystem messageSystem,
    ActionFactory actionFactory,
    ActionRepository actionRepository,
    TravelManager travelManager,
    ActionGenerator actionGenerator,
    PlayerProgression playerProgression,
    OutcomeProcessor outcomeProcessor,
    IConfiguration configuration)
{
    public PlayerState playerState
    {
        get
        {
            return gameState.PlayerState;
        }
    }

    public WorldState worldState
    {
        get
        {
            return gameState.WorldState;
        }
    }

    private bool _useMemory = configuration.GetValue<bool>("useMemory");
    private bool _processStateChanges = configuration.GetValue<bool>("processStateChanges");

    public async Task StartGame()
    {
        ProcessPlayerArchetype();

        string startingLocationName = WorldLocationsContent.GetStartingLocation();
        Location startingLocation = await locationSystem.Initialize(startingLocationName);

        worldState.RecordLocationVisit(startingLocationName);
        travelManager.EndLocationTravel(startingLocationName);

        Location currentLocation = worldState.CurrentLocation;

        if (worldState.CurrentLocationSpot == null && locationSystem.GetLocationSpots(currentLocation.Name).Any() == true)
        {
            Console.WriteLine("Current location spot is null despite spots existing - manually setting");
            worldState.SetCurrentLocationSpot(locationSystem.GetLocationSpots(currentLocation.Name).First());
        }

        await UpdateState();

        Location? currentLoc = currentLocation;
        Console.WriteLine($"Game started at: {currentLoc?.Name}, Current spot: {worldState.CurrentLocationSpot?.Name}");
    }

    private void ProcessPlayerArchetype()
    {
        ArchetypeTypes archetype = playerState.Archetype;
        int XpBonusForArchetype = 300;

        switch (archetype)
        {
            case ArchetypeTypes.Knight:
                playerProgression.AddSkillExp(SkillTypes.Warfare, XpBonusForArchetype);
                break;
            case ArchetypeTypes.Courtier:
                playerProgression.AddSkillExp(SkillTypes.Diplomacy, XpBonusForArchetype);
                break;
            case ArchetypeTypes.Sage:
                playerProgression.AddSkillExp(SkillTypes.Scholarship, XpBonusForArchetype);
                break;
            case ArchetypeTypes.Forester:
                playerProgression.AddSkillExp(SkillTypes.Wilderness, XpBonusForArchetype);
                break;
            case ArchetypeTypes.Shadow:
                playerProgression.AddSkillExp(SkillTypes.Subterfuge, XpBonusForArchetype);
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

        Location location = locationSystem.GetLocation(action.Location);
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

    public async Task ProcessActionCompletion(ActionImplementation action)
    {
        gameState.ActionStateTracker.CompleteAction();
        await HandlePlayerMoving(action);

        outcomeProcessor.ProcessActionCosts(action);
        outcomeProcessor.ProcessActionYields(action);

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
        playerProgression.AddPlayerExp(xpAward);
        messageSystem.AddSystemMessage($"Gained {xpAward} experience points");

        // Grant skill XP based on encounter type
        SkillTypes skill = DetermineSkillForAction(result.ActionImplementation);
        int skillXp = xpAward; // or a fraction thereof
        playerProgression.AddSkillExp(skill, skillXp);
        messageSystem.AddSystemMessage($"Gained {skillXp} {skill} skill experience");
    }


    private async Task HandlePlayerMoving(ActionImplementation actionImplementation)
    {
        string location = actionImplementation.DestinationLocation;
        if (!string.IsNullOrWhiteSpace(location))
        {
            travelManager.EndLocationTravel(location);
            await UpdateState();
        }
        string locationSpot = actionImplementation.DestinationLocationSpot;
        if (!string.IsNullOrWhiteSpace(locationSpot))
        {
            await MoveToLocationSpot(locationSpot);
        }
    }

    public void CreateGlobalActions()
    {
        List<UserActionOption> userActions = new List<UserActionOption>();

        // Only add food consumption if player has food
        if (gameState.PlayerState.Food > 0)
        {
            ActionDefinition foodActionTemplate = actionRepository.GetAction(ActionNames.ConsumeFood.ToString());
            if (foodActionTemplate != null)
            {
                ActionImplementation consumeFoodAction = actionFactory.CreateActionFromTemplate(foodActionTemplate, string.Empty, string.Empty);

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

        gameState.ActionStateTracker.SetGlobalActions(userActions);
    }

    public async Task InitiateTravelToLocation(string locationName)
    {
        // Create travel action using travelManager
        ActionImplementation travelAction = travelManager.TravelToLocation(locationName, TravelMethods.Walking);

        // Create option with consistent structure
        UserActionOption travelOption = new UserActionOption(
            "Travel to " + locationName, false, travelAction,
            worldState.CurrentLocation.Name, worldState.CurrentLocationSpot.Name,
            null, worldState.CurrentLocation.Difficulty, null);

        // Use unified action execution
        await ExecuteAction(travelOption);
    }

    private async Task<List<UserActionOption>> CreateLocationSpotActions(Location location, LocationSpot locationSpot)
    {
        string? currentLocation = worldState.CurrentLocation?.Name;
        if (string.IsNullOrWhiteSpace(currentLocation)) return new List<UserActionOption>();

        IEnumerable<string> locationSpotActions = locationSpot.GetActionsForLevel(locationSpot.CurrentLevel);

        List<UserActionOption> options = new List<UserActionOption>();
        foreach (string locationSpotAction in locationSpotActions)
        {
            ActionDefinition actionTemplate = actionRepository.GetAction(locationSpotAction);

            locationSpot.RegisterActionDefinition(actionTemplate.Name);

            if (actionTemplate == null)
            {
                string actionId =
                    await actionGenerator.GenerateActionAndEncounter(
                    locationSpotAction,
                    locationSpot.Name,
                    location.Name);

                actionTemplate = actionRepository.GetAction(locationSpotAction);
            }

            ActionImplementation actionImplementation = actionFactory.CreateActionFromTemplate(actionTemplate, location.Name, locationSpot.Name);

            UserActionOption userActionOption =
                new UserActionOption(
                    actionImplementation.Id.ToString(),
                    locationSpot.IsClosed,
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
            .Where(c =>
            {
                return c.Location == locationId;
            })
            .ToList();

        // Find available opportunities
        List<Opportunity> opportunities = worldState.GetOpportunities()
            .Where(o =>
            {
                return o.Location == locationId && o.Status == "Available";
            })
            .ToList();

        // Get player's history with this location
        List<string> previousInteractions = new();

        LocationSpot? locationSpot = locationSystem.GetLocationSpot(
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

        EncounterManager encounterManager = await encounterSystem
            .GenerateEncounter(actionImplementation.Id, location, locationSpot, context, worldState, playerState, actionImplementation);

        List<UserEncounterChoiceOption> choiceOptions = GetUserEncounterChoiceOptions(encounterManager.EncounterResult);
        gameState.ActionStateTracker.SetEncounterChoiceOptions(choiceOptions);

        return encounterManager;
    }

    public async Task<EncounterResult> ExecuteEncounterChoice(UserEncounterChoiceOption choiceOption)
    {
        Location location = locationSystem.GetLocation(choiceOption.LocationName);

        string? currentLocation = worldState.CurrentLocation?.Name;
        if (string.IsNullOrWhiteSpace(currentLocation)) return null;

        EncounterResult encounterResult = await encounterSystem.ExecuteChoice(
            choiceOption.NarrativeResult,
            choiceOption.Choice);

        ActionResults currentEncounterResult = encounterResult.ActionResult;
        if (currentEncounterResult == ActionResults.Ongoing)
        {
            ProcessOngoingEncounter(encounterResult);
        }
        else
        {
            await ProcessEncounterNarrativeEnding(encounterResult);
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

    /// <summary>
    /// ONLY Encounter related outcome that is NOT ALSO included in basic actions
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public async Task ProcessEncounterNarrativeEnding(EncounterResult result)
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
        await evolutionSystem.IntegrateEncounterOutcome(evolutionResponse, worldState, playerState);
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
        List<CardDefinition> choices = encounterSystem.GetChoices();
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
        Location location = locationSystem.GetLocation(choiceOption.LocationName);

        // Execute the choice
        EncounterManager encounter = gameState.ActionStateTracker.GetCurrentEncounter();
        ChoiceProjection choiceProjection = encounterSystem.GetChoiceProjection(encounter, choiceOption.Choice);
        return choiceProjection;
    }

    public async Task MoveToLocationSpot(string locationSpotName)
    {
        Location location = gameState.WorldState.CurrentLocation;

        LocationSpot locationSpot = locationSystem.GetLocationSpot(location.Name, locationSpotName);
        gameState.WorldState.SetCurrentLocation(location, locationSpot);

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
        return locationSystem.GetAllLocations();
    }

    public List<string> GetConnectedLocations()
    {
        List<string> loc =
            locationSystem.GetAllLocations()
            .Where(x =>
            {
                return x != gameState.WorldState.CurrentLocation;
            })
            .Select(x =>
            {
                return x.Name;
            })
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
        foreach (Requirement requirement in action.ActionImplementation.Requirements)
        {
            if (!requirement.IsMet(gameState))
            {
                return false; // Requirement not met
            }
        }

        // Check if the action has been completed and is non-repeatable
        if (action.ActionImplementation.ActionType == ActionTypes.Encounter)
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

    public string GetTimeOfDay(int totalMinutes)
    {
        // Calculate hours (24-hour clock)
        int totalHours = (totalMinutes / 60) % 24;

        if (totalHours >= 5 && totalHours < 12) return "Morning";
        if (totalHours >= 12 && totalHours < 17) return "Afternoon";
        if (totalHours >= 17 && totalHours < 21) return "Evening";
        return "Night";
    }

    public List<UserEncounterChoiceOption> GetChoices()
    {
        List<UserEncounterChoiceOption> userEncounterChoiceOptions = encounterSystem.GetUserEncounterChoiceOptions();
        return userEncounterChoiceOptions;
    }

    public EncounterViewModel? GetEncounterViewModel()
    {
        EncounterManager encounterManager = encounterSystem.GetCurrentEncounter();
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

        List<UserActionOption> locationSpotActionOptions =
            await CreateLocationSpotActions(
                worldState.CurrentLocation,
                worldState.CurrentLocationSpot);

        gameState.ActionStateTracker.SetLocationSpotActions(locationSpotActionOptions);
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

    public ActionImplementation GetWaitAction()
    {
        ActionDefinition waitAction = new ActionDefinition("Wait", "Wait")
        {
            TimeCost = 1
        };

        ActionImplementation action = actionFactory
            .CreateActionFromTemplate(waitAction, string.Empty, string.Empty);

        return action;
    }
}