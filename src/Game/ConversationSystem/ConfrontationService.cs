using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.GameState;

namespace Wayfarer.Game.ConversationSystem
{
    /// <summary>
    /// Manages confrontation scenes when players visit NPCs after failing deliveries.
    /// Creates emotionally authentic moments that make consequences feel personal.
    /// </summary>
    public class ConfrontationService
    {
        private readonly GameWorld _gameWorld;
        private readonly NPCRepository _npcRepository;
        private readonly ConsequenceEngine _consequenceEngine;
        private readonly MessageSystem _messageSystem;
        private readonly LetterQueueManager _letterQueueManager;

        public ConfrontationService(
            GameWorld gameWorld,
            NPCRepository npcRepository,
            ConsequenceEngine consequenceEngine,
            MessageSystem messageSystem,
            LetterQueueManager letterQueueManager)
        {
            _gameWorld = gameWorld;
            _npcRepository = npcRepository;
            _consequenceEngine = consequenceEngine;
            _messageSystem = messageSystem;
            _letterQueueManager = letterQueueManager;
        }

        /// <summary>
        /// Check if a confrontation should trigger for this NPC
        /// </summary>
        public bool ShouldTriggerConfrontation(string npcId)
        {
            var player = _gameWorld.GetPlayer();
            if (!player.NPCLetterHistory.TryGetValue(npcId, out var history))
                return false;

            if (history.ExpiredCount == 0)
                return false;

            var npc = _npcRepository.GetById(npcId);
            if (npc == null)
                return false;

            // Closed NPCs refuse even confrontation
            var emotionalState = GetEmotionalStateFromFailures(history.ExpiredCount);
            if (emotionalState == EmotionalState.Closed)
            {
                // Show rejection message instead
                _messageSystem.AddSystemMessage(
                    $"{npc.Name} turns away, refusing to acknowledge your presence.",
                    SystemMessageTypes.Warning
                );
                return false;
            }

            // Check if there are unconfronted failures
            return history.ExpiredCount > npc.LastConfrontationCount;
        }

        /// <summary>
        /// Generate confrontation dialogue based on NPC state and specific failure
        /// </summary>
        public ConfrontationData GenerateConfrontation(string npcId)
        {
            var player = _gameWorld.GetPlayer();
            var npc = _npcRepository.GetById(npcId);
            var history = player.NPCLetterHistory[npcId];
            
            // Get the most recent expired letter details from queue history
            var expiredLetter = GetMostRecentExpiredLetter(npc);
            var emotionalState = GetEmotionalStateFromFailures(history.ExpiredCount);
            var leverage = _consequenceEngine.GetLeverage(npcId);

            var confrontation = new ConfrontationData
            {
                NPC = npc,
                EmotionalState = emotionalState,
                Leverage = leverage,
                ExpiredCount = history.ExpiredCount,
                SpecificLetter = expiredLetter
            };

            // Generate opening based on emotional state
            confrontation.OpeningDialogue = GenerateOpeningDialogue(confrontation);
            confrontation.BodyLanguage = GenerateBodyLanguage(emotionalState);
            confrontation.Choices = GenerateConfrontationChoices(emotionalState);

            // Mark this confrontation as shown
            npc.LastConfrontationCount = history.ExpiredCount;

            return confrontation;
        }

        /// <summary>
        /// Process player's response to confrontation
        /// </summary>
        public ConfrontationResult ProcessConfrontationChoice(
            ConfrontationData confrontation, 
            ConfrontationChoice choice)
        {
            var result = new ConfrontationResult();

            // Generate NPC response based on choice
            result.NPCResponse = GenerateNPCResponse(confrontation, choice);
            
            // Apply any mechanical effects
            switch (choice.ResponseType)
            {
                case ConfrontationResponse.Apologetic:
                    // Small redemption progress
                    confrontation.NPC.RedemptionProgress += 1;
                    result.RedemptionGained = 1;
                    break;
                    
                case ConfrontationResponse.Defensive:
                    // No progress, but not worse
                    result.RedemptionGained = 0;
                    break;
                    
                case ConfrontationResponse.Silent:
                    // Accepting responsibility - slightly better
                    confrontation.NPC.RedemptionProgress += 1;
                    result.RedemptionGained = 1;
                    break;
            }

            // Check if relationship can improve
            if (confrontation.NPC.RedemptionProgress >= GetRedemptionThreshold(confrontation.EmotionalState))
            {
                result.RelationshipImproved = true;
                ImproveEmotionalState(confrontation.NPC);
            }

            result.IsComplete = true; // Confrontations are always single-beat
            return result;
        }

        private string GenerateOpeningDialogue(ConfrontationData data)
        {
            var letterDesc = data.SpecificLetter?.Description ?? "that important letter";
            var recipientName = data.SpecificLetter?.RecipientName ?? "its recipient";
            
            return data.EmotionalState switch
            {
                EmotionalState.Anxious => 
                    $"\"You promised me {letterDesc} would reach {recipientName}. " +
                    $"I trusted you... Was I wrong to do that? Everything depended on that delivery.\"",
                    
                EmotionalState.Hostile => 
                    $"\"You again? After what you've cost me? {letterDesc} never reached {recipientName}. " +
                    $"Do you have any idea what you've destroyed?\"",
                    
                _ => $"\"The letter... it never arrived, did it?\""
            };
        }

        private string GenerateBodyLanguage(EmotionalState state)
        {
            return state switch
            {
                EmotionalState.Anxious => "wringing hands, eyes searching your face for answers",
                EmotionalState.Hostile => "fists clenched, jaw tight with barely contained anger",
                EmotionalState.Closed => "back turned, shoulders rigid with finality",
                _ => "watching you with guarded disappointment"
            };
        }

        private List<ConfrontationChoice> GenerateConfrontationChoices(EmotionalState state)
        {
            var choices = new List<ConfrontationChoice>();

            // Always include these three core responses
            choices.Add(new ConfrontationChoice
            {
                Id = "apologetic",
                Text = "I'm sorry. I failed you. There's no excuse.",
                ResponseType = ConfrontationResponse.Apologetic,
                AttentionCost = 0 // Confrontations are free
            });

            choices.Add(new ConfrontationChoice
            {
                Id = "defensive",
                Text = "I had impossible choices. Someone was going to suffer no matter what.",
                ResponseType = ConfrontationResponse.Defensive,
                AttentionCost = 0
            });

            choices.Add(new ConfrontationChoice
            {
                Id = "silent",
                Text = "...",
                Description = "[Accept their anger in silence]",
                ResponseType = ConfrontationResponse.Silent,
                AttentionCost = 0
            });

            return choices;
        }

        private string GenerateNPCResponse(ConfrontationData data, ConfrontationChoice choice)
        {
            return (data.EmotionalState, choice.ResponseType) switch
            {
                (EmotionalState.Anxious, ConfrontationResponse.Apologetic) =>
                    "\"Sorry doesn't bring back what was lost. But... perhaps there's still time to fix some of this.\"",
                    
                (EmotionalState.Anxious, ConfrontationResponse.Defensive) =>
                    "\"There's always a more important letter, isn't there? I thought mine mattered too.\"",
                    
                (EmotionalState.Anxious, ConfrontationResponse.Silent) =>
                    "\"Your silence says enough. Please... just deliver the next one on time.\"",
                    
                (EmotionalState.Hostile, ConfrontationResponse.Apologetic) =>
                    "\"Words. Just words. You can't apologize away what you've cost me.\"",
                    
                (EmotionalState.Hostile, ConfrontationResponse.Defensive) =>
                    "\"Impossible choices? You made YOUR choice. And I'm the one paying for it.\"",
                    
                (EmotionalState.Hostile, ConfrontationResponse.Silent) =>
                    "\"At least you don't insult me with excuses. Now leave. We're done here.\"",
                    
                _ => "\"I don't know what to say to you anymore.\""
            };
        }

        private Letter GetMostRecentExpiredLetter(NPC npc)
        {
            // Try to find expired letters from this NPC in history
            // For now, return a generic description
            // In full implementation, would track actual expired letters
            return new Letter
            {
                SenderName = npc.Name,
                RecipientName = "the merchant guild",
                Description = "urgent trade agreement",
                Stakes = StakeType.WEALTH
            };
        }

        private EmotionalState GetEmotionalStateFromFailures(int failureCount)
        {
            return failureCount switch
            {
                0 => EmotionalState.Neutral,
                1 => EmotionalState.Anxious,
                2 => EmotionalState.Hostile,
                _ => EmotionalState.Closed
            };
        }

        private int GetRedemptionThreshold(EmotionalState state)
        {
            return state switch
            {
                EmotionalState.Anxious => 3,  // 3 good actions to return to Neutral
                EmotionalState.Hostile => 5,  // 5 good actions to become merely Anxious
                EmotionalState.Closed => 10,  // Nearly impossible to recover from
                _ => 1
            };
        }

        private void ImproveEmotionalState(NPC npc)
        {
            var player = _gameWorld.GetPlayer();
            var history = player.NPCLetterHistory[npc.ID];
            
            // Reduce the emotional impact slightly
            if (history.ExpiredCount > 0)
            {
                // We don't erase history, but we can reset progress
                npc.RedemptionProgress = 0;
                
                _messageSystem.AddSystemMessage(
                    $"Your consistent efforts have slightly improved {npc.Name}'s opinion of you.",
                    SystemMessageTypes.Success
                );
            }
        }
    }

    /// <summary>
    /// Data structure for a confrontation scene
    /// </summary>
    public class ConfrontationData
    {
        public NPC NPC { get; set; }
        public EmotionalState EmotionalState { get; set; }
        public int Leverage { get; set; }
        public int ExpiredCount { get; set; }
        public Letter SpecificLetter { get; set; }
        public string OpeningDialogue { get; set; }
        public string BodyLanguage { get; set; }
        public List<ConfrontationChoice> Choices { get; set; }
    }

    /// <summary>
    /// Player choice during confrontation
    /// </summary>
    public class ConfrontationChoice
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }  // For non-verbal choices like silence
        public ConfrontationResponse ResponseType { get; set; }
        public int AttentionCost { get; set; } = 0;  // Always 0 for confrontations
    }

    /// <summary>
    /// Types of responses to confrontation
    /// </summary>
    public enum ConfrontationResponse
    {
        Apologetic,   // Accept fault, express remorse
        Defensive,    // Justify actions, explain constraints
        Silent        // Accept anger without response
    }

    /// <summary>
    /// Result of processing a confrontation
    /// </summary>
    public class ConfrontationResult
    {
        public string NPCResponse { get; set; }
        public int RedemptionGained { get; set; }
        public bool RelationshipImproved { get; set; }
        public bool IsComplete { get; set; }
    }
}