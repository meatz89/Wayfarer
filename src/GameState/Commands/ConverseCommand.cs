using System;
using System.Threading.Tasks;

/// <summary>
/// Command to have a conversation with an NPC
/// </summary>
public class ConverseCommand : BaseGameCommand
{
    private readonly string _npcId;
    private readonly ConversationFactory _conversationFactory;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;


    public ConverseCommand(
        string npcId,
        ConversationFactory conversationFactory,
        NPCRepository npcRepository,
        MessageSystem messageSystem)
    {
        _npcId = npcId ?? throw new ArgumentNullException(nameof(npcId));
        _conversationFactory = conversationFactory ?? throw new ArgumentNullException(nameof(conversationFactory));
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));

        Description = $"Converse with NPC {npcId}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        // Validate NPC exists
        NPC npc = _npcRepository.GetById(_npcId);
        if (npc == null)
        {
            return CommandValidationResult.Failure("NPC not found");
        }

        // Check if NPC is at current location
        Player player = gameWorld.GetPlayer();
        if (player.CurrentLocationSpot == null)
        {
            return CommandValidationResult.Failure("Player location not set");
        }

        // Check time availability
        TimeBlocks currentTime = gameWorld.CurrentTimeBlock;
        if (!npc.IsAvailableAtTime(player.CurrentLocationSpot.SpotID, currentTime))
        {
            return CommandValidationResult.Failure(
                $"{npc.Name} is not available at this time",
                true,
                "Try visiting at a different time");
        }

        // Time cost check removed - handled by executing service

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        NPC npc = _npcRepository.GetById(_npcId);
        Player player = gameWorld.GetPlayer();

        // Create conversation context
        ConversationContext context = new ConversationContext
        {
            GameWorld = gameWorld,
            Player = player,
            TargetNPC = npc,
            LocationName = player.CurrentLocation?.Name ?? "Unknown",
            LocationSpotName = player.CurrentLocationSpot?.Name ?? "Unknown",
            LocationProperties = player.CurrentLocationSpot?.GetCurrentProperties() ?? new List<string>(),
            ConversationTopic = "General conversation",
            StartingFocusPoints = 10
        };

        // Create conversation manager
        ConversationManager conversationManager = await _conversationFactory.CreateConversation(
            context,
            player
        );

        // Time spending handled by executing service

        // Set up conversation for UI
        gameWorld.PendingConversationManager = conversationManager;
        gameWorld.ConversationPending = true;

        _messageSystem.AddSystemMessage(
            $"Starting conversation with {npc.Name}",
            SystemMessageTypes.Info
        );

        return CommandResult.Success(
            "Conversation initiated",
            new
            {
                NPCName = npc.Name,
                RequiresConversation = true,
                ConversationManager = conversationManager,
                TimeCost = 1  // Add time cost to result
            }
        );
    }

}