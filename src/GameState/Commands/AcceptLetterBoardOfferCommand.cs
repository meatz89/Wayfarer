using System;
using System.Threading.Tasks;

/// <summary>
/// Command to accept a letter offer from the letter board (not from NPCs)
/// </summary>
public class AcceptLetterBoardOfferCommand : BaseGameCommand
{
    private readonly Letter _letter;
    private readonly LetterQueueManager _queueManager;
    private readonly MessageSystem _messageSystem;

    public AcceptLetterBoardOfferCommand(
        Letter letter,
        LetterQueueManager queueManager,
        MessageSystem messageSystem)
    {
        _letter = letter ?? throw new ArgumentNullException(nameof(letter));
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        
        Description = $"Accept letter from {_letter.SenderName}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        // Check if queue has space
        if (_queueManager.IsQueueFull())
        {
            return CommandValidationResult.Failure(
                "Letter queue is full",
                true,
                "Make room in your queue first");
        }

        // Letter board offers are always available during dawn
        if (gameWorld.CurrentTimeBlock != TimeBlocks.Dawn)
        {
            return CommandValidationResult.Failure("Letter board is only available during dawn");
        }

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        // Add letter to queue with leverage-based positioning
        int position = _queueManager.AddLetterWithObligationEffects(_letter);
        
        if (position > 0)
        {
            _messageSystem.AddSystemMessage(
                $"Accepted letter from {_letter.SenderName} to {_letter.RecipientName}",
                SystemMessageTypes.Success
            );
            
            _messageSystem.AddSystemMessage(
                $"Letter enters queue at position {position} - {_letter.Payment} coins on delivery",
                SystemMessageTypes.Info
            );
            
            return CommandResult.Success(
                "Letter accepted",
                new
                {
                    LetterId = _letter.Id,
                    QueuePosition = position,
                    SenderName = _letter.SenderName,
                    RecipientName = _letter.RecipientName,
                    Payment = _letter.Payment,
                    Deadline = _letter.Deadline
                }
            );
        }
        
        return CommandResult.Failure("Failed to add letter to queue");
    }
}