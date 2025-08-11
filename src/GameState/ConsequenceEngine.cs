using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.GameState
{
    /// <summary>
    /// Manages consequences for missed letter deadlines and failed obligations.
    /// Creates mounting pressure through leverage, emotional states, and network effects.
    /// </summary>
    public class ConsequenceEngine
    {
        private readonly GameWorld _gameWorld;
        private readonly ConnectionTokenManager _tokenManager;
        private readonly NPCRepository _npcRepository;
        private readonly MessageSystem _messageSystem;
        private readonly WorldMemorySystem _worldMemory;
        
        // Track leverage for each NPC (how much power they have over player)
        private readonly Dictionary<string, int> _npcLeverage = new Dictionary<string, int>();
        
        // Track locked conversation options per NPC
        private readonly Dictionary<string, HashSet<BaseVerb>> _lockedVerbs = new Dictionary<string, HashSet<BaseVerb>>();
        
        // Track service lockouts per location
        private readonly Dictionary<string, HashSet<ServiceTypes>> _lockedServices = new Dictionary<string, HashSet<ServiceTypes>>();
        
        public ConsequenceEngine(
            GameWorld gameWorld,
            ConnectionTokenManager tokenManager,
            NPCRepository npcRepository,
            MessageSystem messageSystem,
            WorldMemorySystem worldMemory = null)
        {
            _gameWorld = gameWorld;
            _tokenManager = tokenManager;
            _npcRepository = npcRepository;
            _messageSystem = messageSystem;
            _worldMemory = worldMemory;
        }
        
        /// <summary>
        /// Apply all consequences for a missed letter deadline.
        /// This is the main entry point called when a letter expires.
        /// </summary>
        public void ApplyMissedDeadlineConsequences(Letter expiredLetter)
        {
            if (expiredLetter == null) return;
            
            string senderId = GetNPCIdByName(expiredLetter.SenderName);
            if (string.IsNullOrEmpty(senderId)) return;
            
            NPC sender = _npcRepository.GetById(senderId);
            if (sender == null) return;
            
            // Get failure count for escalating consequences
            int failureCount = GetFailureCount(senderId);
            
            // Record event for environmental storytelling
            _worldMemory?.RecordEvent(
                WorldEventType.DeadlineMissed,
                senderId,
                expiredLetter.RecipientName,
                sender.Location);
            
            // 1. IMMEDIATE CONSEQUENCE: Token penalty and leverage increase
            ApplyImmediateConsequences(expiredLetter, sender, failureCount);
            
            // 2. EMOTIONAL STATE CHANGE: Update NPC's emotional state
            UpdateEmotionalState(sender, failureCount);
            
            // 3. ESCALATING CONSEQUENCES: Based on failure count
            if (failureCount == 1)
            {
                ApplyFirstFailureConsequences(sender);
            }
            else if (failureCount == 2)
            {
                ApplySecondFailureConsequences(sender);
            }
            else if (failureCount >= 3)
            {
                ApplyThirdFailureConsequences(sender);
            }
            
            // 4. DRAMATIC NOTIFICATION: Make the failure visceral
            ShowFailureNarrative(expiredLetter, sender, failureCount);
        }
        
        /// <summary>
        /// Apply immediate consequences that happen for every failure
        /// </summary>
        private void ApplyImmediateConsequences(Letter letter, NPC sender, int failureCount)
        {
            // Trust penalty for breaking your promise - this is the emotional cost
            // You promised to deliver and failed. Trust is broken.
            int trustPenalty = 2; // Breaking a promise hurts, but not as much as refusing
            
            // Apply trust loss - this is the primary consequence
            _tokenManager.RemoveTokensFromNPC(ConnectionType.Trust, trustPenalty, sender.ID);
            
            // Also lose tokens of the letter's type
            int baseTokenPenalty = 1;
            int tokenPenalty = (int)(baseTokenPenalty * (1 + failureCount * 0.5f));
            _tokenManager.RemoveTokensFromNPC(letter.TokenType, tokenPenalty, sender.ID);
            
            // Increase leverage - this NPC now has power over you
            IncreaseLeverage(sender.ID, 2);
            
            // Show immediate feedback - emphasize the broken promise
            _messageSystem.AddSystemMessage(
                $"💔 Lost {trustPenalty} Trust with {sender.Name} - you broke your promise!",
                SystemMessageTypes.Danger
            );
            
            _messageSystem.AddSystemMessage(
                $"Lost {tokenPenalty} {letter.TokenType} tokens with {sender.Name}",
                SystemMessageTypes.Warning
            );
            
            _messageSystem.AddSystemMessage(
                $"⚖️ {sender.Name} gains LEVERAGE over you (debt: {GetLeverage(sender.ID)})",
                SystemMessageTypes.Warning
            );
        }
        
        /// <summary>
        /// Update NPC emotional state based on failures
        /// </summary>
        private void UpdateEmotionalState(NPC npc, int failureCount)
        {
            EmotionalState newState = failureCount switch
            {
                0 => EmotionalState.Neutral,
                1 => EmotionalState.Anxious,
                2 => EmotionalState.Hostile,
                _ => EmotionalState.Closed
            };
            
            // TODO: Store the emotional state change when NPCEmotionalStateCalculator supports it
            // For now, just show the message
            
            if (newState != EmotionalState.Neutral)
            {
                _messageSystem.AddSystemMessage(
                    $"😰 {npc.Name} is now {newState.ToString().ToUpper()}",
                    SystemMessageTypes.Warning
                );
            }
        }
        
        /// <summary>
        /// First failure: Individual NPC reacts
        /// </summary>
        private void ApplyFirstFailureConsequences(NPC npc)
        {
            // Future letters from this NPC will have more leverage
            _messageSystem.AddSystemMessage(
                $"📮 Future letters from {npc.Name} will demand higher priority",
                SystemMessageTypes.Info
            );
        }
        
        /// <summary>
        /// Second failure: Network effects begin
        /// </summary>
        private void ApplySecondFailureConsequences(NPC npc)
        {
            // Lock HELP verb - they won't help you anymore
            LockConversationVerb(npc.ID, BaseVerb.HELP);
            
            _messageSystem.AddSystemMessage(
                $"🚫 {npc.Name} refuses to HELP you anymore",
                SystemMessageTypes.Warning
            );
            
            // Network effect: nearby NPCs lose trust
            ApplyNetworkPenalty(npc, radius: 1);
        }
        
        /// <summary>
        /// Third+ failure: Systemic consequences
        /// </summary>
        private void ApplyThirdFailureConsequences(NPC npc)
        {
            // Lock multiple conversation options
            LockConversationVerb(npc.ID, BaseVerb.NEGOTIATE);
            
            // Lock services at their location
            if (!string.IsNullOrEmpty(npc.Location))
            {
                LockLocationService(npc.Location, ServiceTypes.Trade);
                _messageSystem.AddSystemMessage(
                    $"🏪 Trading services LOCKED at {npc.Location}",
                    SystemMessageTypes.Danger
                );
            }
            
            // Create permanent debt obligation
            _messageSystem.AddSystemMessage(
                $"📜 PERMANENT DEBT: {npc.Name} will never forget this betrayal",
                SystemMessageTypes.Danger
            );
        }
        
        /// <summary>
        /// Apply network effects - other NPCs react to your failure
        /// </summary>
        private void ApplyNetworkPenalty(NPC failedNPC, int radius)
        {
            // Get NPCs in the same location
            var nearbyNPCs = _npcRepository.GetAllNPCs()
                .Where(n => n.ID != failedNPC.ID && n.Location == failedNPC.Location)
                .Take(radius * 2); // Limit network effect scope
            
            foreach (var npc in nearbyNPCs)
            {
                // Small token loss with nearby NPCs
                _tokenManager.RemoveTokensFromNPC(ConnectionType.Trust, 1, npc.ID);
            }
            
            _messageSystem.AddSystemMessage(
                $"👥 Word spreads... nearby NPCs lose trust in you",
                SystemMessageTypes.Warning
            );
        }
        
        /// <summary>
        /// Show dramatic narrative for the failure
        /// </summary>
        private void ShowFailureNarrative(Letter letter, NPC sender, int failureCount)
        {
            // Dramatic header
            _messageSystem.AddSystemMessage(
                $"⏰ DEADLINE MISSED: Letter from {letter.SenderName} to {letter.RecipientName}",
                SystemMessageTypes.Danger
            );
            
            // Contextual narrative based on failure count - emphasize broken promises
            string narrative = failureCount switch
            {
                1 => $"\"{sender.Name} trusted you with this. You promised to deliver. That promise is broken.\"",
                2 => $"\"{sender.Name} tells others: 'They make promises they can't keep.'\"",
                3 => $"\"Word spreads: You're someone who breaks their word when things get hard.\"",
                _ => $"\"No one trusts your promises anymore. Your word means nothing.\""
            };
            
            _messageSystem.AddSystemMessage(narrative, SystemMessageTypes.Warning);
            
            // Show the stakes that were lost
            string stakesText = letter.Stakes switch
            {
                StakeType.WEALTH => "Financial opportunity lost",
                StakeType.SAFETY => "Someone's safety was at stake",
                StakeType.REPUTATION => "Reputations have been damaged",
                StakeType.SECRET => "Secrets may be exposed",
                _ => "Important consequences will follow"
            };
            
            _messageSystem.AddSystemMessage(
                $"Lost: {stakesText}",
                SystemMessageTypes.Info
            );
        }
        
        // === LEVERAGE SYSTEM ===
        
        /// <summary>
        /// Get current leverage an NPC has over the player
        /// </summary>
        public int GetLeverage(string npcId)
        {
            return _npcLeverage.TryGetValue(npcId, out int leverage) ? leverage : 0;
        }
        
        /// <summary>
        /// Increase an NPC's leverage over the player
        /// </summary>
        public void IncreaseLeverage(string npcId, int amount)
        {
            if (!_npcLeverage.ContainsKey(npcId))
                _npcLeverage[npcId] = 0;
            
            _npcLeverage[npcId] += amount;
        }
        
        /// <summary>
        /// Decrease leverage through successful deliveries or HELP actions
        /// </summary>
        public void DecreaseLeverage(string npcId, int amount)
        {
            if (_npcLeverage.ContainsKey(npcId))
            {
                _npcLeverage[npcId] = Math.Max(0, _npcLeverage[npcId] - amount);
                
                if (_npcLeverage[npcId] == 0)
                {
                    _messageSystem.AddSystemMessage(
                        $"✨ Debt cleared with {_npcRepository.GetById(npcId)?.Name}",
                        SystemMessageTypes.Success
                    );
                }
            }
        }
        
        // === CONVERSATION LOCKOUT SYSTEM ===
        
        /// <summary>
        /// Check if a conversation verb is locked for an NPC
        /// </summary>
        public bool IsVerbLocked(string npcId, BaseVerb verb)
        {
            return _lockedVerbs.TryGetValue(npcId, out var locked) && locked.Contains(verb);
        }
        
        /// <summary>
        /// Lock a conversation verb for an NPC
        /// </summary>
        private void LockConversationVerb(string npcId, BaseVerb verb)
        {
            if (!_lockedVerbs.ContainsKey(npcId))
                _lockedVerbs[npcId] = new HashSet<BaseVerb>();
            
            _lockedVerbs[npcId].Add(verb);
        }
        
        // === SERVICE LOCKOUT SYSTEM ===
        
        /// <summary>
        /// Check if a service is locked at a location
        /// </summary>
        public bool IsServiceLocked(string locationId, ServiceTypes service)
        {
            return _lockedServices.TryGetValue(locationId, out var locked) && locked.Contains(service);
        }
        
        /// <summary>
        /// Lock a service at a location
        /// </summary>
        private void LockLocationService(string locationId, ServiceTypes service)
        {
            if (!_lockedServices.ContainsKey(locationId))
                _lockedServices[locationId] = new HashSet<ServiceTypes>();
            
            _lockedServices[locationId].Add(service);
        }
        
        // === HELPER METHODS ===
        
        private int GetFailureCount(string npcId)
        {
            Player player = _gameWorld.GetPlayer();
            if (player.NPCLetterHistory.TryGetValue(npcId, out var history))
            {
                return history.ExpiredCount;
            }
            return 0;
        }
        
        private string GetNPCIdByName(string npcName)
        {
            var npc = _npcRepository.GetAllNPCs().FirstOrDefault(n => n.Name == npcName);
            return npc?.ID;
        }
    }
    
    /// <summary>
    /// Emotional states that NPCs can be in based on player failures
    /// </summary>
    public enum EmotionalState
    {
        Neutral,    // Default state, no failures
        Anxious,    // 1 failure - worried about reliability
        Hostile,    // 2 failures - actively angry
        Closed      // 3+ failures - refuses most interaction
    }
}