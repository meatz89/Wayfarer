using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Central rule engine that implements all game mechanics calculations.
/// All game rules are driven by configuration, not hard-coded values.
/// </summary>
public class GameRuleEngine : IGameRuleEngine
{
    private readonly GameConfiguration _config;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly NPCRepository _npcRepository;
    private readonly TimeManager _timeManager;

    public GameRuleEngine(
        GameConfiguration config,
        ConnectionTokenManager tokenManager,
        NPCRepository npcRepository,
        TimeManager timeManager)
    {
        _config = config;
        _tokenManager = tokenManager;
        _npcRepository = npcRepository;
        _timeManager = timeManager;
    }

    public int CalculateLeverage(string npcId, ConnectionType tokenType)
    {
        Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(npcId);
        int tokenBalance = npcTokens.GetValueOrDefault(tokenType, 0);

        // Negative tokens create leverage
        if (tokenBalance < 0)
        {
            return Math.Abs(tokenBalance) * _config.LetterQueue.LeverageMultiplier;
        }

        // High positive relationship reduces leverage
        if (tokenBalance >= _config.TokenEconomy.PremiumLetterThreshold)
        {
            return -1; // Reduce priority by 1
        }

        return 0;
    }

    public int CalculateLetterPosition(Letter letter, int npcTokenBalance)
    {
        // Get base position from configuration
        int basePosition = _config.LetterQueue.BasePositions.GetValueOrDefault(
            letter.TokenType,
            _config.LetterQueue.MaxQueueSize - 1
        );

        // Apply leverage
        int leverageAdjustment = 0;
        if (npcTokenBalance < 0)
        {
            leverageAdjustment = Math.Abs(npcTokenBalance) * _config.LetterQueue.LeverageMultiplier;
        }
        else if (npcTokenBalance >= _config.TokenEconomy.PremiumLetterThreshold)
        {
            leverageAdjustment = -1; // Good relationship reduces priority
        }

        // Calculate final position
        int targetPosition = basePosition - leverageAdjustment;

        // Clamp to valid range
        return Math.Max(1, Math.Min(_config.LetterQueue.MaxQueueSize, targetPosition));
    }

    public int CalculateSkipCost(int fromPosition, Letter letter)
    {
        int baseCost = (fromPosition - 1) * _config.LetterQueue.SkipCostPerPosition;

        // Could apply multipliers based on letter properties or obligations
        return baseCost;
    }

    public bool CanSkipToPosition(Letter letter, int targetPosition)
    {
        // Can't skip to an occupied position
        // Can't skip patron letters (identified by IsFromPatron)
        // Can't skip obligated letters
        return targetPosition >= 1 &&
               targetPosition <= _config.LetterQueue.MaxQueueSize &&
               !letter.IsFromPatron;
    }

    // Token economy
    public int GetTokenThresholdForCategory(LetterCategory category)
    {
        return category switch
        {
            LetterCategory.Basic => _config.TokenEconomy.BasicLetterThreshold,
            LetterCategory.Quality => _config.TokenEconomy.QualityLetterThreshold,
            LetterCategory.Premium => _config.TokenEconomy.PremiumLetterThreshold,
            _ => _config.TokenEconomy.BasicLetterThreshold
        };
    }

    public bool HasRelationshipForCategory(string npcId, LetterCategory category)
    {
        Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);
        int totalTokens = tokens.Values.Sum();
        int threshold = GetTokenThresholdForCategory(category);
        return totalTokens >= threshold;
    }

    public LetterCategory DetermineLetterCategory(int tokenCount)
    {
        if (tokenCount >= _config.TokenEconomy.PremiumLetterThreshold)
            return LetterCategory.Premium;
        if (tokenCount >= _config.TokenEconomy.QualityLetterThreshold)
            return LetterCategory.Quality;
        if (tokenCount >= _config.TokenEconomy.BasicLetterThreshold)
            return LetterCategory.Basic;
        return LetterCategory.None;
    }

    // Travel mechanics
    public int CalculateTravelStamina(RouteOption route)
    {
        int baseCost = _config.Travel.BaseStaminaCost;

        // Apply terrain modifiers
        foreach (TerrainCategory terrain in route.TerrainCategories)
        {
            string terrainName = terrain.ToString();
            if (_config.Travel.TerrainStaminaModifiers.TryGetValue(terrainName, out float modifier))
            {
                baseCost = (int)(baseCost * modifier);
            }
        }

        return baseCost;
    }

    public bool CanTravel(Player player, RouteOption route)
    {
        int staminaCost = CalculateTravelStamina(route);
        int hourCost = route.TravelTimeHours;

        return player.Stamina >= staminaCost &&
               _timeManager.HoursRemaining >= hourCost;
    }

    // Time management
    public TimeBlocks GetTimeBlock(int hour)
    {
        foreach ((string blockName, TimeBlockDefinition definition) in _config.Time.TimeBlocks)
        {
            if (hour >= definition.StartHour && hour < definition.EndHour)
            {
                return EnumParser.Parse<TimeBlocks>(blockName, "TimeBlock");
            }
        }

        // Handle wrap-around for night
        if (hour >= _config.Time.TimeBlocks["Night"].StartHour ||
            hour < _config.Time.TimeBlocks["Dawn"].StartHour)
        {
            return TimeBlocks.Night;
        }

        return TimeBlocks.Dawn; // Default
    }

    public int GetActiveHoursRemaining(int currentHour)
    {
        if (currentHour >= _config.Time.ActiveDayEndHour)
            return 0;

        return _config.Time.ActiveDayEndHour - currentHour;
    }

    public bool IsNPCAvailable(NPC npc, TimeBlocks timeBlock)
    {
        // Simple availability for now - can be expanded with NPC schedules
        return timeBlock switch
        {
            TimeBlocks.Night => npc.LetterTokenTypes.Contains(ConnectionType.Shadow),
            TimeBlocks.Dawn => npc.LetterTokenTypes.Contains(ConnectionType.Trust),
            _ => true // Most NPCs available during day
        };
    }

    // Stamina and recovery
    public int CalculateRestRecovery(string lodgingType)
    {
        return _config.Stamina.RecoveryByLodging.GetValueOrDefault(
            lodgingType,
            _config.Stamina.RecoveryRest
        );
    }

    public int CalculateMaxStamina(Player player)
    {
        // Could be modified by equipment, traits, etc.
        return _config.Stamina.MaxStamina;
    }

    // Debt and emergency actions
    public bool ShouldOfferEmergencyActions(Player player)
    {
        return player.Coins < _config.Debt.DebtThresholdForEmergencyActions;
    }

    public int CalculateDebtLeverage(int tokenBalance)
    {
        if (tokenBalance < 0)
        {
            return Math.Abs(tokenBalance) * _config.LetterQueue.LeverageMultiplier;
        }
        return 0;
    }

    // Letter payments and deadlines
    public (int min, int max) GetPaymentRangeForCategory(LetterCategory category)
    {
        return category switch
        {
            LetterCategory.Basic => (_config.LetterPayment.BasicMin, _config.LetterPayment.BasicMax),
            LetterCategory.Quality => (_config.LetterPayment.QualityMin, _config.LetterPayment.QualityMax),
            LetterCategory.Premium => (_config.LetterPayment.PremiumMin, _config.LetterPayment.PremiumMax),
            _ => (_config.LetterPayment.BasicMin, _config.LetterPayment.BasicMax)
        };
    }

    public int CalculateLateDeliveryPenalty(Letter letter, int daysLate)
    {
        return daysLate * _config.LetterPayment.LateDeliveryPenaltyPerDay;
    }

    public int CalculateChainLetterBonus(Letter letter)
    {
        if (!letter.IsChainLetter) return 0;

        return (int)(letter.Payment * (_config.QueueManagement.ChainLetterPaymentMultiplier - 1));
    }

    // Patron mechanics
    public bool ShouldGeneratePatronLetter(int daysSinceLastLetter)
    {
        if (daysSinceLastLetter < _config.Patron.MinDaysBetweenLetters)
            return false;

        int chance = Math.Min(
            _config.Patron.MaxChancePercent,
            _config.Patron.BaseChancePercent +
            (daysSinceLastLetter - _config.Patron.MinDaysBetweenLetters) * _config.Patron.ChanceIncreasePerDay
        );

        return new Random().Next(100) < chance;
    }

    public int DeterminePatronLetterPosition()
    {
        return new Random().Next(
            _config.Patron.PatronLetterMinPosition,
            _config.Patron.PatronLetterMaxPosition + 1
        );
    }

    // Helper method
    private NPC GetNPC(string npcId)
    {
        return string.IsNullOrEmpty(npcId) ? null : _npcRepository.GetById(npcId);
    }
}

/// <summary>
/// Result of executing an action
/// </summary>
public class ActionResult
{
    public bool Success { get; set; }
    public string NPCId { get; set; }

    // Resource changes
    public int StaminaChange { get; set; }
    public int CoinChange { get; set; }
    public int FocusChange { get; set; }
    public int HoursSpent { get; set; }

    // Token changes (NPCId, TokenType, Amount)
    public List<(string npcId, ConnectionType tokenType, int amount)> TokenChanges { get; set; } = new();

    // Items granted
    public List<string> ItemsGranted { get; set; } = new();

    // Narrative outcome
    public string NarrativeResult { get; set; }
}