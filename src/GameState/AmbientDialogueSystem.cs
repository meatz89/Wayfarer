using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.GameState
{
    /// <summary>
    /// Generates ambient dialogue for NPCs based on recent events.
    /// Makes the world feel alive without requiring full conversations.
    /// </summary>
    public class AmbientDialogueSystem
    {
        private readonly WorldMemorySystem _worldMemory;
        private readonly NPCRepository _npcRepository;
        private readonly Dictionary<string, DateTime> _lastSpeakTime = new();
        private readonly TimeSpan COOLDOWN = TimeSpan.FromMinutes(5); // Real time cooldown

        public AmbientDialogueSystem(
            WorldMemorySystem worldMemory,
            NPCRepository npcRepository)
        {
            _worldMemory = worldMemory;
            _npcRepository = npcRepository;
        }

        /// <summary>
        /// Get ambient comment from an NPC if they have something to say
        /// </summary>
        public string GetAmbientComment(string npcId)
        {
            // Check cooldown
            if (_lastSpeakTime.TryGetValue(npcId, out DateTime lastTime))
            {
                if (DateTime.Now - lastTime < COOLDOWN)
                    return null;
            }

            NPC npc = _npcRepository.GetById(npcId);
            if (npc == null) return null;

            string comment = GenerateComment(npc);
            if (!string.IsNullOrEmpty(comment))
            {
                _lastSpeakTime[npcId] = DateTime.Now;
            }

            return comment;
        }

        /// <summary>
        /// Generate contextual comment based on NPC and recent events
        /// </summary>
        private string GenerateComment(NPC npc)
        {
            // Check for recent events involving this NPC
            List<WorldEvent> npcEvents = _worldMemory.GetEventsForNPC(npc.ID);

            if (npcEvents.Any())
            {
                WorldEvent mostRecent = npcEvents.First();
                return GenerateEventComment(npc, mostRecent);
            }

            // Check for general recent failures in the area
            if (_worldMemory.HasRecentFailureWith(npc.ID))
            {
                return GenerateFailureComment(npc);
            }

            // Check for recent successes
            if (_worldMemory.HasRecentSuccessWith(npc.ID))
            {
                return GenerateSuccessComment(npc);
            }

            // Default ambient based on NPC personality
            return GenerateDefaultComment(npc);
        }

        private string GenerateEventComment(NPC npc, WorldEvent worldEvent)
        {
            return worldEvent.Type switch
            {
                WorldEventType.DeadlineMissed =>
                    $"{npc.Name} mutters: \"Can't rely on anyone these days...\"",

                WorldEventType.LetterDelivered =>
                    $"{npc.Name} nods approvingly: \"Good to see someone keeping their word.\"",

                WorldEventType.ConfrontationOccurred =>
                    $"{npc.Name} avoids eye contact and hurries past.",

                WorldEventType.TrustLost =>
                    $"{npc.Name} shakes their head disapprovingly.",

                WorldEventType.TrustGained =>
                    $"{npc.Name}: \"You're building quite a reputation.\"",

                _ => null
            };
        }

        private string GenerateFailureComment(NPC npc)
        {
            // Different personalities react differently
            return npc.Profession switch
            {
                Professions.Merchant => $"{npc.Name}: \"Bad for business, these delays...\"",
                Professions.Noble => $"{npc.Name} looks at you with disdain.",
                Professions.Scribe => $"{npc.Name}: \"Words once lost cannot be recovered.\"",
                Professions.Soldier => $"{npc.Name}: \"Discipline is everything, Wayfarer.\"",
                _ => $"{npc.Name} seems disappointed."
            };
        }

        private string GenerateSuccessComment(NPC npc)
        {
            return npc.Profession switch
            {
                Professions.Merchant => $"{npc.Name}: \"Reliability is good for trade.\"",
                Professions.Noble => $"{npc.Name} acknowledges you with a slight nod.",
                Professions.Scribe => $"{npc.Name}: \"The written word, safely delivered.\"",
                Professions.Soldier => $"{npc.Name}: \"Well executed, Wayfarer.\"",
                _ => $"{npc.Name} seems pleased."
            };
        }

        private string GenerateDefaultComment(NPC npc)
        {
            // Occasional neutral ambient dialogue
            Random random = new Random();
            if (random.Next(100) > 70) // 30% chance
            {
                return npc.Profession switch
                {
                    Professions.Merchant => $"{npc.Name} is checking inventory.",
                    Professions.Noble => $"{npc.Name} seems preoccupied with affairs of state.",
                    Professions.Scribe => $"{npc.Name} is absorbed in writing.",
                    Professions.Soldier => $"{npc.Name} maintains a watchful stance.",
                    Professions.Innkeeper => $"{npc.Name} wipes down the bar.",
                    _ => $"{npc.Name} goes about their business."
                };
            }

            return null;
        }

        /// <summary>
        /// Get all ambient comments for NPCs at a location
        /// </summary>
        public List<string> GetLocationAmbience(string locationId)
        {
            List<string> comments = new List<string>();
            IEnumerable<NPC> npcsAtLocation = _npcRepository.GetAllNPCs()
                .Where(npc => npc.Location == locationId);

            foreach (NPC? npc in npcsAtLocation)
            {
                string comment = GetAmbientComment(npc.ID);
                if (!string.IsNullOrEmpty(comment))
                {
                    comments.Add(comment);
                    break; // Only one NPC speaks at a time
                }
            }

            return comments;
        }
    }
}