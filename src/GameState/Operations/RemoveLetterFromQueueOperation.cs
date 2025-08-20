
/// <summary>
/// Operation to remove a letter from the queue
/// </summary>
public class RemoveLetterFromQueueOperation : IGameOperation
{
private readonly int _position;

public RemoveLetterFromQueueOperation(int position)
{
    _position = position;
}

public string Description => $"Remove letter from position {_position}";

public bool CanExecute(GameWorld gameWorld)
{
    if (_position < 1 || _position > gameWorld.GetPlayer().ObligationQueue.Length)
        return false;

    return gameWorld.GetPlayer().ObligationQueue[_position - 1] != null;
}

public void Execute(GameWorld gameWorld)
{
    DeliveryObligation[] queue = gameWorld.GetPlayer().ObligationQueue;

    // Remove the letter
    DeliveryObligation removedDeliveryObligation = queue[_position - 1];
    queue[_position - 1] = null;
    removedDeliveryObligation.QueuePosition = 0;

    // Shift all letters below the removed position up by one
    for (int i = _position; i < queue.Length; i++)
    {
        if (queue[i] != null)
        {
            DeliveryObligation obligation = queue[i];

            // Move to new position
            queue[i - 1] = obligation;
            queue[i] = null;
            obligation.QueuePosition = i; // New position (1-based)
        }
    }
}
}