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
        StandingObligationManager obligationManager)
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
        foreach (NPC npc in npcsHere)
        {
            DiscoverNPCCommands(npc, player, gameWorld, result);
        }

        // Discover location-based commands
        DiscoverLocationCommands(spot, player, gameWorld, result);

        // Discover letter-related commands
        DiscoverLetterCommands(player, gameWorld, result);

        return result;
    }

    private void DiscoverNPCCommands(NPC npc, Player player, GameWorld gameWorld, CommandDiscoveryResult result)
    {
        // Converse command - always available if NPC is present
        ConverseCommand converseCommand = new ConverseCommand(npc.ID, _conversationFactory, _npcRepository, _messageSystem);
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
        });

        // Borrow money command - if NPC offers loans (trade or shadow types)
        bool canLend = npc.LetterTokenTypes.Contains(ConnectionType.Trade) ||
                      npc.LetterTokenTypes.Contains(ConnectionType.Shadow);

        if (canLend)
        {
            BorrowMoneyCommand borrowCommand = new BorrowMoneyCommand(npc.ID, _npcRepository, _tokenManager, _messageSystem,
                _gameConfiguration);
            CommandValidationResult borrowValidation = borrowCommand.CanExecute(gameWorld);

            int loanAmount = npc.LetterTokenTypes.Contains(ConnectionType.Shadow) ? 30 : 20;
            ConnectionType tokenType = npc.LetterTokenTypes.FirstOrDefault();

            result.AddCommand(new DiscoveredCommand
            {
                Command = borrowCommand,
                Category = CommandCategory.Economic,
                DisplayName = $"Borrow money from {npc.Name}",
                Description = $"Request a loan of {loanAmount} coins",
                TimeCost = 1,
                StaminaCost = 0,
                CoinCost = 0,
                PotentialReward = $"{loanAmount} coins (-2 {tokenType} tokens)",
            });
        }
    }

    private void DiscoverLocationCommands(LocationSpot spot, Player player, GameWorld gameWorld, CommandDiscoveryResult result)
    {
        // Rest commands - always available at any location
        int[] restOptions = { 1, 2, 4 }; // 1, 2, or 4 hour rest options
        int[] staminaRecovery = { 2, 4, 10 }; // Full recovery at 4 hours

        for (int i = 0; i < restOptions.Length; i++)
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
            });
        }

        // Location-specific action commands

        // Gather resources (only at FEATURE locations)
        if (spot.Type == LocationSpotTypes.FEATURE)
        {
            GatherResourcesCommand gatherCommand = new GatherResourcesCommand(spot.SpotID,
                _itemRepository,
                _messageSystem);
            CommandValidationResult gatherValidation = gatherCommand.CanExecute(gameWorld);

            result.AddCommand(new DiscoveredCommand
            {
                Command = gatherCommand,
                Category = CommandCategory.Economic,
                DisplayName = "Gather resources",
                Description = "Search for materials, food, or medicine",
                TimeCost = 1,
                StaminaCost = 2,
                CoinCost = 0,
                PotentialReward = "1-3 resources",
            });
        }

        // Browse market (if available)
        BrowseCommand browseCommand = new BrowseCommand(spot.LocationId,
            _marketManager,
            _messageSystem);
        CommandValidationResult browseValidation = browseCommand.CanExecute(gameWorld);

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

    // UI helper
    public string UniqueId => $"{Command.GetType().Name}_{Command.GetHashCode()}";
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