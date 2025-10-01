using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Subsystems.ObligationSubsystem;

namespace Wayfarer.Subsystems.ObligationSubsystem
{
    /// <summary>
    /// Manages queue position operations including adding, moving, removing, and swapping obligations.
    /// Handles leverage-based positioning, queue compression, and position validation.
    /// </summary>
    public class QueueManipulator
    {
        private readonly GameWorld _gameWorld;
        private readonly NPCRepository _npcRepository;
        private readonly MessageSystem _messageSystem;
        private readonly TokenMechanicsManager _tokenManager;
        private readonly StandingObligationManager _obligationManager;
        private readonly GameConfiguration _config;
        private readonly TimeManager _timeManager;

        public QueueManipulator(
            GameWorld gameWorld,
            NPCRepository npcRepository,
            MessageSystem messageSystem,
            TokenMechanicsManager tokenManager,
            StandingObligationManager obligationManager,
            GameConfiguration config,
            TimeManager timeManager)
        {
            _gameWorld = gameWorld;
            _npcRepository = npcRepository;
            _messageSystem = messageSystem;
            _tokenManager = tokenManager;
            _obligationManager = obligationManager;
            _config = config;
            _timeManager = timeManager;
        }

        /// <summary>
        /// Add obligation to the first available slot in the queue.
        /// Queue fills from position 1 forward.
        /// </summary>
        public ObligationAddResult AddObligation(DeliveryObligation obligation)
        {
            ObligationAddResult result = new ObligationAddResult();

            if (obligation == null)
            {
                result.ErrorMessage = "Obligation cannot be null";
                return result;
            }

            DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

            // Find the first empty slot, filling from position 1
            for (int i = 0; i < _config.LetterQueue.MaxQueueSize; i++)
            {
                if (queue[i] == null)
                {
                    queue[i] = obligation;
                    obligation.QueuePosition = i + 1;

                    result.Success = true;
                    result.Position = i + 1;
                    result.AddedObligation = obligation;

                    _messageSystem.AddSystemMessage(
                        $"📨 New obligation from {obligation.SenderName} enters queue at position {i + 1}",
                        SystemMessageTypes.Success
                    );

                    return result;
                }
            }

            result.ErrorMessage = "Queue is full";
            return result;
        }

        /// <summary>
        /// Add obligation to a specific position in the queue.
        /// </summary>
        public ObligationAddResult AddObligationToPosition(DeliveryObligation obligation, int position)
        {
            ObligationAddResult result = new ObligationAddResult();

            if (obligation == null || position < 1 || position > _config.LetterQueue.MaxQueueSize)
            {
                result.ErrorMessage = "Invalid obligation or position";
                return result;
            }

            DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

            if (queue[position - 1] != null)
            {
                result.ErrorMessage = "Position is already occupied";
                return result;
            }

            queue[position - 1] = obligation;
            obligation.QueuePosition = position;

            result.Success = true;
            result.Position = position;
            result.AddedObligation = obligation;

            _messageSystem.AddSystemMessage(
                $"📨 Obligation from {obligation.SenderName} added to position {position}",
                SystemMessageTypes.Success
            );

            return result;
        }

        /// <summary>
        /// Add obligation with leverage-based positioning and automatic displacement.
        /// </summary>
        public ObligationAddResult AddObligationWithLeverage(DeliveryObligation obligation)
        {
            ObligationAddResult result = new ObligationAddResult();

            if (obligation == null)
            {
                result.ErrorMessage = "Obligation cannot be null";
                return result;
            }

            if (!ValidateObligationCanBeAdded(obligation))
            {
                result.ErrorMessage = "Queue is full";
                return result;
            }

            // Calculate leverage-based position
            LeverageCalculation leverageCalc = CalculateLeveragePosition(obligation);

            DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

            // If target position is empty, simple insertion
            if (queue[leverageCalc.FinalPosition - 1] == null)
            {
                return InsertObligationAtPosition(obligation, leverageCalc.FinalPosition, leverageCalc);
            }

            // Target occupied - need displacement
            return DisplaceAndInsertObligation(obligation, leverageCalc);
        }

        /// <summary>
        /// Remove obligation from a specific queue position.
        /// </summary>
        public QueueManipulationResult RemoveObligationFromQueue(int position)
        {
            QueueManipulationResult result = new QueueManipulationResult
            {
                OperationType = "Remove",
                Position = position
            };

            if (position < 1 || position > _config.LetterQueue.MaxQueueSize)
            {
                result.ErrorMessage = "Invalid position";
                return result;
            }

            DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;
            DeliveryObligation obligation = queue[position - 1];

            if (obligation == null)
            {
                result.ErrorMessage = "No obligation at specified position";
                return result;
            }

            // Remove the obligation
            obligation.QueuePosition = 0;
            queue[position - 1] = null;

            // Shift remaining letters up
            ShiftQueueUp(position);

            result.Success = true;
            result.AffectedObligation = obligation;

            return result;
        }

        /// <summary>
        /// Move an obligation to a specific target position.
        /// </summary>
        public QueueManipulationResult MoveObligationToPosition(DeliveryObligation obligation, int targetPosition)
        {
            QueueManipulationResult result = new QueueManipulationResult
            {
                OperationType = "Move",
                Position = targetPosition
            };

            if (obligation == null || targetPosition < 1 || targetPosition > _config.LetterQueue.MaxQueueSize)
            {
                result.ErrorMessage = "Invalid obligation or target position";
                return result;
            }

            DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

            // Find current position
            int currentPosition = GetQueuePosition(obligation);
            if (currentPosition <= 0)
            {
                result.ErrorMessage = "Obligation not found in queue";
                return result;
            }

            // Clear current position
            queue[currentPosition - 1] = null;

            // Set new position
            queue[targetPosition - 1] = obligation;
            obligation.QueuePosition = targetPosition;

            result.Success = true;
            result.AffectedObligation = obligation;

            _messageSystem.AddSystemMessage(
                $"📬 {obligation.SenderName}'s letter moved to position {targetPosition}",
                SystemMessageTypes.Info
            );

            return result;
        }

        /// <summary>
        /// Attempt a morning swap of two letters (free once per day).
        /// </summary>
        public QueueManipulationResult TryMorningSwap(int position1, int position2)
        {
            QueueManipulationResult result = new QueueManipulationResult
            {
                OperationType = "Morning Swap"
            };

            // Validate it's morning (dawn time block)
            if (_timeManager.GetCurrentTimeBlock() != TimeBlocks.Dawn)
            {
                result.ErrorMessage = "Morning swap is only available during dawn";
                return result;
            }

            // Validate positions are adjacent
            if (Math.Abs(position1 - position2) != 1)
            {
                result.ErrorMessage = "Can only swap adjacent letters";
                return result;
            }

            // Check daily limit (would need tracking in player state)
            if (HasUsedMorningSwapToday())
            {
                result.ErrorMessage = "Already used morning swap today";
                return result;
            }

            // Execute the swap
            QueueManipulationResult swapResult = SwapObligations(position1, position2);
            if (swapResult.Success)
            {
                MarkMorningSwapUsed();

                result.Success = true;
                result.AffectedObligation = swapResult.AffectedObligation;

                _messageSystem.AddSystemMessage(
                    "🌅 Morning swap completed - letters exchanged positions",
                    SystemMessageTypes.Success
                );
            }
            else
            {
                result.ErrorMessage = swapResult.ErrorMessage;
            }

            return result;
        }

        /// <summary>
        /// Swap two obligations at specified positions.
        /// </summary>
        public QueueManipulationResult SwapObligations(int position1, int position2)
        {
            QueueManipulationResult result = new QueueManipulationResult
            {
                OperationType = "Swap"
            };

            if (position1 < 1 || position1 > _config.LetterQueue.MaxQueueSize ||
                position2 < 1 || position2 > _config.LetterQueue.MaxQueueSize)
            {
                result.ErrorMessage = "Invalid positions for swap";
                return result;
            }

            DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;
            DeliveryObligation obligation1 = queue[position1 - 1];
            DeliveryObligation obligation2 = queue[position2 - 1];

            if (obligation1 == null || obligation2 == null)
            {
                result.ErrorMessage = "Both positions must contain obligations";
                return result;
            }

            // Perform the swap
            queue[position1 - 1] = obligation2;
            queue[position2 - 1] = obligation1;

            obligation1.QueuePosition = position2;
            obligation2.QueuePosition = position1;

            result.Success = true;
            result.AffectedObligation = obligation1; // Primary affected obligation

            _messageSystem.AddSystemMessage(
                $"🔄 Swapped {obligation1.SenderName}'s letter with {obligation2.SenderName}'s letter",
                SystemMessageTypes.Info
            );

            return result;
        }

        /// <summary>
        /// Skip delivery by spending tokens to move a letter to position 1.
        /// </summary>
        public QueueManipulationResult TrySkipToPosition1(int fromPosition)
        {
            QueueManipulationResult result = new QueueManipulationResult
            {
                OperationType = "Skip to Position 1",
                Position = fromPosition
            };

            if (!ValidateSkipDeliveryPosition(fromPosition))
            {
                result.ErrorMessage = "Invalid position for skip";
                return result;
            }

            DeliveryObligation letter = GetLetterAt(fromPosition);
            if (letter == null)
            {
                result.ErrorMessage = "No letter at specified position";
                return result;
            }

            if (!ValidatePosition1Available())
            {
                result.ErrorMessage = "Position 1 is already occupied";
                return result;
            }

            // Calculate token cost
            int tokenCost = CalculateSkipCost(fromPosition, letter);
            string senderId = GetNPCIdByName(letter.SenderName);

            if (!ValidateTokenAvailability(letter, tokenCost))
            {
                result.ErrorMessage = $"Insufficient {letter.TokenType} tokens";
                result.TokensCost = new Dictionary<ConnectionType, int> { { letter.TokenType, tokenCost } };
                return result;
            }

            // Process payment and move letter
            if (SpendTokensForSkip(letter, tokenCost, senderId))
            {
                MoveLetterToPosition1(letter, fromPosition);
                ShiftQueueUp(fromPosition);

                result.Success = true;
                result.AffectedObligation = letter;
                result.TokensCost = new Dictionary<ConnectionType, int> { { letter.TokenType, tokenCost } };

                _messageSystem.AddSystemMessage(
                    $"✅ {letter.SenderName}'s letter jumps to position 1!",
                    SystemMessageTypes.Success
                );
            }
            else
            {
                result.ErrorMessage = "Failed to process token payment";
            }

            return result;
        }

        /// <summary>
        /// Compress the queue by removing gaps and shifting letters forward.
        /// </summary>
        public void CompressQueue()
        {
            Player player = _gameWorld.GetPlayer();
            DeliveryObligation[] queue = player.ObligationQueue;
            int writeIndex = 0;

            for (int i = 0; i < _config.LetterQueue.MaxQueueSize; i++)
            {
                if (queue[i] != null)
                {
                    if (i != writeIndex)
                    {
                        queue[writeIndex] = queue[i];
                        queue[writeIndex].QueuePosition = writeIndex + 1;
                        queue[i] = null;
                    }
                    writeIndex++;
                }
            }
        }

        /// <summary>
        /// Get the current position of an obligation in the queue.
        /// </summary>
        public int GetQueuePosition(DeliveryObligation obligation)
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

        /// <summary>
        /// Get obligation at a specific queue position.
        /// </summary>
        public DeliveryObligation GetLetterAt(int position)
        {
            if (position < 1 || position > _config.LetterQueue.MaxQueueSize)
                return null;

            return _gameWorld.GetPlayer().ObligationQueue[position - 1];
        }

        /// <summary>
        /// Check if the queue is completely full.
        /// </summary>
        public bool IsQueueFull()
        {
            return _gameWorld.GetPlayer().ObligationQueue.All(slot => slot != null);
        }

        /// <summary>
        /// Get the total number of obligations currently in the queue.
        /// </summary>
        public int GetLetterCount()
        {
            return _gameWorld.GetPlayer().ObligationQueue.Count(slot => slot != null);
        }

        /// <summary>
        /// Get detailed information about each queue position.
        /// </summary>
        public List<QueuePositionInfo> GetQueuePositionInfo()
        {
            List<QueuePositionInfo> positions = new List<QueuePositionInfo>();
            DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

            for (int i = 0; i < _config.LetterQueue.MaxQueueSize; i++)
            {
                int position = i + 1;
                DeliveryObligation? obligation = queue[i];

                positions.Add(new QueuePositionInfo
                {
                    Position = position,
                    IsOccupied = obligation != null,
                    CurrentObligation = obligation,
                    CanInsertHere = obligation == null,
                    BlockingReason = obligation != null ? "Position occupied" : "",
                    RequiredTokensToDisplace = obligation != null ?
                        CalculateDisplacementCost(obligation) : new Dictionary<ConnectionType, int>()
                });
            }

            return positions;
        }

        // Private helper methods

        private LeverageCalculation CalculateLeveragePosition(DeliveryObligation obligation)
        {
            LeverageCalculation calc = new LeverageCalculation
            {
                NPCName = obligation.SenderName,
                BasePosition = _config.LetterQueue.MaxQueueSize
            };

            string senderId = GetNPCIdByName(obligation.SenderName);
            calc.NPCId = senderId;

            if (string.IsNullOrEmpty(senderId))
            {
                calc.FinalPosition = calc.BasePosition;
                return calc;
            }

            // Check for active obligations first - highest priority
            calc.HasActiveObligation = HasActiveObligationWithNPC(senderId);
            if (calc.HasActiveObligation)
            {
                calc.FinalPosition = 1;
                calc.PositioningReason = LetterPositioningReason.Obligation;
                return calc;
            }

            // Get all token balances with this NPC
            calc.AllTokens = _tokenManager.GetTokensWithNPC(senderId);

            // Calculate position using algorithm
            calc.HighestPositiveToken = GetHighestPositiveToken(calc.AllTokens);
            calc.WorstNegativeTokenPenalty = GetWorstNegativeTokenPenalty(calc.AllTokens);

            // Base algorithm: Position = MaxSize - (highest positive token) + (worst negative token penalty)
            calc.CalculatedPosition = calc.BasePosition - calc.HighestPositiveToken + calc.WorstNegativeTokenPenalty;

            // Apply Diplomacy debt leverage override
            calc.HasDiplomacyDebtOverride = calc.AllTokens.Any(kvp => kvp.Key == ConnectionType.Diplomacy) &&
                                          calc.AllTokens[ConnectionType.Diplomacy] <= -3;

            if (calc.HasDiplomacyDebtOverride)
            {
                calc.FinalPosition = 2; // Diplomacy debt >= 3 forces position 2
            }
            else
            {
                // Clamp to valid queue range
                calc.FinalPosition = Math.Max(1, Math.Min(calc.BasePosition, calc.CalculatedPosition));
            }

            // Determine positioning reason
            calc.PositioningReason = DeterminePositioningReason(senderId, calc.AllTokens, calc.WorstNegativeTokenPenalty);

            // Calculate leverage boost
            if (calc.FinalPosition < calc.BasePosition)
            {
                calc.LeverageBoost = calc.BasePosition - calc.FinalPosition;
            }

            return calc;
        }

        private ObligationAddResult InsertObligationAtPosition(DeliveryObligation obligation, int position, LeverageCalculation leverageCalc)
        {
            ObligationAddResult result = new ObligationAddResult();
            DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

            queue[position - 1] = obligation;
            obligation.QueuePosition = position;

            // Track leverage effects
            if (leverageCalc.LeverageBoost > 0)
            {
                obligation.OriginalQueuePosition = leverageCalc.BasePosition;
                obligation.LeverageBoost = leverageCalc.LeverageBoost;
                result.UsedLeverage = true;
            }

            // Record positioning data
            RecordObligationPositioning(obligation, leverageCalc);

            result.Success = true;
            result.Position = position;
            result.AddedObligation = obligation;

            ShowLeverageNarrative(obligation, position, leverageCalc);

            return result;
        }

        private ObligationAddResult DisplaceAndInsertObligation(DeliveryObligation obligation, LeverageCalculation leverageCalc)
        {
            ObligationAddResult result = new ObligationAddResult();
            DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;
            int targetPosition = leverageCalc.FinalPosition;

            // Announce displacement
            ShowLeverageDisplacement(obligation, targetPosition, leverageCalc);

            // Collect all letters from target position downward
            List<DeliveryObligation> lettersToDisplace = new List<DeliveryObligation>();
            for (int i = targetPosition - 1; i < _config.LetterQueue.MaxQueueSize; i++)
            {
                if (queue[i] != null)
                {
                    lettersToDisplace.Add(queue[i]);
                    queue[i] = null; // Clear old position
                }
            }

            // Insert new letter at target position
            queue[targetPosition - 1] = obligation;
            obligation.QueuePosition = targetPosition;

            // Track leverage effect
            if (targetPosition < leverageCalc.BasePosition)
            {
                obligation.OriginalQueuePosition = leverageCalc.BasePosition;
                obligation.LeverageBoost = leverageCalc.BasePosition - targetPosition;
            }

            // Reinsert displaced letters
            List<string> displacedIds = new List<string>();
            int nextAvailable = targetPosition;

            foreach (DeliveryObligation displaced in lettersToDisplace)
            {
                nextAvailable++;
                if (nextAvailable <= _config.LetterQueue.MaxQueueSize)
                {
                    queue[nextAvailable - 1] = displaced;
                    displaced.QueuePosition = nextAvailable;
                    NotifyLetterShifted(displaced, nextAvailable);
                    displacedIds.Add(displaced.Id);
                }
                else
                {
                    HandleQueueOverflow(displaced);
                    displacedIds.Add(displaced.Id);
                }
            }

            result.Success = true;
            result.Position = targetPosition;
            result.AddedObligation = obligation;
            result.UsedLeverage = true;
            result.CausedDisplacement = true;
            result.DisplacedObligationIds = displacedIds;

            return result;
        }

        private void ShiftQueueUp(int removedPosition)
        {
            DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;

            // Collect remaining letters after removed position
            List<DeliveryObligation> remainingLetters = new List<DeliveryObligation>();
            for (int i = removedPosition; i < _config.LetterQueue.MaxQueueSize; i++)
            {
                if (queue[i] != null)
                {
                    remainingLetters.Add(queue[i]);
                    queue[i] = null;
                }
            }

            if (!remainingLetters.Any()) return;

            _messageSystem.AddSystemMessage(
                "📬 Your remaining obligations shift forward:",
                SystemMessageTypes.Info
            );

            // Place remaining letters starting from removed position
            int writePosition = removedPosition - 1; // Convert to 0-based
            foreach (DeliveryObligation letter in remainingLetters)
            {
                while (writePosition < _config.LetterQueue.MaxQueueSize && queue[writePosition] != null)
                {
                    writePosition++;
                }

                if (writePosition < _config.LetterQueue.MaxQueueSize)
                {
                    int oldPosition = letter.QueuePosition;
                    queue[writePosition] = letter;
                    letter.QueuePosition = writePosition + 1;

                    string urgencyText = letter.DeadlineInSegments <= 48 ? " ⚠️ URGENT!" : "";
                    string deadlineText = letter.DeadlineInSegments <= 1440 ? "expires today!" : $"{letter.DeadlineInSegments / 1440} days left";

                    _messageSystem.AddSystemMessage(
                        $"  • {letter.SenderName}'s letter moves from slot {oldPosition} → {letter.QueuePosition} ({deadlineText}){urgencyText}",
                        letter.DeadlineInSegments <= 48 ? SystemMessageTypes.Warning : SystemMessageTypes.Info
                    );
                }
            }
        }

        private bool ValidateObligationCanBeAdded(DeliveryObligation obligation)
        {
            if (IsQueueFull())
            {
                _messageSystem.AddSystemMessage(
                    $"Cannot accept obligation from {obligation.SenderName} - your queue is completely full!",
                    SystemMessageTypes.Danger
                );
                return false;
            }
            return true;
        }

        private bool ValidateSkipDeliveryPosition(int position)
        {
            return position > 1 && position <= _config.LetterQueue.MaxQueueSize;
        }

        private bool ValidatePosition1Available()
        {
            if (GetLetterAt(1) != null)
            {
                _messageSystem.AddSystemMessage(
                    "Cannot skip - position 1 is already occupied!",
                    SystemMessageTypes.Danger
                );
                return false;
            }
            return true;
        }

        private int CalculateSkipCost(int position, DeliveryObligation letter)
        {
            int baseCost = position - 1;
            int multiplier = _obligationManager.CalculateSkipCostMultiplier(letter);
            return baseCost * multiplier;
        }

        private bool ValidateTokenAvailability(DeliveryObligation letter, int tokenCost)
        {
            if (!_tokenManager.HasTokens(letter.TokenType, tokenCost))
            {
                _messageSystem.AddSystemMessage(
                    $"Insufficient {letter.TokenType} tokens! Need {tokenCost}, have {_tokenManager.GetTokenCount(letter.TokenType)}",
                    SystemMessageTypes.Danger
                );
                return false;
            }
            return true;
        }

        private bool SpendTokensForSkip(DeliveryObligation letter, int tokenCost, string senderId)
        {
            _messageSystem.AddSystemMessage(
                $"💸 Spending {tokenCost} {letter.TokenType} tokens with {letter.SenderName}...",
                SystemMessageTypes.Warning
            );

            return _tokenManager.SpendTokensWithNPC(letter.TokenType, tokenCost, senderId);
        }

        private void MoveLetterToPosition1(DeliveryObligation letter, int fromPosition)
        {
            DeliveryObligation[] queue = _gameWorld.GetPlayer().ObligationQueue;
            queue[0] = letter;
            queue[fromPosition - 1] = null;
            letter.QueuePosition = 1;
        }

        private bool HasActiveObligationWithNPC(string npcId)
        {
            List<StandingObligation> activeObligations = _obligationManager.GetActiveObligations();
            return activeObligations.Any(obligation => obligation.RelatedNPCId == npcId);
        }

        private int GetHighestPositiveToken(Dictionary<ConnectionType, int> allTokens)
        {
            return allTokens.Values.Where(v => v > 0).DefaultIfEmpty(0).Max();
        }

        private int GetWorstNegativeTokenPenalty(Dictionary<ConnectionType, int> allTokens)
        {
            return allTokens.Values.Where(v => v < 0).Select(Math.Abs).DefaultIfEmpty(0).Max();
        }

        private LetterPositioningReason DeterminePositioningReason(string senderId, Dictionary<ConnectionType, int> allTokens, int worstNegativeTokenPenalty)
        {
            if (HasActiveObligationWithNPC(senderId))
                return LetterPositioningReason.Obligation;

            if (allTokens.Any(kvp => kvp.Key == ConnectionType.Diplomacy) && allTokens[ConnectionType.Diplomacy] <= -3)
                return LetterPositioningReason.DiplomacyDebt;

            if (worstNegativeTokenPenalty > 0)
                return LetterPositioningReason.PoorStanding;

            if (GetHighestPositiveToken(allTokens) > 0)
                return LetterPositioningReason.GoodStanding;

            return LetterPositioningReason.Neutral;
        }

        private void RecordObligationPositioning(DeliveryObligation letter, LeverageCalculation leverageCalc)
        {
            letter.PositioningReason = leverageCalc.PositioningReason;
            letter.RelationshipStrength = leverageCalc.HighestPositiveToken;
            letter.RelationshipDebt = leverageCalc.WorstNegativeTokenPenalty;
            letter.FinalQueuePosition = leverageCalc.FinalPosition;

            _messageSystem.AddLetterPositioningMessage(
                letter.SenderName,
                leverageCalc.PositioningReason,
                leverageCalc.FinalPosition,
                leverageCalc.HighestPositiveToken,
                leverageCalc.WorstNegativeTokenPenalty
            );
        }

        private void ShowLeverageNarrative(DeliveryObligation letter, int actualPosition, LeverageCalculation leverageCalc)
        {
            int basePosition = leverageCalc.BasePosition;

            if (actualPosition < basePosition)
            {
                string senderId = leverageCalc.NPCId;
                if (!string.IsNullOrEmpty(senderId))
                {
                    int balance = leverageCalc.AllTokens.Any(kvp => kvp.Key == letter.TokenType) ?
                                 leverageCalc.AllTokens[letter.TokenType] : 0;

                    if (balance < 0)
                    {
                        _messageSystem.AddSystemMessage(
                            $"💸 Debt leverage: {letter.SenderName}'s letter jumps to position {actualPosition} (you owe {Math.Abs(balance)} {letter.TokenType} tokens)",
                            SystemMessageTypes.Warning
                        );
                    }
                }
            }
            else if (actualPosition > basePosition)
            {
                _messageSystem.AddSystemMessage(
                    $"💚 Strong relationship: {letter.SenderName}'s letter enters at position {actualPosition} (reduced leverage)",
                    SystemMessageTypes.Success
                );
            }
            else
            {
                _messageSystem.AddSystemMessage(
                    $"📨 Obligation from {letter.SenderName} enters queue at position {actualPosition}",
                    SystemMessageTypes.Info
                );
            }
        }

        private void ShowLeverageDisplacement(DeliveryObligation letter, int targetPosition, LeverageCalculation leverageCalc)
        {
            int balance = leverageCalc.AllTokens.Any(kvp => kvp.Key == letter.TokenType) ?
                         leverageCalc.AllTokens[letter.TokenType] : 0;

            if (balance < 0)
            {
                _messageSystem.AddSystemMessage(
                    $"⚡ {letter.SenderName} demands position {targetPosition} - you owe them!",
                    SystemMessageTypes.Danger
                );
                _messageSystem.AddSystemMessage(
                    $"  • Your {Math.Abs(balance)} token debt gives them power to displace others",
                    SystemMessageTypes.Warning
                );
            }
            else
            {
                _messageSystem.AddSystemMessage(
                    $"📬 {letter.SenderName}'s letter pushes into position {targetPosition}",
                    SystemMessageTypes.Warning
                );
            }
        }

        private void NotifyLetterShifted(DeliveryObligation letter, int newPosition)
        {
            string urgency = letter.DeadlineInSegments <= 48 ? " 🆘" : "";
            _messageSystem.AddSystemMessage(
                $"  • {letter.SenderName}'s letter pushed to position {newPosition}{urgency}",
                letter.DeadlineInSegments <= 48 ? SystemMessageTypes.Warning : SystemMessageTypes.Info
            );
        }

        private void HandleQueueOverflow(DeliveryObligation overflowLetter)
        {
            _messageSystem.AddSystemMessage(
                $"💥 {overflowLetter.SenderName}'s letter FORCED OUT by leverage!",
                SystemMessageTypes.Danger
            );

            // Apply relationship damage
            string senderId = GetNPCIdByName(overflowLetter.SenderName);
            int tokenPenalty = 2; // Same penalty as expiration

            _tokenManager.RemoveTokensFromNPC(overflowLetter.TokenType, tokenPenalty, senderId);

            _messageSystem.AddSystemMessage(
                $"💔 Lost {tokenPenalty} {overflowLetter.TokenType} tokens with {overflowLetter.SenderName}!",
                SystemMessageTypes.Danger
            );

            // Record in history
            Player player = _gameWorld.GetPlayer();
            if (!player.NPCLetterHistory.Any(h => h.NpcId == senderId))
            {
                player.NPCLetterHistory.AddOrUpdateHistory(senderId, new LetterHistory());
            }
            player.NPCLetterHistory.GetHistory(senderId).RecordExpiry();
        }

        private bool HasUsedMorningSwapToday()
        {
            // Would need to track this in player state or day tracking system
            // For now, simplified to allow one swap per morning
            return false;
        }

        private void MarkMorningSwapUsed()
        {
            // Would mark that morning swap has been used today
            // Implementation depends on day tracking system
        }

        private Dictionary<ConnectionType, int> CalculateDisplacementCost(DeliveryObligation obligation)
        {
            // Calculate the cost to displace this obligation
            Dictionary<ConnectionType, int> cost = new Dictionary<ConnectionType, int>();

            if (obligation.TokenType != ConnectionType.None)
            {
                cost[obligation.TokenType] = 1; // Base displacement cost
            }

            return cost;
        }

        private string GetNPCIdByName(string npcName)
        {
            NPC npc = _npcRepository.GetByName(npcName);
            return npc?.ID ?? "";
        }
    }
}