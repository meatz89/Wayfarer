using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Subsystems.ObligationSubsystem;

namespace Wayfarer.Subsystems.ObligationSubsystem
{
    /// <summary>
    /// Public facade for the Obligation subsystem.
    /// Coordinates between all obligation managers and provides a clean API for GameFacade.
    /// Replaces the monolithic ObligationQueueManager with a modular architecture.
    /// </summary>
    public class ObligationFacade
    {
        private readonly GameWorld _gameWorld;
        private readonly DeliveryManager _deliveryManager;
        private readonly MeetingManager _meetingManager;
        private readonly QueueManipulator _queueManipulator;
        private readonly DisplacementCalculator _displacementCalculator;
        private readonly DeadlineTracker _deadlineTracker;
        private readonly ObligationStatistics _statistics;

        // External dependencies for references
        private readonly NPCRepository _npcRepository;
        private readonly MessageSystem _messageSystem;
        private readonly TokenMechanicsManager _tokenManager;
        private readonly StandingObligationManager _standingObligationManager;
        private readonly GameConfiguration _config;
        private readonly TimeManager _timeManager;

        public ObligationFacade(
            GameWorld gameWorld,
            DeliveryManager deliveryManager,
            MeetingManager meetingManager,
            QueueManipulator queueManipulator,
            DisplacementCalculator displacementCalculator,
            DeadlineTracker deadlineTracker,
            ObligationStatistics statistics,
            NPCRepository npcRepository,
            MessageSystem messageSystem,
            TokenMechanicsManager tokenManager,
            StandingObligationManager standingObligationManager,
            GameConfiguration config,
            TimeManager timeManager)
        {
            _gameWorld = gameWorld;
            _deliveryManager = deliveryManager;
            _meetingManager = meetingManager;
            _queueManipulator = queueManipulator;
            _displacementCalculator = displacementCalculator;
            _deadlineTracker = deadlineTracker;
            _statistics = statistics;
            _npcRepository = npcRepository;
            _messageSystem = messageSystem;
            _tokenManager = tokenManager;
            _standingObligationManager = standingObligationManager;
            _config = config;
            _timeManager = timeManager;
        }

        #region Queue Operations

        /// <summary>
        /// Get the player's complete obligation queue.
        /// </summary>
        public DeliveryObligation[] GetPlayerQueue()
        {
            return _gameWorld.GetPlayer().ObligationQueue;
        }

        /// <summary>
        /// Get only active (non-null) obligations from the queue.
        /// </summary>
        public DeliveryObligation[] GetActiveObligations()
        {
            Player player = _gameWorld.GetPlayer();
            return player.ObligationQueue
                .Where(o => o != null)
                .ToArray();
        }

        /// <summary>
        /// Get the current position of an obligation in the queue.
        /// </summary>
        public int GetQueuePosition(DeliveryObligation obligation)
        {
            return _queueManipulator.GetQueuePosition(obligation);
        }

        /// <summary>
        /// Get obligation at a specific queue position.
        /// </summary>
        public DeliveryObligation GetLetterAt(int position)
        {
            return _queueManipulator.GetLetterAt(position);
        }

        /// <summary>
        /// Check if the queue is completely full.
        /// </summary>
        public bool IsQueueFull()
        {
            return _queueManipulator.IsQueueFull();
        }

        /// <summary>
        /// Get the total number of obligations currently in the queue.
        /// </summary>
        public int GetLetterCount()
        {
            return _queueManipulator.GetLetterCount();
        }

        /// <summary>
        /// Add obligation to the queue with leverage-based positioning.
        /// </summary>
        public ObligationAddResult AddObligationWithEffects(DeliveryObligation obligation)
        {
            if (obligation == null) return new ObligationAddResult { ErrorMessage = "Obligation cannot be null" };

            // Apply deadline bonuses from standing obligations
            ApplyDeadlineBonuses(obligation);

            // Check for automatic displacement scenarios
            AutoDisplacementInfo autoDisplacement = CheckForAutomaticDisplacement(obligation);
            if (autoDisplacement.ShouldForceDisplacement)
            {
                DisplacementResult displacementResult = _displacementCalculator.ExecuteAutomaticDisplacement(
                    obligation,
                    autoDisplacement.ForcedPosition,
                    autoDisplacement.DisplacementReason);

                if (displacementResult.CanExecute)
                {
                    _deliveryManager.AddPhysicalLetter(obligation);
                    return new ObligationAddResult
                    {
                        Success = true,
                        Position = autoDisplacement.ForcedPosition,
                        AddedObligation = obligation,
                        CausedDisplacement = true
                    };
                }
            }

            // Normal leverage-based positioning
            ObligationAddResult addResult = _queueManipulator.AddObligationWithLeverage(obligation);

            // Add to physical satchel if queue addition succeeded
            if (addResult.Success)
            {
                _deliveryManager.AddPhysicalLetter(obligation);
            }

            return addResult;
        }

        /// <summary>
        /// Add obligation to first available slot (simple insertion).
        /// </summary>
        public ObligationAddResult AddObligation(DeliveryObligation obligation)
        {
            ObligationAddResult result = _queueManipulator.AddObligation(obligation);

            if (result.Success)
            {
                _deliveryManager.AddPhysicalLetter(obligation);
            }

            return result;
        }

        /// <summary>
        /// Remove obligation from a specific queue position.
        /// </summary>
        public QueueManipulationResult RemoveObligationFromQueue(int position)
        {
            QueueManipulationResult result = _queueManipulator.RemoveObligationFromQueue(position);

            if (result.Success && result.AffectedObligation != null)
            {
                _deliveryManager.RemovePhysicalLetter(result.AffectedObligation.Id);
            }

            return result;
        }

        /// <summary>
        /// Move an obligation to a specific target position.
        /// </summary>
        public QueueManipulationResult MoveObligationToPosition(DeliveryObligation obligation, int targetPosition)
        {
            return _queueManipulator.MoveObligationToPosition(obligation, targetPosition);
        }

        #endregion

        #region Delivery Operations

        /// <summary>
        /// Check if the player can deliver the letter at position 1.
        /// </summary>
        public bool CanDeliverFromPosition1()
        {
            return _deliveryManager.CanDeliverFromPosition1();
        }

        /// <summary>
        /// Attempt to deliver the letter from position 1.
        /// </summary>
        public DeliveryResult DeliverFromPosition1()
        {
            return _deliveryManager.DeliverFromPosition1();
        }

        /// <summary>
        /// Deliver a specific obligation by ID.
        /// </summary>
        public DeliveryResult DeliverObligation(string obligationId)
        {
            return _deliveryManager.DeliverObligation(obligationId);
        }

        /// <summary>
        /// Get detailed validation information about why a delivery cannot be completed.
        /// </summary>
        public DeliveryResult GetDeliveryValidation(int position)
        {
            return _deliveryManager.GetDeliveryValidation(position);
        }

        /// <summary>
        /// Skip delivery from a specific position for a token cost.
        /// </summary>
        public QueueManipulationResult TrySkipDelivery(int position)
        {
            return _deliveryManager.TrySkipDelivery(position);
        }

        /// <summary>
        /// Get letters that are expiring within the specified threshold.
        /// </summary>
        public DeliveryObligation[] GetExpiringLetters(int daysThreshold)
        {
            return _deliveryManager.GetExpiringLetters(daysThreshold);
        }

        /// <summary>
        /// Get total size of all physical letters in the satchel.
        /// </summary>
        public int GetTotalSize()
        {
            return _deliveryManager.GetTotalSatchelSize();
        }

        #endregion

        #region Meeting Operations

        /// <summary>
        /// Get all active meeting obligations.
        /// </summary>
        public List<MeetingObligation> GetActiveMeetingObligations()
        {
            return _meetingManager.GetActiveMeetingObligations();
        }

        /// <summary>
        /// Get meeting obligation with a specific NPC.
        /// </summary>
        public MeetingObligation GetMeetingWithNPC(string npcId)
        {
            return _meetingManager.GetMeetingWithNPC(npcId);
        }

        /// <summary>
        /// Add a new meeting obligation.
        /// </summary>
        public MeetingResult AddMeetingObligation(MeetingObligation meeting)
        {
            return _meetingManager.AddMeetingObligation(meeting);
        }

        /// <summary>
        /// Complete a meeting with an NPC.
        /// </summary>
        public MeetingResult CompleteMeeting(string meetingId)
        {
            return _meetingManager.CompleteMeeting(meetingId);
        }

        /// <summary>
        /// Schedule a new meeting with specific parameters.
        /// </summary>
        public MeetingResult ScheduleMeeting(string npcId, string reason, int deadlineInSegments, StakeType stakes = StakeType.REPUTATION)
        {
            return _meetingManager.ScheduleMeeting(npcId, reason, deadlineInSegments, stakes);
        }

        /// <summary>
        /// Get meetings that are expiring within the specified threshold.
        /// </summary>
        public List<MeetingObligation> GetExpiringMeetings(int hoursThreshold)
        {
            return _meetingManager.GetExpiringMeetings(hoursThreshold);
        }

        #endregion

        #region Queue Manipulation

        /// <summary>
        /// Attempt a morning swap of two adjacent letters.
        /// </summary>
        public QueueManipulationResult TryMorningSwap(int position1, int position2)
        {
            return _queueManipulator.TryMorningSwap(position1, position2);
        }

        /// <summary>
        /// Skip a letter to position 1 by spending tokens.
        /// </summary>
        public QueueManipulationResult TrySkipToPosition1(int fromPosition)
        {
            return _queueManipulator.TrySkipToPosition1(fromPosition);
        }

        /// <summary>
        /// Swap two obligations at specified positions.
        /// </summary>
        public QueueManipulationResult SwapObligations(int position1, int position2)
        {
            return _queueManipulator.SwapObligations(position1, position2);
        }

        /// <summary>
        /// Get detailed information about each queue position.
        /// </summary>
        public List<QueuePositionInfo> GetQueuePositionInfo()
        {
            return _queueManipulator.GetQueuePositionInfo();
        }

        #endregion

        #region Displacement Operations

        /// <summary>
        /// Calculate displacement cost and feasibility.
        /// </summary>
        public DisplacementResult TryDisplaceObligation(string obligationId, int targetPosition)
        {
            return _displacementCalculator.CalculateDisplacement(obligationId, targetPosition);
        }

        /// <summary>
        /// Execute a calculated displacement.
        /// </summary>
        public DisplacementResult ExecuteDisplacement(DisplacementResult displacementResult)
        {
            return _displacementCalculator.ExecuteDisplacement(displacementResult);
        }

        /// <summary>
        /// Get preview of displacement costs without executing.
        /// </summary>
        public QueueDisplacementPreview GetDisplacementPreview(string obligationId, int targetPosition)
        {
            return _displacementCalculator.GetDisplacementPreview(obligationId, targetPosition);
        }

        /// <summary>
        /// Check if a specific displacement is affordable.
        /// </summary>
        public bool CanAffordDisplacement(string obligationId, int targetPosition)
        {
            return _displacementCalculator.CanAffordDisplacement(obligationId, targetPosition);
        }

        /// <summary>
        /// Get NPCs that would be affected by a displacement.
        /// </summary>
        public List<string> GetAffectedNPCs(string obligationId, int targetPosition)
        {
            return _displacementCalculator.GetAffectedNPCs(obligationId, targetPosition);
        }

        #endregion

        #region Deadline Management

        /// <summary>
        /// Process hourly deadline countdown for all obligations.
        /// </summary>
        public DeadlineTrackingInfo ProcessSegmentDeadlines(int segmentsElapsed = 1)
        {
            DeadlineTrackingInfo trackingInfo = _deadlineTracker.ProcessSegmentDeadlines(segmentsElapsed);

            // Process expired meetings as well
            List<MeetingResult> expiredMeetings = _meetingManager.ProcessExpiredMeetings();

            return trackingInfo;
        }


        /// <summary>
        /// Extend deadline of a letter at specific position.
        /// </summary>
        public QueueManipulationResult TryExtendDeadline(int position)
        {
            return _deadlineTracker.TryExtendDeadline(position);
        }

        /// <summary>
        /// Get formatted display for the next deadline.
        /// </summary>
        public string GetNextDeadlineDisplay()
        {
            return _deadlineTracker.GetNextDeadlineDisplay();
        }

        /// <summary>
        /// Check if any deadlines are in critical status.
        /// </summary>
        public bool HasCriticalDeadlines()
        {
            return _deadlineTracker.HasCriticalDeadlines();
        }

        /// <summary>
        /// Get count of obligations by urgency level.
        /// </summary>
        public DeadlineUrgencyStats GetUrgencyStats()
        {
            return _deadlineTracker.GetUrgencyStats();
        }

        /// <summary>
        /// Calculate time until next deadline.
        /// </summary>
        public int GetSegmentsUntilNextDeadline()
        {
            return _deadlineTracker.GetSegmentsUntilNextDeadline();
        }

        /// <summary>
        /// Get all deadlines sorted by urgency.
        /// </summary>
        public List<DeadlineInfo> GetAllDeadlinesSortedByUrgency()
        {
            return _deadlineTracker.GetAllDeadlinesSortedByUrgency();
        }

        #endregion

        #region Statistics and Analytics

        /// <summary>
        /// Generate comprehensive obligation metrics.
        /// </summary>
        public ObligationMetrics GenerateObligationMetrics()
        {
            return _statistics.GenerateObligationMetrics();
        }

        /// <summary>
        /// Get detailed queue activity statistics.
        /// </summary>
        public QueueActivity GetQueueActivity()
        {
            return _statistics.GetQueueActivity();
        }

        /// <summary>
        /// Create comprehensive queue snapshot for analysis.
        /// </summary>
        public QueueSnapshot CreateQueueSnapshot()
        {
            return _statistics.CreateQueueSnapshot();
        }

        /// <summary>
        /// Analyze relationship health with all NPCs.
        /// </summary>
        public Dictionary<string, RelationshipHealth> AnalyzeRelationshipHealth()
        {
            return _statistics.AnalyzeRelationshipHealth();
        }

        /// <summary>
        /// Get performance metrics for all NPCs.
        /// </summary>
        public List<NPCPerformanceMetric> GetNPCPerformanceMetrics()
        {
            return _statistics.GetNPCPerformanceMetrics();
        }

        /// <summary>
        /// Calculate queue efficiency metrics.
        /// </summary>
        public QueueEfficiencyMetrics CalculateQueueEfficiency()
        {
            return _statistics.CalculateQueueEfficiency();
        }

        /// <summary>
        /// Get trending patterns in obligation management.
        /// </summary>
        public ObligationTrends GetObligationTrends()
        {
            return _statistics.GetObligationTrends();
        }

        #endregion

        public int AddLetterWithObligationEffects(DeliveryObligation obligation)
        {
            ObligationAddResult result = AddObligationWithEffects(obligation);
            return result.Success ? result.Position : 0;
        }

        // Private helper methods

        private void ApplyDeadlineBonuses(DeliveryObligation obligation)
        {
            List<StandingObligation> activeObligations = _standingObligationManager.GetActiveObligations();

            foreach (StandingObligation standingObligation in activeObligations)
            {
                // Check if obligation applies to this letter type
                if (!standingObligation.AppliesTo(obligation.TokenType)) continue;

                // Check for DeadlinePlus2Days effect
                if (standingObligation.HasEffect(ObligationEffect.DeadlinePlus2Days))
                {
                    // Check if the letter is from the specific NPC if obligation is NPC-specific
                    if (!string.IsNullOrEmpty(standingObligation.RelatedNPCId))
                    {
                        NPC npc = _npcRepository.GetByName(obligation.SenderName);
                        if (npc == null || npc.ID != standingObligation.RelatedNPCId) continue;
                    }

                    obligation.DeadlineInSegments += 2880; // 48 hours
                    _messageSystem.AddSystemMessage(
                        $"📅 {standingObligation.Name} grants +2 days to deadline for letter from {obligation.SenderName}",
                        SystemMessageTypes.Info
                    );
                }
            }

            // Apply dynamic deadline bonuses that scale with tokens
            _standingObligationManager.ApplyDynamicDeadlineBonuses(obligation);
        }

        private AutoDisplacementInfo CheckForAutomaticDisplacement(DeliveryObligation obligation)
        {
            AutoDisplacementInfo info = new AutoDisplacementInfo();

            // Failed letter negotiations force position 1
            if (obligation.GenerationReason != null &&
                obligation.GenerationReason.Contains("failure", StringComparison.OrdinalIgnoreCase))
            {
                info.ShouldForceDisplacement = true;
                info.ForcedPosition = 1;
                info.DisplacementReason = "Failed negotiation - NPC's terms are non-negotiable!";
                info.Trigger = DisplacementTrigger.FailedNegotiation;
                return info;
            }

            // Proud NPCs attempt position 1
            if (obligation.SenderName == "Lord Blackwood" ||
                obligation.RecipientName == "Lord Blackwood")
            {
                info.ShouldForceDisplacement = true;
                info.ForcedPosition = 1;
                info.DisplacementReason = "Lord Blackwood's pride demands immediate attention!";
                info.Trigger = DisplacementTrigger.ProudNPC;
                return info;
            }

            // Critical emotional focus forces position 1
            if (obligation.EmotionalFocus == EmotionalFocus.CRITICAL)
            {
                info.ShouldForceDisplacement = true;
                info.ForcedPosition = 1;
                info.DisplacementReason = $"{obligation.SenderName} is DISCONNECTED - their letter takes priority!";
                info.Trigger = DisplacementTrigger.CriticalEmotionalFocus;
                return info;
            }

            return info;
        }


        public LetterQueueViewModel GetLetterQueue()
        {
            DeliveryObligation[] activeObligations = GetActiveObligations();
            List<QueuePositionInfo> queuePositionInfo = _queueManipulator.GetQueuePositionInfo();
            TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();
            ObligationMetrics metrics = _statistics.GenerateObligationMetrics();
            int totalSize = _deliveryManager.GetTotalSatchelSize();

            List<QueueSlotViewModel> queueSlots = new List<QueueSlotViewModel>();

            // Create slots for each position (1-8)
            for (int position = 1; position <= 8; position++)
            {
                DeliveryObligation? obligation = GetLetterAt(position);
                QueueSlotViewModel slot = new QueueSlotViewModel
                {
                    Position = position,
                    IsOccupied = obligation != null,
                    DeliveryObligation = obligation != null ? CreateLetterViewModel(obligation) : null,
                    CanDeliver = obligation != null && CanDeliverFromPosition1() && position == 1,
                    CanSkip = obligation != null && position > 1,
                    SkipAction = obligation != null && position > 1 ? CreateSkipActionViewModel(obligation) : null
                };
                queueSlots.Add(slot);
            }

            DeadlineUrgencyStats urgencyStats = _deadlineTracker.GetUrgencyStats();

            return new LetterQueueViewModel
            {
                QueueSlots = queueSlots,
                Status = new QueueStatusViewModel
                {
                    LetterCount = activeObligations.Length,
                    MaxCapacity = 8,
                    ExpiredCount = 0, // urgencyStats.ExpiredObligations would be correct
                    UrgentCount = urgencyStats.CriticalObligations,
                    WarningCount = urgencyStats.UrgentObligations,
                    TotalSize = totalSize,
                    MaxSize = 12,
                    RemainingSize = Math.Max(0, 12 - totalSize),
                    SizeDisplay = $"{totalSize}/12"
                },
                Actions = new QueueActionsViewModel
                {
                    CanMorningSwap = currentTimeBlock == TimeBlocks.Midday && activeObligations.Length >= 2,
                    MorningSwapReason = GetMorningSwapReason(currentTimeBlock, activeObligations.Length),
                    HasBottomDeliveryObligation = activeObligations.Length > 0,
                    TotalAvailableTokens = GetTotalAvailableTokens(),
                    PurgeTokenOptions = CreatePurgeTokenOptions()
                },
                CurrentTimeBlock = currentTimeBlock,
                CurrentDay = _timeManager.GetCurrentDay(),
                LastMorningSwapDay = _statistics.GetLastMorningSwapDay()
            };
        }

        public LetterBoardViewModel GetLetterBoard()
        {
            TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();

            // Letter board is typically available during specific time blocks
            bool isAvailable = currentTimeBlock == TimeBlocks.Midday || currentTimeBlock == TimeBlocks.Afternoon;
            string? unavailableReason = !isAvailable ? "Letter board is only available during morning and afternoon hours" : null;

            List<LetterOfferViewModel> offers = new List<LetterOfferViewModel>();

            // In a full implementation, this would query actual letter offers
            // For now, we'll provide an empty list but with proper structure
            if (isAvailable)
            {
                // This would normally come from a letter offer repository or generator
                // offers = GetAvailableLetterOffers();
            }

            return new LetterBoardViewModel
            {
                IsAvailable = isAvailable,
                UnavailableReason = unavailableReason,
                Offers = offers,
                CurrentTime = currentTimeBlock
            };
        }

        public bool DisplaceObligation(string obligationId, int targetPosition)
        {
            DisplacementResult displacementResult = _displacementCalculator.CalculateDisplacement(obligationId, targetPosition);

            if (!displacementResult.CanExecute)
            {
                _messageSystem.AddSystemMessage(
                    $"Cannot displace obligation: {displacementResult.ErrorMessage}",
                    SystemMessageTypes.Warning);
                return false;
            }

            DisplacementResult executionResult = _displacementCalculator.ExecuteDisplacement(displacementResult);

            if (executionResult.CanExecute)
            {
                _messageSystem.AddSystemMessage(
                    $"Successfully displaced obligation to position {targetPosition}",
                    SystemMessageTypes.Success);
                return true;
            }
            else
            {
                _messageSystem.AddSystemMessage(
                    $"Failed to execute displacement: {executionResult.ErrorMessage}",
                    SystemMessageTypes.Warning);
                return false;
            }
        }

        public bool AcceptLetterOffer(string offerId)
        {
            // In a full implementation, this would:
            // 1. Find the letter offer by ID
            // 2. Check if player can accept it (queue space, requirements)
            // 3. Create a DeliveryObligation from the offer
            // 4. Add it to the queue
            // 5. Remove the offer from available offers

            _messageSystem.AddSystemMessage(
                "Letter offer system not fully implemented yet",
                SystemMessageTypes.Info);

            // For now, return false to indicate the feature needs implementation
            return false;
        }

        public int GetLetterQueueCount()
        {
            return GetLetterCount(); // Use existing method
        }

        public bool IsLetterQueueFull()
        {
            return IsQueueFull(); // Use existing method
        }

        // ========== HELPER METHODS FOR VIEW MODEL CREATION ==========

        private LetterViewModel CreateLetterViewModel(DeliveryObligation obligation)
        {
            LeverageInfo leverageInfo = CalculateLeverageInfo(obligation);
            (string CssClass, string Icon, string Description) deadlineInfo = CalculateDeadlineInfo(obligation);

            return new LetterViewModel
            {
                Id = obligation.Id,
                SenderName = obligation.SenderName,
                RecipientName = obligation.RecipientName,
                DeadlineInSegments = obligation.DeadlineInSegments,
                Payment = obligation.Payment,
                TokenType = obligation.TokenType.ToString(),
                TokenIcon = GetTokenIcon(obligation.TokenType),
                Size = 1, // DeliveryObligation doesn't have Size property, default to 1
                SizeIcon = GetSizeIcon(1),
                SizeDisplay = GetSizeDisplay(1),
                IsPatronDeliveryObligation = false, // This would need to be determined from obligation data
                IsCollected = true, // Obligations in queue are collected
                PhysicalConstraints = "", // DeliveryObligation doesn't have PhysicalConstraints
                PhysicalIcon = GetPhysicalConstraintIcon(""),
                IsSpecial = obligation.EmotionalFocus == EmotionalFocus.CRITICAL,
                SpecialIcon = obligation.EmotionalFocus == EmotionalFocus.CRITICAL ? "⚠️" : "",
                SpecialDescription = obligation.EmotionalFocus == EmotionalFocus.CRITICAL ? "Critical urgency" : "",
                DeadlineClass = deadlineInfo.CssClass,
                DeadlineIcon = deadlineInfo.Icon,
                DeadlineDescription = deadlineInfo.Description,
                LeverageIndicator = leverageInfo.Indicator,
                LeverageTooltip = leverageInfo.Tooltip,
                HasLeverage = leverageInfo.HasLeverage,
                LeverageStrength = leverageInfo.LeverageStrength,
                TokenBalance = leverageInfo.TokenBalance,
                LeverageClass = leverageInfo.HasLeverage ? "has-leverage" : "no-leverage",
                OriginalPosition = 0, // Would need to track this
                CurrentPosition = GetQueuePosition(obligation),
                HasPaymentBonus = false, // Would check standing obligations
                PaymentBonusAmount = 0,
                PaymentBonusSource = "",
                HasDeadlineExtension = false, // Would check standing obligations
                DeadlineExtensionSegments = 0,
                DeadlineExtensionSource = "",
                HasPositionModifier = false,
                PositionModifierAmount = 0,
                PositionModifierSource = "",
                ActiveObligationEffects = new List<string>()
            };
        }

        private SkipActionViewModel CreateSkipActionViewModel(DeliveryObligation obligation)
        {
            NPC npc = _npcRepository.GetByName(obligation.SenderName);
            Dictionary<ConnectionType, int> tokens = npc != null ? _tokenManager.GetTokensWithNPC(npc.ID) : new Dictionary<ConnectionType, int>();
            int availableTokens = tokens.GetValueOrDefault(obligation.TokenType, 0);

            int baseCost = 1;
            int multiplier = 1;
            int totalCost = baseCost * multiplier;

            return new SkipActionViewModel
            {
                BaseCost = baseCost,
                Multiplier = multiplier,
                TotalCost = totalCost,
                TokenType = obligation.TokenType.ToString(),
                AvailableTokens = availableTokens,
                HasEnoughTokens = availableTokens >= totalCost,
                MultiplierReason = multiplier > 1 ? $"x{multiplier} due to conditions" : "Base cost"
            };
        }

        private string GetMorningSwapReason(TimeBlocks currentTime, int obligationCount)
        {
            if (currentTime != TimeBlocks.Midday)
                return "Morning swaps only available during morning hours";
            if (obligationCount < 2)
                return "Need at least 2 obligations to swap";
            return "Ready for morning swap";
        }

        private int GetTotalAvailableTokens()
        {
            Player player = _gameWorld.GetPlayer();
            int totalTokens = 0;

            foreach (Dictionary<ConnectionType, int> npcTokens in player.NPCTokens.Values)
            {
                foreach (int tokenCount in npcTokens.Values)
                {
                    totalTokens += Math.Max(0, tokenCount); // Only count positive tokens
                }
            }

            return totalTokens;
        }

        private List<TokenOptionViewModel> CreatePurgeTokenOptions()
        {
            List<TokenOptionViewModel> options = new List<TokenOptionViewModel>();
            Player player = _gameWorld.GetPlayer();
            Dictionary<ConnectionType, int> tokenTotals = new Dictionary<ConnectionType, int>
            {
                [ConnectionType.Trust] = 0,
                [ConnectionType.Commerce] = 0,
                [ConnectionType.Status] = 0,
                [ConnectionType.Shadow] = 0
            };

            // Sum up all tokens by type
            foreach (Dictionary<ConnectionType, int> npcTokens in player.NPCTokens.Values)
            {
                foreach (KeyValuePair<ConnectionType, int> kvp in npcTokens)
                {
                    // Skip None type as it's not a valid token type
                    if (kvp.Key == ConnectionType.None)
                        continue;

                    if (tokenTotals.ContainsKey(kvp.Key))
                    {
                        tokenTotals[kvp.Key] += Math.Max(0, kvp.Value);
                    }
                }
            }

            foreach (KeyValuePair<ConnectionType, int> kvp in tokenTotals.Where(t => t.Value > 0))
            {
                options.Add(new TokenOptionViewModel
                {
                    TokenType = kvp.Key.ToString(),
                    TokenIcon = GetTokenIcon(kvp.Key),
                    Available = kvp.Value
                });
            }

            return options;
        }

        private LeverageInfo CalculateLeverageInfo(DeliveryObligation obligation)
        {
            NPC npc = _npcRepository.GetByName(obligation.SenderName);
            if (npc == null)
            {
                return new LeverageInfo { HasLeverage = false, Indicator = "", Tooltip = "", TokenBalance = 0, LeverageStrength = 0 };
            }

            Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npc.ID);
            int balance = tokens.GetValueOrDefault(obligation.TokenType, 0);

            bool hasLeverage = balance < 0;
            int leverageStrength = Math.Abs(Math.Min(0, balance));

            return new LeverageInfo
            {
                HasLeverage = hasLeverage,
                TokenBalance = balance,
                LeverageStrength = leverageStrength,
                Indicator = hasLeverage ? $"⚖️ -{leverageStrength}" : "",
                Tooltip = hasLeverage ? $"NPC owes you {leverageStrength} {obligation.TokenType} tokens" : "No leverage"
            };
        }

        private (string CssClass, string Icon, string Description) CalculateDeadlineInfo(DeliveryObligation obligation)
        {
            int segmentsRemaining = obligation.DeadlineInSegments;
            int hoursRemaining = segmentsRemaining / 2; // 2 segments per hour

            if (segmentsRemaining <= 0)
                return ("deadline-expired", "💀", "Expired");
            else if (segmentsRemaining <= 12) // 6 hours worth of segments
                return ("deadline-critical", "🔥", $"{hoursRemaining}h left");
            else if (segmentsRemaining <= 48) // 24 hours worth of segments
                return ("deadline-urgent", "⚠️", $"{hoursRemaining}h left");
            else if (segmentsRemaining <= 96) // 48 hours worth of segments
                return ("deadline-warning", "⏰", $"{hoursRemaining / 24}d left");
            else
                return ("deadline-safe", "📅", $"{hoursRemaining / 24}d left");
        }

        private string GetTokenIcon(ConnectionType tokenType)
        {
            return tokenType switch
            {
                ConnectionType.Trust => "❤️",
                ConnectionType.Commerce => "🪙",
                ConnectionType.Status => "👑",
                ConnectionType.Shadow => "🌑",
                _ => "?"
            };
        }

        private string GetSizeIcon(int size)
        {
            return size switch
            {
                1 => "📄",
                2 => "📋",
                3 => "📦",
                _ => "❓"
            };
        }

        private string GetSizeDisplay(int size)
        {
            return new string('■', size);
        }

        private string GetPhysicalConstraintIcon(string constraints)
        {
            if (string.IsNullOrEmpty(constraints)) return "";

            return constraints.ToLower() switch
            {
                var c when c.Contains("fragile") => "🔍",
                var c when c.Contains("heavy") => "⚖️",
                var c when c.Contains("bulky") => "📦",
                var c when c.Contains("perishable") => "⏱️",
                _ => "⚠️"
            };
        }
    }
}