using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;

namespace Wayfarer.GameState
{
    /// <summary>
    /// Calculates leverage (power dynamics) between player and NPCs.
    /// Core mechanic: Negative tokens = NPC has leverage over player.
    /// Standing obligations provide additional leverage.
    /// Failed deliveries create persistent leverage.
    /// </summary>
    public class LeverageCalculator
    {
        private readonly ConnectionTokenManager _tokenManager;
        private readonly StandingObligationManager _obligationManager;
        private readonly ConsequenceEngine _consequenceEngine;
        private readonly NPCRepository _npcRepository;
        private readonly MessageSystem _messageSystem;
        
        // Cache leverage calculations for performance - cleared each turn
        private readonly Dictionary<string, LeverageData> _leverageCache;
        private int _lastCalculationTurn;
        
        public LeverageCalculator(
            ConnectionTokenManager tokenManager,
            StandingObligationManager obligationManager,
            ConsequenceEngine consequenceEngine,
            NPCRepository npcRepository,
            MessageSystem messageSystem)
        {
            _tokenManager = tokenManager;
            _obligationManager = obligationManager;
            _consequenceEngine = consequenceEngine;
            _npcRepository = npcRepository;
            _messageSystem = messageSystem;
            _leverageCache = new Dictionary<string, LeverageData>();
            _lastCalculationTurn = -1;
        }
        
        /// <summary>
        /// Calculate total leverage an NPC has over the player.
        /// Combines token debt, obligations, and failure-based leverage.
        /// </summary>
        public LeverageData CalculateLeverage(string npcId, ConnectionType tokenType)
        {
            // Check cache first (cleared each turn)
            string cacheKey = $"{npcId}_{tokenType}";
            if (_leverageCache.ContainsKey(cacheKey))
            {
                return _leverageCache[cacheKey];
            }
            
            var leverageData = new LeverageData
            {
                NPCId = npcId,
                TokenType = tokenType
            };
            
            // 1. TOKEN DEBT LEVERAGE (negative tokens = leverage)
            var npcTokens = _tokenManager.GetTokensWithNPC(npcId);
            int tokenBalance = npcTokens.GetValueOrDefault(tokenType, 0);
            
            if (tokenBalance < 0)
            {
                // Each negative token = 1 point of leverage
                leverageData.TokenDebtLeverage = Math.Abs(tokenBalance);
                leverageData.TokenBalance = tokenBalance;
            }
            else
            {
                leverageData.TokenBalance = tokenBalance;
            }
            
            // 2. OBLIGATION LEVERAGE (standing obligations provide leverage)
            var activeObligations = _obligationManager.GetActiveObligations();
            foreach (var obligation in activeObligations)
            {
                if (IsObligationRelevant(obligation, npcId, tokenType))
                {
                    leverageData.ObligationLeverage += CalculateObligationLeverage(obligation);
                    leverageData.RelevantObligations.Add(obligation.Name);
                }
            }
            
            // 3. FAILURE LEVERAGE (from ConsequenceEngine)
            leverageData.FailureLeverage = _consequenceEngine.GetLeverage(npcId);
            
            // 4. CALCULATE TOTAL AND QUEUE POSITION
            leverageData.TotalLeverage = 
                leverageData.TokenDebtLeverage + 
                leverageData.ObligationLeverage + 
                leverageData.FailureLeverage;
            
            // Map leverage to queue position
            leverageData.TargetQueuePosition = CalculateQueuePosition(leverageData.TotalLeverage, tokenType);
            
            // 5. DISPLACEMENT COST (how expensive to displace letters from this NPC)
            leverageData.DisplacementCost = CalculateDisplacementCost(leverageData.TotalLeverage, tokenType);
            
            // Cache the result
            _leverageCache[cacheKey] = leverageData;
            
            return leverageData;
        }
        
        /// <summary>
        /// Calculate queue entry position based on leverage.
        /// Higher leverage = closer to position 1.
        /// </summary>
        public int CalculateQueuePosition(int totalLeverage, ConnectionType tokenType)
        {
            // Base position by token type
            int basePosition = tokenType switch
            {
                ConnectionType.Status => 3,    // Aristocratic letters start high
                ConnectionType.Commerce => 5,   // Business letters mid-range
                ConnectionType.Trust => 6,      // Personal letters lower
                ConnectionType.Shadow => 7,     // Clandestine letters lowest
                _ => 8
            };
            
            // Leverage moves letters forward
            // Every 2 points of leverage = 1 position forward
            int leverageBoost = totalLeverage / 2;
            
            // Special thresholds for extreme leverage
            if (totalLeverage >= 10)
            {
                // Extreme leverage (e.g., patron with -20 tokens) = position 1
                return 1;
            }
            else if (totalLeverage >= 5)
            {
                // High leverage = at least position 2-3
                return Math.Min(2, basePosition - leverageBoost);
            }
            else if (totalLeverage >= 3)
            {
                // Moderate leverage = at least position 3-4
                return Math.Min(3, basePosition - leverageBoost);
            }
            
            // Apply normal calculation
            int targetPosition = Math.Max(1, basePosition - leverageBoost);
            
            // Clamp to valid range
            return Math.Min(8, targetPosition);
        }
        
        /// <summary>
        /// Calculate the token cost to displace a letter based on leverage.
        /// Higher leverage = more expensive to displace.
        /// </summary>
        public int CalculateDisplacementCost(int totalLeverage, ConnectionType tokenType)
        {
            // Base cost to displace
            int baseCost = 2;
            
            // Each point of leverage increases cost
            int leverageCost = totalLeverage;
            
            // Token type modifiers
            float typeModifier = tokenType switch
            {
                ConnectionType.Status => 1.5f,     // Noble letters cost more to displace
                ConnectionType.Shadow => 1.3f,     // Dangerous to cross shadow contacts
                ConnectionType.Commerce => 1.0f,   // Standard cost
                ConnectionType.Trust => 0.8f,      // Personal letters slightly cheaper
                _ => 1.0f
            };
            
            int totalCost = (int)Math.Ceiling((baseCost + leverageCost) * typeModifier);
            
            // Extreme leverage makes displacement very expensive
            if (totalLeverage >= 10)
            {
                totalCost *= 2; // Double cost for extreme leverage
            }
            
            return Math.Max(1, totalCost);
        }
        
        /// <summary>
        /// Get the most appropriate token type to burn when displacing a letter.
        /// </summary>
        public (ConnectionType tokenType, int cost) GetDisplacementTokenType(
            string displacedNPCId, 
            int baseCost)
        {
            var npcTokens = _tokenManager.GetTokensWithNPC(displacedNPCId);
            
            // Priority order for displacement (political > commercial > personal)
            var priorityOrder = new[]
            {
                (ConnectionType.Status, 1.0f),     // Status tokens are most appropriate
                (ConnectionType.Commerce, 1.2f),   // Commerce tokens cost 20% more
                (ConnectionType.Trust, 1.5f),      // Trust tokens cost 50% more
                (ConnectionType.Shadow, 1.3f)      // Shadow tokens cost 30% more
            };
            
            foreach (var (tokenType, multiplier) in priorityOrder)
            {
                int requiredTokens = (int)Math.Ceiling(baseCost * multiplier);
                if (npcTokens.GetValueOrDefault(tokenType, 0) >= requiredTokens)
                {
                    return (tokenType, requiredTokens);
                }
            }
            
            // Fallback: use any available tokens at higher cost
            foreach (var kvp in npcTokens)
            {
                if (kvp.Value >= baseCost * 2)
                {
                    return (kvp.Key, baseCost * 2);
                }
            }
            
            // No tokens available
            return (ConnectionType.Status, int.MaxValue);
        }
        
        /// <summary>
        /// Clear the leverage cache (should be called at turn start).
        /// </summary>
        public void ClearCache(int currentTurn)
        {
            if (currentTurn != _lastCalculationTurn)
            {
                _leverageCache.Clear();
                _lastCalculationTurn = currentTurn;
            }
        }
        
        /// <summary>
        /// Get a narrative description of leverage for UI display.
        /// </summary>
        public string GetLeverageNarrative(LeverageData leverageData)
        {
            var npc = _npcRepository.GetById(leverageData.NPCId);
            if (npc == null) return "";
            
            if (leverageData.TotalLeverage == 0)
            {
                return $"No special leverage with {npc.Name}";
            }
            
            var parts = new List<string>();
            
            if (leverageData.TokenDebtLeverage > 0)
            {
                parts.Add($"owes {leverageData.TokenDebtLeverage} {leverageData.TokenType} tokens");
            }
            
            if (leverageData.ObligationLeverage > 0)
            {
                parts.Add($"bound by {leverageData.RelevantObligations.Count} obligation(s)");
            }
            
            if (leverageData.FailureLeverage > 0)
            {
                parts.Add($"carries {leverageData.FailureLeverage} points of failure debt");
            }
            
            string leverageDescription = string.Join(", ", parts);
            
            return leverageData.TotalLeverage switch
            {
                >= 10 => $"🔴 EXTREME LEVERAGE: {npc.Name} has absolute power (you {leverageDescription})",
                >= 5 => $"🟠 HIGH LEVERAGE: {npc.Name} has significant influence (you {leverageDescription})",
                >= 3 => $"🟡 MODERATE LEVERAGE: {npc.Name} has some power (you {leverageDescription})",
                >= 1 => $"⚪ LOW LEVERAGE: {npc.Name} has minor influence (you {leverageDescription})",
                _ => ""
            };
        }
        
        // === HELPER METHODS ===
        
        private bool IsObligationRelevant(StandingObligation obligation, string npcId, ConnectionType tokenType)
        {
            // Check if obligation applies to this NPC
            if (!string.IsNullOrEmpty(obligation.RelatedNPCId) && obligation.RelatedNPCId != npcId)
            {
                return false;
            }
            
            // Check if obligation applies to this token type
            if (obligation.RelatedTokenType.HasValue && obligation.RelatedTokenType.Value != tokenType)
            {
                return false;
            }
            
            return obligation.IsActive;
        }
        
        private int CalculateObligationLeverage(StandingObligation obligation)
        {
            int leverage = 0;
            
            // Different obligation effects provide different leverage
            if (obligation.HasEffect(ObligationEffect.TrustPriority))
                leverage += 2;
            
            if (obligation.HasEffect(ObligationEffect.StatusPriority))
                leverage += 2;
            
            if (obligation.HasEffect(ObligationEffect.ShadowForced))
                leverage += 3; // Shadow obligations are particularly coercive
            
            if (obligation.HasEffect(ObligationEffect.PatronMonthly))
                leverage += 5; // Patron has significant leverage
            
            if (obligation.HasEffect(ObligationEffect.DebtSpiral))
                leverage += 1; // Additional leverage from debt spiral
            
            return leverage;
        }
    }
    
    /// <summary>
    /// Data structure containing all leverage calculations for an NPC.
    /// </summary>
    public class LeverageData
    {
        public string NPCId { get; set; }
        public ConnectionType TokenType { get; set; }
        
        // Leverage components
        public int TokenDebtLeverage { get; set; }     // From negative tokens
        public int ObligationLeverage { get; set; }    // From standing obligations
        public int FailureLeverage { get; set; }       // From failed deliveries
        public int TotalLeverage { get; set; }         // Sum of all leverage
        
        // Calculated effects
        public int TargetQueuePosition { get; set; }   // Where letters should enter
        public int DisplacementCost { get; set; }      // Cost to displace this NPC's letters
        
        // Context data
        public int TokenBalance { get; set; }          // Current token balance with NPC
        public List<string> RelevantObligations { get; set; } = new List<string>();
        
        // Leverage state for display
        public LeverageLevel Level => TotalLeverage switch
        {
            >= 10 => LeverageLevel.Extreme,
            >= 5 => LeverageLevel.High,
            >= 3 => LeverageLevel.Moderate,
            >= 1 => LeverageLevel.Low,
            _ => LeverageLevel.None
        };
    }
    
    /// <summary>
    /// Leverage levels for UI display and decision making.
    /// </summary>
    public enum LeverageLevel
    {
        None,       // No leverage (0)
        Low,        // Minor influence (1-2)
        Moderate,   // Significant power (3-4)
        High,       // Major control (5-9)
        Extreme     // Absolute dominance (10+)
    }
}