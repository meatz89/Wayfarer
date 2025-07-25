using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Command to share lunch with an NPC, building common connection
/// </summary>
public class ShareLunchCommand : BaseGameCommand
{
    private readonly string _npcId;
    private readonly NPCRepository _npcRepository;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly MessageSystem _messageSystem;
    private readonly ItemRepository _itemRepository;


    public ShareLunchCommand(
        string npcId,
        NPCRepository npcRepository,
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem,
        ItemRepository itemRepository)
    {
        _npcId = npcId ?? throw new ArgumentNullException(nameof(npcId));
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));

        Description = $"Share lunch with NPC {npcId}";
        
        // Sharing lunch is a common social activity that builds everyday connections
        TokenTypeGranted = ConnectionType.Common;
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        // Validate NPC exists
        NPC npc = _npcRepository.GetById(_npcId);
        if (npc == null)
        {
            return CommandValidationResult.Failure("NPC not found");
        }

        // Check if already have relationship
        Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(_npcId);
        int currentTokens = npcTokens.Values.Sum();
        if (currentTokens == 0)
        {
            return CommandValidationResult.Failure(
                $"You don't know {npc.Name} well enough to share lunch",
                true,
                "Have a conversation first to introduce yourself");
        }

        // Check if NPC is at current location
        Player player = gameWorld.GetPlayer();
        if (player.CurrentLocationSpot == null)
        {
            return CommandValidationResult.Failure("Player location not set");
        }

        // Check time availability (only during afternoon for lunch)
        TimeBlocks currentTime = gameWorld.CurrentTimeBlock;
        if (currentTime != TimeBlocks.Afternoon)
        {
            return CommandValidationResult.Failure(
                "It's not lunchtime",
                true,
                "Come back during afternoon");
        }
        
        if (!npc.IsAvailableAtTime(player.CurrentLocationSpot.SpotID, currentTime))
        {
            return CommandValidationResult.Failure(
                $"{npc.Name} is not available at this time",
                true,
                "Try visiting at a different time");
        }

        // Check if player has food
        bool hasFood = false;
        foreach (string itemId in player.Inventory.ItemSlots)
        {
            if (!string.IsNullOrEmpty(itemId))
            {
                Item item = _itemRepository.GetItemById(itemId);
                if (item != null && item.Categories.Contains(ItemCategory.Food))
                {
                    hasFood = true;
                    break;
                }
            }
        }
        
        if (!hasFood)
        {
            return CommandValidationResult.Failure(
                "You need food to share lunch",
                true,
                "Gather or buy some food first");
        }

        // Check stamina
        if (player.Stamina < 1)
        {
            return CommandValidationResult.Failure(
                "Not enough stamina",
                true,
                "Rest to recover stamina");
        }

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        NPC npc = _npcRepository.GetById(_npcId);
        Player player = gameWorld.GetPlayer();

        // Find and consume one food item
        string foodItemId = null;
        foreach (string itemId in player.Inventory.ItemSlots)
        {
            if (!string.IsNullOrEmpty(itemId))
            {
                Item item = _itemRepository.GetItemById(itemId);
                if (item != null && item.Categories.Contains(ItemCategory.Food))
                {
                    foodItemId = itemId;
                    break;
                }
            }
        }
        
        player.Inventory.RemoveItem(foodItemId);
        Item foodItem = _itemRepository.GetItemById(foodItemId);

        // Time spending handled by executing service
        // Stamina cost still applied here
        player.ModifyStamina(-1);

        // Higher chance to earn token (75%) because you're sharing resources
        bool earnedToken = new Random().Next(4) > 0; // 3 out of 4 chance
        ConnectionType tokenType = TokenTypeGranted.Value;
        
        if (earnedToken)
        {
            _tokenManager.AddTokensToNPC(tokenType, 1, _npcId);

            _messageSystem.AddSystemMessage(
                $"Shared a pleasant lunch with {npc.Name}. The conversation flows easily over food! (+1 {tokenType} token)",
                SystemMessageTypes.Success
            );
        }
        else
        {
            _messageSystem.AddSystemMessage(
                $"Enjoyed lunch with {npc.Name}. They appreciated the company and the meal.",
                SystemMessageTypes.Success
            );
        }

        return CommandResult.Success(
            "Lunch shared successfully",
            new
            {
                NPCName = npc.Name,
                EarnedToken = earnedToken,
                TokenType = tokenType,
                StaminaSpent = 1,
                TimeCost = 1,
                FoodConsumed = foodItem.Name
            }
        );
    }
}