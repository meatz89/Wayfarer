using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Subsystems.ObligationSubsystem;

namespace Wayfarer.Subsystems.ObligationSubsystem
{
    /// <summary>
    /// Manages time-based deadline tracking, expiration processing, and deadline manipulation.
    /// Handles hourly deadline countdown, expiration consequences, and deadline extensions.
    /// </summary>
    public class DeadlineTracker
    {
        private readonly GameWorld _gameWorld;
        private readonly NPCRepository _npcRepository;
        private readonly MessageSystem _messageSystem;
        private readonly TokenMechanicsManager _tokenManager;
        private readonly GameConfiguration _config;

        public DeadlineTracker(
            GameWorld gameWorld,
            NPCRepository npcRepository,
            MessageSystem messageSystem,
            TokenMechanicsManager tokenManager,
            GameConfiguration config)
        {
            _gameWorld = gameWorld;
            _npcRepository = npcRepository;
            _messageSystem = messageSystem;
            _tokenManager = tokenManager;
            _config = config;
        }

        /// <summary>
        /// Process segment deadline countdown for all obligations.
        /// Updates deadlines, handles expirations, and compacts the queue.
        /// </summary>
        public DeadlineTrackingInfo ProcessSegmentDeadlines(int segmentsElapsed = 1)
        {
            DeadlineTrackingInfo trackingInfo = new DeadlineTrackingInfo
            {
                SegmentsElapsed = segmentsElapsed
            };

            if (segmentsElapsed <= 0) return trackingInfo;

            DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

            // Phase 1: Update deadlines and identify expired obligations
            List<DeliveryObligation> expiredObligations = new List<DeliveryObligation>();
            for (int i = 0; i < _config.LetterQueue.MaxQueueSize; i++)
            {
                if (queue[i] != null)
                {
                    queue[i].DeadlineInSegments -= segmentsElapsed;
                    if (queue[i].DeadlineInSegments <= 0)
                    {
                        expiredObligations.Add(queue[i]);
                    }
                }
            }

            // Phase 2: Process expired obligations
            foreach (DeliveryObligation expiredObligation in expiredObligations)
            {
                ProcessExpiredObligation(expiredObligation);
                trackingInfo.ExpiredObligationIds.Add(expiredObligation.Id);
            }

            // Phase 3: Process expired meetings
            List<MeetingObligation> expiredMeetings = ProcessExpiredMeetings(segmentsElapsed);
            trackingInfo.ExpiredMeetingIds.AddRange(expiredMeetings.Select(m => m.Id));

            // Phase 4: Compact queue removing expired items
            CompactQueueAfterExpiration();

            // Phase 5: Identify still-expiring items for warnings
            trackingInfo.ExpiringObligations = GetExpiringObligations(24); // Next 24 segments (1 day)
            trackingInfo.ExpiringMeetings = GetExpiringMeetings(24);

            return trackingInfo;
        }

        /// <summary>
        /// Get obligations that will expire within the specified segments threshold.
        /// </summary>
        public List<DeliveryObligation> GetExpiringObligations(int segmentsThreshold)
        {
            return GetActiveObligations()
                .Where(o => o.DeadlineInSegments <= segmentsThreshold && o.DeadlineInSegments > 0)
                .OrderBy(o => o.DeadlineInSegments)
                .ToList();
        }

        /// <summary>
        /// Get meetings that will expire within the specified segments threshold.
        /// </summary>
        public List<MeetingObligation> GetExpiringMeetings(int segmentsThreshold)
        {
            Player player = _gameWorld.GetPlayer();
            return player.MeetingObligations
                .Where(m => m.DeadlineInSegments <= segmentsThreshold && m.DeadlineInSegments > 0)
                .OrderBy(m => m.DeadlineInSegments)
                .ToList();
        }

        /// <summary>
        /// Get the most urgent obligation (closest to expiring).
        /// </summary>
        public DeliveryObligation GetMostUrgentObligation()
        {
            return GetActiveObligations()
                .Where(o => o.DeadlineInSegments > 0)
                .OrderBy(o => o.DeadlineInSegments)
                .FirstOrDefault();
        }

        /// <summary>
        /// Get the most urgent meeting (closest to expiring).
        /// </summary>
        public MeetingObligation GetMostUrgentMeeting()
        {
            Player player = _gameWorld.GetPlayer();
            return player.MeetingObligations
                .Where(m => m.DeadlineInSegments > 0)
                .OrderBy(m => m.DeadlineInSegments)
                .FirstOrDefault();
        }

        /// <summary>
        /// Extend the deadline of a specific obligation by paying tokens.
        /// </summary>
        public QueueManipulationResult TryExtendDeadline(int position)
        {
            QueueManipulationResult result = new QueueManipulationResult
            {
                OperationType = "Extend Deadline",
                Position = position
            };

            if (position < 1 || position > _config.LetterQueue.MaxQueueSize)
            {
                result.ErrorMessage = "Invalid position";
                return result;
            }

            DeliveryObligation letter = GetLetterAt(position);
            if (letter == null)
            {
                result.ErrorMessage = "No letter at specified position";
                return result;
            }

            string senderId = GetNPCIdByName(letter.SenderName);

            // Show negotiation attempt
            _messageSystem.AddSystemMessage(
                $"📅 Negotiating deadline extension with {letter.SenderName}...",
                SystemMessageTypes.Info
            );

            // Show current deadline pressure
            string urgency = letter.DeadlineInSegments <= 2 ? " 🆘 CRITICAL!" : "";
            int currentDays = letter.DeadlineInSegments / 24; // 24 segments per day
            _messageSystem.AddSystemMessage(
                $"  • Current deadline: {currentDays} days{urgency}",
                letter.DeadlineInSegments <= 2 ? SystemMessageTypes.Danger : SystemMessageTypes.Info
            );

            // Check token cost (2 matching tokens)
            Dictionary<ConnectionType, int> extensionCost = new Dictionary<ConnectionType, int> { { letter.TokenType, 2 } };

            if (!ValidateTokenAvailability(extensionCost))
            {
                result.ErrorMessage = $"Insufficient {letter.TokenType} tokens! Need 2";
                result.TokensCost = extensionCost;

                _messageSystem.AddSystemMessage(
                    $"  Insufficient {letter.TokenType} tokens! Need 2, have {_tokenManager.GetTokenCount(letter.TokenType)}",
                    SystemMessageTypes.Danger
                );
                return result;
            }

            // Spend the tokens
            _messageSystem.AddSystemMessage(
                $"  • Offering 2 {letter.TokenType} tokens to {letter.SenderName}...",
                SystemMessageTypes.Warning
            );

            if (!_tokenManager.SpendTokensWithNPC(letter.TokenType, 2, senderId))
            {
                result.ErrorMessage = "Failed to spend tokens";
                return result;
            }

            // Extend the deadline by 2 days (32 segments)
            int oldDeadlineSegments = letter.DeadlineInSegments;
            letter.DeadlineInSegments += 48; // 2 days * 24 segments per day

            result.Success = true;
            result.AffectedObligation = letter;
            result.TokensCost = extensionCost;

            // Success narrative
            _messageSystem.AddSystemMessage(
                $"✅ {letter.SenderName} grants a 2-day extension!",
                SystemMessageTypes.Success
            );

            int newDeadlineDays = letter.DeadlineInSegments / 24;
            int oldDeadlineDays = oldDeadlineSegments / 24;
            _messageSystem.AddSystemMessage(
                $"  • New deadline: {newDeadlineDays} days (was {oldDeadlineDays})",
                SystemMessageTypes.Info
            );

            _messageSystem.AddSystemMessage(
                $"  • \"Just this once,\" {letter.SenderName} says, \"but don't make a habit of it.\"",
                SystemMessageTypes.Info
            );

            return result;
        }

        /// <summary>
        /// Get a formatted display string for the next deadline.
        /// </summary>
        public string GetNextDeadlineDisplay()
        {
            DeliveryObligation mostUrgent = GetMostUrgentObligation();
            if (mostUrgent != null)
            {
                return $"⏰ {mostUrgent.SegmentsUntilDeadline}s";
            }

            MeetingObligation mostUrgentMeeting = GetMostUrgentMeeting();
            if (mostUrgentMeeting != null)
            {
                return $"📅 {mostUrgentMeeting.DeadlineInSegments_Display}s";
            }

            return "";
        }

        /// <summary>
        /// Check if any obligations or meetings are in critical deadline status.
        /// </summary>
        public bool HasCriticalDeadlines()
        {
            List<DeliveryObligation> criticalObligations = GetExpiringObligations(2); // 2 segments
            List<MeetingObligation> criticalMeetings = GetExpiringMeetings(2);

            return criticalObligations.Any() || criticalMeetings.Any();
        }

        /// <summary>
        /// Get count of obligations by urgency level.
        /// </summary>
        public DeadlineUrgencyStats GetUrgencyStats()
        {
            DeliveryObligation[] activeObligations = GetActiveObligations();
            List<MeetingObligation> activeMeetings = GetActiveMeetings();

            return new DeadlineUrgencyStats
            {
                CriticalObligations = activeObligations.Count(o => o.DeadlineInSegments <= 2), // ≤2 segments
                UrgentObligations = activeObligations.Count(o => o.DeadlineInSegments > 2 && o.DeadlineInSegments <= 4), // 2-4 segments
                NormalObligations = activeObligations.Count(o => o.DeadlineInSegments > 4),
                CriticalMeetings = activeMeetings.Count(m => m.IsCritical),
                UrgentMeetings = activeMeetings.Count(m => m.IsUrgent && !m.IsCritical),
                NormalMeetings = activeMeetings.Count(m => !m.IsUrgent)
            };
        }

        /// <summary>
        /// Calculate time until next deadline in segments.
        /// </summary>
        public int GetSegmentsUntilNextDeadline()
        {
            DeliveryObligation nextObligation = GetMostUrgentObligation();
            MeetingObligation nextMeeting = GetMostUrgentMeeting();

            int obligationSegments = nextObligation?.DeadlineInSegments ?? int.MaxValue;
            int meetingSegments = nextMeeting?.DeadlineInSegments ?? int.MaxValue;

            int nextDeadline = Math.Min(obligationSegments, meetingSegments);
            return nextDeadline == int.MaxValue ? -1 : nextDeadline;
        }

        /// <summary>
        /// Get all deadlines sorted by urgency for dashboard display.
        /// </summary>
        public List<DeadlineInfo> GetAllDeadlinesSortedByUrgency()
        {
            List<DeadlineInfo> deadlines = new List<DeadlineInfo>();

            // Add obligation deadlines
            foreach (DeliveryObligation obligation in GetActiveObligations())
            {
                deadlines.Add(new DeadlineInfo
                {
                    Type = "Letter",
                    Title = $"Deliver to {obligation.RecipientName}",
                    From = obligation.SenderName,
                    DeadlineInSegments = obligation.DeadlineInSegments,
                    IsUrgent = obligation.DeadlineInSegments <= 4,
                    IsCritical = obligation.DeadlineInSegments <= 2,
                    ObligationId = obligation.Id
                });
            }

            // Add meeting deadlines
            foreach (MeetingObligation meeting in GetActiveMeetings())
            {
                deadlines.Add(new DeadlineInfo
                {
                    Type = "Meeting",
                    Title = $"Meet with {meeting.RequesterName}",
                    From = meeting.RequesterName,
                    DeadlineInSegments = meeting.DeadlineInSegments,
                    IsUrgent = meeting.IsUrgent,
                    IsCritical = meeting.IsCritical,
                    ObligationId = meeting.Id
                });
            }

            return deadlines.OrderBy(d => d.DeadlineInSegments).ToList();
        }

        // Private helper methods

        private void ProcessExpiredObligation(DeliveryObligation expiredObligation)
        {
            string senderId = GetNPCIdByName(expiredObligation.SenderName);
            if (string.IsNullOrEmpty(senderId)) return;

            // Record expiry in history first
            RecordExpiryInHistory(senderId);

            // Apply relationship consequences
            ApplyExpirationPenalty(expiredObligation, senderId);

            // Show dramatic failure message
            ShowExpiryFailure(expiredObligation);
        }

        private List<MeetingObligation> ProcessExpiredMeetings(int segmentsElapsed)
        {
            Player player = _gameWorld.GetPlayer();
            List<MeetingObligation> expiredMeetings = new List<MeetingObligation>();

            foreach (MeetingObligation? meeting in player.MeetingObligations.ToList())
            {
                meeting.DeadlineInSegments -= segmentsElapsed;

                if (meeting.DeadlineInSegments <= 0)
                {
                    expiredMeetings.Add(meeting);
                    ProcessExpiredMeeting(meeting);
                }
            }

            // Remove expired meetings from player's list
            foreach (MeetingObligation expiredMeeting in expiredMeetings)
            {
                player.MeetingObligations.Remove(expiredMeeting);
            }

            return expiredMeetings;
        }

        private void ProcessExpiredMeeting(MeetingObligation expiredMeeting)
        {
            _messageSystem.AddSystemMessage(
                $"⏰ Meeting with {expiredMeeting.RequesterName} has expired!",
                SystemMessageTypes.Danger
            );

            // Apply relationship penalty
            int tokenPenalty = 2; // Meetings are important social commitments
            _tokenManager.RemoveTokensFromNPC(ConnectionType.Trust, tokenPenalty, expiredMeeting.RequesterId);

            _messageSystem.AddSystemMessage(
                $"💔 Lost {tokenPenalty} Trust tokens with {expiredMeeting.RequesterName} for missing meeting!",
                SystemMessageTypes.Danger
            );
        }

        private void CompactQueueAfterExpiration()
        {
            Player player = _gameWorld.GetPlayer();
            DeliveryObligation[] queue = player.ObligationQueue;
            int writeIndex = 0;

            // Compact array in-place, removing expired obligations
            for (int readIndex = 0; readIndex < _config.LetterQueue.MaxQueueSize; readIndex++)
            {
                if (queue[readIndex] != null && queue[readIndex].DeadlineInSegments > 0)
                {
                    if (writeIndex != readIndex)
                    {
                        queue[writeIndex] = queue[readIndex];
                        queue[writeIndex].QueuePosition = writeIndex + 1;
                        queue[readIndex] = null;
                    }
                    writeIndex++;
                }
            }

            // Clear remaining positions
            for (int i = writeIndex; i < _config.LetterQueue.MaxQueueSize; i++)
            {
                queue[i] = null;
            }
        }

        private void ApplyExpirationPenalty(DeliveryObligation expiredObligation, string senderId)
        {
            int tokenPenalty = _config.LetterQueue.DeadlinePenaltyTokens;
            NPC senderNpc = _npcRepository.GetById(senderId);

            _tokenManager.RemoveTokensFromNPC(expiredObligation.TokenType, tokenPenalty, senderId);

            _messageSystem.AddSystemMessage(
                $"💔 Lost {tokenPenalty} {expiredObligation.TokenType} tokens with {expiredObligation.SenderName}. Trust broken.",
                SystemMessageTypes.Danger
            );

            if (senderNpc != null)
            {
                ShowConsequenceNarrative(senderNpc, expiredObligation);
            }

            ShowCumulativeDamage(expiredObligation, senderId);
        }

        private void ShowExpiryFailure(DeliveryObligation expiredObligation)
        {
            _messageSystem.AddSystemMessage(
                $"⏰ TIME'S UP! {expiredObligation.SenderName}'s letter has expired!",
                SystemMessageTypes.Danger
            );
        }

        private void ShowConsequenceNarrative(NPC senderNpc, DeliveryObligation expiredObligation)
        {
            string consequence = GetExpiryConsequence(senderNpc, expiredObligation);
            _messageSystem.AddSystemMessage(
                $"  • {consequence}",
                SystemMessageTypes.Warning
            );
        }

        private void ShowCumulativeDamage(DeliveryObligation expiredObligation, string senderId)
        {
            Player player = _gameWorld.GetPlayer();
            if (!player.NPCLetterHistory.ContainsKey(senderId)) return;

            LetterHistory history = player.NPCLetterHistory[senderId];
            if (history.ExpiredCount > 1)
            {
                _messageSystem.AddSystemMessage(
                    $"  ⚠️ This is the {GetOrdinal(history.ExpiredCount)} letter from {expiredObligation.SenderName} you've let expire.",
                    SystemMessageTypes.Danger
                );
            }
        }

        private string GetExpiryConsequence(NPC npc, DeliveryObligation expiredObligation)
        {
            if (npc.LetterTokenTypes.Contains(ConnectionType.Trust))
            {
                return $"{npc.Name} waited for your help that never came. Some wounds don't heal.";
            }
            else if (npc.LetterTokenTypes.Contains(ConnectionType.Commerce))
            {
                return $"{npc.Name}'s opening has passed. 'Time is money, and you've cost me both.'";
            }
            else if (npc.LetterTokenTypes.Contains(ConnectionType.Status))
            {
                return "Word of your failure reaches court circles. Your standing suffers.";
            }
            else if (npc.LetterTokenTypes.Contains(ConnectionType.Shadow))
            {
                return $"{npc.Name} doesn't forget broken promises. You've made an enemy in dark places.";
            }
            else
            {
                return $"{npc.Name} needed this delivered on time. Another bridge burned.";
            }
        }

        private void RecordExpiryInHistory(string senderId)
        {
            Player player = _gameWorld.GetPlayer();
            if (!player.NPCLetterHistory.ContainsKey(senderId))
            {
                player.NPCLetterHistory[senderId] = new LetterHistory();
            }
            player.NPCLetterHistory[senderId].RecordExpiry();
        }

        private bool ValidateTokenAvailability(Dictionary<ConnectionType, int> requiredTokens)
        {
            foreach (KeyValuePair<ConnectionType, int> tokenRequirement in requiredTokens)
            {
                if (!_tokenManager.HasTokens(tokenRequirement.Key, tokenRequirement.Value))
                {
                    return false;
                }
            }
            return true;
        }

        private DeliveryObligation GetLetterAt(int position)
        {
            if (position < 1 || position > _config.LetterQueue.MaxQueueSize)
                return null;

            return _gameWorld.GetPlayer().ObligationQueue[position - 1];
        }

        private DeliveryObligation[] GetActiveObligations()
        {
            return _gameWorld.GetPlayer().ObligationQueue
                .Where(o => o != null && o.DeadlineInSegments > 0)
                .ToArray();
        }

        private List<MeetingObligation> GetActiveMeetings()
        {
            return _gameWorld.GetPlayer().MeetingObligations
                .Where(m => m.DeadlineInSegments > 0)
                .ToList();
        }

        private string GetNPCIdByName(string npcName)
        {
            NPC npc = _npcRepository.GetByName(npcName);
            return npc?.ID ?? "";
        }

        private string GetOrdinal(int number)
        {
            if (number <= 0) return number.ToString();

            switch (number % 100)
            {
                case 11:
                case 12:
                case 13:
                    return number + "th";
            }

            switch (number % 10)
            {
                case 1:
                    return number + "st";
                case 2:
                    return number + "nd";
                case 3:
                    return number + "rd";
                default:
                    return number + "th";
            }
        }

    }

    /// <summary>
    /// Statistics about deadline urgency levels.
    /// </summary>
    public class DeadlineUrgencyStats
    {
        public int CriticalObligations { get; set; }
        public int UrgentObligations { get; set; }
        public int NormalObligations { get; set; }
        public int CriticalMeetings { get; set; }
        public int UrgentMeetings { get; set; }
        public int NormalMeetings { get; set; }
    }

    /// <summary>
    /// Information about a deadline for display purposes.
    /// </summary>
    public class DeadlineInfo
    {
        public string Type { get; set; } // "Letter" or "Meeting"
        public string Title { get; set; }
        public string From { get; set; }
        public int DeadlineInSegments { get; set; }
        public bool IsUrgent { get; set; }
        public bool IsCritical { get; set; }
        public string ObligationId { get; set; }
    }
}