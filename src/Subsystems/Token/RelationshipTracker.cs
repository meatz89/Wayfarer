using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Subsystems.TokenSubsystem;

namespace Wayfarer.Subsystems.TokenSubsystem
{
    /// <summary>
    /// Tracks relationship states, progression, debts, and milestones with NPCs.
    /// </summary>
    public class RelationshipTracker
    {
        private readonly GameWorld _gameWorld;
        private readonly ConnectionTokenManager _tokenManager;
        private readonly NPCRepository _npcRepository;
        private readonly MessageSystem _messageSystem;
        
        // Track debts separately for easier querying
        private readonly Dictionary<string, Dictionary<ConnectionType, int>> _activeDebts;
        
        // Track last interaction times for decay calculations
        private readonly Dictionary<string, DateTime> _lastInteractionTimes;
        
        // Relationship milestones
        private readonly Dictionary<int, string> _relationshipMilestones = new Dictionary<int, string>
        {
            { 3, "now trusts you enough to share private correspondence" },
            { 5, "bond has deepened. They'll offer more valuable letters" },
            { 8, "considers you among their most trusted associates" },
            { 12, "few people enjoy this level of trust" },
            { 15, "would trust you with their life" },
            { 20, "shares an unbreakable bond with you" }
        };
        
        public RelationshipTracker(
            GameWorld gameWorld,
            ConnectionTokenManager tokenManager,
            NPCRepository npcRepository,
            MessageSystem messageSystem)
        {
            _gameWorld = gameWorld;
            _tokenManager = tokenManager;
            _npcRepository = npcRepository;
            _messageSystem = messageSystem;
            _activeDebts = new Dictionary<string, Dictionary<ConnectionType, int>>();
            _lastInteractionTimes = new Dictionary<string, DateTime>();
        }
        
        /// <summary>
        /// Update relationship state after token changes
        /// </summary>
        public void UpdateRelationshipState(string npcId)
        {
            if (string.IsNullOrEmpty(npcId)) return;
            
            // Update last interaction time
            _lastInteractionTimes[npcId] = DateTime.Now;
            
            // Check for new debts
            Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);
            bool hasDebt = false;
            
            foreach (ConnectionType type in Enum.GetValues<ConnectionType>())
            {
                if (type == ConnectionType.None) continue;
                
                int tokenCount = tokens.GetValueOrDefault(type, 0);
                if (tokenCount < 0)
                {
                    hasDebt = true;
                    
                    // Track this debt
                    if (!_activeDebts.ContainsKey(npcId))
                    {
                        _activeDebts[npcId] = new Dictionary<ConnectionType, int>();
                    }
                    _activeDebts[npcId][type] = Math.Abs(tokenCount);
                }
                else if (_activeDebts.ContainsKey(npcId) && _activeDebts[npcId].ContainsKey(type))
                {
                    // Debt cleared
                    _activeDebts[npcId].Remove(type);
                    if (_activeDebts[npcId].Count == 0)
                    {
                        _activeDebts.Remove(npcId);
                    }
                }
            }
            
            // Update NPC state if needed
            UpdateNPCDisposition(npcId, hasDebt);
        }
        
        /// <summary>
        /// Check and announce relationship milestones
        /// </summary>
        public void CheckRelationshipMilestone(string npcId, int totalTokens)
        {
            NPC npc = _npcRepository.GetById(npcId);
            if (npc == null) return;
            
            foreach (KeyValuePair<int, string> milestone in _relationshipMilestones)
            {
                if (totalTokens == milestone.Key)
                {
                    _messageSystem.AddSystemMessage(
                        $"{npc.Name} {milestone.Value}.",
                        SystemMessageTypes.Success
                    );
                    
                    // Special announcements for major milestones
                    if (milestone.Key >= 10)
                    {
                        _messageSystem.AddSystemMessage(
                            $"Your relationship with {npc.Name} has reached a rare level of trust.",
                            SystemMessageTypes.Success
                        );
                    }
                    
                    break;
                }
            }
        }
        
        /// <summary>
        /// Record a debt to an NPC
        /// </summary>
        public void RecordDebt(string npcId, ConnectionType type, int amount)
        {
            if (amount <= 0) return;
            
            if (!_activeDebts.ContainsKey(npcId))
            {
                _activeDebts[npcId] = new Dictionary<ConnectionType, int>();
            }
            
            _activeDebts[npcId][type] = amount;
            
            NPC npc = _npcRepository.GetById(npcId);
            if (npc != null)
            {
                _messageSystem.AddSystemMessage(
                    $"You now owe {npc.Name} {amount} {type} favor{(amount > 1 ? "s" : "")}.",
                    SystemMessageTypes.Warning
                );
            }
        }
        
        /// <summary>
        /// Get the primary connection type with an NPC (highest positive token count)
        /// </summary>
        public ConnectionType GetPrimaryConnection(string npcId)
        {
            Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);
            
            ConnectionType primaryType = ConnectionType.None;
            int highestCount = 0;
            
            foreach (KeyValuePair<ConnectionType, int> kvp in tokens)
            {
                if (kvp.Key != ConnectionType.None && kvp.Value > highestCount)
                {
                    highestCount = kvp.Value;
                    primaryType = kvp.Key;
                }
            }
            
            return primaryType;
        }
        
        /// <summary>
        /// Get relationship tier based on total tokens
        /// </summary>
        public RelationshipTier GetRelationshipTier(string npcId)
        {
            int totalTokens = GetTotalPositiveTokens(npcId);
            
            if (totalTokens >= 13) return RelationshipTier.InnerCircle;
            if (totalTokens >= 9) return RelationshipTier.Confidant;
            if (totalTokens >= 6) return RelationshipTier.CloseFriend;
            if (totalTokens >= 3) return RelationshipTier.Friend;
            if (totalTokens >= 1) return RelationshipTier.Acquaintance;
            return RelationshipTier.None;
        }
        
        /// <summary>
        /// Get all NPCs the player owes tokens to
        /// </summary>
        public List<DebtInfo> GetAllDebts()
        {
            List<DebtInfo> debts = new List<DebtInfo>();
            
            foreach (KeyValuePair<string, Dictionary<ConnectionType, int>> npcDebt in _activeDebts)
            {
                NPC npc = _npcRepository.GetById(npcDebt.Key);
                if (npc == null) continue;
                
                DebtInfo debtInfo = new DebtInfo
                {
                    NPCId = npcDebt.Key,
                    NPCName = npc.Name,
                    Debts = new Dictionary<ConnectionType, int>(npcDebt.Value),
                    TotalDebt = npcDebt.Value.Values.Sum()
                };
                
                debts.Add(debtInfo);
            }
            
            return debts.OrderByDescending(d => d.TotalDebt).ToList();
        }
        
        /// <summary>
        /// Check if player has any debts
        /// </summary>
        public bool HasAnyDebt()
        {
            return _activeDebts.Count > 0;
        }
        
        /// <summary>
        /// Get relationship summary for an NPC
        /// </summary>
        public RelationshipSummary GetRelationshipSummary(string npcId)
        {
            NPC npc = _npcRepository.GetById(npcId);
            if (npc == null) return null;
            
            Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);
            
            return new RelationshipSummary
            {
                NPCId = npcId,
                NPCName = npc.Name,
                Tokens = new Dictionary<ConnectionType, int>(tokens),
                PrimaryConnection = GetPrimaryConnection(npcId),
                RelationshipTier = GetRelationshipTier(npcId),
                TotalPositiveTokens = GetTotalPositiveTokens(npcId),
                HasDebt = _activeDebts.ContainsKey(npcId),
                TotalDebt = _activeDebts.ContainsKey(npcId) ? _activeDebts[npcId].Values.Sum() : 0,
                LastInteraction = _lastInteractionTimes.ContainsKey(npcId) ? _lastInteractionTimes[npcId] : DateTime.MinValue
            };
        }
        
        /// <summary>
        /// Process relationship decay over time
        /// </summary>
        public void ProcessRelationshipDecay()
        {
            DateTime now = DateTime.Now;
            
            foreach (string npcId in _tokenManager.GetNPCsWithTokens())
            {
                if (!_lastInteractionTimes.ContainsKey(npcId)) continue;
                
                DateTime lastInteraction = _lastInteractionTimes[npcId];
                int daysSinceInteraction = (int)(now - lastInteraction).TotalDays;
                
                // Only decay after a week of no interaction
                if (daysSinceInteraction < 7) continue;
                
                Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);
                bool hadDecay = false;
                
                foreach (ConnectionType type in Enum.GetValues<ConnectionType>())
                {
                    if (type == ConnectionType.None) continue;
                    
                    int currentTokens = tokens.GetValueOrDefault(type, 0);
                    if (currentTokens <= 0) continue;
                    
                    // Calculate decay based on time and token type
                    int decay = CalculateDecay(type, currentTokens, daysSinceInteraction);
                    if (decay > 0)
                    {
                        _tokenManager.RemoveTokensFromNPC(type, decay, npcId);
                        hadDecay = true;
                    }
                }
                
                if (hadDecay)
                {
                    NPC npc = _npcRepository.GetById(npcId);
                    if (npc != null)
                    {
                        _messageSystem.AddSystemMessage(
                            $"Your relationship with {npc.Name} has weakened due to lack of contact.",
                            SystemMessageTypes.Warning
                        );
                    }
                }
            }
        }
        
        // ========== PRIVATE HELPER METHODS ==========
        
        private void UpdateNPCDisposition(string npcId, bool hasDebt)
        {
            // This would update NPC's disposition/attitude towards player
            // For now, we just track the state
            
            if (hasDebt)
            {
                // NPC is less friendly when owed favors
                // This could affect conversation options, prices, etc.
            }
        }
        
        private int GetTotalPositiveTokens(string npcId)
        {
            Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);
            return tokens.Values.Where(v => v > 0).Sum();
        }
        
        private int CalculateDecay(ConnectionType type, int currentTokens, int daysSinceInteraction)
        {
            // Different token types decay at different rates
            float decayRate = type switch
            {
                ConnectionType.Trust => 0.02f,    // Trust decays slowly
                ConnectionType.Commerce => 0.01f, // Commerce is most stable
                ConnectionType.Status => 0.04f,   // Status decays faster
                ConnectionType.Shadow => 0.05f,   // Shadow decays fastest
                _ => 0.02f
            };
            
            // Decay accelerates with time
            int weeksWithoutContact = daysSinceInteraction / 7;
            float decayMultiplier = 1.0f + (weeksWithoutContact * 0.1f);
            
            int decay = (int)Math.Floor(currentTokens * decayRate * decayMultiplier);
            
            // Never decay more than 1 token per week for low token counts
            if (currentTokens <= 3)
            {
                decay = Math.Min(decay, weeksWithoutContact);
            }
            
            return decay;
        }
    }
    
    // ========== SUPPORTING TYPES ==========
    
    public class RelationshipSummary
    {
        public string NPCId { get; set; }
        public string NPCName { get; set; }
        public Dictionary<ConnectionType, int> Tokens { get; set; }
        public ConnectionType PrimaryConnection { get; set; }
        public RelationshipTier RelationshipTier { get; set; }
        public int TotalPositiveTokens { get; set; }
        public bool HasDebt { get; set; }
        public int TotalDebt { get; set; }
        public DateTime LastInteraction { get; set; }
    }
}