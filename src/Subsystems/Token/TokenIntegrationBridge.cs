using System;
using System.Collections.Generic;

namespace Wayfarer.Subsystems.TokenSubsystem
{
    /// <summary>
    /// Bridge class to integrate the new TokenFacade with the existing TokenMechanicsManager.
    /// This allows for gradual migration from the old system to the new subsystem.
    /// </summary>
    public class TokenIntegrationBridge
    {
        private readonly TokenFacade _tokenFacade;
        private readonly TokenMechanicsManager _tokenManager;

        public TokenIntegrationBridge(
            TokenFacade tokenFacade,
            TokenMechanicsManager tokenManager)
        {
            _tokenFacade = tokenFacade;
            _tokenManager = tokenManager;
        }

        /// <summary>
        /// Migrate token operations to use the new facade while maintaining compatibility
        /// </summary>
        public void AddTokensToNPC(ConnectionType type, int count, string npcId)
        {
            // Use the new facade for all operations
            _tokenFacade.AddTokensToNPC(type, count, npcId);
        }

        /// <summary>
        /// Get tokens using the new facade
        /// </summary>
        public Dictionary<ConnectionType, int> GetTokensWithNPC(string npcId)
        {
            return _tokenFacade.GetTokensWithNPC(npcId);
        }

        /// <summary>
        /// Spend tokens using the new facade
        /// </summary>
        public bool SpendTokens(ConnectionType type, int amount, string npcId)
        {
            return _tokenFacade.SpendTokensWithNPC(type, amount, npcId);
        }

        /// <summary>
        /// Check relationship status using new subsystem features
        /// </summary>
        public RelationshipSummary GetRelationshipStatus(string npcId)
        {
            return new RelationshipSummary
            {
                NPCId = npcId,
                Tokens = _tokenFacade.GetTokensWithNPC(npcId),
                RelationshipTier = _tokenFacade.GetRelationshipTier(npcId),
                PrimaryConnection = _tokenFacade.GetPrimaryConnection(npcId),
                HasDebt = _tokenFacade.GetTotalLeverage(npcId) > 0
            };
        }

        /// <summary>
        /// Calculate token effects for conversations
        /// </summary>
        public int CalculateConversationBonus(string npcId, ConnectionType cardType)
        {
            // Use facade's effect calculation
            int baseChance = 50; // Default base chance
            int tokenBonus = _tokenFacade.CalculateTokenBonus(cardType, baseChance);

            // Add relationship-specific bonus
            if (_tokenFacade.GetRelationshipTier(npcId) >= RelationshipTier.Friend)
            {
                tokenBonus += 10; // Friend bonus
            }

            return tokenBonus;
        }

        /// <summary>
        /// Check if player can access special content
        /// </summary>
        public bool CanAccessSpecialContent(string npcId, string contentType)
        {
            switch (contentType.ToLower())
            {
                case "personal_letter":
                    return _tokenFacade.GetTokenCount(npcId, ConnectionType.Trust) >= 3;

                case "business_deal":
                    return _tokenFacade.GetTokenCount(npcId, ConnectionType.Commerce) >= 4;

                case "noble_invitation":
                    return _tokenFacade.GetTokenCount(npcId, ConnectionType.Status) >= 5;

                case "secret_mission":
                    return _tokenFacade.GetTokenCount(npcId, ConnectionType.Shadow) >= 4;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Process relationship decay (to be called periodically)
        /// </summary>
        public void ProcessRelationshipDecay()
        {
            // The RelationshipTracker handles decay internally
            // This would be called by a time system update

            // For now, we can check for NPCs that haven't been interacted with
            List<string> allNPCsWithTokens = _tokenFacade.GetNPCsWithTokens();

            foreach (string npcId in allNPCsWithTokens)
            {
                // In a full implementation, this would check last interaction time
                // and apply decay if needed
            }
        }
    }

    // RelationshipSummary class removed - use the one in RelationshipTracker.cs instead
}