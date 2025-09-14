using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Subsystems.ObligationSubsystem;
using Wayfarer.GameState.Enums;

namespace Wayfarer.Subsystems.ObligationSubsystem
{
    /// <summary>
    /// Handles all queue displacement calculations and token burning logic.
    /// Manages voluntary displacement (player-initiated) and automatic displacement (forced by game mechanics).
    /// </summary>
    public class DisplacementCalculator
    {
        private readonly GameWorld _gameWorld;
        private readonly NPCRepository _npcRepository;
        private readonly MessageSystem _messageSystem;
        private readonly TokenMechanicsManager _tokenManager;
        private readonly GameConfiguration _config;

        public DisplacementCalculator(
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
        /// Calculate the cost and feasibility of displacing an obligation to a target position.
        /// </summary>
        public DisplacementResult CalculateDisplacement(string obligationId, int targetPosition)
        {
            DisplacementResult result = new DisplacementResult();

            // Find the obligation in the queue
            DeliveryObligation obligation = FindObligationById(obligationId);
            if (obligation == null)
            {
                result.ErrorMessage = "Obligation not found in queue";
                return result;
            }

            int currentPosition = GetQueuePosition(obligation);
            if (currentPosition <= 0)
            {
                result.ErrorMessage = "Unable to determine obligation position";
                return result;
            }

            // Validate displacement parameters
            DisplacementResult validation = ValidateDisplacementRequest(obligation, currentPosition, targetPosition);
            if (!validation.CanExecute)
            {
                result.ErrorMessage = validation.ErrorMessage;
                return result;
            }

            // Calculate displacement plan
            ObligationDisplacementPlan displacementPlan = CreateDisplacementPlan(obligation, currentPosition, targetPosition);
            result.DisplacementPlan = displacementPlan;

            // Check token availability
            result.CanExecute = ValidateTokenAvailability(displacementPlan);
            result.RequiredTokens = CalculateRequiredTokens(displacementPlan);
            result.TotalTokenCost = displacementPlan.TotalTokenCost;

            if (!result.CanExecute)
            {
                result.ErrorMessage = "Insufficient tokens for displacement";
            }

            return result;
        }

        /// <summary>
        /// Execute a calculated displacement by burning tokens and rearranging the queue.
        /// </summary>
        public DisplacementResult ExecuteDisplacement(DisplacementResult displacementResult)
        {
            DisplacementResult result = new DisplacementResult();

            if (!displacementResult.CanExecute || displacementResult.DisplacementPlan == null)
            {
                result.ErrorMessage = "Cannot execute invalid displacement";
                return result;
            }

            ObligationDisplacementPlan plan = displacementResult.DisplacementPlan;

            _messageSystem.AddSystemMessage(
                $"🔥 BURNING TOKENS: Moving {plan.ObligationToMove.SenderName}'s letter from position {plan.OriginalPosition} to {plan.TargetPosition}",
                SystemMessageTypes.Warning
            );

            // Execute token burning for each displaced obligation
            foreach (ObligationDisplacement displacement in plan.Displacements)
            {
                if (!BurnTokensForDisplacement(displacement))
                {
                    result.ErrorMessage = $"Failed to burn tokens with {displacement.DisplacedObligation.SenderName}";
                    return result;
                }
            }

            // Perform the queue rearrangement
            PerformQueueDisplacement(plan);

            result.CanExecute = true;
            result.DisplacementPlan = plan;

            _messageSystem.AddSystemMessage(
                $"✅ Successfully moved {plan.ObligationToMove.SenderName}'s letter to position {plan.TargetPosition}!",
                SystemMessageTypes.Success
            );

            _messageSystem.AddSystemMessage(
                $"⚠️ Total relationship cost: {plan.TotalTokenCost} tokens burned permanently",
                SystemMessageTypes.Warning
            );

            return result;
        }

        /// <summary>
        /// Execute automatic displacement when a new obligation forces a specific position.
        /// Used for failed negotiations, crisis letters, and proud NPCs.
        /// </summary>
        public DisplacementResult ExecuteAutomaticDisplacement(DeliveryObligation newObligation, int forcedPosition, string displacementReason)
        {
            DisplacementResult result = new DisplacementResult();

            if (newObligation == null || forcedPosition < 1 || forcedPosition > _config.LetterQueue.MaxQueueSize)
            {
                result.ErrorMessage = $"Invalid automatic displacement request for position {forcedPosition}";
                return result;
            }

            DeliveryObligation[] queue = GetPlayerQueue();

            // Announce the forced displacement
            _messageSystem.AddSystemMessage(
                $"⚡ {newObligation.SenderName}'s letter FORCES position {forcedPosition}!",
                SystemMessageTypes.Danger
            );
            _messageSystem.AddSystemMessage(
                $"  • {displacementReason}",
                SystemMessageTypes.Warning
            );

            // Check if the forced position is occupied
            if (queue[forcedPosition - 1] != null)
            {
                // Calculate displacement cascade
                AutoDisplacementInfo displacementInfo = CalculateAutomaticDisplacementCascade(queue, forcedPosition);

                if (displacementInfo.DisplacedObligations.Any())
                {
                    _messageSystem.AddSystemMessage(
                        $"📦 QUEUE CASCADE: {displacementInfo.DisplacedObligations.Count} obligations displaced!",
                        SystemMessageTypes.Warning
                    );

                    // Burn tokens for each displaced obligation
                    foreach (DisplacedObligation displaced in displacementInfo.DisplacedObligations)
                    {
                        BurnTokensForAutomaticDisplacement(displaced.Obligation, displaced.DisplacementAmount);
                    }

                    // Execute the cascade
                    ExecuteAutomaticDisplacementCascade(queue, forcedPosition, displacementInfo.DisplacedObligations);
                }
            }

            // Place the new obligation at the forced position
            queue[forcedPosition - 1] = newObligation;
            newObligation.QueuePosition = forcedPosition;

            // Set positioning metadata
            newObligation.PositioningReason = DeterminePositioningReasonForForced(newObligation, displacementReason);
            newObligation.FinalQueuePosition = forcedPosition;

            result.CanExecute = true;

            _messageSystem.AddSystemMessage(
                $"✅ {newObligation.SenderName}'s letter locked into position {forcedPosition}",
                SystemMessageTypes.Success
            );

            return result;
        }

        /// <summary>
        /// Get a preview of displacement costs without executing the displacement.
        /// </summary>
        public QueueDisplacementPreview GetDisplacementPreview(string obligationId, int targetPosition)
        {
            DisplacementResult displacementResult = CalculateDisplacement(obligationId, targetPosition);

            QueueDisplacementPreview preview = new QueueDisplacementPreview
            {
                CanExecute = displacementResult.CanExecute,
                ErrorMessage = displacementResult.ErrorMessage,
                TotalTokenCost = displacementResult.TotalTokenCost,
                DisplacementDetails = new List<DisplacementDetail>()
            };

            if (displacementResult.DisplacementPlan != null)
            {
                foreach (ObligationDisplacement displacement in displacementResult.DisplacementPlan.Displacements)
                {
                    preview.DisplacementDetails.Add(new DisplacementDetail
                    {
                        NPCName = displacement.DisplacedObligation.SenderName,
                        TokenType = displacement.DisplacedObligation.TokenType,
                        TokenCost = displacement.TokenCost,
                        FromPosition = displacement.OriginalPosition,
                        ToPosition = displacement.NewPosition
                    });
                }
            }

            return preview;
        }

        /// <summary>
        /// Check if a specific displacement is feasible given current token balances.
        /// </summary>
        public bool CanAffordDisplacement(string obligationId, int targetPosition)
        {
            DisplacementResult displacementResult = CalculateDisplacement(obligationId, targetPosition);
            return displacementResult.CanExecute;
        }

        /// <summary>
        /// Calculate total token cost for a potential displacement without creating full plan.
        /// </summary>
        public int CalculateDisplacementCost(string obligationId, int targetPosition)
        {
            DisplacementResult displacementResult = CalculateDisplacement(obligationId, targetPosition);
            return displacementResult.TotalTokenCost;
        }

        /// <summary>
        /// Get information about which NPCs would be affected by a displacement.
        /// </summary>
        public List<string> GetAffectedNPCs(string obligationId, int targetPosition)
        {
            DisplacementResult displacementResult = CalculateDisplacement(obligationId, targetPosition);
            List<string> affectedNPCs = new List<string>();

            if (displacementResult.DisplacementPlan != null)
            {
                foreach (ObligationDisplacement displacement in displacementResult.DisplacementPlan.Displacements)
                {
                    affectedNPCs.Add(displacement.DisplacedObligation.SenderName);
                }
            }

            return affectedNPCs;
        }

        // Private helper methods

        private DisplacementResult ValidateDisplacementRequest(DeliveryObligation obligation, int currentPosition, int targetPosition)
        {
            DisplacementResult result = new DisplacementResult();

            // Can't displace backwards (position must be lower number = earlier in queue)
            if (targetPosition >= currentPosition)
            {
                result.CanExecute = false;
                result.ErrorMessage = $"Cannot displace backwards from position {currentPosition} to {targetPosition}";
                return result;
            }

            // Validate target position range
            if (targetPosition < 1 || targetPosition > _config.LetterQueue.MaxQueueSize)
            {
                result.CanExecute = false;
                result.ErrorMessage = $"Invalid target position {targetPosition}";
                return result;
            }

            result.CanExecute = true;
            return result;
        }

        private ObligationDisplacementPlan CreateDisplacementPlan(DeliveryObligation obligation, int currentPosition, int targetPosition)
        {
            ObligationDisplacementPlan plan = new ObligationDisplacementPlan
            {
                ObligationToMove = obligation,
                OriginalPosition = currentPosition,
                TargetPosition = targetPosition
            };

            DeliveryObligation[] queue = GetPlayerQueue();
            int positionsJumped = currentPosition - targetPosition;

            // Each obligation that gets displaced costs tokens equal to the jump distance
            for (int pos = targetPosition; pos < currentPosition; pos++)
            {
                DeliveryObligation displacedObligation = queue[pos - 1];
                if (displacedObligation != null)
                {
                    ObligationDisplacement displacement = new ObligationDisplacement
                    {
                        DisplacedObligation = displacedObligation,
                        OriginalPosition = pos,
                        NewPosition = pos + 1,
                        TokenCost = positionsJumped
                    };

                    plan.Displacements.Add(displacement);
                    plan.TotalTokenCost += displacement.TokenCost;
                }
            }

            return plan;
        }

        private bool ValidateTokenAvailability(ObligationDisplacementPlan plan)
        {
            foreach (ObligationDisplacement displacement in plan.Displacements)
            {
                string npcId = GetNPCIdByName(displacement.DisplacedObligation.SenderName);
                if (string.IsNullOrEmpty(npcId)) continue;

                Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);
                int availableTokens = tokens.GetValueOrDefault(displacement.DisplacedObligation.TokenType, 0);

                if (availableTokens < displacement.TokenCost)
                {
                    return false;
                }
            }
            return true;
        }

        private Dictionary<ConnectionType, int> CalculateRequiredTokens(ObligationDisplacementPlan plan)
        {
            Dictionary<ConnectionType, int> required = new Dictionary<ConnectionType, int>();

            foreach (ObligationDisplacement displacement in plan.Displacements)
            {
                ConnectionType tokenType = displacement.DisplacedObligation.TokenType;
                if (!required.ContainsKey(tokenType))
                {
                    required[tokenType] = 0;
                }
                required[tokenType] += displacement.TokenCost;
            }

            return required;
        }

        private bool BurnTokensForDisplacement(ObligationDisplacement displacement)
        {
            string npcId = GetNPCIdByName(displacement.DisplacedObligation.SenderName);
            if (string.IsNullOrEmpty(npcId)) return false;

            bool success = _tokenManager.SpendTokensWithNPC(
                displacement.DisplacedObligation.TokenType,
                displacement.TokenCost,
                npcId
            );

            if (success)
            {
                _messageSystem.AddSystemMessage(
                    $"💸 Burned {displacement.TokenCost} {displacement.DisplacedObligation.TokenType} tokens with {displacement.DisplacedObligation.SenderName}",
                    SystemMessageTypes.Warning
                );

                // Add burden cards to represent burned relationships
                AddBurdenCardsForBurnedTokens(npcId, displacement.DisplacedObligation.TokenType, displacement.TokenCost);
            }
            else
            {
                _messageSystem.AddSystemMessage(
                    $"❌ Failed to burn tokens with {displacement.DisplacedObligation.SenderName}",
                    SystemMessageTypes.Danger
                );
            }

            return success;
        }

        private void PerformQueueDisplacement(ObligationDisplacementPlan plan)
        {
            DeliveryObligation[] queue = GetPlayerQueue();

            // Remove the obligation from its current position
            queue[plan.OriginalPosition - 1] = null;

            // Shift affected obligations down by one position
            for (int pos = plan.TargetPosition; pos < plan.OriginalPosition; pos++)
            {
                if (queue[pos - 1] != null)
                {
                    DeliveryObligation shiftedObligation = queue[pos - 1];
                    queue[pos] = shiftedObligation;
                    shiftedObligation.QueuePosition = pos + 1;
                }
            }

            // Insert the displaced obligation at the target position
            queue[plan.TargetPosition - 1] = plan.ObligationToMove;
            plan.ObligationToMove.QueuePosition = plan.TargetPosition;
        }

        private AutoDisplacementInfo CalculateAutomaticDisplacementCascade(DeliveryObligation[] queue, int forcedPosition)
        {
            AutoDisplacementInfo info = new AutoDisplacementInfo
            {
                ShouldForceDisplacement = true,
                ForcedPosition = forcedPosition,
                DisplacedObligations = new List<DisplacedObligation>()
            };

            // Starting from the forced position, cascade everything down
            for (int i = forcedPosition - 1; i < queue.Length - 1; i++)
            {
                if (queue[i] != null)
                {
                    info.DisplacedObligations.Add(new DisplacedObligation
                    {
                        Obligation = queue[i],
                        OriginalPosition = i + 1,
                        NewPosition = i + 2,
                        DisplacementAmount = 1
                    });
                }
            }

            // Check if any obligation falls off the end
            if (queue[queue.Length - 1] != null)
            {
                info.DisplacedObligations.Add(new DisplacedObligation
                {
                    Obligation = queue[queue.Length - 1],
                    OriginalPosition = queue.Length,
                    NewPosition = -1, // Falls off the queue
                    DisplacementAmount = 1
                });
            }

            return info;
        }

        private void BurnTokensForAutomaticDisplacement(DeliveryObligation displacedObligation, int positionsDisplaced)
        {
            string npcId = GetNPCIdByName(displacedObligation.SenderName);
            if (string.IsNullOrEmpty(npcId)) return;

            // Calculate token cost based on displacement distance (capped at 3)
            int tokenCost = Math.Min(3, positionsDisplaced);

            // Try to burn tokens with this NPC
            Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npcId);
            int availableTokens = tokens.GetValueOrDefault(displacedObligation.TokenType, 0);

            if (availableTokens >= tokenCost)
            {
                // Burn the full cost
                _tokenManager.RemoveTokensFromNPC(displacedObligation.TokenType, tokenCost, npcId);

                _messageSystem.AddSystemMessage(
                    $"💔 Burned {tokenCost} {displacedObligation.TokenType} token(s) with {displacedObligation.SenderName}",
                    SystemMessageTypes.Danger
                );

                AddBurdenCardsForBurnedTokens(npcId, displacedObligation.TokenType, tokenCost);
            }
            else if (availableTokens > 0)
            {
                // Burn what we can
                _tokenManager.RemoveTokensFromNPC(displacedObligation.TokenType, availableTokens, npcId);

                _messageSystem.AddSystemMessage(
                    $"💔 Burned {availableTokens} {displacedObligation.TokenType} token(s) with {displacedObligation.SenderName} (all they had)",
                    SystemMessageTypes.Danger
                );

                AddBurdenCardsForBurnedTokens(npcId, displacedObligation.TokenType, availableTokens);
            }
            else
            {
                _messageSystem.AddSystemMessage(
                    $"⚠️ No {displacedObligation.TokenType} tokens to burn with {displacedObligation.SenderName}",
                    SystemMessageTypes.Warning
                );
            }
        }

        private void ExecuteAutomaticDisplacementCascade(DeliveryObligation[] queue, int forcedPosition, List<DisplacedObligation> displacedObligations)
        {
            // Clear positions starting from forced position
            for (int i = forcedPosition - 1; i < queue.Length; i++)
            {
                queue[i] = null;
            }

            // Place displaced obligations in their new positions
            foreach (DisplacedObligation displaced in displacedObligations)
            {
                if (displaced.NewPosition > 0 && displaced.NewPosition <= _config.LetterQueue.MaxQueueSize)
                {
                    queue[displaced.NewPosition - 1] = displaced.Obligation;
                    displaced.Obligation.QueuePosition = displaced.NewPosition;

                    _messageSystem.AddSystemMessage(
                        $"  • {displaced.Obligation.SenderName}'s letter pushed from position {displaced.OriginalPosition} → {displaced.NewPosition}",
                        SystemMessageTypes.Warning
                    );
                }
                else
                {
                    // Obligation fell off the queue
                    HandleQueueOverflow(displaced.Obligation);
                }
            }
        }

        private void AddBurdenCardsForBurnedTokens(string npcId, ConnectionType tokenType, int count)
        {
            // Add burden cards to the NPC's deck for each burned token
            // This represents the permanent relationship damage from displacement
            for (int i = 0; i < count; i++)
            {
                AddBurdenCardToNPC(npcId, tokenType);
            }
        }

        private void AddBurdenCardToNPC(string npcId, ConnectionType tokenType)
        {
            // Add a burden card to the NPC's burden deck
            // Burden cards represent damaged relationships and make future interactions harder
            NPC npc = _npcRepository.GetById(npcId);
            if (npc == null) return;

            // Initialize burden deck if needed
            if (npc.BurdenDeck == null)
            {
                npc.InitializeBurdenDeck();
            }

            // Create a burden card for the displacement damage
            ConversationCard burdenCard = new ConversationCard
            {
                Id = $"burden_displacement_{npcId}_{Guid.NewGuid()}",
                CardType = CardType.BurdenGoal,  // Burden cards use BurdenGoal type
                Description = $"The memory of a broken promise lingers",
                TokenType = tokenType,
                Persistence = PersistenceType.Thought,  // Burdens persist
                SuccessType = SuccessEffectType.None,
                FailureType = FailureEffectType.None,
                ExhaustType = ExhaustEffectType.None
            };

            // Add to the NPC's burden deck
            npc.BurdenDeck.AddCard(burdenCard);

            _messageSystem.AddSystemMessage(
                $"⚡ Added burden card to affect future interactions with {GetNPCNameById(npcId)}",
                SystemMessageTypes.Warning
            );
        }

        private void HandleQueueOverflow(DeliveryObligation overflowObligation)
        {
            _messageSystem.AddSystemMessage(
                $"💥 {overflowObligation.SenderName}'s letter FORCED OUT by displacement!",
                SystemMessageTypes.Danger
            );

            // Apply relationship damage for overflow
            string senderId = GetNPCIdByName(overflowObligation.SenderName);
            int tokenPenalty = 2; // Same penalty as expiration

            _tokenManager.RemoveTokensFromNPC(overflowObligation.TokenType, tokenPenalty, senderId);

            _messageSystem.AddSystemMessage(
                $"💔 Lost {tokenPenalty} {overflowObligation.TokenType} tokens with {overflowObligation.SenderName}!",
                SystemMessageTypes.Danger
            );

            // Record in history
            RecordOverflowInHistory(senderId);
        }

        private LetterPositioningReason DeterminePositioningReasonForForced(DeliveryObligation newObligation, string displacementReason)
        {
            if (displacementReason.Contains("failure", StringComparison.OrdinalIgnoreCase))
                return LetterPositioningReason.Obligation;

            if (displacementReason.Contains("pride", StringComparison.OrdinalIgnoreCase))
                return LetterPositioningReason.Obligation;

            if (displacementReason.Contains("disconnected", StringComparison.OrdinalIgnoreCase))
                return LetterPositioningReason.Obligation;

            return LetterPositioningReason.Obligation;
        }

        private void RecordOverflowInHistory(string senderId)
        {
            Player player = _gameWorld.GetPlayer();
            if (!player.NPCLetterHistory.ContainsKey(senderId))
            {
                player.NPCLetterHistory[senderId] = new LetterHistory();
            }
            player.NPCLetterHistory[senderId].RecordExpiry(); // Use existing expiry tracking for overflow
        }

        private DeliveryObligation FindObligationById(string obligationId)
        {
            return GetActiveObligations().FirstOrDefault(o => o.Id == obligationId);
        }

        private DeliveryObligation[] GetActiveObligations()
        {
            Player player = _gameWorld.GetPlayer();
            return player.ObligationQueue.Where(o => o != null).ToArray();
        }

        private DeliveryObligation[] GetPlayerQueue()
        {
            return _gameWorld.GetPlayer().ObligationQueue;
        }

        private int GetQueuePosition(DeliveryObligation obligation)
        {
            if (obligation == null) return -1;

            DeliveryObligation[] queue = GetPlayerQueue();
            for (int i = 0; i < queue.Length; i++)
            {
                if (queue[i]?.Id == obligation.Id)
                {
                    return i + 1; // Return 1-based position
                }
            }

            return -1; // Not found
        }

        private string GetNPCIdByName(string npcName)
        {
            NPC npc = _npcRepository.GetByName(npcName);
            return npc?.ID ?? "";
        }

        private string GetNPCNameById(string npcId)
        {
            NPC npc = _npcRepository.GetById(npcId);
            return npc?.Name ?? "Unknown";
        }
    }

    /// <summary>
    /// Represents a displaced obligation in automatic displacement scenarios
    /// </summary>
    public class DisplacedObligation
    {
        public DeliveryObligation Obligation { get; set; }
        public int OriginalPosition { get; set; }
        public int NewPosition { get; set; }
        public int DisplacementAmount { get; set; }
    }

    /// <summary>
    /// Information about automatic displacement scenarios
    /// </summary>
    public class AutoDisplacementInfo
    {
        public bool ShouldForceDisplacement { get; set; }
        public int ForcedPosition { get; set; }
        public List<DisplacedObligation> DisplacedObligations { get; set; } = new List<DisplacedObligation>();
        public string DisplacementReason { get; set; } = "";
        public DisplacementTrigger Trigger { get; set; } = DisplacementTrigger.None;
    }
}