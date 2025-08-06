
/// <summary>
/// Operation to add a letter to the queue at a specific position
/// </summary>
public class AddLetterToQueueOperation : IGameOperation
{
    private readonly Letter _letter;
    private readonly int _targetPosition;
    private readonly LetterQueueManager _queueManager;

    public AddLetterToQueueOperation(Letter letter, int targetPosition, LetterQueueManager queueManager)
    {
        _letter = letter ?? throw new ArgumentNullException(nameof(letter));
        _targetPosition = targetPosition;
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
    }

    public string Description => $"Add letter from {_letter.SenderName} at position {_targetPosition}";

    public bool CanExecute(GameWorld gameWorld)
    {
        Letter[] queue = gameWorld.GetPlayer().LetterQueue;

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
        Letter[] queue = gameWorld.GetPlayer().LetterQueue;

        // If target position is empty, simple insertion
        if (queue[_targetPosition - 1] == null)
        {
            queue[_targetPosition - 1] = _letter;
            _letter.QueuePosition = _targetPosition;
            _letter.State = LetterState.Accepted;
            return;
        }

        // Target occupied - need to displace letters
        List<Letter> displacedLetters = new();

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
        _letter.State = LetterState.Accepted;

        // Reinsert displaced letters
        int nextAvailable = _targetPosition;
        foreach (Letter displaced in displacedLetters)
        {
            nextAvailable++;
            if (nextAvailable <= queue.Length)
            {
                queue[nextAvailable - 1] = displaced;
                displaced.QueuePosition = nextAvailable;
            }
            else
            {
                // Letter falls off the queue - this should be handled by the transaction
                throw new InvalidOperationException($"Letter from {displaced.SenderName} would be pushed out of queue");
            }
        }
    }
}