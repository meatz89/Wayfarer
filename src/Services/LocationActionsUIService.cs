using System;
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
    private readonly NPCRepository _npcRepository;
    private readonly MarketManager _marketManager;

    public LocationActionsUIService(
        GameWorld gameWorld,
        CommandDiscoveryService commandDiscovery,
        CommandExecutor commandExecutor,
        MessageSystem messageSystem,
        ITimeManager timeManager,
        NPCRepository npcRepository,
        MarketManager marketManager)
    {
        _gameWorld = gameWorld;
        _commandDiscovery = commandDiscovery;
        _commandExecutor = commandExecutor;
        _messageSystem = messageSystem;
        _timeManager = timeManager;
        _npcRepository = npcRepository;
        _marketManager = marketManager;
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
        
        // Add closed services information
        AddClosedServicesInfo(viewModel, currentSpot);

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
    
    private void AddClosedServicesInfo(LocationActionsViewModel viewModel, LocationSpot currentSpot)
    {
        // Check for Letter Board availability
        if (_timeManager.GetCurrentTimeBlock() != TimeBlocks.Dawn)
        {
            ActionOptionViewModel letterBoardInfo = new ActionOptionViewModel
            {
                Id = "letter_board_closed",
                Description = "Visit Letter Board",
                IsAvailable = false,
                IsServiceClosed = true,
                NextAvailableTime = GetNextAvailableTime(TimeBlocks.Dawn),
                ServiceSchedule = "Available only at Dawn",
                UnavailableReasons = new List<string> { "Letter Board is closed. Only available during Dawn hours." }
            };
            
            // Add to Special category
            ActionGroupViewModel specialGroup = viewModel.ActionGroups.FirstOrDefault(g => g.ActionType == CommandCategory.Special.ToString());
            if (specialGroup == null)
            {
                specialGroup = new ActionGroupViewModel
                {
                    ActionType = CommandCategory.Special.ToString(),
                    Actions = new List<ActionOptionViewModel>()
                };
                viewModel.ActionGroups.Add(specialGroup);
            }
            specialGroup.Actions.Add(letterBoardInfo);
        }
        
        // Check for Market availability
        string marketStatus = _marketManager.GetMarketAvailabilityStatus(currentSpot.LocationId);
        if (!marketStatus.Contains("Market Open"))
        {
            List<NPC> allTraders = _marketManager.GetAllTraders(currentSpot.LocationId);
            if (allTraders.Any())
            {
                string schedule = GetTradersSchedule(allTraders);
                string nextAvailable = GetNextMarketAvailable(currentSpot.LocationId, allTraders);
                
                ActionOptionViewModel marketInfo = new ActionOptionViewModel
                {
                    Id = "market_closed",
                    Description = "Browse Market",
                    IsAvailable = false,
                    IsServiceClosed = true,
                    NextAvailableTime = nextAvailable,
                    ServiceSchedule = schedule,
                    UnavailableReasons = new List<string> { marketStatus }
                };
                
                // Add to Economic category
                ActionGroupViewModel economicGroup = viewModel.ActionGroups.FirstOrDefault(g => g.ActionType == CommandCategory.Economic.ToString());
                if (economicGroup == null)
                {
                    economicGroup = new ActionGroupViewModel
                    {
                        ActionType = CommandCategory.Economic.ToString(),
                        Actions = new List<ActionOptionViewModel>()
                    };
                    viewModel.ActionGroups.Add(economicGroup);
                }
                economicGroup.Actions.Add(marketInfo);
            }
        }
        
        // Check for missing NPCs and their schedules
        List<NPC> allNPCs = _npcRepository.GetNPCsForLocation(currentSpot.LocationId);
        List<NPC> currentNPCs = _npcRepository.GetNPCsForLocationSpotAndTime(currentSpot.SpotID, _timeManager.GetCurrentTimeBlock());
        List<NPC> missingNPCs = allNPCs.Where(npc => !currentNPCs.Any(c => c.ID == npc.ID)).ToList();
        
        foreach (NPC missingNPC in missingNPCs)
        {
            string schedule = GetNPCSchedule(missingNPC);
            string nextAvailable = GetNextNPCAvailable(missingNPC);
            
            // Add info about when this NPC will be available
            ActionOptionViewModel npcInfo = new ActionOptionViewModel
            {
                Id = $"npc_unavailable_{missingNPC.ID}",
                Description = $"Wait for {missingNPC.Name}",
                NPCName = missingNPC.Name,
                NPCProfession = missingNPC.Profession.ToString(),
                IsAvailable = false,
                IsServiceClosed = true,
                NextAvailableTime = nextAvailable,
                ServiceSchedule = schedule,
                UnavailableReasons = new List<string> { $"{missingNPC.Name} is not here right now." }
            };
            
            // Add to Social category
            ActionGroupViewModel socialGroup = viewModel.ActionGroups.FirstOrDefault(g => g.ActionType == CommandCategory.Social.ToString());
            if (socialGroup == null)
            {
                socialGroup = new ActionGroupViewModel
                {
                    ActionType = CommandCategory.Social.ToString(),
                    Actions = new List<ActionOptionViewModel>()
                };
                viewModel.ActionGroups.Add(socialGroup);
            }
            socialGroup.Actions.Add(npcInfo);
        }
    }
    
    private string GetNextAvailableTime(TimeBlocks targetTime)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        int currentHour = _timeManager.GetCurrentTimeHours();
        
        // Calculate hours until target time
        int hoursUntilTarget = CalculateHoursUntilTimeBlock(currentTime, targetTime, currentHour);
        
        if (hoursUntilTarget <= 0)
        {
            // It's available next day
            return "Available tomorrow at " + GetTimeBlockDisplayName(targetTime);
        }
        else if (hoursUntilTarget == 1)
        {
            return "Available in 1 hour";
        }
        else if (hoursUntilTarget < 12)
        {
            return $"Available in {hoursUntilTarget} hours";
        }
        else
        {
            return "Available tomorrow at " + GetTimeBlockDisplayName(targetTime);
        }
    }
    
    private string GetTradersSchedule(List<NPC> traders)
    {
        List<string> schedules = new List<string>();
        foreach (NPC trader in traders)
        {
            List<TimeBlocks> availableTimes = GetNPCAvailableTimes(trader);
            if (availableTimes.Any())
            {
                string timeList = string.Join(", ", availableTimes.Select(t => GetTimeBlockDisplayName(t)));
                schedules.Add($"{trader.Name}: {timeList}");
            }
        }
        return string.Join("; ", schedules);
    }
    
    private string GetNPCSchedule(NPC npc)
    {
        List<TimeBlocks> availableTimes = GetNPCAvailableTimes(npc);
        if (!availableTimes.Any())
        {
            return "Schedule unknown";
        }
        
        string timeList = string.Join(", ", availableTimes.Select(t => GetTimeBlockDisplayName(t)));
        return $"Available: {timeList}";
    }
    
    private List<TimeBlocks> GetNPCAvailableTimes(NPC npc)
    {
        List<TimeBlocks> availableTimes = new List<TimeBlocks>();
        TimeBlocks[] allTimes = new[] { TimeBlocks.Dawn, TimeBlocks.Morning, TimeBlocks.Afternoon, TimeBlocks.Evening, TimeBlocks.Night };
        
        foreach (TimeBlocks time in allTimes)
        {
            if (npc.IsAvailable(time))
            {
                availableTimes.Add(time);
            }
        }
        
        return availableTimes;
    }
    
    private string GetNextNPCAvailable(NPC npc)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        List<TimeBlocks> availableTimes = GetNPCAvailableTimes(npc);
        
        if (!availableTimes.Any())
        {
            return "Availability unknown";
        }
        
        // Find next available time
        TimeBlocks? nextTime = GetNextAvailableTimeBlock(currentTime, availableTimes);
        if (nextTime.HasValue)
        {
            return GetNextAvailableTime(nextTime.Value);
        }
        
        return "Available tomorrow";
    }
    
    private string GetNextMarketAvailable(string locationId, List<NPC> traders)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        
        // Find all times when at least one trader is available
        HashSet<TimeBlocks> marketTimes = new HashSet<TimeBlocks>();
        foreach (NPC trader in traders)
        {
            List<TimeBlocks> traderTimes = GetNPCAvailableTimes(trader);
            foreach (TimeBlocks time in traderTimes)
            {
                marketTimes.Add(time);
            }
        }
        
        if (!marketTimes.Any())
        {
            return "Market schedule unknown";
        }
        
        TimeBlocks? nextTime = GetNextAvailableTimeBlock(currentTime, marketTimes.ToList());
        if (nextTime.HasValue)
        {
            return GetNextAvailableTime(nextTime.Value);
        }
        
        return "Available tomorrow";
    }
    
    private TimeBlocks? GetNextAvailableTimeBlock(TimeBlocks current, List<TimeBlocks> availableTimes)
    {
        TimeBlocks[] timeOrder = new[] { TimeBlocks.Dawn, TimeBlocks.Morning, TimeBlocks.Afternoon, TimeBlocks.Evening, TimeBlocks.Night };
        int currentIndex = Array.IndexOf(timeOrder, current);
        
        // Check remaining times today
        for (int i = currentIndex + 1; i < timeOrder.Length; i++)
        {
            if (availableTimes.Contains(timeOrder[i]))
            {
                return timeOrder[i];
            }
        }
        
        // Check times tomorrow (starting from Dawn)
        for (int i = 0; i <= currentIndex; i++)
        {
            if (availableTimes.Contains(timeOrder[i]))
            {
                return timeOrder[i];
            }
        }
        
        return null;
    }
    
    private int CalculateHoursUntilTimeBlock(TimeBlocks current, TimeBlocks target, int currentHour)
    {
        // Map time blocks to hour ranges
        int targetStartHour = target switch
        {
            TimeBlocks.Dawn => 4,
            TimeBlocks.Morning => 8,
            TimeBlocks.Afternoon => 12,
            TimeBlocks.Evening => 17,
            TimeBlocks.Night => 20,
            _ => 0
        };
        
        if (targetStartHour > currentHour)
        {
            return targetStartHour - currentHour;
        }
        else
        {
            // Next day
            return (24 - currentHour) + targetStartHour;
        }
    }
    
    private string GetTimeBlockDisplayName(TimeBlocks timeBlock)
    {
        return timeBlock switch
        {
            TimeBlocks.Dawn => "Dawn (4-8 AM)",
            TimeBlocks.Morning => "Morning (8 AM-12 PM)",
            TimeBlocks.Afternoon => "Afternoon (12-5 PM)",
            TimeBlocks.Evening => "Evening (5-8 PM)",
            TimeBlocks.Night => "Night (8 PM-4 AM)",
            _ => timeBlock.ToString()
        };
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
    
    // Service availability info
    public bool IsServiceClosed { get; init; }
    public string NextAvailableTime { get; init; }
    public string ServiceSchedule { get; init; }
}