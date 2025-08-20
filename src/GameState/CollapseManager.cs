using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Game.MainSystem;
using Wayfarer.GameState;
using Wayfarer.GameState.Constants;

/// <summary>
/// Manages the collapse mechanic when player stamina reaches 0.
/// This is a categorical mechanic that applies to all stamina depletion.
/// </summary>
public class CollapseManager
{
    private readonly GameWorld _gameWorld;
    private readonly ITimeManager _timeManager;
    private readonly MessageSystem _messageSystem;
    private readonly RestManager _restManager;
    private readonly LocationRepository _locationRepository;
    private readonly ILogger<CollapseManager> _logger;
    private readonly Random _random = new Random();

    // Constants for collapse effects
    private const int COLLAPSE_TIME_HOURS = 4;
    private const int MIN_COINS_LOST = 1;
    private const int MAX_COINS_LOST_PERCENT = 30; // Lose up to 30% of coins
    private const int COLLAPSE_STAMINA_RECOVERY = 2; // Wake up with minimal stamina

    public CollapseManager(
        GameWorld gameWorld,
        ITimeManager timeManager,
        MessageSystem messageSystem,
        RestManager restManager,
        LocationRepository locationRepository,
        ILogger<CollapseManager> logger)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _timeManager = timeManager ?? throw new ArgumentNullException(nameof(timeManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _restManager = restManager ?? throw new ArgumentNullException(nameof(restManager));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Check if player has collapsed and handle the collapse if necessary.
    /// Should be called after any stamina modification.
    /// </summary>
    public bool CheckAndHandleCollapse()
    {
        Player player = _gameWorld.GetPlayer();

        if (player.Stamina > 0)
        {
            return false; // No collapse
        }

        _logger.LogInformation("Player collapsed due to exhaustion");
        HandleCollapse();
        return true;
    }

    /// <summary>
    /// Handle the collapse event when player stamina reaches 0.
    /// </summary>
    private void HandleCollapse()
    {
        Player player = _gameWorld.GetPlayer();
        LocationSpot? locationSpot = player.CurrentLocationSpot;
        Location? location = locationSpot != null ? _locationRepository.GetLocation(locationSpot.LocationId) : null;

        // Show collapse narrative
        _messageSystem.AddSystemMessage(
            "💫 Your vision blurs and darkness takes you...",
            SystemMessageTypes.Danger
        );

        _messageSystem.AddSystemMessage(
            $"You collapse from exhaustion in {locationSpot?.Name ?? location?.Name ?? "an unknown place"}!",
            SystemMessageTypes.Danger
        );

        // Time passes while unconscious
        _timeManager.AdvanceTime(COLLAPSE_TIME_HOURS);

        // Calculate coins lost (random amount based on current coins)
        int coinsLost = 0;
        if (player.Coins > 0)
        {
            int maxLoss = Math.Max(MIN_COINS_LOST, (player.Coins * MAX_COINS_LOST_PERCENT) / 100);
            coinsLost = _random.Next(MIN_COINS_LOST, maxLoss + 1);
            player.ModifyCoins(-coinsLost);
        }

        // Recover minimal stamina
        player.ModifyStamina(COLLAPSE_STAMINA_RECOVERY);

        // Generate location-specific wake up narrative
        string wakeUpNarrative = GenerateWakeUpNarrative(location, locationSpot, coinsLost);

        _messageSystem.AddSystemMessage(
            $"⏰ {COLLAPSE_TIME_HOURS} hours later...",
            SystemMessageTypes.Warning
        );

        _messageSystem.AddSystemMessage(
            wakeUpNarrative,
            SystemMessageTypes.Warning
        );

        if (coinsLost > 0)
        {
            _messageSystem.AddSystemMessage(
                $"💰 You discover {coinsLost} coins are missing from your purse!",
                SystemMessageTypes.Danger
            );
        }

        _messageSystem.AddSystemMessage(
            $"💪 You've recovered slightly (Stamina: {player.Stamina}/{player.MaxStamina})",
            SystemMessageTypes.Info
        );

        // Add a memory of the collapse
        player.AddMemory(
            $"collapse_day_{_gameWorld.CurrentDay}",
            $"Collapsed from exhaustion in {location?.Name ?? "unknown location"} and lost {coinsLost} coins",
            _gameWorld.CurrentDay,
            importance: 3,
            expirationDays: 7
        );

        _logger.LogInformation($"Player woke up after collapse. Lost {coinsLost} coins, recovered {COLLAPSE_STAMINA_RECOVERY} stamina");
    }

    /// <summary>
    /// Generate contextual wake up narrative based on location.
    /// </summary>
    private string GenerateWakeUpNarrative(Location location, LocationSpot locationSpot, int coinsLost)
    {
        // Base narrative
        string narrative = "You wake up groggy and disoriented";

        // Add location-specific details
        if (locationSpot != null)
        {
            // Check location spot type for specific narratives
            string spotName = locationSpot.Name.ToLower();
            List<string> spotProperties = locationSpot.GetCurrentProperties();

            if (spotName.Contains("tavern") || spotName.Contains("inn"))
            {
                narrative += ", slumped over a tavern table. The barkeep eyes you suspiciously";
            }
            else if (spotName.Contains("market") || spotProperties.Contains("market"))
            {
                narrative += " in a market alley. Your pockets feel lighter";
            }
            else if (spotName.Contains("church") || spotName.Contains("temple"))
            {
                narrative += " on a church pew. A priest watches over you with concern";
            }
            else if (spotName.Contains("alley") || spotName.Contains("street"))
            {
                narrative += " in a dangerous area. You're lucky to be alive";
            }
            else
            {
                narrative += ", lying on the cold ground";
            }
        }
        else if (location != null)
        {
            // Fallback to location-based narrative
            if (location.Id == "Thornwood")
            {
                narrative += " in the wilderness. Forest creatures have been through your belongings";
            }
            else if (location.Id == "Crossbridge")
            {
                narrative += " near the bridge. The sound of water fills your ears";
            }
            else if (location.Id == "Millbrook")
            {
                narrative += " by the roadside. A few villagers glance at you with pity";
            }
            else
            {
                narrative += " in an unfamiliar place";
            }
        }

        narrative += ".";

        // Add theft narrative if coins were lost
        if (coinsLost > 0)
        {
            if (coinsLost == 1)
            {
                narrative += " Someone has rifled through your purse while you were unconscious.";
            }
            else if (coinsLost < 5)
            {
                narrative += " Your purse feels noticeably lighter.";
            }
            else
            {
                narrative += " It's clear someone took advantage of your vulnerable state.";
            }
        }

        return narrative;
    }

    /// <summary>
    /// Check if player is at risk of collapse (low stamina warning).
    /// </summary>
    public bool IsAtRiskOfCollapse()
    {
        Player player = _gameWorld.GetPlayer();
        return player.Stamina <= 2 && player.Stamina > 0;
    }

    /// <summary>
    /// Get a warning message for low stamina.
    /// </summary>
    public string GetLowStaminaWarning()
    {
        Player player = _gameWorld.GetPlayer();

        if (player.Stamina == 1)
        {
            return "⚠️ You're on the verge of collapse! Rest immediately!";
        }
        else if (player.Stamina == 2)
        {
            return "⚠️ You're exhausted and struggling to continue.";
        }

        return null;
    }
}