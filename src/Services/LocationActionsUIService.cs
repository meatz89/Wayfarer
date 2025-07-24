using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


/// <summary>
/// Service that provides UI data and handles UI actions for Location Commands
/// Bridges between UI and game logic using the Command pattern
/// </summary>
public class LocationActionsUIService
{
    private readonly GameWorld _gameWorld;
    private readonly CommandDiscoveryService _commandDiscovery;
    private readonly CommandExecutor _commandExecutor;
    private readonly MessageSystem _messageSystem;
    private readonly ITimeManager _timeManager;

    public LocationActionsUIService(
        GameWorld gameWorld,
        CommandDiscoveryService commandDiscovery,
        CommandExecutor commandExecutor,
        MessageSystem messageSystem,
        ITimeManager timeManager)
    {
        _gameWorld = gameWorld;
        _commandDiscovery = commandDiscovery;
        _commandExecutor = commandExecutor;
        _messageSystem = messageSystem;
        _timeManager = timeManager;
    }

    /// <summary>
    /// Get location actions view model for current location
    /// </summary>
    public LocationActionsViewModel GetLocationActionsViewModel()
    {
        Player player = _gameWorld.GetPlayer();
        LocationSpot currentSpot = player.CurrentLocationSpot;

        if (currentSpot == null)
        {
            return new LocationActionsViewModel
            {
                LocationName = "Unknown Location",
                CurrentTimeBlock = _timeManager.GetCurrentTimeBlock().ToString(),
                HoursRemaining = _timeManager.HoursRemaining,
                PlayerStamina = player.Stamina,
                PlayerCoins = player.Coins,
                ActionGroups = new List<ActionGroupViewModel>()
            };
        }

        // Discover available commands
        CommandDiscoveryResult discovery = _commandDiscovery.DiscoverCommands(_gameWorld);

        LocationActionsViewModel viewModel = new LocationActionsViewModel
        {
            LocationName = currentSpot.Name,
            CurrentTimeBlock = _timeManager.GetCurrentTimeBlock().ToString(),
            HoursRemaining = _timeManager.HoursRemaining,
            PlayerStamina = player.Stamina,
            PlayerCoins = player.Coins,
            ActionGroups = new List<ActionGroupViewModel>()
        };

        // Convert command categories to action groups
        foreach (KeyValuePair<CommandCategory, List<DiscoveredCommand>> categoryGroup in discovery.CommandsByCategory)
        {
            ActionGroupViewModel group = new ActionGroupViewModel
            {
                ActionType = categoryGroup.Key.ToString(),
                Actions = ConvertCommands(categoryGroup.Value)
            };

            viewModel.ActionGroups.Add(group);
        }

        return viewModel;
    }

    private List<ActionOptionViewModel> ConvertCommands(List<DiscoveredCommand> commands)
    {
        return commands.Select(cmd => new ActionOptionViewModel
        {
            Id = cmd.UniqueId,
            Description = cmd.DisplayName,
            NPCName = ExtractNPCName(cmd.DisplayName),

            // Costs
            TimeCost = cmd.TimeCost,
            StaminaCost = cmd.StaminaCost,
            CoinCost = cmd.CoinCost,

            // Affordability
            HasEnoughTime = cmd.TimeCost == 0 || _timeManager.HoursRemaining >= cmd.TimeCost,
            HasEnoughStamina = cmd.StaminaCost == 0 || _gameWorld.GetPlayer().Stamina >= cmd.StaminaCost,
            HasEnoughCoins = cmd.CoinCost == 0 || _gameWorld.GetPlayer().Coins >= cmd.CoinCost,

            // Rewards
            RewardsDescription = cmd.PotentialReward,

            // Availability
            IsAvailable = cmd.IsAvailable,
            UnavailableReasons = cmd.IsAvailable ? new List<string>() : new List<string> { cmd.UnavailableReason }
        }).ToList();
    }

    /// <summary>
    /// Execute a location command using the Command pattern
    /// </summary>
    public async Task<bool> ExecuteActionAsync(string commandId)
    {
        // Discover commands again to find the one to execute
        CommandDiscoveryResult discovery = _commandDiscovery.DiscoverCommands(_gameWorld);
        DiscoveredCommand commandToExecute = discovery.AllCommands.FirstOrDefault(c => c.UniqueId == commandId);

        if (commandToExecute == null)
        {
            _messageSystem.AddSystemMessage("Command not found", SystemMessageTypes.Danger);
            return false;
        }

        if (!commandToExecute.IsAvailable)
        {
            _messageSystem.AddSystemMessage(commandToExecute.UnavailableReason, SystemMessageTypes.Warning);
            return false;
        }

        // Execute through CommandExecutor
        CommandResult result = await _commandExecutor.ExecuteAsync(commandToExecute.Command);

        return result.IsSuccess;
    }

    private string ExtractNPCName(string displayName)
    {
        // Simple extraction - could be improved
        if (displayName.Contains(" with "))
        {
            int startIndex = displayName.IndexOf(" with ") + 6;
            return displayName.Substring(startIndex);
        }
        if (displayName.Contains(" for "))
        {
            int startIndex = displayName.IndexOf(" for ") + 5;
            return displayName.Substring(startIndex);
        }
        if (displayName.Contains(" from "))
        {
            int startIndex = displayName.IndexOf(" from ") + 6;
            return displayName.Substring(startIndex);
        }
        if (displayName.Contains(" to "))
        {
            int startIndex = displayName.IndexOf(" to ") + 4;
            return displayName.Substring(startIndex);
        }
        return null;
    }
}

/// <summary>
/// ViewModel for location actions display
/// </summary>
public class LocationActionsViewModel
{
    public string LocationName { get; init; }
    public string CurrentTimeBlock { get; init; }
    public int HoursRemaining { get; init; }
    public int PlayerStamina { get; init; }
    public int PlayerCoins { get; init; }

    public List<ActionGroupViewModel> ActionGroups { get; init; } = new();
}

/// <summary>
/// ViewModel for a group of actions
/// </summary>
public class ActionGroupViewModel
{
    public string ActionType { get; init; }
    public List<ActionOptionViewModel> Actions { get; init; } = new();
}

/// <summary>
/// ViewModel for an individual action option
/// </summary>
public class ActionOptionViewModel
{
    public string Id { get; init; }
    public string Description { get; init; }
    public string NPCName { get; init; }
    public string NPCProfession { get; init; }

    // Costs
    public int TimeCost { get; init; }
    public int StaminaCost { get; init; }
    public int CoinCost { get; init; }

    // Affordability
    public bool HasEnoughTime { get; init; }
    public bool HasEnoughStamina { get; init; }
    public bool HasEnoughCoins { get; init; }

    // Rewards
    public int StaminaReward { get; init; }
    public int CoinReward { get; init; }
    public string TokenType { get; init; }
    public int TokenReward { get; init; }
    public string RewardsDescription { get; init; }

    // Overall availability
    public bool IsAvailable { get; init; }
    public List<string> UnavailableReasons { get; init; } = new();
}