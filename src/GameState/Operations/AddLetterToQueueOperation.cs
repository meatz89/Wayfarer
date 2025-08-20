using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;

namespace Wayfarer.GameState.Operations
{
    /// <summary>
    /// Operation to add a letter to the queue at a specific position
    /// </summary>
public class AddLetterToQueueOperation : IGameOperation
{
    private readonly DeliveryObligation _letter;
    private readonly int _targetPosition;
    private readonly ObligationQueueManager _queueManager;

    public AddLetterToQueueOperation(DeliveryObligation letter, int targetPosition, ObligationQueueManager queueManager)
    {
        _letter = letter ?? throw new ArgumentNullException(nameof(letter));
        _targetPosition = targetPosition;
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
    }

    public string Description => $"Add letter from {_letter.SenderName} at position {_targetPosition}";

    public bool CanExecute(GameWorld gameWorld)
    {
        DeliveryObligation[] queue = gameWorld.GetPlayer().ObligationQueue;

        // Check if queue has any space at all
        if (queue.All(slot => slot != null))
        {
            return false; // Queue completely full
        }

        // Check if target position is valid
        return _targetPosition >= 1 && _targetPosition <= queue.Length;
    }

    public void Execute(GameWorld gameWorld)
    {
        DeliveryObligation[] queue = gameWorld.GetPlayer().ObligationQueue;

        // If target position is empty, simple insertion
        if (queue[_targetPosition - 1] == null)
        {
            queue[_targetPosition - 1] = _letter;
            _letter.QueuePosition = _targetPosition;
            return;
        }

        // Target occupied - need to displace letters
        List<DeliveryObligation> displacedLetters = new();

        // Collect all letters from target position downward
        for (int i = _targetPosition - 1; i < queue.Length; i++)
        {
            if (queue[i] != null)
            {
                displacedLetters.Add(queue[i]);
                queue[i] = null;
            }
        }

        // Insert new letter at target position
        queue[_targetPosition - 1] = _letter;
        _letter.QueuePosition = _targetPosition;

        // Reinsert displaced letters
        int nextAvailable = _targetPosition;
        foreach (DeliveryObligation displaced in displacedLetters)
        {
            nextAvailable++;
            if (nextAvailable <= queue.Length)
            {
                queue[nextAvailable - 1] = displaced;
                displaced.QueuePosition = nextAvailable;
            }
            else
            {
                // DeliveryObligation falls off the queue - this should be handled by the transaction
                throw new InvalidOperationException($"DeliveryObligation from {displaced.SenderName} would be pushed out of queue");
            }
        }
    }
}
}