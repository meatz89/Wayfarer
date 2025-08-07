using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.GameState;

/// <summary>
/// Plans and previews queue displacement effects before committing changes
/// </summary>
public class QueueDisplacementPlanner
{
    private readonly LetterQueueManager _queueManager;
    private readonly GameConfiguration _config;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly MessageSystem _messageSystem;

    public QueueDisplacementPlanner(
        LetterQueueManager queueManager,
        GameConfiguration config,
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem)
    {
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
    }

    /// <summary>
    /// Plans the displacement effects of adding a letter at a specific position
    /// </summary>
    public DisplacementPlan PlanLetterAddition(Letter newLetter, int targetPosition)
    {
        DisplacementPlan plan = new DisplacementPlan
        {
            NewLetter = newLetter,
            TargetPosition = targetPosition,
            ActionType = DisplacementActionType.AddLetter
        };

        Letter[] currentQueue = _queueManager.GetPlayerQueue();

        // Check if queue is full
        if (currentQueue.All(slot => slot != null))
        {
            plan.IsQueueFull = true;
            plan.CanExecute = false;
            plan.FailureReason = "Queue is completely full - no letter can be added";
            return plan;
        }

        // If target position is empty, no displacement needed
        if (currentQueue[targetPosition - 1] == null)
        {
            plan.CanExecute = true;
            plan.SimpleInsertion = true;
            return plan;
        }

        // Calculate all movements
        for (int i = targetPosition - 1; i < currentQueue.Length; i++)
        {
            if (currentQueue[i] != null)
            {
                LetterMovement movement = new LetterMovement
                {
                    Letter = currentQueue[i],
                    FromPosition = i + 1,
                    ToPosition = i + 2
                };

                // Check if this letter would be pushed out
                if (movement.ToPosition > _config.LetterQueue.MaxQueueSize)
                {
                    plan.Evictions.Add(new LetterEviction
                    {
                        Letter = movement.Letter,
                        FromPosition = movement.FromPosition,
                        Reason = "Pushed out by leverage",
                        TokenPenalty = _config.LetterQueue.DeadlinePenaltyTokens
                    });
                }
                else
                {
                    plan.Movements.Add(movement);
                }
            }
        }

        plan.CanExecute = true;
        return plan;
    }

    /// <summary>
    /// Plans the effects of skipping a letter to position 1
    /// </summary>
    public DisplacementPlan PlanSkipDelivery(int fromPosition)
    {
        DisplacementPlan plan = new DisplacementPlan
        {
            TargetPosition = 1,
            ActionType = DisplacementActionType.SkipDelivery
        };

        Letter[] currentQueue = _queueManager.GetPlayerQueue();

        // Validate position
        if (fromPosition <= 1 || fromPosition > _config.LetterQueue.MaxQueueSize)
        {
            plan.CanExecute = false;
            plan.FailureReason = "Invalid skip position";
            return plan;
        }

        Letter letter = currentQueue[fromPosition - 1];
        if (letter == null)
        {
            plan.CanExecute = false;
            plan.FailureReason = "No letter at specified position";
            return plan;
        }

        plan.NewLetter = letter;

        // Check if position 1 is occupied
        if (currentQueue[0] != null)
        {
            plan.CanExecute = false;
            plan.FailureReason = "Position 1 is already occupied";
            return plan;
        }

        // Calculate token cost
        int baseCost = fromPosition - 1;
        plan.TokenCost = new Dictionary<ConnectionType, int>
        {
            { letter.TokenType, baseCost }
        };

        // Check token availability
        if (!_tokenManager.HasTokens(letter.TokenType, baseCost))
        {
            plan.CanExecute = false;
            plan.FailureReason = $"Insufficient {letter.TokenType} tokens (need {baseCost})";
            plan.TokenDeficit = new Dictionary<ConnectionType, int>
            {
                { letter.TokenType, baseCost - _tokenManager.GetTokenCount(letter.TokenType) }
            };
            return plan;
        }

        // Track skipped letters
        for (int i = 1; i < fromPosition - 1; i++)
        {
            if (currentQueue[i] != null)
            {
                plan.SkippedLetters.Add(new SkippedLetter
                {
                    Letter = currentQueue[i],
                    Position = i + 1,
                    RelationshipImpact = -1 // Skipping damages relationship
                });
            }
        }

        // Plan the movement
        plan.Movements.Add(new LetterMovement
        {
            Letter = letter,
            FromPosition = fromPosition,
            ToPosition = 1
        });

        plan.CanExecute = true;
        return plan;
    }

    /// <summary>
    /// Plans the effects of purging the bottom letter
    /// </summary>
    public DisplacementPlan PlanPurge()
    {
        DisplacementPlan plan = new DisplacementPlan
        {
            TargetPosition = _config.LetterQueue.MaxQueueSize,
            ActionType = DisplacementActionType.Purge
        };

        Letter[] currentQueue = _queueManager.GetPlayerQueue();
        Letter letterToPurge = currentQueue[_config.LetterQueue.MaxQueueSize - 1];

        if (letterToPurge == null)
        {
            plan.CanExecute = false;
            plan.FailureReason = $"No letter in position {_config.LetterQueue.MaxQueueSize} to purge";
            return plan;
        }

        plan.NewLetter = letterToPurge;

        // Set token cost (3 tokens of any type)
        plan.TokenCost = new Dictionary<ConnectionType, int>();
        plan.RequiredTokenTotal = _config.LetterQueue.PurgeCostTokens;

        // Add purge to evictions
        plan.Evictions.Add(new LetterEviction
        {
            Letter = letterToPurge,
            FromPosition = _config.LetterQueue.MaxQueueSize,
            Reason = "Purged by player",
            TokenPenalty = 0 // No relationship penalty for purging
        });

        plan.CanExecute = true;
        return plan;
    }

    /// <summary>
    /// Plans the effects of a morning swap
    /// </summary>
    public DisplacementPlan PlanMorningSwap(int position1, int position2)
    {
        DisplacementPlan plan = new DisplacementPlan
        {
            ActionType = DisplacementActionType.MorningSwap
        };

        Letter[] currentQueue = _queueManager.GetPlayerQueue();

        // Validate positions are adjacent
        if (Math.Abs(position1 - position2) != 1)
        {
            plan.CanExecute = false;
            plan.FailureReason = "Positions must be adjacent for morning swap";
            return plan;
        }

        // Validate positions are in range
        if (position1 < 1 || position1 > _config.LetterQueue.MaxQueueSize ||
            position2 < 1 || position2 > _config.LetterQueue.MaxQueueSize)
        {
            plan.CanExecute = false;
            plan.FailureReason = "Invalid positions for swap";
            return plan;
        }

        Letter? letter1 = currentQueue[position1 - 1];
        Letter letter2 = currentQueue[position2 - 1];

        // At least one position must have a letter
        if (letter1 == null && letter2 == null)
        {
            plan.CanExecute = false;
            plan.FailureReason = "At least one position must contain a letter";
            return plan;
        }

        // Add swap movements
        if (letter1 != null)
        {
            plan.Movements.Add(new LetterMovement
            {
                Letter = letter1,
                FromPosition = position1,
                ToPosition = position2
            });
        }

        if (letter2 != null)
        {
            plan.Movements.Add(new LetterMovement
            {
                Letter = letter2,
                FromPosition = position2,
                ToPosition = position1
            });
        }

        plan.CanExecute = true;
        plan.IsFreeAction = true;
        return plan;
    }

    /// <summary>
    /// Creates a preview message for a displacement plan
    /// </summary>
    public void ShowDisplacementPreview(DisplacementPlan plan)
    {
        if (!plan.CanExecute)
        {
            _messageSystem.AddSystemMessage(
                $"❌ Cannot execute: {plan.FailureReason}",
                SystemMessageTypes.Danger
            );
            return;
        }

        // Show action summary
        switch (plan.ActionType)
        {
            case DisplacementActionType.AddLetter:
                _messageSystem.AddSystemMessage(
                    $"📨 PREVIEW: Adding {plan.NewLetter.SenderName}'s letter at position {plan.TargetPosition}",
                    SystemMessageTypes.Info
                );
                break;

            case DisplacementActionType.SkipDelivery:
                _messageSystem.AddSystemMessage(
                    $"⏭️ PREVIEW: Skipping {plan.NewLetter.SenderName}'s letter to position 1",
                    SystemMessageTypes.Info
                );
                break;

            case DisplacementActionType.Purge:
                _messageSystem.AddSystemMessage(
                    $"🔥 PREVIEW: Purging {plan.NewLetter.SenderName}'s letter",
                    SystemMessageTypes.Warning
                );
                break;

            case DisplacementActionType.MorningSwap:
                _messageSystem.AddSystemMessage(
                    $"🔄 PREVIEW: Morning swap",
                    SystemMessageTypes.Info
                );
                break;
        }

        // Show token costs
        if (plan.TokenCost?.Any() == true)
        {
            _messageSystem.AddSystemMessage("💸 Token Cost:", SystemMessageTypes.Warning);
            foreach (KeyValuePair<ConnectionType, int> cost in plan.TokenCost)
            {
                _messageSystem.AddSystemMessage(
                    $"  • {cost.Value} {cost.Key} tokens",
                    SystemMessageTypes.Info
                );
            }
        }

        // Show movements
        if (plan.Movements.Any())
        {
            _messageSystem.AddSystemMessage("📬 Queue Changes:", SystemMessageTypes.Info);
            foreach (LetterMovement movement in plan.Movements)
            {
                string urgency = movement.Letter.DeadlineInHours <= 2 ? " ⚠️" : "";
                _messageSystem.AddSystemMessage(
                    $"  • {movement.Letter.SenderName}: position {movement.FromPosition} → {movement.ToPosition}{urgency}",
                    SystemMessageTypes.Info
                );
            }
        }

        // Show skipped letters
        if (plan.SkippedLetters.Any())
        {
            _messageSystem.AddSystemMessage("⏩ Skipped Letters:", SystemMessageTypes.Warning);
            foreach (SkippedLetter skipped in plan.SkippedLetters)
            {
                _messageSystem.AddSystemMessage(
                    $"  • {skipped.Letter.SenderName} at position {skipped.Position} (relationship -1)",
                    SystemMessageTypes.Warning
                );
            }
        }

        // Show evictions
        if (plan.Evictions.Any())
        {
            _messageSystem.AddSystemMessage("💥 EVICTIONS:", SystemMessageTypes.Danger);
            foreach (LetterEviction eviction in plan.Evictions)
            {
                _messageSystem.AddSystemMessage(
                    $"  • {eviction.Letter.SenderName} will be REMOVED ({eviction.Reason})",
                    SystemMessageTypes.Danger
                );
                if (eviction.TokenPenalty > 0)
                {
                    _messageSystem.AddSystemMessage(
                        $"    → Lose {eviction.TokenPenalty} {eviction.Letter.TokenType} tokens!",
                        SystemMessageTypes.Danger
                    );
                }
            }
        }

        // Show summary
        if (plan.SimpleInsertion)
        {
            _messageSystem.AddSystemMessage(
                "✅ Simple insertion - no displacement needed",
                SystemMessageTypes.Success
            );
        }
        else if (plan.Movements.Any() || plan.Evictions.Any())
        {
            int totalAffected = plan.Movements.Count + plan.Evictions.Count;
            _messageSystem.AddSystemMessage(
                $"📊 Total affected: {totalAffected} letter(s)",
                SystemMessageTypes.Info
            );
        }
    }
}

/// <summary>
/// Represents a planned displacement of letters in the queue
/// </summary>
public class DisplacementPlan
{
    public DisplacementActionType ActionType { get; set; }
    public Letter NewLetter { get; set; }
    public int TargetPosition { get; set; }
    public List<LetterMovement> Movements { get; set; } = new();
    public List<LetterEviction> Evictions { get; set; } = new();
    public List<SkippedLetter> SkippedLetters { get; set; } = new();
    public Dictionary<ConnectionType, int> TokenCost { get; set; } = new();
    public Dictionary<ConnectionType, int> TokenDeficit { get; set; } = new();
    public int RequiredTokenTotal { get; set; }
    public bool CanExecute { get; set; }
    public string FailureReason { get; set; }
    public bool SimpleInsertion { get; set; }
    public bool IsQueueFull { get; set; }
    public bool IsFreeAction { get; set; }
}

/// <summary>
/// Represents a letter movement in the queue
/// </summary>
public class LetterMovement
{
    public Letter Letter { get; set; }
    public int FromPosition { get; set; }
    public int ToPosition { get; set; }
}

/// <summary>
/// Represents a letter being evicted from the queue
/// </summary>
public class LetterEviction
{
    public Letter Letter { get; set; }
    public int FromPosition { get; set; }
    public string Reason { get; set; }
    public int TokenPenalty { get; set; }
}

/// <summary>
/// Represents a letter being skipped over
/// </summary>
public class SkippedLetter
{
    public Letter Letter { get; set; }
    public int Position { get; set; }
    public int RelationshipImpact { get; set; }
}

/// <summary>
/// Types of displacement actions
/// </summary>
public enum DisplacementActionType
{
    AddLetter,
    SkipDelivery,
    Purge,
    MorningSwap,
    PriorityMove,
    ExtendDeadline
}