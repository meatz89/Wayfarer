using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Subsystems.ObligationSubsystem;

namespace Wayfarer.Subsystems.ObligationSubsystem
{
    /// <summary>
    /// Provides comprehensive analytics and reporting for the obligation system.
    /// Tracks performance metrics, relationship impacts, and queue efficiency.
    /// </summary>
    public class ObligationStatistics
    {
        private readonly GameWorld _gameWorld;
        private readonly NPCRepository _npcRepository;
        private readonly TokenMechanicsManager _tokenManager;
        private readonly GameConfiguration _config;

        private int _lastMorningSwapDay = 0;

        public ObligationStatistics(
            GameWorld gameWorld,
            NPCRepository npcRepository,
            TokenMechanicsManager tokenManager,
            GameConfiguration config)
        {
            _gameWorld = gameWorld;
            _npcRepository = npcRepository;
            _tokenManager = tokenManager;
            _config = config;
        }

        /// <summary>
        /// Get the last day a morning swap was performed.
        /// </summary>
        public int GetLastMorningSwapDay()
        {
            return _lastMorningSwapDay;
        }

        /// <summary>
        /// Update the last morning swap day when a swap is performed.
        /// </summary>
        public void RecordMorningSwap(int currentDay)
        {
            _lastMorningSwapDay = currentDay;
        }

        /// <summary>
        /// Generate comprehensive obligation metrics for the current game state.
        /// </summary>
        public ObligationMetrics GenerateObligationMetrics()
        {
            Player player = _gameWorld.GetPlayer();
            DeliveryObligation[] activeObligations = GetActiveObligations();
            List<MeetingObligation> activeMeetings = GetActiveMeetings();

            ObligationMetrics metrics = new ObligationMetrics
            {
                ActiveObligationCount = activeObligations.Length,
                ActiveMeetingCount = activeMeetings.Count,
                TotalQueueSize = _config.LetterQueue.MaxQueueSize,
                EmptyQueueSlots = _config.LetterQueue.MaxQueueSize - activeObligations.Length
            };

            // Calculate deadline metrics
            if (activeObligations.Any())
            {
                metrics.AverageDeadlineHours = activeObligations.Average(o => o.DeadlineInMinutes) / 60.0;
                metrics.MostUrgentObligation = activeObligations.OrderBy(o => o.DeadlineInMinutes).First();

                int nextDeadline = Math.Min(
                    activeObligations.Min(o => o.DeadlineInMinutes),
                    activeMeetings.Any() ? activeMeetings.Min(m => m.DeadlineInMinutes) : int.MaxValue
                );

                if (nextDeadline != int.MaxValue)
                {
                    metrics.TotalMinutesUntilNextDeadline = nextDeadline;
                    metrics.NextDeadlineDescription = FormatDeadlineDescription(nextDeadline);
                }
            }

            // Count urgency levels
            metrics.ExpiredObligationCount = activeObligations.Count(o => o.DeadlineInMinutes <= 0);
            metrics.UrgentObligationCount = activeObligations.Count(o => o.DeadlineInMinutes <= 360 && o.DeadlineInMinutes > 0); // < 6 hours
            metrics.CriticalObligationCount = activeObligations.Count(o => o.DeadlineInMinutes <= 180 && o.DeadlineInMinutes > 0); // < 3 hours
            metrics.OverdueObligationCount = metrics.ExpiredObligationCount;

            // Position 1 status
            DeliveryObligation position1Obligation = player.ObligationQueue[0];
            if (position1Obligation == null)
            {
                metrics.Position1Status = Position1Status.Empty;
            }
            else if (position1Obligation.DeadlineInMinutes <= 0)
            {
                metrics.Position1Status = Position1Status.Blocked;
            }
            else
            {
                // Would need to check if player can actually deliver (location, etc.)
                metrics.Position1Status = Position1Status.CanDeliver;
            }

            // Calculate token leverage metrics
            metrics.TotalTokenLeverage = CalculateTokenLeverage(activeObligations);
            LeverageAnalysis leverageAnalysis = AnalyzeLeverageEffects(activeObligations);
            metrics.PositionsGainedFromLeverage = leverageAnalysis.PositionsGained;
            metrics.PositionsLostFromDebt = leverageAnalysis.PositionsLost;

            // Performance metrics from history
            DeliveryPerformance deliveryStats = CalculateDeliveryPerformance(player);
            metrics.TotalDeliveredToday = deliveryStats.DeliveredToday;
            metrics.TotalSkippedToday = deliveryStats.SkippedToday;
            metrics.DeliverySuccessRate = deliveryStats.SuccessRate;

            return metrics;
        }

        /// <summary>
        /// Get detailed queue activity statistics.
        /// </summary>
        public QueueActivity GetQueueActivity()
        {
            Player player = _gameWorld.GetPlayer();
            QueueActivity activity = new QueueActivity();

            // Calculate daily statistics from letter history
            foreach (KeyValuePair<string, LetterHistory> npcHistory in player.NPCLetterHistory)
            {
                LetterHistory history = npcHistory.Value;

                // These would ideally be time-filtered for "today"
                activity.LettersDeliveredToday += history.DeliveredCount;
                activity.LettersSkippedToday += history.SkippedCount;
                activity.LettersExpiredToday += history.ExpiredCount;
            }

            // Meeting statistics
            foreach (MeetingObligation meeting in player.MeetingObligations)
            {
                if (meeting.DeadlineInMinutes <= 0)
                {
                    activity.MeetingsExpiredToday++;
                }
            }

            // Token statistics would need tracking system
            activity.TokensEarnedToday = CalculateTodaysTokenEarnings();
            activity.TokensSpentToday = CalculateTodaysTokenSpending();

            return activity;
        }

        /// <summary>
        /// Generate a comprehensive queue snapshot for analysis.
        /// </summary>
        public QueueSnapshot CreateQueueSnapshot()
        {
            Player player = _gameWorld.GetPlayer();

            return new QueueSnapshot
            {
                SnapshotTime = DateTime.UtcNow,
                Queue = player.ObligationQueue,
                Meetings = new List<MeetingObligation>(player.MeetingObligations),
                Metrics = GenerateObligationMetrics(),
                PositionInfo = GetQueuePositionAnalysis(),
                SatchelState = GetSatchelAnalysis(),
                TodaysActivity = GetQueueActivity()
            };
        }

        /// <summary>
        /// Analyze relationship health based on token balances and letter history.
        /// </summary>
        public Dictionary<string, RelationshipHealth> AnalyzeRelationshipHealth()
        {
            Player player = _gameWorld.GetPlayer();
            Dictionary<string, RelationshipHealth> relationshipHealth = new Dictionary<string, RelationshipHealth>();

            foreach (NPC npc in _npcRepository.GetAllNPCs())
            {
                Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npc.ID);
                
                // If NPC has no letter history, they haven't interacted yet
                if (!player.NPCLetterHistory.ContainsKey(npc.ID))
                {
                    // Skip NPCs with no letter history - they have no relationship health to report
                    continue;
                }
                
                LetterHistory history = player.NPCLetterHistory[npc.ID];

                RelationshipHealth health = new RelationshipHealth
                {
                    NPCId = npc.ID,
                    NPCName = npc.Name,
                    TotalPositiveTokens = tokens.Values.Where(v => v > 0).Sum(),
                    TotalNegativeTokens = tokens.Values.Where(v => v < 0).Sum(),
                    DeliveredCount = history.DeliveredCount,
                    ExpiredCount = history.ExpiredCount,
                    SkippedCount = history.SkippedCount
                };

                health.ReliabilityScore = CalculateReliabilityScore(health);
                health.RiskLevel = DetermineRiskLevel(health);

                relationshipHealth[npc.ID] = health;
            }

            return relationshipHealth;
        }

        /// <summary>
        /// Get performance metrics for specific NPCs.
        /// </summary>
        public List<NPCPerformanceMetric> GetNPCPerformanceMetrics()
        {
            Player player = _gameWorld.GetPlayer();
            List<NPCPerformanceMetric> metrics = new List<NPCPerformanceMetric>();

            foreach (NPC npc in _npcRepository.GetAllNPCs())
            {
                // Skip NPCs with no letter history - they have no performance metrics
                if (!player.NPCLetterHistory.ContainsKey(npc.ID))
                    continue;
                    
                LetterHistory history = player.NPCLetterHistory[npc.ID];
                Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npc.ID);

                metrics.Add(new NPCPerformanceMetric
                {
                    NPCId = npc.ID,
                    NPCName = npc.Name,
                    TotalInteractions = history.DeliveredCount + history.ExpiredCount + history.SkippedCount,
                    SuccessfulDeliveries = history.DeliveredCount,
                    FailedDeliveries = history.ExpiredCount,
                    SkippedDeliveries = history.SkippedCount,
                    CurrentTokenBalance = tokens.Values.Sum(),
                    PrimaryConnectionType = GetPrimaryConnectionType(npc),
                    SuccessRate = CalculateSuccessRate(history),
                    LastInteractionResult = GetLastInteractionResult(npc.ID, history)
                });
            }

            return metrics.OrderByDescending(m => m.TotalInteractions).ToList();
        }

        /// <summary>
        /// Calculate queue efficiency metrics.
        /// </summary>
        public QueueEfficiencyMetrics CalculateQueueEfficiency()
        {
            DeliveryObligation[] activeObligations = GetActiveObligations();
            Player player = _gameWorld.GetPlayer();

            QueueEfficiencyMetrics metrics = new QueueEfficiencyMetrics
            {
                CurrentUtilization = (double)activeObligations.Length / _config.LetterQueue.MaxQueueSize,
                AverageTimeInQueue = CalculateAverageTimeInQueue(activeObligations),
                PositionOptimizationScore = CalculatePositionOptimizationScore(activeObligations),
                DeadlinePressure = CalculateDeadlinePressure(activeObligations)
            };

            // Analyze position distribution
            Dictionary<int, int> positionDistribution = new Dictionary<int, int>();
            for (int i = 1; i <= _config.LetterQueue.MaxQueueSize; i++)
            {
                positionDistribution[i] = player.ObligationQueue[i - 1] != null ? 1 : 0;
            }
            metrics.PositionDistribution = positionDistribution;

            return metrics;
        }

        /// <summary>
        /// Get trending patterns in obligation management.
        /// </summary>
        public ObligationTrends GetObligationTrends()
        {
            // This would require historical data tracking over time
            // For now, provide current state analysis
            DeliveryObligation[] activeObligations = GetActiveObligations();
            List<MeetingObligation> activeMeetings = GetActiveMeetings();

            return new ObligationTrends
            {
                ObligationCountTrend = TrendDirection.Stable, // Would need historical data
                DeadlinePressureTrend = CalculateDeadlinePressureTrend(activeObligations),
                RelationshipHealthTrend = TrendDirection.Stable, // Would need historical data
                TokenLeverageTrend = CalculateTokenLeverageTrend(),
                MostActiveNPCs = GetMostActiveNPCs(3),
                MostProblematicNPCs = GetMostProblematicNPCs(3)
            };
        }

        // Private helper methods

        private DeliveryObligation[] GetActiveObligations()
        {
            return _gameWorld.GetPlayer().ObligationQueue
                .Where(o => o != null)
                .ToArray();
        }

        private List<MeetingObligation> GetActiveMeetings()
        {
            return _gameWorld.GetPlayer().MeetingObligations
                .Where(m => m.DeadlineInMinutes > 0)
                .ToList();
        }

        private string FormatDeadlineDescription(int minutes)
        {
            if (minutes <= 30) return $"{minutes}m ⚡ CRITICAL!";
            if (minutes <= 120) return $"{minutes}m 🔥 URGENT";

            int hours = minutes / 60;
            int remainingMinutes = minutes % 60;

            if (hours <= 6)
            {
                return remainingMinutes == 0 ? $"{hours}h ⚠️ urgent" : $"{hours}h {remainingMinutes}m ⚠️ urgent";
            }

            return hours <= 16 ? $"{hours}h today" : $"{hours / 24}d {hours % 24}h";
        }

        private Dictionary<ConnectionType, int> CalculateTokenLeverage(DeliveryObligation[] obligations)
        {
            Dictionary<ConnectionType, int> leverage = new Dictionary<ConnectionType, int>();

            foreach (DeliveryObligation obligation in obligations)
            {
                string senderId = GetNPCIdByName(obligation.SenderName);
                if (!string.IsNullOrEmpty(senderId))
                {
                    Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(senderId);
                    foreach (KeyValuePair<ConnectionType, int> token in tokens)
                    {
                        if (!leverage.ContainsKey(token.Key))
                            leverage[token.Key] = 0;
                        leverage[token.Key] += token.Value;
                    }
                }
            }

            return leverage;
        }

        private LeverageAnalysis AnalyzeLeverageEffects(DeliveryObligation[] obligations)
        {
            LeverageAnalysis analysis = new LeverageAnalysis();
            int basePosition = _config.LetterQueue.MaxQueueSize;

            foreach (DeliveryObligation obligation in obligations)
            {
                if (obligation.OriginalQueuePosition.HasValue)
                {
                    int originalPos = obligation.OriginalQueuePosition.Value;
                    int currentPos = obligation.QueuePosition;

                    if (currentPos < originalPos)
                    {
                        analysis.PositionsGained += originalPos - currentPos;
                    }
                    else if (currentPos > originalPos)
                    {
                        analysis.PositionsLost += currentPos - originalPos;
                    }
                }
            }

            return analysis;
        }

        private DeliveryPerformance CalculateDeliveryPerformance(Player player)
        {
            DeliveryPerformance performance = new DeliveryPerformance();
            int totalDelivered = 0;
            int totalExpired = 0;
            int totalSkipped = 0;

            foreach (LetterHistory history in player.NPCLetterHistory.Values)
            {
                totalDelivered += history.DeliveredCount;
                totalExpired += history.ExpiredCount;
                totalSkipped += history.SkippedCount;
            }

            performance.DeliveredToday = totalDelivered; // Would need date filtering
            performance.SkippedToday = totalSkipped;

            int totalAttempts = totalDelivered + totalExpired;
            performance.SuccessRate = totalAttempts > 0 ? (double)totalDelivered / totalAttempts : 1.0;

            return performance;
        }

        private Dictionary<ConnectionType, int> CalculateTodaysTokenEarnings()
        {
            // Would need historical tracking of token gains
            return new Dictionary<ConnectionType, int>
            {
                [ConnectionType.Trust] = 0,
                [ConnectionType.Commerce] = 0,
                [ConnectionType.Status] = 0,
                [ConnectionType.Shadow] = 0
            };
        }

        private Dictionary<ConnectionType, int> CalculateTodaysTokenSpending()
        {
            // Would need historical tracking of token spending
            return new Dictionary<ConnectionType, int>
            {
                [ConnectionType.Trust] = 0,
                [ConnectionType.Commerce] = 0,
                [ConnectionType.Status] = 0,
                [ConnectionType.Shadow] = 0
            };
        }

        private List<QueuePositionInfo> GetQueuePositionAnalysis()
        {
            List<QueuePositionInfo> positions = new List<QueuePositionInfo>();
            DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

            for (int i = 0; i < _config.LetterQueue.MaxQueueSize; i++)
            {
                int position = i + 1;
                DeliveryObligation? obligation = queue[i];

                positions.Add(new QueuePositionInfo
                {
                    Position = position,
                    IsOccupied = obligation != null,
                    CurrentObligation = obligation,
                    CanInsertHere = obligation == null,
                    BlockingReason = obligation != null ? "Position occupied" : ""
                });
            }

            return positions;
        }

        private SatchelInfo GetSatchelAnalysis()
        {
            Player player = _gameWorld.GetPlayer();
            SatchelInfo info = new SatchelInfo
            {
                CarriedLetters = new List<Letter>(player.CarriedLetters),
                TotalSize = player.CarriedLetters.Sum(l => l.Size),
                MaxCapacity = 20 // Would come from config or player stats
            };

            info.RemainingSpace = info.MaxCapacity - info.TotalSize;

            info.FragileLetters = info.CarriedLetters
                .Where(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Fragile))
                .ToList();

            info.PerishableLetters = info.CarriedLetters
                .Where(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Perishable))
                .ToList();

            info.ValuableLetters = info.CarriedLetters
                .Where(l => l.PhysicalProperties.HasFlag(LetterPhysicalProperties.Valuable))
                .ToList();

            return info;
        }

        private double CalculateReliabilityScore(RelationshipHealth health)
        {
            if (health.DeliveredCount + health.ExpiredCount == 0) return 1.0; // No history = neutral

            int totalInteractions = health.DeliveredCount + health.ExpiredCount + health.SkippedCount;
            double successRate = (double)health.DeliveredCount / (health.DeliveredCount + health.ExpiredCount);
            double tokenBonus = Math.Max(0, health.TotalPositiveTokens) * 0.1;
            double tokenPenalty = Math.Min(0, health.TotalNegativeTokens) * 0.1;

            return Math.Max(0, Math.Min(1, successRate + tokenBonus + tokenPenalty));
        }

        private RiskLevel DetermineRiskLevel(RelationshipHealth health)
        {
            if (health.TotalNegativeTokens < -5) return RiskLevel.High;
            if (health.ExpiredCount > health.DeliveredCount) return RiskLevel.High;
            if (health.ReliabilityScore < 0.3) return RiskLevel.High;
            if (health.ReliabilityScore < 0.6) return RiskLevel.Medium;
            return RiskLevel.Low;
        }

        private ConnectionType GetPrimaryConnectionType(NPC npc)
        {
            return npc.LetterTokenTypes.FirstOrDefault();
        }

        private double CalculateSuccessRate(LetterHistory history)
        {
            int total = history.DeliveredCount + history.ExpiredCount;
            return total > 0 ? (double)history.DeliveredCount / total : 1.0;
        }

        private string GetLastInteractionResult(string npcId, LetterHistory history)
        {
            // Would need detailed interaction history with timestamps
            if (history.DeliveredCount > 0) return "Delivered";
            if (history.ExpiredCount > 0) return "Expired";
            if (history.SkippedCount > 0) return "Skipped";
            return "None";
        }

        private double CalculateAverageTimeInQueue(DeliveryObligation[] obligations)
        {
            // Would need timestamp tracking for when obligations entered queue
            return obligations.Length > 0 ? obligations.Average(o => o.DaysInQueue * 24) : 0;
        }

        private double CalculatePositionOptimizationScore(DeliveryObligation[] obligations)
        {
            // Measure how well positioned obligations are based on deadlines
            if (!obligations.Any()) return 1.0;

            double score = 0.0;
            for (int i = 0; i < obligations.Length; i++)
            {
                DeliveryObligation obligation = obligations[i];
                int expectedPosition = i + 1; // Current position
                int idealPosition = CalculateIdealPosition(obligation, obligations);

                // Score based on how close to ideal position
                int positionDiff = Math.Abs(expectedPosition - idealPosition);
                score += Math.Max(0, 1.0 - (positionDiff / (double)_config.LetterQueue.MaxQueueSize));
            }

            return score / obligations.Length;
        }

        private int CalculateIdealPosition(DeliveryObligation obligation, DeliveryObligation[] allObligations)
        {
            // Ideal position based on deadline urgency
            int moreUrgentCount = allObligations.Count(o => o.DeadlineInMinutes < obligation.DeadlineInMinutes);
            return moreUrgentCount + 1;
        }

        private double CalculateDeadlinePressure(DeliveryObligation[] obligations)
        {
            if (!obligations.Any()) return 0.0;

            double totalPressure = obligations.Sum(o =>
            {
                if (o.DeadlineInMinutes <= 180) return 1.0; // Critical
                if (o.DeadlineInMinutes <= 360) return 0.7; // Urgent
                if (o.DeadlineInMinutes <= 720) return 0.3; // Moderate
                return 0.1; // Low
            });

            return totalPressure / obligations.Length;
        }

        private TrendDirection CalculateDeadlinePressureTrend(DeliveryObligation[] obligations)
        {
            // Would need historical data to calculate trends
            double currentPressure = CalculateDeadlinePressure(obligations);

            // Simplified logic based on current state
            if (currentPressure > 0.7) return TrendDirection.Increasing;
            if (currentPressure < 0.3) return TrendDirection.Decreasing;
            return TrendDirection.Stable;
        }

        private TrendDirection CalculateTokenLeverageTrend()
        {
            // Would need historical token data
            return TrendDirection.Stable;
        }

        private List<string> GetMostActiveNPCs(int count)
        {
            Player player = _gameWorld.GetPlayer();
            return player.NPCLetterHistory
                .OrderByDescending(h => h.Value.DeliveredCount + h.Value.ExpiredCount + h.Value.SkippedCount)
                .Take(count)
                .Select(h => GetNPCNameById(h.Key))
                .ToList();
        }

        private List<string> GetMostProblematicNPCs(int count)
        {
            Player player = _gameWorld.GetPlayer();
            return player.NPCLetterHistory
                .Where(h => h.Value.ExpiredCount > 0)
                .OrderByDescending(h => h.Value.ExpiredCount)
                .Take(count)
                .Select(h => GetNPCNameById(h.Key))
                .ToList();
        }

        private string GetNPCIdByName(string npcName)
        {
            NPC npc = _npcRepository.GetByName(npcName);
            return npc?.ID ?? "";
        }

        private string GetNPCNameById(string npcId)
        {
            NPC npc = _npcRepository.GetById(npcId);
            return npc?.Name ?? "Unknown";
        }
    }

    // Supporting models for statistics

    public class LeverageAnalysis
    {
        public int PositionsGained { get; set; }
        public int PositionsLost { get; set; }
    }

    public class DeliveryPerformance
    {
        public int DeliveredToday { get; set; }
        public int SkippedToday { get; set; }
        public double SuccessRate { get; set; }
    }

    public class RelationshipHealth
    {
        public string NPCId { get; set; } = "";
        public string NPCName { get; set; } = "";
        public int TotalPositiveTokens { get; set; }
        public int TotalNegativeTokens { get; set; }
        public int DeliveredCount { get; set; }
        public int ExpiredCount { get; set; }
        public int SkippedCount { get; set; }
        public double ReliabilityScore { get; set; }
        public RiskLevel RiskLevel { get; set; }
    }

    public class NPCPerformanceMetric
    {
        public string NPCId { get; set; } = "";
        public string NPCName { get; set; } = "";
        public int TotalInteractions { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public int FailedDeliveries { get; set; }
        public int SkippedDeliveries { get; set; }
        public int CurrentTokenBalance { get; set; }
        public ConnectionType PrimaryConnectionType { get; set; }
        public double SuccessRate { get; set; }
        public string LastInteractionResult { get; set; } = "";
    }

    public class QueueEfficiencyMetrics
    {
        public double CurrentUtilization { get; set; }
        public double AverageTimeInQueue { get; set; }
        public double PositionOptimizationScore { get; set; }
        public double DeadlinePressure { get; set; }
        public Dictionary<int, int> PositionDistribution { get; set; } = new Dictionary<int, int>();
    }

    public class ObligationTrends
    {
        public TrendDirection ObligationCountTrend { get; set; }
        public TrendDirection DeadlinePressureTrend { get; set; }
        public TrendDirection RelationshipHealthTrend { get; set; }
        public TrendDirection TokenLeverageTrend { get; set; }
        public List<string> MostActiveNPCs { get; set; } = new List<string>();
        public List<string> MostProblematicNPCs { get; set; } = new List<string>();
    }

    public enum RiskLevel
    {
        Low,
        Medium,
        High
    }

    public enum TrendDirection
    {
        Decreasing,
        Stable,
        Increasing
    }
}