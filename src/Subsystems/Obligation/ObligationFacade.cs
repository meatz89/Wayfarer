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
            var player = _gameWorld.GetPlayer();
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
            var autoDisplacement = CheckForAutomaticDisplacement(obligation);
            if (autoDisplacement.ShouldForceDisplacement)
            {
                var displacementResult = _displacementCalculator.ExecuteAutomaticDisplacement(
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
            var addResult = _queueManipulator.AddObligationWithLeverage(obligation);
            
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
            var result = _queueManipulator.AddObligation(obligation);
            
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
            var result = _queueManipulator.RemoveObligationFromQueue(position);
            
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
        public MeetingResult ScheduleMeeting(string npcId, string reason, int deadlineInMinutes, StakeType stakes = StakeType.REPUTATION)
        {
            return _meetingManager.ScheduleMeeting(npcId, reason, deadlineInMinutes, stakes);
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
        public DeadlineTrackingInfo ProcessHourlyDeadlines(int hoursElapsed = 1)
        {
            var trackingInfo = _deadlineTracker.ProcessHourlyDeadlines(hoursElapsed);
            
            // Process expired meetings as well
            var expiredMeetings = _meetingManager.ProcessExpiredMeetings();
            
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
        public int GetMinutesUntilNextDeadline()
        {
            return _deadlineTracker.GetMinutesUntilNextDeadline();
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

        #region Legacy Compatibility Methods

        /// <summary>
        /// Legacy method for backward compatibility.
        /// </summary>
        public int? GetObligationPosition(string obligationId)
        {
            var obligation = GetActiveObligations().FirstOrDefault(o => o.Id == obligationId);
            if (obligation == null) return null;
            
            var position = _queueManipulator.GetQueuePosition(obligation);
            return position > 0 ? position : (int?)null;
        }

        /// <summary>
        /// Legacy method for backward compatibility.
        /// </summary>
        public int? GetLetterPosition(string obligationId)
        {
            return GetObligationPosition(obligationId);
        }

        /// <summary>
        /// Legacy method for backward compatibility.
        /// </summary>
        public int AddLetterWithObligationEffects(DeliveryObligation obligation)
        {
            var result = AddObligationWithEffects(obligation);
            return result.Success ? result.Position : 0;
        }

        /// <summary>
        /// Legacy method for backward compatibility.
        /// </summary>
        public void MoveLetterToPosition(DeliveryObligation letter, int targetPosition)
        {
            _queueManipulator.MoveObligationToPosition(letter, targetPosition);
        }

        /// <summary>
        /// Legacy method for backward compatibility.
        /// </summary>
        public void RecordLetterDelivery(DeliveryObligation letter)
        {
            _deliveryManager.RecordLetterDelivery(letter);
        }

        /// <summary>
        /// Legacy method for backward compatibility.
        /// </summary>
        public void RecordLetterSkip(DeliveryObligation letter)
        {
            _deliveryManager.RecordLetterSkip(letter);
        }

        #endregion

        // Private helper methods

        private void ApplyDeadlineBonuses(DeliveryObligation obligation)
        {
            var activeObligations = _standingObligationManager.GetActiveObligations();

            foreach (var standingObligation in activeObligations)
            {
                // Check if obligation applies to this letter type
                if (!standingObligation.AppliesTo(obligation.TokenType)) continue;

                // Check for DeadlinePlus2Days effect
                if (standingObligation.HasEffect(ObligationEffect.DeadlinePlus2Days))
                {
                    // Check if the letter is from the specific NPC if obligation is NPC-specific
                    if (!string.IsNullOrEmpty(standingObligation.RelatedNPCId))
                    {
                        var npc = _npcRepository.GetByName(obligation.SenderName);
                        if (npc == null || npc.ID != standingObligation.RelatedNPCId) continue;
                    }

                    obligation.DeadlineInMinutes += 2880; // 48 hours
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
            var info = new AutoDisplacementInfo();

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

            // Critical emotional weight forces position 1
            if (obligation.EmotionalWeight == EmotionalWeight.CRITICAL)
            {
                info.ShouldForceDisplacement = true;
                info.ForcedPosition = 1;
                info.DisplacementReason = $"{obligation.SenderName} is DESPERATE - their letter takes priority!";
                info.Trigger = DisplacementTrigger.CriticalEmotionalWeight;
                return info;
            }

            return info;
        }
        
        // ========== MISSING METHODS (STUBS) ==========
        
        public LetterQueueViewModel GetLetterQueue()
        {
            // TODO: Implement proper letter queue view model
            return new LetterQueueViewModel
            {
                QueueSlots = new List<QueueSlotViewModel>(),
                Status = new QueueStatusViewModel
                {
                    LetterCount = 0,
                    MaxCapacity = 8,
                    ExpiredCount = 0,
                    UrgentCount = 0,
                    WarningCount = 0,
                    TotalSize = 0,
                    MaxSize = 12,
                    RemainingSize = 12,
                    SizeDisplay = "0/12"
                },
                Actions = new QueueActionsViewModel
                {
                    CanMorningSwap = false,
                    MorningSwapReason = "No obligations to swap",
                    HasBottomDeliveryObligation = false,
                    TotalAvailableTokens = 0,
                    PurgeTokenOptions = new List<TokenOptionViewModel>()
                }
            };
        }
        
        public LetterBoardViewModel GetLetterBoard()
        {
            // TODO: Implement proper letter board view model
            return new LetterBoardViewModel
            {
                IsAvailable = true,
                UnavailableReason = null,
                Offers = new List<LetterOfferViewModel>(),
                CurrentTime = TimeBlocks.Dawn
            };
        }
        
        public bool DisplaceObligation(string obligationId, int targetPosition)
        {
            // TODO: Implement obligation displacement
            return false;
        }
        
        public bool AcceptLetterOffer(string offerId)
        {
            // TODO: Implement letter offer acceptance
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
    }
}