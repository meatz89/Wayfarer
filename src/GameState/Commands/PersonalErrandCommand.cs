using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Command to run a personal errand for an NPC (deliver medicine, find lost item, etc.)
/// This builds trust through helping with personal matters
/// </summary>
public class PersonalErrandCommand : BaseGameCommand
{
    private readonly string _npcId;
    private readonly NPCRepository _npcRepository;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly MessageSystem _messageSystem;
    private readonly ItemRepository _itemRepository;


    public PersonalErrandCommand(
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

        Description = $"Run personal errand for NPC {npcId}";
        
        // Personal errands build trust through actions
        TokenTypeGranted = ConnectionType.Trust;
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        // Validate NPC exists
        NPC npc = _npcRepository.GetById(_npcId);
        if (npc == null)
        {
            return CommandValidationResult.Failure("NPC not found");
        }

        // Check if have relationship (need at least 2 tokens)
        Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(_npcId);
        int currentTokens = npcTokens.Values.Sum();
        if (currentTokens < 2)
        {
            return CommandValidationResult.Failure(
                $"{npc.Name} doesn't know you well enough to ask for personal help",
                true,
                "Build more connection first (need 2+ tokens)");
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

        // Check if player has medicine (for most personal errands)
        bool hasMedicine = false;
        foreach (string itemId in player.Inventory.ItemSlots)
        {
            if (!string.IsNullOrEmpty(itemId))
            {
                Item item = _itemRepository.GetItemById(itemId);
                if (item != null && item.Categories.Contains(ItemCategory.Medicine))
                {
                    hasMedicine = true;
                    break;
                }
            }
        }
        
        if (!hasMedicine)
        {
            return CommandValidationResult.Failure(
                "You need medicine or supplies for this errand",
                true,
                "Gather or buy some medicine first");
        }

        // Check stamina (errands require effort)
        if (player.Stamina < 2)
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

        // Find and consume one medicine item
        string medicineItemId = null;
        foreach (string itemId in player.Inventory.ItemSlots)
        {
            if (!string.IsNullOrEmpty(itemId))
            {
                Item item = _itemRepository.GetItemById(itemId);
                if (item != null && item.Categories.Contains(ItemCategory.Medicine))
                {
                    medicineItemId = itemId;
                    break;
                }
            }
        }
        
        player.Inventory.RemoveItem(medicineItemId);
        Item medicineItem = _itemRepository.GetItemById(medicineItemId);

        // Time spending handled by executing service (2 hours for the errand)
        // Stamina cost
        player.ModifyStamina(-2);

        // Always grants a token - personal help builds trust
        ConnectionType tokenType = TokenTypeGranted.Value;
        _tokenManager.AddTokensToNPC(tokenType, 1, _npcId);

        // Generate contextual errand based on NPC
        string errandContext = GetErrandContext(npc);

        _messageSystem.AddSystemMessage(
            $"You help {npc.Name} with a personal matter: {errandContext} (+1 {tokenType} token)",
            SystemMessageTypes.Success
        );

        return CommandResult.Success(
            "Personal errand completed successfully",
            new
            {
                NPCName = npc.Name,
                EarnedToken = true,
                TokenType = tokenType,
                StaminaSpent = 2,
                TimeCost = 2,
                ItemUsed = medicineItem.Name,
                ErrandContext = errandContext
            }
        );
    }

    private string GetErrandContext(NPC npc)
    {
        return npc.Profession switch
        {
            Professions.Dock_Boss => "Delivered medicine to their sick daughter",
            Professions.Merchant => "Found their missing ledger before the audit",
            Professions.Noble => "Discreetly delivered a remedy for their ailment",
            Professions.Innkeeper => "Helped treat a regular patron who fell ill",
            Professions.TavernKeeper => "Brought supplies for an emergency at the tavern",
            Professions.Scholar => "Retrieved rare ink ingredients they desperately needed",
            Professions.Craftsman => "Delivered healing salve for a workshop accident",
            Professions.Soldier => "Brought medicine for wounds they couldn't report",
            _ => "Helped with an urgent personal matter"
        };
    }
}