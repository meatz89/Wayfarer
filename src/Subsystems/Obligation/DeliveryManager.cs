using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Subsystems.ObligationSubsystem;

namespace Wayfarer.Subsystems.ObligationSubsystem
{
    /// <summary>
    /// Manages all letter delivery operations including validation, execution, and tracking.
    /// Handles location validation, NPC focus checks, and delivery outcomes.
    /// </summary>
    public class DeliveryManager
    {
        private readonly GameWorld _gameWorld;
        private readonly NPCRepository _npcRepository;
        private readonly MessageSystem _messageSystem;
        private readonly TokenMechanicsManager _tokenManager;
        private readonly GameConfiguration _config;

        public DeliveryManager(
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
        /// Attempt to deliver a letter from position 1 of the queue.
        /// Validates location, NPC focus, and executes delivery with token rewards.
        /// </summary>
        public DeliveryResult DeliverFromPosition1()
        {
            DeliveryResult result = new DeliveryResult();

            DeliveryObligation letter = GetLetterAt(1);
            if (letter == null)
            {
                result.ErrorMessage = "No letter at position 1";
                return result;
            }

            // Validate delivery is possible
            DeliveryResult validation = ValidateDelivery(letter);
            if (!validation.Success)
            {
                result.ErrorMessage = validation.ErrorMessage;
                return result;
            }

            // Execute the delivery
            result = ExecuteDelivery(letter);
            if (result.Success)
            {
                // Remove from queue and shift remaining letters up
                RemoveFromQueueAndShift(1);

                // Record successful delivery
                RecordLetterDelivery(letter);

                _messageSystem.AddSystemMessage(
                    $"✅ Letter delivered to {letter.RecipientName}!",
                    SystemMessageTypes.Success
                );
            }

            return result;
        }

        /// <summary>
        /// Deliver a specific obligation by ID from any position in the queue.
        /// </summary>
        public DeliveryResult DeliverObligation(string obligationId)
        {
            DeliveryResult result = new DeliveryResult();

            DeliveryObligation obligation = FindObligationById(obligationId);
            if (obligation == null)
            {
                result.ErrorMessage = "Obligation not found in queue";
                return result;
            }

            int position = GetQueuePosition(obligation);
            if (position <= 0)
            {
                result.ErrorMessage = "Obligation not in queue";
                return result;
            }

            // Can only deliver from position 1
            if (position != 1)
            {
                result.ErrorMessage = "Can only deliver from position 1";
                return result;
            }

            return DeliverFromPosition1();
        }

        /// <summary>
        /// Check if the player can deliver the letter at position 1.
        /// Validates location and NPC focus requirements.
        /// </summary>
        public bool CanDeliverFromPosition1()
        {
            DeliveryObligation letter = GetLetterAt(1);
            if (letter == null) return false;

            return ValidatePlayerLocation(letter).Success;
        }

        /// <summary>
        /// Get detailed information about why a delivery cannot be completed.
        /// </summary>
        public DeliveryResult GetDeliveryValidation(int position)
        {
            DeliveryResult result = new DeliveryResult();

            DeliveryObligation letter = GetLetterAt(position);
            if (letter == null)
            {
                result.ErrorMessage = $"No letter at position {position}";
                return result;
            }

            return ValidateDelivery(letter);
        }

        /// <summary>
        /// Add a physical letter to the player's satchel when an obligation is created.
        /// </summary>
        public void AddPhysicalLetter(DeliveryObligation obligation)
        {
            Letter physicalLetter = new Letter
            {
                Id = obligation.Id,
                SenderName = obligation.SenderName,
                RecipientName = obligation.RecipientName,
                Size = CalculateLetterSize(obligation),
                PhysicalProperties = DeterminePhysicalProperties(obligation),
                SpecialType = LetterSpecialType.None
            };

            _gameWorld.GetPlayer().CarriedLetters.Add(physicalLetter);

            _messageSystem.AddSystemMessage(
                $"📨 Physical letter from {obligation.SenderName} added to satchel",
                SystemMessageTypes.Info
            );
        }

        /// <summary>
        /// Remove a physical letter from the satchel when delivered.
        /// </summary>
        public void RemovePhysicalLetter(string letterId)
        {
            Player player = _gameWorld.GetPlayer();
            Letter? physicalLetter = player.CarriedLetters.FirstOrDefault(l => l.Id == letterId);

            if (physicalLetter != null)
            {
                player.CarriedLetters.Remove(physicalLetter);

                _messageSystem.AddSystemMessage(
                    $"📮 Physical letter to {physicalLetter.RecipientName} removed from satchel",
                    SystemMessageTypes.Info
                );
            }
        }

        /// <summary>
        /// Calculate total size of all physical letters in the player's satchel.
        /// </summary>
        public int GetTotalSatchelSize()
        {
            Player player = _gameWorld.GetPlayer();
            return player.CarriedLetters.Sum(letter => letter.Size);
        }

        /// <summary>
        /// Get all physical letters currently carried by the player.
        /// </summary>
        public List<Letter> GetCarriedLetters()
        {
            return new List<Letter>(_gameWorld.GetPlayer().CarriedLetters);
        }

        /// <summary>
        /// Record a successful letter delivery for statistics and NPC relationships.
        /// </summary>
        public void RecordLetterDelivery(DeliveryObligation letter)
        {
            Player player = _gameWorld.GetPlayer();
            string senderId = GetNPCIdByName(letter.SenderName);

            if (!string.IsNullOrEmpty(senderId))
            {
                // Track delivery history
                if (!player.NPCLetterHistory.ContainsKey(senderId))
                {
                    player.NPCLetterHistory[senderId] = new LetterHistory();
                }
                player.NPCLetterHistory[senderId].RecordDelivery();

                // Award reputation bonus for on-time delivery
                if (letter.DeadlineInMinutes > 0)
                {
                    _messageSystem.AddSystemMessage(
                        $"⭐ On-time delivery bonus with {letter.SenderName}!",
                        SystemMessageTypes.Success
                    );
                }
            }
        }

        /// <summary>
        /// Record a letter being skipped for statistics tracking.
        /// </summary>
        public void RecordLetterSkip(DeliveryObligation letter)
        {
            Player player = _gameWorld.GetPlayer();
            string senderId = GetNPCIdByName(letter.SenderName);

            if (!string.IsNullOrEmpty(senderId))
            {
                if (!player.NPCLetterHistory.ContainsKey(senderId))
                {
                    player.NPCLetterHistory[senderId] = new LetterHistory();
                }
                player.NPCLetterHistory[senderId].RecordSkip();
            }
        }

        /// <summary>
        /// Get letters that are expiring within the specified threshold.
        /// </summary>
        public DeliveryObligation[] GetExpiringLetters(int daysThreshold)
        {
            int minutesThreshold = daysThreshold * 24 * 60;

            return GetActiveObligations()
                .Where(o => o.DeadlineInMinutes <= minutesThreshold && o.DeadlineInMinutes > 0)
                .OrderBy(o => o.DeadlineInMinutes)
                .ToArray();
        }

        /// <summary>
        /// Attempt to skip delivery from a specific position for a cost.
        /// </summary>
        public QueueManipulationResult TrySkipDelivery(int position)
        {
            QueueManipulationResult result = new QueueManipulationResult
            {
                OperationType = "Skip Delivery",
                Position = position
            };

            DeliveryObligation letter = GetLetterAt(position);
            if (letter == null)
            {
                result.ErrorMessage = "No letter at specified position";
                return result;
            }

            // Only position 1 can be skipped
            if (position != 1)
            {
                result.ErrorMessage = "Can only skip delivery from position 1";
                return result;
            }

            // Calculate skip cost (typically requires tokens)
            Dictionary<ConnectionType, int> skipCost = CalculateSkipCost(letter);
            if (!CanAffordSkipCost(skipCost))
            {
                result.ErrorMessage = "Cannot afford skip cost";
                result.TokensCost = skipCost;
                return result;
            }

            // Execute skip
            PaySkipCost(skipCost);
            RecordLetterSkip(letter);
            RemoveFromQueueAndShift(position);

            result.Success = true;
            result.AffectedObligation = letter;
            result.TokensCost = skipCost;

            _messageSystem.AddSystemMessage(
                $"⏭️ Skipped delivery to {letter.RecipientName}",
                SystemMessageTypes.Warning
            );

            return result;
        }

        /// <summary>
        /// Check if the player is currently at the recipient's location for delivery.
        /// </summary>
        public bool IsPlayerAtRecipientLocation(DeliveryObligation letter)
        {
            if (letter == null) return false;

            Player player = _gameWorld.GetPlayer();
            if (player.CurrentLocationSpot == null) return false;

            // Get the recipient NPC
            NPC recipient = _npcRepository.GetById(letter.RecipientId);
            if (recipient == null)
            {
                // Fallback: try to find by name
                recipient = _npcRepository.GetByName(letter.RecipientName);
                if (recipient == null) return false;
            }

            // Check if NPC is at player's current location
            TimeBlocks currentTime = GetCurrentTimeBlock();
            List<NPC> npcsAtCurrentSpot = _npcRepository.GetNPCsForLocationSpotAndTime(
                player.CurrentLocationSpot.SpotID,
                currentTime);

            return npcsAtCurrentSpot.Any(npc => npc.ID == recipient.ID);
        }

        // Private helper methods

        private DeliveryObligation GetLetterAt(int position)
        {
            if (position < 1 || position > _config.LetterQueue.MaxQueueSize)
                return null;

            return _gameWorld.GetPlayer().ObligationQueue[position - 1];
        }

        private DeliveryObligation[] GetActiveObligations()
        {
            Player player = _gameWorld.GetPlayer();
            return player.ObligationQueue
                .Where(o => o != null)
                .ToArray();
        }

        private DeliveryObligation FindObligationById(string obligationId)
        {
            return GetActiveObligations()
                .FirstOrDefault(o => o.Id == obligationId);
        }

        private int GetQueuePosition(DeliveryObligation obligation)
        {
            if (obligation == null) return -1;

            DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;
            for (int i = 0; i < queue.Length; i++)
            {
                if (queue[i]?.Id == obligation.Id)
                {
                    return i + 1; // Return 1-based position
                }
            }

            return -1; // Not found
        }

        private DeliveryResult ValidateDelivery(DeliveryObligation letter)
        {
            DeliveryResult result = new DeliveryResult();

            // Check if letter has expired
            if (letter.IsExpired)
            {
                result.ErrorMessage = "Letter has expired and cannot be delivered";
                return result;
            }

            // Validate player location
            DeliveryResult locationValidation = ValidatePlayerLocation(letter);
            if (!locationValidation.Success)
            {
                result.ErrorMessage = locationValidation.ErrorMessage;
                return result;
            }

            result.Success = true;
            return result;
        }

        private DeliveryResult ValidatePlayerLocation(DeliveryObligation letter)
        {
            DeliveryResult result = new DeliveryResult();

            if (!IsPlayerAtRecipientLocation(letter))
            {
                NPC recipient = _npcRepository.GetById(letter.RecipientId) ??
                               _npcRepository.GetByName(letter.RecipientName);

                string recipientName = recipient?.Name ?? letter.RecipientName;
                result.ErrorMessage = $"You must be at {recipientName}'s location to deliver";
                return result;
            }

            result.Success = true;
            return result;
        }

        private DeliveryResult ExecuteDelivery(DeliveryObligation letter)
        {
            DeliveryResult result = new DeliveryResult
            {
                Success = true,
                DeliveredObligation = letter
            };

            // Grant tokens for successful delivery
            if (letter.TokenType != ConnectionType.None)
            {
                int tokensGranted = 1; // Standard delivery reward
                _tokenManager.AddTokensToNPC(letter.TokenType, tokensGranted, letter.RecipientId);

                result.TokensGranted = tokensGranted;
                result.TokenType = letter.TokenType;
                result.RecipientId = letter.RecipientId;

                _messageSystem.AddSystemMessage(
                    $"🎖️ Gained {tokensGranted} {letter.TokenType} token with {letter.RecipientName}",
                    SystemMessageTypes.Success
                );
            }

            // Remove physical letter from satchel
            RemovePhysicalLetter(letter.Id);

            return result;
        }

        private void RemoveFromQueueAndShift(int position)
        {
            DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

            // Clear the position
            queue[position - 1] = null;

            // Shift all letters below up by one position
            for (int i = position - 1; i < _config.LetterQueue.MaxQueueSize - 1; i++)
            {
                if (queue[i + 1] != null)
                {
                    queue[i] = queue[i + 1];
                    queue[i].QueuePosition = i + 1; // Update position tracking
                    queue[i + 1] = null; // Clear old position
                }
            }
        }

        private int CalculateLetterSize(DeliveryObligation obligation)
        {
            // Standard letters are size 1, could be modified based on letter properties
            int baseSize = 1;

            // Future: could factor in tier, special properties, etc.
            if (obligation.Tier == TierLevel.T3)
                baseSize += 1;

            return baseSize;
        }

        private LetterPhysicalProperties DeterminePhysicalProperties(DeliveryObligation obligation)
        {
            LetterPhysicalProperties properties = LetterPhysicalProperties.None;

            // Determine properties based on obligation characteristics
            if (obligation.Stakes == StakeType.WEALTH)
                properties |= LetterPhysicalProperties.Valuable;

            if (obligation.DeadlineInMinutes < 180) // Less than 3 hours
                properties |= LetterPhysicalProperties.Perishable;

            if (obligation.Tier == TierLevel.T3)
                properties |= LetterPhysicalProperties.Fragile;

            return properties;
        }

        private Dictionary<ConnectionType, int> CalculateSkipCost(DeliveryObligation letter)
        {
            // Skip cost is typically 1 token of the same type as the letter
            Dictionary<ConnectionType, int> cost = new Dictionary<ConnectionType, int>();

            if (letter.TokenType != ConnectionType.None)
            {
                cost[letter.TokenType] = 1;
            }

            return cost;
        }

        private bool CanAffordSkipCost(Dictionary<ConnectionType, int> cost)
        {
            foreach (KeyValuePair<ConnectionType, int> tokenCost in cost)
            {
                // Check if player has enough tokens with any NPC
                // This would need access to token balances across all NPCs
                // For now, simplified logic
                if (tokenCost.Value > 0)
                {
                    // Player needs at least the required tokens
                    return true; // Simplified for POC
                }
            }
            return true;
        }

        private void PaySkipCost(Dictionary<ConnectionType, int> cost)
        {
            foreach (KeyValuePair<ConnectionType, int> tokenCost in cost)
            {
                // Deduct tokens from player's reserves
                // Implementation would depend on how player tokens are managed
                _messageSystem.AddSystemMessage(
                    $"💰 Paid {tokenCost.Value} {tokenCost.Key} tokens to skip delivery",
                    SystemMessageTypes.Warning
                );
            }
        }

        private string GetNPCIdByName(string npcName)
        {
            NPC npc = _npcRepository.GetByName(npcName);
            return npc?.ID ?? "";
        }

        private TimeBlocks GetCurrentTimeBlock()
        {
            // This would typically come from TimeManager
            // Simplified for now
            return TimeBlocks.Morning;
        }
    }
}