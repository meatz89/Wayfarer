using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.GameState
{
    /// <summary>
    /// Tracks recent significant events to make the world feel responsive.
    /// Limited memory (7 events) creates a living world that moves on.
    /// </summary>
    public class WorldMemorySystem
    {
        private const int MAX_EVENTS = 7;
        private readonly Queue<WorldEvent> _recentEvents = new();
        private readonly MessageSystem _messageSystem;

        public WorldMemorySystem(MessageSystem messageSystem)
        {
            _messageSystem = messageSystem;
        }

        /// <summary>
        /// Record a significant world event
        /// </summary>
        public void RecordEvent(WorldEventType type, string actorId, string targetId = null, string locationId = null)
        {
            var worldEvent = new WorldEvent
            {
                Type = type,
                ActorId = actorId,
                TargetId = targetId,
                LocationId = locationId,
                Timestamp = DateTime.Now,
                GameHour = 0 // Will be set by TimeManager integration
            };

            // Add new event
            _recentEvents.Enqueue(worldEvent);

            // Evict oldest if over capacity
            while (_recentEvents.Count > MAX_EVENTS)
            {
                _recentEvents.Dequeue();
            }

            // Subtle notification for major events only
            if (type == WorldEventType.DeadlineMissed || type == WorldEventType.ConfrontationOccurred)
            {
                _messageSystem.AddSystemMessage(
                    "The town will remember this...",
                    SystemMessageTypes.Info
                );
            }
        }

        /// <summary>
        /// Get events related to a specific NPC
        /// </summary>
        public List<WorldEvent> GetEventsForNPC(string npcId)
        {
            return _recentEvents
                .Where(e => e.ActorId == npcId || e.TargetId == npcId)
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }

        /// <summary>
        /// Get events at a specific location
        /// </summary>
        public List<WorldEvent> GetEventsAtLocation(string locationId)
        {
            return _recentEvents
                .Where(e => e.LocationId == locationId)
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }

        /// <summary>
        /// Check if there's been a recent failure with an NPC
        /// </summary>
        public bool HasRecentFailureWith(string npcId)
        {
            return _recentEvents.Any(e => 
                (e.ActorId == npcId || e.TargetId == npcId) &&
                (e.Type == WorldEventType.DeadlineMissed || 
                 e.Type == WorldEventType.ConfrontationOccurred));
        }

        /// <summary>
        /// Check if there's been a recent success with an NPC
        /// </summary>
        public bool HasRecentSuccessWith(string npcId)
        {
            return _recentEvents.Any(e => 
                (e.ActorId == npcId || e.TargetId == npcId) &&
                e.Type == WorldEventType.LetterDelivered);
        }

        /// <summary>
        /// Get the most recent significant event
        /// </summary>
        public WorldEvent GetMostRecentEvent()
        {
            return _recentEvents.LastOrDefault();
        }

        /// <summary>
        /// Clear all memory (for new game)
        /// </summary>
        public void Clear()
        {
            _recentEvents.Clear();
        }
    }

    /// <summary>
    /// Types of events the world remembers
    /// </summary>
    public enum WorldEventType
    {
        LetterDelivered,      // Successfully delivered a letter
        DeadlineMissed,       // Failed to deliver on time
        ConfrontationOccurred, // NPC confronted player about failure
        TrustGained,          // Gained significant trust with NPC
        TrustLost,            // Lost significant trust with NPC
        ObligationFulfilled,  // Completed a binding obligation
        ObligationBroken      // Failed a binding obligation
    }

    /// <summary>
    /// A significant event that occurred in the game world
    /// </summary>
    public class WorldEvent
    {
        public WorldEventType Type { get; set; }
        public string ActorId { get; set; }      // Who initiated (usually NPC)
        public string TargetId { get; set; }     // Who was affected (optional)
        public string LocationId { get; set; }   // Where it happened
        public DateTime Timestamp { get; set; }  // Real time for ordering
        public int GameHour { get; set; }        // Game time when it occurred
    }
}