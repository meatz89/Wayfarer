
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
        if (_position < 1 || _position > gameWorld.GetPlayer().LetterQueue.Length)
            return false;

        return gameWorld.GetPlayer().LetterQueue[_position - 1] != null;
    }

    public void Execute(GameWorld gameWorld)
    {
        Letter[] queue = gameWorld.GetPlayer().LetterQueue;

        // Remove the letter
        Letter removedLetter = queue[_position - 1];
        queue[_position - 1] = null;
        removedLetter.QueuePosition = 0;

        // Shift all letters below the removed position up by one
        for (int i = _position; i < queue.Length; i++)
        {
            if (queue[i] != null)
            {
                Letter letter = queue[i];

                // Move to new position
                queue[i - 1] = letter;
                queue[i] = null;
                letter.QueuePosition = i; // New position (1-based)
            }
        }
    }
}