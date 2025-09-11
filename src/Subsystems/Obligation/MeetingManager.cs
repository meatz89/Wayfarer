using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Subsystems.ObligationSubsystem;

namespace Wayfarer.Subsystems.ObligationSubsystem
{
    /// <summary>
    /// Manages all meeting-related obligations including scheduling, completion, and deadline tracking.
    /// Handles NPC meeting requests, appointment management, and meeting outcome processing.
    /// </summary>
    public class MeetingManager
    {
        private readonly GameWorld _gameWorld;
        private readonly NPCRepository _npcRepository;
        private readonly MessageSystem _messageSystem;
        private readonly TokenMechanicsManager _tokenManager;
        private readonly GameConfiguration _config;

        public MeetingManager(
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
        /// Get all active meeting obligations that haven't expired.
        /// </summary>
        public List<MeetingObligation> GetActiveMeetingObligations()
        {
            Player player = _gameWorld.GetPlayer();
            return player.MeetingObligations
                .Where(m => m.DeadlineInSegments > 0)
                .OrderBy(m => m.DeadlineInSegments)
                .ToList();
        }

        /// <summary>
        /// Get meeting obligation with a specific NPC.
        /// </summary>
        public MeetingObligation GetMeetingWithNPC(string npcId)
        {
            return GetActiveMeetingObligations()
                .FirstOrDefault(m => m.RequesterId == npcId);
        }

        /// <summary>
        /// Get meeting obligation by ID.
        /// </summary>
        public MeetingObligation GetMeetingById(string meetingId)
        {
            Player player = _gameWorld.GetPlayer();
            return player.MeetingObligations
                .FirstOrDefault(m => m.Id == meetingId);
        }

        /// <summary>
        /// Add a new meeting obligation from an NPC request.
        /// </summary>
        public MeetingResult AddMeetingObligation(MeetingObligation meeting)
        {
            MeetingResult result = new MeetingResult
            {
                Operation = MeetingOperation.Add,
                NPCId = meeting.RequesterId,
                NPCName = meeting.RequesterName
            };

            // Validate the meeting request
            MeetingResult validation = ValidateMeetingRequest(meeting);
            if (!validation.Success)
            {
                result.ErrorMessage = validation.ErrorMessage;
                return result;
            }

            // Check if there's already a meeting with this NPC
            MeetingObligation existingMeeting = GetMeetingWithNPC(meeting.RequesterId);
            if (existingMeeting != null)
            {
                result.ErrorMessage = $"Already have a meeting scheduled with {meeting.RequesterName}";
                return result;
            }

            // Add the meeting
            _gameWorld.GetPlayer().MeetingObligations.Add(meeting);

            result.Success = true;
            result.AffectedMeeting = meeting;

            // Send appropriate message based on urgency
            MeetingUrgency urgencyLevel = GetMeetingUrgencyLevel(meeting);
            SystemMessageTypes messageType = urgencyLevel switch
            {
                MeetingUrgency.Critical => SystemMessageTypes.Danger,
                MeetingUrgency.Urgent => SystemMessageTypes.Warning,
                _ => SystemMessageTypes.Info
            };

            _messageSystem.AddSystemMessage(
                $"📅 {meeting.RequesterName} urgently requests to meet you! ({meeting.DeadlineInSegments / 2}h remaining)",
                messageType
            );

            return result;
        }

        /// <summary>
        /// Complete a meeting obligation when the player meets with the NPC.
        /// </summary>
        public MeetingResult CompleteMeeting(string meetingId)
        {
            MeetingResult result = new MeetingResult
            {
                Operation = MeetingOperation.Complete
            };

            MeetingObligation meeting = GetMeetingById(meetingId);
            if (meeting == null)
            {
                result.ErrorMessage = "Meeting not found";
                return result;
            }

            result.NPCId = meeting.RequesterId;
            result.NPCName = meeting.RequesterName;

            // Validate the meeting can be completed
            MeetingResult validation = ValidateMeetingCompletion(meeting);
            if (!validation.Success)
            {
                result.ErrorMessage = validation.ErrorMessage;
                return result;
            }

            // Remove the meeting from obligations
            _gameWorld.GetPlayer().MeetingObligations.Remove(meeting);

            // Award reputation based on meeting completion timing
            AwardMeetingCompletionTokens(meeting);

            result.Success = true;
            result.AffectedMeeting = meeting;

            _messageSystem.AddSystemMessage(
                $"✅ Met with {meeting.RequesterName}",
                SystemMessageTypes.Success
            );

            return result;
        }

        /// <summary>
        /// Cancel a meeting obligation (typically for testing or special circumstances).
        /// </summary>
        public MeetingResult CancelMeeting(string meetingId)
        {
            MeetingResult result = new MeetingResult
            {
                Operation = MeetingOperation.Cancel
            };

            MeetingObligation meeting = GetMeetingById(meetingId);
            if (meeting == null)
            {
                result.ErrorMessage = "Meeting not found";
                return result;
            }

            result.NPCId = meeting.RequesterId;
            result.NPCName = meeting.RequesterName;

            // Remove the meeting
            _gameWorld.GetPlayer().MeetingObligations.Remove(meeting);

            // Apply minor relationship penalty for cancellation
            if (meeting.RequesterId != null)
            {
                _tokenManager.RemoveTokensFromNPC(ConnectionType.Trust, 1, meeting.RequesterId);
                _messageSystem.AddSystemMessage(
                    $"💔 Lost 1 Trust token with {meeting.RequesterName} for canceling meeting",
                    SystemMessageTypes.Warning
                );
            }

            result.Success = true;
            result.AffectedMeeting = meeting;

            _messageSystem.AddSystemMessage(
                $"❌ Canceled meeting with {meeting.RequesterName}",
                SystemMessageTypes.Warning
            );

            return result;
        }

        /// <summary>
        /// Process meeting deadline expiration. Called by DeadlineTracker.
        /// </summary>
        public List<MeetingResult> ProcessExpiredMeetings()
        {
            List<MeetingResult> results = new List<MeetingResult>();
            Player player = _gameWorld.GetPlayer();
            List<MeetingObligation> expiredMeetings = new List<MeetingObligation>();

            // Find expired meetings
            foreach (MeetingObligation? meeting in player.MeetingObligations.ToList())
            {
                if (meeting.DeadlineInSegments <= 0)
                {
                    expiredMeetings.Add(meeting);
                }
            }

            // Process each expired meeting
            foreach (MeetingObligation expiredMeeting in expiredMeetings)
            {
                MeetingResult result = ExpireMeeting(expiredMeeting);
                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Get all meetings that are expiring within the specified threshold.
        /// </summary>
        public List<MeetingObligation> GetExpiringMeetings(int segmentsThreshold)
        {
            return GetActiveMeetingObligations()
                .Where(m => m.DeadlineInSegments <= segmentsThreshold && m.DeadlineInSegments > 0)
                .OrderBy(m => m.DeadlineInSegments)
                .ToList();
        }

        /// <summary>
        /// Get meetings by urgency level.
        /// </summary>
        public List<MeetingObligation> GetMeetingsByUrgency(MeetingUrgency urgency)
        {
            return GetActiveMeetingObligations()
                .Where(m => GetMeetingUrgencyLevel(m) == urgency)
                .ToList();
        }

        /// <summary>
        /// Check if the player can meet with a specific NPC right now.
        /// </summary>
        public bool CanMeetWithNPC(string npcId)
        {
            MeetingObligation meeting = GetMeetingWithNPC(npcId);
            if (meeting == null) return false;

            // Check if player is at NPC's location
            return IsPlayerAtNPCLocation(npcId);
        }

        /// <summary>
        /// Get detailed meeting statistics.
        /// </summary>
        public MeetingStatistics GetMeetingStatistics()
        {
            List<MeetingObligation> activeMeetings = GetActiveMeetingObligations();

            return new MeetingStatistics
            {
                TotalActiveMeetings = activeMeetings.Count,
                CriticalMeetings = activeMeetings.Count(m => m.IsCritical),
                UrgentMeetings = activeMeetings.Count(m => m.IsUrgent && !m.IsCritical),
                NextMeetingDeadlineSegments = activeMeetings.Any() ?
                    activeMeetings.Min(m => m.DeadlineInSegments) : -1,
                MostUrgentMeeting = activeMeetings.OrderBy(m => m.DeadlineInSegments).FirstOrDefault(),
                MeetingsByStakes = GroupMeetingsByStakes(activeMeetings),
                AverageDeadlineSegments = activeMeetings.Any() ?
                    activeMeetings.Average(m => m.DeadlineInSegments) : 0
            };
        }

        /// <summary>
        /// Schedule a meeting with specific timing and requirements.
        /// </summary>
        public MeetingResult ScheduleMeeting(string npcId, string reason, int deadlineInSegments, StakeType stakes = StakeType.REPUTATION)
        {
            NPC npc = _npcRepository.GetById(npcId);
            if (npc == null)
            {
                return new MeetingResult
                {
                    Operation = MeetingOperation.Add,
                    ErrorMessage = "NPC not found"
                };
            }

            MeetingObligation meeting = new MeetingObligation
            {
                Id = Guid.NewGuid().ToString(),
                RequesterId = npcId,
                RequesterName = npc.Name,
                DeadlineInSegments = deadlineInSegments,
                Stakes = stakes,
                Reason = reason
            };

            return AddMeetingObligation(meeting);
        }

        // Private helper methods

        private MeetingResult ValidateMeetingRequest(MeetingObligation meeting)
        {
            MeetingResult result = new MeetingResult { Success = true };

            if (string.IsNullOrEmpty(meeting.RequesterId))
            {
                result.Success = false;
                result.ErrorMessage = "Meeting must have a valid requester ID";
                return result;
            }

            NPC npc = _npcRepository.GetById(meeting.RequesterId);
            if (npc == null)
            {
                result.Success = false;
                result.ErrorMessage = "Requester NPC not found";
                return result;
            }

            if (meeting.DeadlineInSegments <= 0)
            {
                result.Success = false;
                result.ErrorMessage = "Meeting deadline must be positive";
                return result;
            }

            return result;
        }

        private MeetingResult ValidateMeetingCompletion(MeetingObligation meeting)
        {
            MeetingResult result = new MeetingResult { Success = true };

            // Check if meeting has expired
            if (meeting.DeadlineInSegments <= 0)
            {
                result.Success = false;
                result.ErrorMessage = "Cannot complete expired meeting";
                return result;
            }

            // Check if player is at NPC's location
            if (!IsPlayerAtNPCLocation(meeting.RequesterId))
            {
                result.Success = false;
                result.ErrorMessage = $"You must be at {meeting.RequesterName}'s location to meet";
                return result;
            }

            return result;
        }

        private MeetingResult ExpireMeeting(MeetingObligation expiredMeeting)
        {
            MeetingResult result = new MeetingResult
            {
                Operation = MeetingOperation.Expire,
                NPCId = expiredMeeting.RequesterId,
                NPCName = expiredMeeting.RequesterName,
                AffectedMeeting = expiredMeeting
            };

            // Remove from player's obligations
            _gameWorld.GetPlayer().MeetingObligations.Remove(expiredMeeting);

            // Apply relationship penalty based on stakes
            ApplyMeetingExpirationPenalty(expiredMeeting);

            result.Success = true;

            _messageSystem.AddSystemMessage(
                $"⏰ Meeting with {expiredMeeting.RequesterName} has expired!",
                SystemMessageTypes.Danger
            );

            return result;
        }

        private void AwardMeetingCompletionTokens(MeetingObligation meeting)
        {
            int tokensAwarded = 1; // Base reward
            ConnectionType tokenType = ConnectionType.Trust; // Meetings typically build trust

            // Bonus for early completion
            if (meeting.DeadlineInSegments > 180) // More than 3 hours remaining
            {
                tokensAwarded += 1;
                _messageSystem.AddSystemMessage(
                    "⭐ Early meeting bonus!",
                    SystemMessageTypes.Success
                );
            }

            _tokenManager.AddTokensToNPC(tokenType, tokensAwarded, meeting.RequesterId);

            _messageSystem.AddSystemMessage(
                $"🎖️ Gained {tokensAwarded} {tokenType} tokens with {meeting.RequesterName}",
                SystemMessageTypes.Success
            );
        }

        private void ApplyMeetingExpirationPenalty(MeetingObligation expiredMeeting)
        {
            int tokenPenalty = 2; // Meetings are important social commitments
            ConnectionType tokenType = ConnectionType.Trust;

            _tokenManager.RemoveTokensFromNPC(tokenType, tokenPenalty, expiredMeeting.RequesterId);

            _messageSystem.AddSystemMessage(
                $"💔 Lost {tokenPenalty} {tokenType} tokens with {expiredMeeting.RequesterName} for missing meeting!",
                SystemMessageTypes.Danger
            );
        }

        private MeetingUrgency GetMeetingUrgencyLevel(MeetingObligation meeting)
        {
            if (meeting.IsCritical) return MeetingUrgency.Critical;
            if (meeting.IsUrgent) return MeetingUrgency.Urgent;
            return MeetingUrgency.Normal;
        }

        private bool IsPlayerAtNPCLocation(string npcId)
        {
            Player player = _gameWorld.GetPlayer();
            if (player.CurrentLocationSpot == null) return false;

            // Get current time block for NPC location checking
            TimeBlocks currentTime = GetCurrentTimeBlock();
            List<NPC> npcsAtCurrentSpot = _npcRepository.GetNPCsForLocationSpotAndTime(
                player.CurrentLocationSpot.SpotID,
                currentTime);

            return npcsAtCurrentSpot.Any(npc => npc.ID == npcId);
        }

        private Dictionary<StakeType, int> GroupMeetingsByStakes(List<MeetingObligation> meetings)
        {
            return meetings
                .GroupBy(m => m.Stakes)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private TimeBlocks GetCurrentTimeBlock()
        {
            // This would typically come from TimeManager
            // Simplified for now
            return TimeBlocks.Midday;
        }
    }

    /// <summary>
    /// Meeting urgency levels for categorization and UI display.
    /// </summary>
    public enum MeetingUrgency
    {
        Normal,   // > 6 hours
        Urgent,   // 3-6 hours  
        Critical  // < 3 hours
    }

    /// <summary>
    /// Statistics about meeting obligations.
    /// </summary>
    public class MeetingStatistics
    {
        public int TotalActiveMeetings { get; set; }
        public int CriticalMeetings { get; set; }
        public int UrgentMeetings { get; set; }
        public int NextMeetingDeadlineSegments { get; set; }
        public MeetingObligation MostUrgentMeeting { get; set; }
        public Dictionary<StakeType, int> MeetingsByStakes { get; set; } = new Dictionary<StakeType, int>();
        public double AverageDeadlineSegments { get; set; }
    }
}