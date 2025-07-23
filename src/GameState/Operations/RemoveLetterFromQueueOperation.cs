namespace Wayfarer.GameState.Operations;

/// <summary>
/// Operation to remove a letter from the queue
/// </summary>
public class RemoveLetterFromQueueOperation : IGameOperation
{
    private readonly int _position;
    private Letter _removedLetter;
    private List<(Letter letter, int originalPosition)> _shiftedLetters = new();
    
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
        var queue = gameWorld.GetPlayer().LetterQueue;
        
        // Store the letter being removed
        _removedLetter = queue[_position - 1];
        queue[_position - 1] = null;
        _removedLetter.QueuePosition = 0;
        
        // Shift all letters below the removed position up by one
        _shiftedLetters.Clear();
        for (int i = _position; i < queue.Length; i++)
        {
            if (queue[i] != null)
            {
                var letter = queue[i];
                _shiftedLetters.Add((letter, i + 1)); // Store original position (1-based)
                
                // Move to new position
                queue[i - 1] = letter;
                queue[i] = null;
                letter.QueuePosition = i; // New position (1-based)
            }
        }
    }
    
    public void Rollback(GameWorld gameWorld)
    {
        var queue = gameWorld.GetPlayer().LetterQueue;
        
        // First, move shifted letters back to their original positions
        foreach (var (letter, originalPosition) in _shiftedLetters.AsEnumerable().Reverse())
        {
            queue[originalPosition - 1] = letter;
            queue[originalPosition - 2] = null; // Clear the shifted position
            letter.QueuePosition = originalPosition;
        }
        
        // Restore the removed letter
        if (_removedLetter != null)
        {
            queue[_position - 1] = _removedLetter;
            _removedLetter.QueuePosition = _position;
        }
    }
}