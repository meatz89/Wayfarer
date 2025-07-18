using System;
using System.Collections.Generic;
using System.Linq;
    /// <summary>
    /// Manages NPC network unlocks based on relationship levels.
    /// When players build strong relationships (5+ tokens), NPCs can introduce them to new contacts.
    /// </summary>
    public class NetworkUnlockManager
    {
        private readonly GameWorld _gameWorld;
        private readonly NPCRepository _npcRepository;
        private readonly NetworkUnlockRepository _unlockRepository;
        private readonly ConnectionTokenManager _tokenManager;
        private readonly MessageSystem _messageSystem;
        
        public NetworkUnlockManager(
            GameWorld gameWorld,
            NPCRepository npcRepository,
            NetworkUnlockRepository unlockRepository,
            ConnectionTokenManager tokenManager,
            MessageSystem messageSystem)
        {
            _gameWorld = gameWorld;
            _npcRepository = npcRepository;
            _unlockRepository = unlockRepository;
            _tokenManager = tokenManager;
            _messageSystem = messageSystem;
        }
        
        /// <summary>
        /// Check if an NPC can unlock network contacts based on relationship level.
        /// </summary>
        public bool CanNPCUnlockNetwork(string npcId)
        {
            var unlocks = _unlockRepository.GetNetworkUnlocksForNpc(npcId);
            if (!unlocks.Any())
            {
                return false;
            }
            
            var npcTokens = _tokenManager.GetTokensWithNPC(npcId);
            var totalTokens = npcTokens.Values.Sum();
            
            // Check if player meets token requirement for any unlock rule
            return unlocks.Any(u => totalTokens >= u.TokensRequired);
        }
        
        /// <summary>
        /// Get list of NPCs that can be unlocked by a specific NPC.
        /// </summary>
        public List<string> GetUnlockableNPCs(string npcId)
        {
            if (!CanNPCUnlockNetwork(npcId))
            {
                return new List<string>();
            }
            
            var unlocks = _unlockRepository.GetNetworkUnlocksForNpc(npcId);
            var npcTokens = _tokenManager.GetTokensWithNPC(npcId);
            var totalTokens = npcTokens.Values.Sum();
            var player = _gameWorld.GetPlayer();
            var unlockedNPCs = player.UnlockedNPCIds;
            
            var unlockableNPCs = new List<string>();
            
            // Check each unlock rule
            foreach (var unlock in unlocks)
            {
                if (totalTokens >= unlock.TokensRequired)
                {
                    // Add NPCs from this unlock that aren't already unlocked
                    var newNPCs = unlock.Unlocks
                        .Where(u => !unlockedNPCs.Contains(u.NpcId))
                        .Select(u => u.NpcId);
                    unlockableNPCs.AddRange(newNPCs);
                }
            }
            
            return unlockableNPCs.Distinct().ToList();
        }
        
        /// <summary>
        /// Unlock a network contact through an NPC introduction.
        /// </summary>
        public bool UnlockNetworkContact(string introducerNpcId, string unlockNpcId)
        {
            // Validate introducer can unlock
            if (!CanNPCUnlockNetwork(introducerNpcId))
            {
                _messageSystem.AddSystemMessage(
                    "You don't have a strong enough relationship for introductions.",
                    SystemMessageTypes.Warning
                );
                return false;
            }
            
            var introducer = _npcRepository.GetNPCById(introducerNpcId);
            var unlockNpc = _npcRepository.GetNPCById(unlockNpcId);
            
            if (introducer == null || unlockNpc == null)
            {
                return false;
            }
            
            // Check if introducer can unlock this specific NPC
            var unlockTarget = _unlockRepository.GetUnlockTarget(introducerNpcId, unlockNpcId);
            if (unlockTarget == null)
            {
                return false;
            }
            
            // Check if already unlocked
            var player = _gameWorld.GetPlayer();
            if (player.UnlockedNPCIds.Contains(unlockNpcId))
            {
                _messageSystem.AddSystemMessage(
                    $"You already know {unlockNpc.Name}.",
                    SystemMessageTypes.Info
                );
                return false;
            }
            
            // Unlock the NPC
            player.UnlockedNPCIds.Add(unlockNpcId);
            
            // Success message with introduction text
            _messageSystem.AddSystemMessage(
                $"🤝 {introducer.Name} introduces you to {unlockNpc.Name}!",
                SystemMessageTypes.Success
            );
            
            if (!string.IsNullOrEmpty(unlockTarget.IntroductionText))
            {
                _messageSystem.AddSystemMessage(
                    $"\"{unlockTarget.IntroductionText}\"",
                    SystemMessageTypes.Info
                );
            }
            
            _messageSystem.AddSystemMessage(
                $"You can now find {unlockNpc.Name} at {GetNPCLocationName(unlockNpc)}.",
                SystemMessageTypes.Info
            );
            
            // Give a small token bonus with the new contact
            if (unlockNpc.LetterTokenTypes != null && unlockNpc.LetterTokenTypes.Any())
            {
                // Give 1 token of their primary type
                var primaryTokenType = unlockNpc.LetterTokenTypes.First();
                _tokenManager.AddTokens(primaryTokenType, 1, unlockNpcId);
                _messageSystem.AddSystemMessage(
                    $"You gain 1 {primaryTokenType} token with {unlockNpc.Name}.",
                    SystemMessageTypes.Success
                );
            }
            
            return true;
        }
        
        /// <summary>
        /// Check all NPCs for potential network unlocks.
        /// Called when player visits a location.
        /// </summary>
        public void CheckForNetworkUnlocks(string locationId)
        {
            var locationNPCs = _npcRepository.GetNPCsForLocation(locationId);
            
            foreach (var npc in locationNPCs)
            {
                if (CanNPCUnlockNetwork(npc.ID))
                {
                    var unlockableNPCs = GetUnlockableNPCs(npc.ID);
                    if (unlockableNPCs.Any())
                    {
                        _messageSystem.AddSystemMessage(
                            $"💡 {npc.Name} could introduce you to their contacts!",
                            SystemMessageTypes.Info
                        );
                    }
                }
            }
        }
        
        /// <summary>
        /// Get display name for NPC's location.
        /// </summary>
        private string GetNPCLocationName(NPC npc)
        {
            var location = _gameWorld.WorldState.locations
                .FirstOrDefault(l => l.Id == npc.Location);
            
            return location?.Name ?? "an unknown location";
        }
    }
