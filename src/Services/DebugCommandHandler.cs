using System.Text;

/// <summary>
/// DebugCommandHandler - handles all debug/testing commands
/// Extracted from GameOrchestrator via COMPOSITION OVER INHERITANCE principle
/// Completely isolated from core game logic - can be removed in production builds
/// </summary>
public class DebugCommandHandler
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly RewardApplicationService _rewardApplicationService;

    public DebugCommandHandler(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        RewardApplicationService rewardApplicationService)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _rewardApplicationService = rewardApplicationService ?? throw new ArgumentNullException(nameof(rewardApplicationService));
    }

    /// <summary>
    /// Debug: Set player stat to specific level
    /// TWO PILLARS: Uses Consequence + ApplyConsequence for stat changes
    /// </summary>
    public async Task<bool> SetStatLevel(PlayerStatType statType, int level)
    {
        if (level < 0 || level > 10)
        {
            _messageSystem.AddSystemMessage($"Invalid stat level {level}. Must be 0-10.", SystemMessageTypes.Danger);
            return false;
        }

        Player player = _gameWorld.GetPlayer();

        int delta = statType switch
        {
            PlayerStatType.Insight => level - player.Insight,
            PlayerStatType.Rapport => level - player.Rapport,
            PlayerStatType.Authority => level - player.Authority,
            PlayerStatType.Diplomacy => level - player.Diplomacy,
            PlayerStatType.Cunning => level - player.Cunning,
            _ => 0
        };

        Consequence statChange = statType switch
        {
            PlayerStatType.Insight => new Consequence { Insight = delta },
            PlayerStatType.Rapport => new Consequence { Rapport = delta },
            PlayerStatType.Authority => new Consequence { Authority = delta },
            PlayerStatType.Diplomacy => new Consequence { Diplomacy = delta },
            PlayerStatType.Cunning => new Consequence { Cunning = delta },
            _ => new Consequence()
        };
        await _rewardApplicationService.ApplyConsequence(statChange, null);

        _messageSystem.AddSystemMessage($"Set {statType} to {level}", SystemMessageTypes.Success);
        return true;
    }

    /// <summary>
    /// Debug: Add points to a specific stat
    /// TWO PILLARS: Uses Consequence + ApplyConsequence for stat changes
    /// </summary>
    public async Task<bool> AddStatXP(PlayerStatType statType, int points)
    {
        if (points <= 0)
        {
            _messageSystem.AddSystemMessage($"Invalid points amount {points}. Must be positive.", SystemMessageTypes.Danger);
            return false;
        }

        Player player = _gameWorld.GetPlayer();

        Consequence statChange = statType switch
        {
            PlayerStatType.Insight => new Consequence { Insight = points },
            PlayerStatType.Rapport => new Consequence { Rapport = points },
            PlayerStatType.Authority => new Consequence { Authority = points },
            PlayerStatType.Diplomacy => new Consequence { Diplomacy = points },
            PlayerStatType.Cunning => new Consequence { Cunning = points },
            _ => new Consequence()
        };
        await _rewardApplicationService.ApplyConsequence(statChange, null);

        int newValue = statType switch
        {
            PlayerStatType.Insight => player.Insight,
            PlayerStatType.Rapport => player.Rapport,
            PlayerStatType.Authority => player.Authority,
            PlayerStatType.Diplomacy => player.Diplomacy,
            PlayerStatType.Cunning => player.Cunning,
            _ => 0
        };
        _messageSystem.AddSystemMessage($"Added {points} to {statType}. Now {newValue}", SystemMessageTypes.Success);

        return true;
    }

    /// <summary>
    /// Debug: Set all stats to a specific level
    /// </summary>
    public async Task SetAllStats(int level)
    {
        if (level < 0 || level > 10)
        {
            _messageSystem.AddSystemMessage($"Invalid stat level {level}. Must be 0-10.", SystemMessageTypes.Danger);
            return;
        }

        foreach (PlayerStatType statType in Enum.GetValues(typeof(PlayerStatType)))
        {
            await SetStatLevel(statType, level);
        }

        _messageSystem.AddSystemMessage($"All stats set to {level}", SystemMessageTypes.Success);
    }

    /// <summary>
    /// Debug: Display current stat values
    /// </summary>
    public string GetStatInfo()
    {
        Player player = _gameWorld.GetPlayer();
        StringBuilder statInfo = new StringBuilder();

        statInfo.AppendLine("=== Player Stats ===");
        statInfo.AppendLine($"Insight: {player.Insight}");
        statInfo.AppendLine($"Rapport: {player.Rapport}");
        statInfo.AppendLine($"Authority: {player.Authority}");
        statInfo.AppendLine($"Diplomacy: {player.Diplomacy}");
        statInfo.AppendLine($"Cunning: {player.Cunning}");

        return statInfo.ToString();
    }

    /// <summary>
    /// Debug: Grant resources (coins, health, etc.)
    /// TWO PILLARS: Delegates mutations to RewardApplicationService
    /// </summary>
    public async Task GiveResources(int coins = 0, int health = 0, int hunger = 0)
    {
        Player player = _gameWorld.GetPlayer();

        Consequence debugConsequence = new Consequence
        {
            Coins = coins,
            Health = health,
            Hunger = hunger
        };
        await _rewardApplicationService.ApplyConsequence(debugConsequence, null);

        if (coins != 0)
        {
            _messageSystem.AddSystemMessage($"Coins {(coins > 0 ? "+" : "")}{coins} (now {player.Coins})", SystemMessageTypes.Success);
        }

        if (health != 0)
        {
            _messageSystem.AddSystemMessage($"Health {(health > 0 ? "+" : "")}{health} (now {player.Health})", SystemMessageTypes.Success);
        }

        if (hunger != 0)
        {
            _messageSystem.AddSystemMessage($"Hunger {(hunger > 0 ? "+" : "")}{hunger} (now {player.Hunger})", SystemMessageTypes.Success);
        }
    }

    /// <summary>
    /// Debug: Teleport player to a specific location
    /// </summary>
    public void TeleportToLocation(string venueName, string locationName)
    {
        Player player = _gameWorld.GetPlayer();
        Location location = _gameWorld.Locations.FirstOrDefault(l => l.Name == locationName);

        if (location == null)
        {
            _messageSystem.AddSystemMessage($"location '{locationName}' not found", SystemMessageTypes.Warning);
            return;
        }

        if (!location.HexPosition.HasValue)
        {
            _messageSystem.AddSystemMessage($"location '{locationName}' has no HexPosition - cannot teleport", SystemMessageTypes.Warning);
            return;
        }

        Venue venue = _gameWorld.Venues.FirstOrDefault(l => l.Name == venueName);
        if (venue == null)
        {
            _messageSystem.AddSystemMessage($"Location '{venueName}' not found", SystemMessageTypes.Warning);
            return;
        }

        player.CurrentPosition = location.HexPosition.Value;

        _messageSystem.AddSystemMessage($"Teleported to {venue.Name} - {location.Name}", SystemMessageTypes.Success);
    }
}
