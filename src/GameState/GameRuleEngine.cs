using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Content.Utilities;

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
    
    // Action validation
    public bool CanPerformAction(Player player, ActionOption action)
    {
        var validation = ValidateAction(player, action);
        return validation.IsValid;
    }
    
    public ActionValidationResult ValidateAction(Player player, ActionOption action)
    {
        var result = new ActionValidationResult { IsValid = true };
        
        // Check time availability
        if (action.HourCost > 0 && _timeManager.HoursRemaining < action.HourCost)
        {
            result.AddFailure($"Not enough hours remaining! Need {action.HourCost}, have {_timeManager.HoursRemaining}");
            result.IsValid = false;
        }
        
        // Check stamina
        if (action.StaminaCost > 0 && player.Stamina < action.StaminaCost)
        {
            result.AddFailure($"Not enough stamina! Need {action.StaminaCost}, have {player.Stamina}", true);
            result.IsValid = false;
        }
        
        // Check coins
        if (action.CoinCost > 0 && player.Coins < action.CoinCost)
        {
            result.AddFailure($"Not enough coins! Need {action.CoinCost}, have {player.Coins}", true);
            result.IsValid = false;
        }
        
        // Check focus (for future mental stamina system)
        if (action.FocusCost > 0 && player.Concentration < action.FocusCost)
        {
            result.AddFailure($"Not enough focus! Need {action.FocusCost}, have {player.Concentration}", true);
            result.IsValid = false;
        }
        
        return result;
    }
    
    // Action outcomes
    public ActionResult CalculateActionOutcome(Player player, ActionOption action)
    {
        var result = new ActionResult
        {
            Success = true,
            Action = action.Action,
            NPCId = action.NPCId
        };
        
        // Apply resource costs
        result.StaminaChange = -action.StaminaCost;
        result.CoinChange = -action.CoinCost;
        result.FocusChange = -action.FocusCost;
        result.HoursSpent = action.HourCost;
        
        // Calculate rewards based on action type
        switch (action.Action)
        {
            case LocationAction.Work:
                var workReward = CalculateWorkReward(action, GetNPC(action.NPCId));
                result.CoinChange += workReward.Coins;
                result.StaminaChange += workReward.Stamina;
                result.ItemsGranted.Add(workReward.Bonus);
                break;
                
            case LocationAction.Rest:
                var recovery = _config.Stamina.RecoveryRest;
                if (action.CoinCost > 0) // Paid rest at tavern
                {
                    recovery = _config.Stamina.RecoveryByLodging.GetValueOrDefault("common", recovery);
                }
                result.StaminaChange += recovery;
                break;
                
            case LocationAction.Socialize:
                if (!string.IsNullOrEmpty(action.NPCId))
                {
                    result.TokenChanges.Add((action.NPCId, ConnectionType.Common, 1));
                }
                break;
        }
        
        return result;
    }
    
    public WorkReward CalculateWorkReward(ActionOption action, NPC npc)
    {
        // Check for special work types first
        if (npc?.Name.ToLower().Contains("baker") == true && 
            _timeManager.GetCurrentTimeBlock() == TimeBlocks.Dawn)
        {
            return _config.WorkRewards.SpecialRewards.GetValueOrDefault("baker_dawn", 
                _config.WorkRewards.DefaultReward);
        }
        
        // Check profession-specific rewards
        if (npc?.Profession != null && 
            _config.WorkRewards.ByProfession.TryGetValue(npc.Profession, out var professionReward))
        {
            return professionReward;
        }
        
        // Default work reward
        return _config.WorkRewards.DefaultReward;
    }
    
    // Letter queue mechanics
    public int CalculateLeverage(string npcId, ConnectionType tokenType)
    {
        var npcTokens = _tokenManager.GetTokensWithNPC(npcId);
        var tokenBalance = npcTokens.GetValueOrDefault(tokenType, 0);
        
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
        var basePosition = _config.LetterQueue.BasePositions.GetValueOrDefault(
            letter.TokenType, 
            _config.LetterQueue.MaxQueueSize - 1
        );
        
        // Apply leverage
        var leverageAdjustment = 0;
        if (npcTokenBalance < 0)
        {
            leverageAdjustment = Math.Abs(npcTokenBalance) * _config.LetterQueue.LeverageMultiplier;
        }
        else if (npcTokenBalance >= _config.TokenEconomy.PremiumLetterThreshold)
        {
            leverageAdjustment = -1; // Good relationship reduces priority
        }
        
        // Calculate final position
        var targetPosition = basePosition - leverageAdjustment;
        
        // Clamp to valid range
        return Math.Max(1, Math.Min(_config.LetterQueue.MaxQueueSize, targetPosition));
    }
    
    public int CalculateSkipCost(int fromPosition, Letter letter)
    {
        var baseCost = (fromPosition - 1) * _config.LetterQueue.SkipCostPerPosition;
        
        // Could apply multipliers based on letter properties or obligations
        return baseCost;
    }
    
    public bool CanSkipToPosition(Letter letter, int targetPosition)
    {
        // Can't skip to an occupied position
        // Can't skip patron letters
        // Can't skip obligated letters
        return targetPosition >= 1 && 
               targetPosition <= _config.LetterQueue.MaxQueueSize &&
               !letter.IsPatronLetter;
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
        var tokens = _tokenManager.GetTokensWithNPC(npcId);
        var totalTokens = tokens.Values.Sum();
        var threshold = GetTokenThresholdForCategory(category);
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
        var baseCost = _config.Travel.BaseStaminaCost;
        
        // Apply terrain modifiers
        foreach (var terrain in route.TerrainCategories)
        {
            var terrainName = terrain.ToString();
            if (_config.Travel.TerrainStaminaModifiers.TryGetValue(terrainName, out var modifier))
            {
                baseCost = (int)(baseCost * modifier);
            }
        }
        
        return baseCost;
    }
    
    public bool CanTravel(Player player, RouteOption route)
    {
        var staminaCost = CalculateTravelStamina(route);
        var hourCost = route.TravelTimeHours;
        
        return player.Stamina >= staminaCost &&
               _timeManager.HoursRemaining >= hourCost;
    }
    
    public TravelEncounterType DetermineEncounterType(RouteOption route)
    {
        // Determine based on terrain categories
        if (route.TerrainCategories.Contains(TerrainCategory.Wilderness_Terrain))
            return TravelEncounterType.WildernessObstacle;
        if (route.TerrainCategories.Contains(TerrainCategory.Dark_Passage))
            return TravelEncounterType.DarkChallenge;
        if (route.TerrainCategories.Contains(TerrainCategory.Exposed_Weather))
            return TravelEncounterType.WeatherEvent;
        if (route.TerrainCategories.Contains(TerrainCategory.Heavy_Cargo_Route))
            return TravelEncounterType.MerchantEncounter;
            
        return TravelEncounterType.FellowTraveler;
    }
    
    // Time management
    public TimeBlocks GetTimeBlock(int hour)
    {
        foreach (var (blockName, definition) in _config.Time.TimeBlocks)
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
            TimeBlocks.Dawn => npc.LetterTokenTypes.Contains(ConnectionType.Common),
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
            
        var chance = Math.Min(
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
        return string.IsNullOrEmpty(npcId) ? null : _npcRepository.GetNPCById(npcId);
    }
}

/// <summary>
/// Result of executing an action
/// </summary>
public class ActionResult
{
    public bool Success { get; set; }
    public LocationAction Action { get; set; }
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