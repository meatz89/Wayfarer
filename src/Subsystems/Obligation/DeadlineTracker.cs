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
        /// Process hourly deadline countdown for all obligations.
        /// Updates deadlines, handles expirations, and compacts the queue.
        /// </summary>
        public DeadlineTrackingInfo ProcessHourlyDeadlines(int hoursElapsed = 1)
        {
            var trackingInfo = new DeadlineTrackingInfo
            {
                HoursElapsed = hoursElapsed
            };

            if (hoursElapsed <= 0) return trackingInfo;

            var queue = _gameWorld.GetPlayer().ObligationQueue;

            // Phase 1: Update deadlines and identify expired obligations
            var expiredObligations = new List<DeliveryObligation>();
            for (int i = 0; i < _config.LetterQueue.MaxQueueSize; i++)
            {
                if (queue[i] != null)
                {
                    queue[i].DeadlineInMinutes -= (hoursElapsed * 60);
                    if (queue[i].DeadlineInMinutes <= 0)
                    {
                        expiredObligations.Add(queue[i]);
                    }
                }
            }

            // Phase 2: Process expired obligations
            foreach (var expiredObligation in expiredObligations)
            {
                ProcessExpiredObligation(expiredObligation);
                trackingInfo.ExpiredObligationIds.Add(expiredObligation.Id);
            }

            // Phase 3: Process expired meetings
            var expiredMeetings = ProcessExpiredMeetings(hoursElapsed);
            trackingInfo.ExpiredMeetingIds.AddRange(expiredMeetings.Select(m => m.Id));

            // Phase 4: Compact queue removing expired items
            CompactQueueAfterExpiration();

            // Phase 5: Identify still-expiring items for warnings
            trackingInfo.ExpiringObligations = GetExpiringObligations(24); // Next 24 hours
            trackingInfo.ExpiringMeetings = GetExpiringMeetings(24);

            return trackingInfo;
        }

        /// <summary>
        /// Get obligations that will expire within the specified hours threshold.
        /// </summary>
        public List<DeliveryObligation> GetExpiringObligations(int hoursThreshold)
        {
            var minutesThreshold = hoursThreshold * 60;
            
            return GetActiveObligations()
                .Where(o => o.DeadlineInMinutes <= minutesThreshold && o.DeadlineInMinutes > 0)
                .OrderBy(o => o.DeadlineInMinutes)
                .ToList();
        }

        /// <summary>
        /// Get meetings that will expire within the specified hours threshold.
        /// </summary>
        public List<MeetingObligation> GetExpiringMeetings(int hoursThreshold)
        {
            var minutesThreshold = hoursThreshold * 60;
            
            var player = _gameWorld.GetPlayer();
            return player.MeetingObligations
                .Where(m => m.DeadlineInMinutes <= minutesThreshold && m.DeadlineInMinutes > 0)
                .OrderBy(m => m.DeadlineInMinutes)
                .ToList();
        }

        /// <summary>
        /// Get the most urgent obligation (closest to expiring).
        /// </summary>
        public DeliveryObligation GetMostUrgentObligation()
        {
            return GetActiveObligations()
                .Where(o => o.DeadlineInMinutes > 0)
                .OrderBy(o => o.DeadlineInMinutes)
                .FirstOrDefault();
        }

        /// <summary>
        /// Get the most urgent meeting (closest to expiring).
        /// </summary>
        public MeetingObligation GetMostUrgentMeeting()
        {
            var player = _gameWorld.GetPlayer();
            return player.MeetingObligations
                .Where(m => m.DeadlineInMinutes > 0)
                .OrderBy(m => m.DeadlineInMinutes)
                .FirstOrDefault();
        }

        /// <summary>
        /// Extend the deadline of a specific obligation by paying tokens.
        /// </summary>
        public QueueManipulationResult TryExtendDeadline(int position)
        {
            var result = new QueueManipulationResult
            {
                OperationType = "Extend Deadline",
                Position = position
            };

            if (position < 1 || position > _config.LetterQueue.MaxQueueSize)
            {
                result.ErrorMessage = "Invalid position";
                return result;
            }

            var letter = GetLetterAt(position);
            if (letter == null)
            {
                result.ErrorMessage = "No letter at specified position";
                return result;
            }

            var senderId = GetNPCIdByName(letter.SenderName);

            // Show negotiation attempt
            _messageSystem.AddSystemMessage(
                $"📅 Negotiating deadline extension with {letter.SenderName}...",
                SystemMessageTypes.Info
            );

            // Show current deadline pressure
            var urgency = letter.DeadlineInMinutes <= 48 * 60 ? " 🆘 CRITICAL!" : "";
            var currentDays = letter.DeadlineInMinutes / (24 * 60);
            _messageSystem.AddSystemMessage(
                $"  • Current deadline: {currentDays} days{urgency}",
                letter.DeadlineInMinutes <= 48 * 60 ? SystemMessageTypes.Danger : SystemMessageTypes.Info
            );

            // Check token cost (2 matching tokens)
            var extensionCost = new Dictionary<ConnectionType, int> { { letter.TokenType, 2 } };
            
            if (!ValidateTokenAvailability(extensionCost))
            {
                result.ErrorMessage = $"Insufficient {letter.TokenType} tokens! Need 2";
                result.TokensCost = extensionCost;
                
                _messageSystem.AddSystemMessage(
                    $"  ❌ Insufficient {letter.TokenType} tokens! Need 2, have {_tokenManager.GetTokenCount(letter.TokenType)}",
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

            // Extend the deadline by 2 days (2880 minutes)
            var oldDeadlineHours = letter.DeadlineInMinutes / 60;
            letter.DeadlineInMinutes += 2880;

            result.Success = true;
            result.AffectedObligation = letter;
            result.TokensCost = extensionCost;

            // Success narrative
            _messageSystem.AddSystemMessage(
                $"✅ {letter.SenderName} grants a 2-day extension!",
                SystemMessageTypes.Success
            );

            var newDeadlineDays = letter.DeadlineInMinutes / (24 * 60);
            var oldDeadlineDays = oldDeadlineHours / 24;
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
            var mostUrgent = GetMostUrgentObligation();
            if (mostUrgent != null)
            {
                return $"⏰ {mostUrgent.HoursUntilDeadline}h";
            }

            var mostUrgentMeeting = GetMostUrgentMeeting();
            if (mostUrgentMeeting != null)
            {
                return $"📅 {mostUrgentMeeting.DeadlineInHours}h";
            }

            return "";
        }

        /// <summary>
        /// Check if any obligations or meetings are in critical deadline status.
        /// </summary>
        public bool HasCriticalDeadlines()
        {
            var criticalObligations = GetExpiringObligations(3); // 3 hours
            var criticalMeetings = GetExpiringMeetings(3);
            
            return criticalObligations.Any() || criticalMeetings.Any();
        }

        /// <summary>
        /// Get count of obligations by urgency level.
        /// </summary>
        public DeadlineUrgencyStats GetUrgencyStats()
        {
            var activeObligations = GetActiveObligations();
            var activeMeetings = GetActiveMeetings();

            return new DeadlineUrgencyStats
            {
                CriticalObligations = activeObligations.Count(o => o.DeadlineInMinutes <= 180), // 3 hours
                UrgentObligations = activeObligations.Count(o => o.DeadlineInMinutes > 180 && o.DeadlineInMinutes <= 360), // 3-6 hours
                NormalObligations = activeObligations.Count(o => o.DeadlineInMinutes > 360),
                CriticalMeetings = activeMeetings.Count(m => m.IsCritical),
                UrgentMeetings = activeMeetings.Count(m => m.IsUrgent && !m.IsCritical),
                NormalMeetings = activeMeetings.Count(m => !m.IsUrgent)
            };
        }

        /// <summary>
        /// Calculate time until next deadline in minutes.
        /// </summary>
        public int GetMinutesUntilNextDeadline()
        {
            var nextObligation = GetMostUrgentObligation();
            var nextMeeting = GetMostUrgentMeeting();

            var obligationMinutes = nextObligation?.DeadlineInMinutes ?? int.MaxValue;
            var meetingMinutes = nextMeeting?.DeadlineInMinutes ?? int.MaxValue;

            var nextDeadline = Math.Min(obligationMinutes, meetingMinutes);
            return nextDeadline == int.MaxValue ? -1 : nextDeadline;
        }

        /// <summary>
        /// Get all deadlines sorted by urgency for dashboard display.
        /// </summary>
        public List<DeadlineInfo> GetAllDeadlinesSortedByUrgency()
        {
            var deadlines = new List<DeadlineInfo>();

            // Add obligation deadlines
            foreach (var obligation in GetActiveObligations())
            {
                deadlines.Add(new DeadlineInfo
                {
                    Type = "Letter",
                    Title = $"Deliver to {obligation.RecipientName}",
                    From = obligation.SenderName,
                    DeadlineInMinutes = obligation.DeadlineInMinutes,
                    IsUrgent = obligation.DeadlineInMinutes <= 360,
                    IsCritical = obligation.DeadlineInMinutes <= 180,
                    ObligationId = obligation.Id
                });
            }

            // Add meeting deadlines
            foreach (var meeting in GetActiveMeetings())
            {
                deadlines.Add(new DeadlineInfo
                {
                    Type = "Meeting",
                    Title = $"Meet with {meeting.RequesterName}",
                    From = meeting.RequesterName,
                    DeadlineInMinutes = meeting.DeadlineInMinutes,
                    IsUrgent = meeting.IsUrgent,
                    IsCritical = meeting.IsCritical,
                    ObligationId = meeting.Id
                });
            }

            return deadlines.OrderBy(d => d.DeadlineInMinutes).ToList();
        }

        // Private helper methods

        private void ProcessExpiredObligation(DeliveryObligation expiredObligation)
        {
            var senderId = GetNPCIdByName(expiredObligation.SenderName);
            if (string.IsNullOrEmpty(senderId)) return;

            // Record expiry in history first
            RecordExpiryInHistory(senderId);

            // Apply relationship consequences
            ApplyExpirationPenalty(expiredObligation, senderId);

            // Show dramatic failure message
            ShowExpiryFailure(expiredObligation);
        }

        private List<MeetingObligation> ProcessExpiredMeetings(int hoursElapsed)
        {
            var player = _gameWorld.GetPlayer();
            var expiredMeetings = new List<MeetingObligation>();

            foreach (var meeting in player.MeetingObligations.ToList())
            {
                meeting.DeadlineInMinutes -= (hoursElapsed * 60);
                
                if (meeting.DeadlineInMinutes <= 0)
                {
                    expiredMeetings.Add(meeting);
                    ProcessExpiredMeeting(meeting);
                }
            }

            // Remove expired meetings from player's list
            foreach (var expiredMeeting in expiredMeetings)
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
            var tokenPenalty = 2; // Meetings are important social commitments
            _tokenManager.RemoveTokensFromNPC(ConnectionType.Trust, tokenPenalty, expiredMeeting.RequesterId);

            _messageSystem.AddSystemMessage(
                $"💔 Lost {tokenPenalty} Trust tokens with {expiredMeeting.RequesterName} for missing meeting!",
                SystemMessageTypes.Danger
            );
        }

        private void CompactQueueAfterExpiration()
        {
            var player = _gameWorld.GetPlayer();
            var queue = player.ObligationQueue;
            int writeIndex = 0;

            // Compact array in-place, removing expired obligations
            for (int readIndex = 0; readIndex < _config.LetterQueue.MaxQueueSize; readIndex++)
            {
                if (queue[readIndex] != null && queue[readIndex].DeadlineInMinutes > 0)
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
            var tokenPenalty = _config.LetterQueue.DeadlinePenaltyTokens;
            var senderNpc = _npcRepository.GetById(senderId);

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
            var consequence = GetExpiryConsequence(senderNpc, expiredObligation);
            _messageSystem.AddSystemMessage(
                $"  • {consequence}",
                SystemMessageTypes.Warning
            );
        }

        private void ShowCumulativeDamage(DeliveryObligation expiredObligation, string senderId)
        {
            var player = _gameWorld.GetPlayer();
            if (!player.NPCLetterHistory.ContainsKey(senderId)) return;

            var history = player.NPCLetterHistory[senderId];
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
                return $"{npc.Name}'s opportunity has passed. 'Time is money, and you've cost me both.'";
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
            var player = _gameWorld.GetPlayer();
            if (!player.NPCLetterHistory.ContainsKey(senderId))
            {
                player.NPCLetterHistory[senderId] = new LetterHistory();
            }
            player.NPCLetterHistory[senderId].RecordExpiry();
        }

        private bool ValidateTokenAvailability(Dictionary<ConnectionType, int> requiredTokens)
        {
            foreach (var tokenRequirement in requiredTokens)
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
                .Where(o => o != null && o.DeadlineInMinutes > 0)
                .ToArray();
        }

        private List<MeetingObligation> GetActiveMeetings()
        {
            return _gameWorld.GetPlayer().MeetingObligations
                .Where(m => m.DeadlineInMinutes > 0)
                .ToList();
        }

        private string GetNPCIdByName(string npcName)
        {
            var npc = _npcRepository.GetByName(npcName);
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
        
        public int TotalCritical => CriticalObligations + CriticalMeetings;
        public int TotalUrgent => UrgentObligations + UrgentMeetings;
        public int TotalNormal => NormalObligations + NormalMeetings;
        public int TotalAll => TotalCritical + TotalUrgent + TotalNormal;
    }

    /// <summary>
    /// Information about a deadline for display purposes.
    /// </summary>
    public class DeadlineInfo
    {
        public string Type { get; set; } // "Letter" or "Meeting"
        public string Title { get; set; }
        public string From { get; set; }
        public int DeadlineInMinutes { get; set; }
        public bool IsUrgent { get; set; }
        public bool IsCritical { get; set; }
        public string ObligationId { get; set; }
        
        public int DeadlineInHours => (int)Math.Ceiling(DeadlineInMinutes / 60.0);
        public string UrgencyIcon => IsCritical ? "🆘" : IsUrgent ? "⚠️" : "";
    }
}