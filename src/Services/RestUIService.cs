using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


/// <summary>
/// Service that provides UI data and handles UI actions for Rest UI
/// Bridges between UI and game logic using the Command pattern
/// </summary>
public class RestUIService
{
    private readonly GameWorld _gameWorld;
    private readonly RestManager _restManager;
    private readonly CommandDiscoveryService _commandDiscovery;
    private readonly CommandExecutor _commandExecutor;
    private readonly GameWorldManager _gameWorldManager;
    private readonly ItemRepository _itemRepository;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;

    public RestUIService(
        GameWorld gameWorld,
        RestManager restManager,
        CommandDiscoveryService commandDiscovery,
        CommandExecutor commandExecutor,
        GameWorldManager gameWorldManager,
        ItemRepository itemRepository,
        NPCRepository npcRepository,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld;
        _restManager = restManager;
        _commandDiscovery = commandDiscovery;
        _commandExecutor = commandExecutor;
        _gameWorldManager = gameWorldManager;
        _itemRepository = itemRepository;
        _npcRepository = npcRepository;
        _messageSystem = messageSystem;
    }

    /// <summary>
    /// Get rest view model for current location
    /// </summary>
    public RestViewModel GetRestViewModel()
    {
        RestViewModel viewModel = new RestViewModel
        {
            RestOptions = GetRestOptions(),
            LocationActions = GetLocationActions(),
            WaitOptions = GetWaitOptions()
        };

        return viewModel;
    }

    private List<RestOptionViewModel> GetRestOptions()
    {
        Player player = _gameWorld.GetPlayer();
        List<RestOption> restOptions = _restManager.GetAvailableRestOptions()
            .Where(o => o.StaminaRecovery > 0 && o.RestTimeHours >= 1)
            .ToList();

        return restOptions.Select(option =>
        {
            bool hasRequiredItem = string.IsNullOrEmpty(option.RequiredItem) ||
                                 player.Inventory.HasItem(option.RequiredItem);
            bool canAffordCoins = player.Coins >= option.CoinCost;
            bool isAvailable = canAffordCoins && hasRequiredItem;

            string unavailableReason = null;
            if (!canAffordCoins)
                unavailableReason = $"Need {option.CoinCost} coins";
            else if (!hasRequiredItem)
                unavailableReason = $"Need {option.RequiredItem}";

            return new RestOptionViewModel
            {
                Id = option.Id,
                Name = option.Name,
                Description = option.Name, // RestOption doesn't have Description, use Name
                StaminaRecovery = option.StaminaRecovery,
                CoinCost = option.CoinCost,
                TimeHours = option.RestTimeHours,
                RequiredItem = option.RequiredItem,
                CanAffordCoins = canAffordCoins,
                HasRequiredItem = hasRequiredItem,
                IsAvailable = isAvailable,
                UnavailableReason = unavailableReason
            };
        }).ToList();
    }

    private List<LocationActionViewModel> GetLocationActions()
    {
        // Discover commands and filter for Rest category
        CommandDiscoveryResult discovery = _commandDiscovery.DiscoverCommands(_gameWorld);
        List<DiscoveredCommand> restCommands = discovery.CommandsByCategory
            .Where(kvp => kvp.Key == CommandCategory.Rest)
            .SelectMany(kvp => kvp.Value)
            .ToList();

        return restCommands.Select(cmd =>
        {
            string npcName = ExtractNPCName(cmd.DisplayName);
            NPC npc = string.IsNullOrEmpty(npcName) ? null : _npcRepository.GetAllNPCs().FirstOrDefault(n => n.Name == npcName);

            return new LocationActionViewModel
            {
                Id = cmd.UniqueId,
                Description = cmd.Description,
                NPCName = npc?.Name,
                NPCProfession = npc?.Profession.ToString(),
                TimeCost = cmd.TimeCost,
                StaminaCost = cmd.StaminaCost,
                CoinCost = cmd.CoinCost,
                StaminaReward = ExtractStaminaReward(cmd.PotentialReward),
                IsAvailable = cmd.IsAvailable,
                UnavailableReason = cmd.UnavailableReason,
                CanBeRemedied = cmd.CanRemediate,
                RemediationHint = cmd.RemediationHint
            };
        }).ToList();
    }

    private List<WaitOptionViewModel> GetWaitOptions()
    {
        // Simple wait options - could be expanded
        return new List<WaitOptionViewModel>
        {
            new WaitOptionViewModel { Hours = 1, Description = "Wait 1 hour" },
            new WaitOptionViewModel { Hours = 2, Description = "Wait 2 hours" },
            new WaitOptionViewModel { Hours = 4, Description = "Wait 4 hours" }
        };
    }

    /// <summary>
    /// Execute a rest option
    /// </summary>
    public async Task<bool> ExecuteRestOptionAsync(string optionId)
    {
        RestOption option = _restManager.GetAvailableRestOptions().FirstOrDefault(o => o.Id == optionId);
        if (option == null)
        {
            _messageSystem.AddSystemMessage("Rest option not found", SystemMessageTypes.Danger);
            return false;
        }

        // Create rest command
        RestCommand restCommand = new RestCommand(
            _gameWorld.GetPlayer().CurrentLocationSpot?.SpotID,
            option.RestTimeHours,
            option.StaminaRecovery,
            _messageSystem
        );

        // Apply costs if any
        if (option.CoinCost > 0)
        {
            SpendCoinsCommand spendCoinsCommand = new SpendCoinsCommand(option.CoinCost, $"Rest at {option.Name}");
            CommandResult coinResult = await _commandExecutor.ExecuteAsync(spendCoinsCommand);
            if (!coinResult.IsSuccess)
            {
                return false;
            }
        }

        // Execute rest command
        CommandResult result = await _commandExecutor.ExecuteAsync(restCommand);
        return result.IsSuccess;
    }

    /// <summary>
    /// Execute a location action (rest command)
    /// </summary>
    public async Task<bool> ExecuteLocationActionAsync(string commandId)
    {
        return await _gameWorldManager.ExecuteCommandAsync(commandId);
    }

    /// <summary>
    /// Wait for specified hours
    /// </summary>
    public async Task<bool> WaitAsync(int hours)
    {
        // For now, just advance time - could be a proper WaitCommand
        AdvanceTimeCommand advanceTimeCommand = new AdvanceTimeCommand(hours, "Wait");
        CommandResult result = await _commandExecutor.ExecuteAsync(advanceTimeCommand);
        return result.IsSuccess;
    }

    private string ExtractNPCName(string displayName)
    {
        if (displayName.Contains(" with "))
        {
            int startIndex = displayName.IndexOf(" with ") + 6;
            return displayName.Substring(startIndex);
        }
        if (displayName.Contains(" at "))
        {
            int startIndex = displayName.IndexOf(" at ") + 4;
            return displayName.Substring(startIndex);
        }
        return null;
    }

    private int ExtractStaminaReward(string rewardDescription)
    {
        if (string.IsNullOrEmpty(rewardDescription))
            return 0;

        // Simple extraction from "+X stamina" format
        if (rewardDescription.Contains("stamina"))
        {
            string[] parts = rewardDescription.Split(' ');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (parts[i].StartsWith("+") && parts[i + 1].StartsWith("stamina"))
                {
                    if (int.TryParse(parts[i].Substring(1), out int stamina))
                    {
                        return stamina;
                    }
                }
            }
        }
        return 0;
    }
}

/// <summary>
/// ViewModel for rest UI
/// </summary>
public class RestViewModel
{
    public List<RestOptionViewModel> RestOptions { get; init; } = new();
    public List<LocationActionViewModel> LocationActions { get; init; } = new();
    public List<WaitOptionViewModel> WaitOptions { get; init; } = new();
}

/// <summary>
/// ViewModel for a rest option
/// </summary>
public class RestOptionViewModel
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public int StaminaRecovery { get; init; }
    public int CoinCost { get; init; }
    public int TimeHours { get; init; }
    public string RequiredItem { get; init; }
    public bool CanAffordCoins { get; init; }
    public bool HasRequiredItem { get; init; }
    public bool IsAvailable { get; init; }
    public string UnavailableReason { get; init; }
}

/// <summary>
/// ViewModel for a location action
/// </summary>
public class LocationActionViewModel
{
    public string Id { get; init; }
    public string Description { get; init; }
    public string NPCName { get; init; }
    public string NPCProfession { get; init; }
    public int TimeCost { get; init; }
    public int StaminaCost { get; init; }
    public int CoinCost { get; init; }
    public int StaminaReward { get; init; }
    public bool IsAvailable { get; init; }
    public string UnavailableReason { get; init; }
    public bool CanBeRemedied { get; init; }
    public string RemediationHint { get; init; }
}

/// <summary>
/// ViewModel for wait options
/// </summary>
public class WaitOptionViewModel
{
    public int Hours { get; init; }
    public string Description { get; init; }
}