using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Command to collect a physical letter from an NPC sender
/// </summary>
public class CollectLetterCommand : BaseGameCommand
{
    private readonly string _letterId;
    private readonly LetterQueueManager _queueManager;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;


    public CollectLetterCommand(
        string letterId,
        LetterQueueManager queueManager,
        NPCRepository npcRepository,
        MessageSystem messageSystem)
    {
        _letterId = letterId ?? throw new ArgumentNullException(nameof(letterId));
        _queueManager = queueManager ?? throw new ArgumentNullException(nameof(queueManager));
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));

        Description = $"Collect letter {letterId}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();

        // Find the letter in queue
        Letter letter = player.LetterQueue.FirstOrDefault(l => l?.Id == _letterId);
        if (letter == null)
        {
            return CommandValidationResult.Failure("Letter not found in queue");
        }

        // Check if already collected
        if (letter.State == LetterState.Collected)
        {
            return CommandValidationResult.Failure("Letter already collected");
        }

        // Verify player is at sender's location
        NPC sender = _npcRepository.GetById(letter.SenderId);
        if (sender == null)
        {
            return CommandValidationResult.Failure("Letter sender not found");
        }

        if (!sender.IsAvailableAtLocation(player.CurrentLocationSpot?.SpotID))
        {
            return CommandValidationResult.Failure(
                $"Must visit {sender.Name} at their location to collect the letter",
                true,
                $"Travel to where {sender.Name} can be found");
        }

        // Check time availability
        TimeBlocks currentTime = gameWorld.TimeManager.GetCurrentTimeBlock();
        if (!sender.IsAvailableAtTime(player.CurrentLocationSpot.SpotID, currentTime))
        {
            return CommandValidationResult.Failure(
                $"{sender.Name} is not available at this time",
                true,
                "Try visiting at a different time");
        }

        // Check resource cost (1 hour)
        if (!gameWorld.TimeManager.CanPerformAction(1))
        {
            return CommandValidationResult.Failure(
                "Not enough time remaining",
                true,
                "Rest or wait until tomorrow");
        }

        // Check inventory space
        if (player.Inventory.IsFull())
        {
            return CommandValidationResult.Failure(
                "Inventory is full",
                true,
                "Make room in your inventory");
        }

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        // Find the letter again in execute method
        Letter letter = player.LetterQueue.FirstOrDefault(l => l?.Id == _letterId);
        NPC sender = _npcRepository.GetById(letter.SenderId);

        // Spend time
        gameWorld.TimeManager.SpendHours(1);

        // Mark letter as collected
        letter.State = LetterState.Collected;

        // Add to carried letters
        player.CarriedLetters.Add(letter);

        // Add to inventory
        player.Inventory.AddItem($"letter_{letter.Id}");

        _messageSystem.AddSystemMessage(
            $"Collected letter from {sender.Name} to {letter.RecipientName}",
            SystemMessageTypes.Success
        );

        return CommandResult.Success(
            "Letter collected",
            new
            {
                LetterId = letter.Id,
                SenderName = sender.Name,
                RecipientName = letter.RecipientName,
                Payment = letter.Payment,
                Deadline = letter.Deadline
            }
        );
    }

}