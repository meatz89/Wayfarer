using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service that discovers available commands based on game context
/// Replaces the old ActionOption discovery system
/// </summary>
public class CommandDiscoveryService
{
    private readonly NPCRepository _npcRepository;
    private readonly LetterQueueManager _letterQueueManager;
    private readonly ConversationFactory _conversationFactory;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly MessageSystem _messageSystem;
    private readonly IGameRuleEngine _ruleEngine;
    private readonly CommandExecutor _commandExecutor;
    private readonly MarketManager _marketManager;
    private readonly ItemRepository _itemRepository;
    private readonly LocationRepository _locationRepository;
    private readonly GameConfiguration _gameConfiguration;
    private readonly StandingObligationManager _obligationManager;
    private readonly ConversationStateManager _conversationStateManager;
    private readonly NarrativeManager _narrativeManager;
    private readonly NarrativeRequirement _narrativeRequirement;
    private readonly LocationSpotRepository _spotRepository;
    private readonly RouteDiscoveryManager _routeDiscoveryManager;
    private readonly RouteRepository _routeRepository;
    private readonly InformationDiscoveryManager _informationDiscoveryManager;
    private readonly ITimeManager _timeManager;
    private readonly EndorsementManager _endorsementManager;

    public CommandDiscoveryService(
        NPCRepository npcRepository,
        LetterQueueManager letterQueueManager,
        ConversationFactory conversationFactory,
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem,
        IGameRuleEngine ruleEngine,
        CommandExecutor commandExecutor,
        MarketManager marketManager,
        ItemRepository itemRepository,
        LocationRepository locationRepository,
        GameConfiguration gameConfiguration,
        StandingObligationManager obligationManager,
        ConversationStateManager conversationStateManager,
        LocationSpotRepository spotRepository,
        RouteDiscoveryManager routeDiscoveryManager = null,
        NarrativeManager narrativeManager = null,
        NarrativeRequirement narrativeRequirement = null,
        RouteRepository routeRepository = null,
        InformationDiscoveryManager informationDiscoveryManager = null,
        ITimeManager timeManager = null,
        EndorsementManager endorsementManager = null)
    {
        _npcRepository = npcRepository;
        _letterQueueManager = letterQueueManager;
        _conversationFactory = conversationFactory;
        _tokenManager = tokenManager;
        _messageSystem = messageSystem;
        _ruleEngine = ruleEngine;
        _commandExecutor = commandExecutor;
        _marketManager = marketManager;
        _itemRepository = itemRepository;
        _locationRepository = locationRepository;
        _gameConfiguration = gameConfiguration;
        _obligationManager = obligationManager;
        _conversationStateManager = conversationStateManager;
        _narrativeManager = narrativeManager;
        _narrativeRequirement = narrativeRequirement;
        _spotRepository = spotRepository;
        _routeDiscoveryManager = routeDiscoveryManager;
        _routeRepository = routeRepository;
        _informationDiscoveryManager = informationDiscoveryManager;
        _timeManager = timeManager;
        _endorsementManager = endorsementManager;
    }

    /// <summary>
    /// Discovers all available commands at the current location
    /// </summary>
    public CommandDiscoveryResult DiscoverCommands(GameWorld gameWorld)
    {
        CommandDiscoveryResult result = new CommandDiscoveryResult();
        Player player = gameWorld.GetPlayer();

        if (player.CurrentLocationSpot == null)
        {
            return result; // No commands available without location
        }

        LocationSpot spot = player.CurrentLocationSpot;
        TimeBlocks currentTime = gameWorld.CurrentTimeBlock;

        // Discover NPC-based commands
        List<NPC> npcsHere = _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTime);
        
        // Filter NPCs based on narrative visibility if narrative manager is available
        if (_narrativeManager != null)
        {
            npcsHere = npcsHere.Where(npc => _narrativeManager.IsNPCVisible(npc.ID)).ToList();
        }
        
        foreach (NPC npc in npcsHere)
        {
            DiscoverNPCCommands(npc, player, gameWorld, result);
        }

        // Discover location-based commands
        DiscoverLocationCommands(spot, player, gameWorld, result);

        // Discover letter-related commands
        DiscoverLetterCommands(player, gameWorld, result);

        // Filter commands based on narrative restrictions if narrative manager is available
        if (_narrativeManager != null)
        {
            var allCommands = result.AllCommands.ToList();
            var filteredCommands = _narrativeManager.FilterCommands(allCommands);
            
            // Create new result with filtered commands
            var filteredResult = new CommandDiscoveryResult();
            foreach (var command in filteredCommands)
            {
                filteredResult.AddCommand(command);
            }
            return filteredResult;
        }

        return result;
    }

    private void DiscoverNPCCommands(NPC npc, Player player, GameWorld gameWorld, CommandDiscoveryResult result)
    {
        // Converse command - always available if NPC is present
        ConverseCommand converseCommand = new ConverseCommand(npc.ID, _conversationFactory, _npcRepository, _messageSystem, _conversationStateManager);
        CommandValidationResult converseValidation = converseCommand.CanExecute(gameWorld);

        result.AddCommand(new DiscoveredCommand
        {
            Command = converseCommand,
            Category = CommandCategory.Social,
            DisplayName = $"Talk with {npc.Name}",
            Description = $"Have a conversation with {npc.Name}",
            TimeCost = 1,
            StaminaCost = 0,
            CoinCost = 0,
            PotentialReward = "Information and relationship building",
            IsAvailable = converseValidation.IsValid,
            UnavailableReason = converseValidation.FailureReason,
            CanRemediate = converseValidation.CanBeRemedied,
            RemediationHint = converseValidation.RemediationHint
        });


        // Get current relationship for later commands
        Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(npc.ID);
        int currentTokens = npcTokens.Values.Sum();





        // Discover routes from this NPC
        if (_routeDiscoveryManager != null)
        {
            List<RouteDiscoveryOption> routeDiscoveries = _routeDiscoveryManager.GetDiscoveriesFromNPC(npc);
            
            foreach (var discovery in routeDiscoveries)
            {
                // Only show undiscovered routes
                if (!discovery.Route.IsDiscovered)
                {
                    DiscoverRouteCommand discoverCommand = new DiscoverRouteCommand(
                        discovery, 
                        _routeDiscoveryManager, 
                        _messageSystem);
                    CommandValidationResult discoverValidation = discoverCommand.CanExecute(gameWorld);
                    
                    // Determine token type for this route
                    ConnectionType tokenType = _routeDiscoveryManager.DetermineTokenTypeForRoute(
                        discovery.Route, 
                        discovery.Discovery, 
                        discovery.TeachingNPC);
                    
                    result.AddCommand(new DiscoveredCommand
                    {
                        Command = discoverCommand,
                        Category = CommandCategory.Social,
                        DisplayName = $"Learn route: {discovery.Route.Name}",
                        Description = $"Learn about {discovery.Route.Name} from {npc.Name}",
                        TimeCost = 1,
                        StaminaCost = 0,
                        CoinCost = 0,
                        PotentialReward = $"Discover route to {discovery.Route.Destination} (costs {discovery.Discovery.RequiredTokensWithNPC} tokens)",
                        IsAvailable = discoverValidation.IsValid,
                        UnavailableReason = discoverValidation.FailureReason,
                        CanRemediate = discoverValidation.CanBeRemedied,
                        RemediationHint = discoverValidation.RemediationHint
                    });
                }
            }
        }
    }

    private void DiscoverLocationCommands(LocationSpot spot, Player player, GameWorld gameWorld, CommandDiscoveryResult result)
    {
        // Travel to other spots in the same location
        var currentLocation = _locationRepository.GetCurrentLocation();
        var allSpots = _spotRepository.GetAllLocationSpots()
            .Where(s => s.LocationId == currentLocation.Id && s.SpotID != spot.SpotID)
            .ToList();
        
        // During narrative, filter visible spots
        if (_narrativeManager != null && _narrativeManager.HasActiveNarrative())
        {
            foreach (var narrativeId in _narrativeManager.GetActiveNarratives())
            {
                var currentStep = _narrativeManager.GetCurrentStep(narrativeId);
                if (currentStep != null && currentStep.VisibleSpots != null && currentStep.VisibleSpots.Any())
                {
                    // Only show spots explicitly marked as visible in the narrative
                    allSpots = allSpots.Where(s => currentStep.VisibleSpots.Contains(s.SpotID)).ToList();
                }
            }
        }
        
        foreach (var targetSpot in allSpots)
        {
            var travelCommand = new TravelToSpotCommand(targetSpot.SpotID, _locationRepository, _spotRepository, _messageSystem);
            var validation = travelCommand.CanExecute(gameWorld);
            
            result.AddCommand(new DiscoveredCommand
            {
                Command = travelCommand,
                Category = CommandCategory.Travel,
                DisplayName = $"Travel to {targetSpot.Name}",
                Description = "Move to another area",
                TimeCost = 0,
                StaminaCost = 1,
                CoinCost = 0,
                IsAvailable = validation.IsValid,
                UnavailableReason = validation.FailureReason,
                CanRemediate = validation.CanBeRemedied,
                RemediationHint = validation.RemediationHint
            });
        }
        
        // Rest commands - always available at any location
        int[] restOptions = { 1, 2, 4 }; // 1, 2, or 4 hour rest options
        int[] staminaRecovery = { 2, 4, 10 }; // Full recovery at 4 hours

        // During tutorial, only show the first rest option
        bool isTutorialActive = _narrativeManager != null && _narrativeManager.IsNarrativeActive("wayfarer_tutorial");
        int maxOptions = isTutorialActive ? 1 : restOptions.Length;

        for (int i = 0; i < maxOptions; i++)
        {
            RestCommand restCommand = new RestCommand(spot.SpotID, restOptions[i], staminaRecovery[i], _messageSystem);
            CommandValidationResult restValidation = restCommand.CanExecute(gameWorld);

            result.AddCommand(new DiscoveredCommand
            {
                Command = restCommand,
                Category = CommandCategory.Rest,
                DisplayName = $"Rest for {restOptions[i]} hour{(restOptions[i] > 1 ? "s" : "")}",
                Description = $"Recover {staminaRecovery[i]} stamina",
                TimeCost = restOptions[i],
                StaminaCost = 0,
                CoinCost = 0,
                PotentialReward = $"+{staminaRecovery[i]} stamina",
                IsAvailable = restValidation.IsValid,
                UnavailableReason = restValidation.FailureReason,
                CanRemediate = restValidation.CanBeRemedied,
                RemediationHint = restValidation.RemediationHint
            });
        }

        // Location-specific action commands



        // Observe location (always available)
        ObserveCommand observeCommand = new ObserveCommand(spot.SpotID, _npcRepository,
            _locationRepository,
            _messageSystem);
        CommandValidationResult observeValidation = observeCommand.CanExecute(gameWorld);

        result.AddCommand(new DiscoveredCommand
        {
            Command = observeCommand,
            Category = CommandCategory.Special,
            DisplayName = "Observe location",
            Description = "Study your surroundings for opportunities",
            TimeCost = 1,
            StaminaCost = 0,
            CoinCost = 0,
        });

        // Explore area (discover hidden routes)
        Location exploreLocation = _locationRepository.GetCurrentLocation();
        if (exploreLocation != null)
        {
            ExploreCommand exploreCommand = new ExploreCommand(
                exploreLocation.Id,
                exploreLocation.Tier,
                _routeRepository,
                _informationDiscoveryManager,
                _messageSystem,
                _timeManager);
            CommandValidationResult exploreValidation = exploreCommand.CanExecute(gameWorld);

            result.AddCommand(new DiscoveredCommand
            {
                Command = exploreCommand,
                Category = CommandCategory.Special,
                DisplayName = "Explore area",
                Description = exploreCommand.Description,
                TimeCost = exploreCommand.TimeCost,
                StaminaCost = 2,
                CoinCost = 0,
                PotentialReward = "May discover hidden routes",
                IsAvailable = exploreValidation.IsValid,
                UnavailableReason = exploreValidation.FailureReason,
                CanRemediate = exploreValidation.CanBeRemedied,
                RemediationHint = exploreValidation.RemediationHint
            });
        }

        // Patron funds (if player has patron)
        if (player.HasPatron)
        {
            PatronFundsCommand patronFundsCommand = new PatronFundsCommand(_tokenManager, _messageSystem,
                _gameConfiguration);
            CommandValidationResult patronValidation = patronFundsCommand.CanExecute(gameWorld);

            result.AddCommand(new DiscoveredCommand
            {
                Command = patronFundsCommand,
                Category = CommandCategory.Economic,
                DisplayName = "Request patron funds",
                Description = "Write to patron for financial assistance",
                TimeCost = 1,
                StaminaCost = 0,
                CoinCost = 0,
                PotentialReward = "30 coins",
            });
        }
        
        // Guild interactions (if at a guild location)
        if (_endorsementManager != null && IsGuildLocation(spot.LocationId))
        {
            var conversionOptions = _endorsementManager.GetAvailableSealConversions(spot.LocationId);
            foreach (var option in conversionOptions.Where(o => o.CanConvert))
            {
                ConvertEndorsementsCommand convertCommand = new ConvertEndorsementsCommand(
                    spot.LocationId,
                    option.TargetTier,
                    _endorsementManager,
                    _messageSystem,
                    _locationRepository);
                
                CommandValidationResult convertValidation = convertCommand.CanExecute(gameWorld);
                
                result.AddCommand(new DiscoveredCommand
                {
                    Command = convertCommand,
                    Category = CommandCategory.Special,
                    DisplayName = $"Convert to {option.TargetTier} Seal",
                    Description = $"Exchange {option.RequiredEndorsements} endorsements for guild seal",
                    TimeCost = 0,
                    StaminaCost = 0,
                    CoinCost = 0,
                    PotentialReward = $"{option.TargetTier} {option.SealType} Seal",
                    IsAvailable = convertValidation.IsValid,
                    UnavailableReason = convertValidation.FailureReason
                });
            }
        }
    }
    
    private bool IsGuildLocation(string locationId)
    {
        return locationId == "merchant_guild" || 
               locationId == "messenger_guild" || 
               locationId == "scholar_guild";
    }

    private void DiscoverLetterCommands(Player player, GameWorld gameWorld, CommandDiscoveryResult result)
    {
        // Check for letters that need collection
        foreach (Letter letter in player.LetterQueue.Where(l => l != null && l.State == LetterState.Accepted))
        {
            CollectLetterCommand collectCommand = new CollectLetterCommand(letter.Id, _letterQueueManager, _npcRepository, _messageSystem);
            CommandValidationResult collectValidation = collectCommand.CanExecute(gameWorld);

            NPC sender = _npcRepository.GetById(letter.SenderId);
            if (sender != null)
            {
                result.AddCommand(new DiscoveredCommand
                {
                    Command = collectCommand,
                    Category = CommandCategory.Letter,
                    DisplayName = $"Collect letter from {sender.Name}",
                    Description = $"Pick up the letter to {letter.RecipientName}",
                    TimeCost = 1,
                    StaminaCost = 0,
                    CoinCost = 0,
                });
            }
        }

        // Check for letter delivery at position 1
        Letter topLetter = player.LetterQueue[0];
        if (topLetter != null && topLetter.State == LetterState.Collected)
        {
            NPC recipient = _npcRepository.GetById(topLetter.RecipientId);
            if (recipient != null && recipient.IsAvailableAtLocation(player.CurrentLocationSpot?.SpotID))
            {
                DeliverLetterCommand deliverCommand = new DeliverLetterCommand(
                    topLetter,
                    _letterQueueManager,
                    _obligationManager,
                    _tokenManager,
                    _messageSystem
                );

                CommandValidationResult deliverValidation = deliverCommand.CanExecute(gameWorld);

                result.AddCommand(new DiscoveredCommand
                {
                    Command = deliverCommand,
                    Category = CommandCategory.Letter,
                    DisplayName = $"Deliver letter to {recipient.Name}",
                    Description = $"Complete delivery and earn {topLetter.Payment} coins",
                    TimeCost = 0, // Delivery is instant
                    StaminaCost = 0,
                    CoinCost = 0,
                    PotentialReward = $"{topLetter.Payment} coins + chance of token",
                });
            }
        }
    }
}

/// <summary>
/// Result of command discovery
/// </summary>
public class CommandDiscoveryResult
{
    private readonly List<DiscoveredCommand> _commands = new List<DiscoveredCommand>();

    public IReadOnlyList<DiscoveredCommand> AllCommands => _commands;

    public IReadOnlyList<DiscoveredCommand> AvailableCommands => _commands.Where(c => c.IsAvailable).ToList();

    public IReadOnlyList<DiscoveredCommand> UnavailableCommands => _commands.Where(c => !c.IsAvailable).ToList();

    public Dictionary<CommandCategory, List<DiscoveredCommand>> CommandsByCategory => _commands.GroupBy(c => c.Category)
                 .ToDictionary(g => g.Key, g => g.ToList());

    public void AddCommand(DiscoveredCommand command)
    {
        _commands.Add(command);
    }
}

/// <summary>
/// A discovered command with its metadata
/// </summary>
public class DiscoveredCommand
{
    public IGameCommand Command { get; init; }
    public CommandCategory Category { get; init; }
    public string DisplayName { get; init; }
    public string Description { get; init; }

    // Costs
    public int TimeCost { get; init; }
    public int StaminaCost { get; init; }
    public int CoinCost { get; init; }

    // Rewards
    public string PotentialReward { get; init; }

    // Availability
    public bool IsAvailable { get; init; }
    public string UnavailableReason { get; init; }
    public bool CanRemediate { get; init; }
    public string RemediationHint { get; init; }

    // UI helper - Generate a stable ID based on command type and properties
    public string UniqueId => GenerateStableId();
    
    private string GenerateStableId()
    {
        // For RestCommand, include location, hours, and stamina recovery
        if (Command is RestCommand)
        {
            // Replace spaces and special characters to make URL-safe
            var safeReward = PotentialReward?
                .Replace(" ", "_")
                .Replace("+", "plus")
                .Replace("-", "minus") ?? "0";
            return $"RestCommand_{TimeCost}h_{safeReward}";
        }
        
        // For ConverseCommand, use the NPC ID for stable identification
        if (Command is ConverseCommand converseCmd)
        {
            // Add debug logging to verify this is being used
            Console.WriteLine($"[DEBUG] Generating stable ID for ConverseCommand with NPC: {converseCmd.NpcId}");
            return $"ConverseCommand_{converseCmd.NpcId}";
        }
        
        // For other commands, use CommandId which is stable for the session
        return $"{Command.GetType().Name}_{Command.CommandId}";
    }
}

/// <summary>
/// Categories for organizing commands in UI
/// </summary>
public enum CommandCategory
{
    Social,      // Converse, Socialize
    Economic,    // Work, Trade
    Letter,      // Collect, Deliver
    Rest,        // Rest options
    Travel,      // Travel commands
    Special      // Patron requests, etc.
}