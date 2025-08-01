using System;
using System.Threading.Tasks;

/// <summary>
/// Specialized socialize command that allows equipment to enable specific token types
/// </summary>
public class EquipmentSocializeCommand : BaseGameCommand
{
    private readonly string _npcId;
    private readonly NPCRepository _npcRepository;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly MessageSystem _messageSystem;
    private readonly ConnectionType _specificTokenType;

    public EquipmentSocializeCommand(
        string npcId,
        ConnectionType tokenType,
        NPCRepository npcRepository,
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem)
    {
        _npcId = npcId ?? throw new ArgumentNullException(nameof(npcId));
        _specificTokenType = tokenType;
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));

        Description = $"Socialize with NPC {npcId} for {tokenType} tokens";
        
        // Set the token type this command will grant
        TokenTypeGranted = tokenType;
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
                $"You don't know {npc.Name} well enough to socialize",
                true,
                "Have a conversation first to introduce yourself");
        }

        // Check if equipment enables this token type
        if (!_tokenManager.CanGenerateTokenType(_specificTokenType, _npcId))
        {
            return CommandValidationResult.Failure(
                $"You lack the proper equipment to earn {_specificTokenType} tokens",
                true,
                GetEquipmentHint(_specificTokenType));
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

        // Stamina cost for socializing
        player.ModifyStamina(-1); // Light social activity

        // 50% chance to earn a token (same as letter delivery)
        bool earnedToken = new Random().Next(2) == 0;

        if (earnedToken)
        {
            _tokenManager.AddTokensToNPC(_specificTokenType, 1, _npcId);

            string flavorText = GetFlavorText(_specificTokenType, npc.Name);
            _messageSystem.AddSystemMessage(
                $"{flavorText} (+1 {_specificTokenType} token)",
                SystemMessageTypes.Success
            );
        }
        else
        {
            _messageSystem.AddSystemMessage(
                $"Spent time with {npc.Name}. They appreciated your company.",
                SystemMessageTypes.Success
            );
        }

        return CommandResult.Success(
            "Equipment-enabled socialization completed",
            new
            {
                NPCName = npc.Name,
                EarnedToken = earnedToken,
                TokenType = _specificTokenType,
                StaminaSpent = 1, // Light social activity
                TimeCost = 1
            }
        );
    }

    private string GetEquipmentHint(ConnectionType tokenType)
    {
        return tokenType switch
        {
            ConnectionType.Status => "Fine Clothes required for noble connections",
            ConnectionType.Commerce => "Merchant Ledger required for trade connections",
            _ => "Special equipment required"
        };
    }

    private string GetFlavorText(ConnectionType tokenType, string npcName)
    {
        return tokenType switch
        {
            ConnectionType.Status => $"Your refined attire impressed {npcName}, opening doors to noble discourse",
            ConnectionType.Commerce => $"Your merchant ledger facilitated productive business discussions with {npcName}",
            _ => $"Had a pleasant time with {npcName} and strengthened your connection!"
        };
    }
}