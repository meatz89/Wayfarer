using System;
using System.Threading.Tasks;

/// <summary>
/// Command to socialize with an NPC to build relationship tokens
/// </summary>
public class SocializeCommand : BaseGameCommand
{
    private readonly string _npcId;
    private readonly NPCRepository _npcRepository;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly MessageSystem _messageSystem;


    public SocializeCommand(
        string npcId,
        NPCRepository npcRepository,
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem)
    {
        _npcId = npcId ?? throw new ArgumentNullException(nameof(npcId));
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));

        Description = $"Socialize with NPC {npcId}";
        
        // Socialize grants Common tokens by default (casual conversation)
        // More specific social actions can grant other token types
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

        // Time spending handled by executing service
        // Stamina cost still applied here
        player.ModifyStamina(-1);

        // 50% chance to earn a token (same as letter delivery)
        bool earnedToken = new Random().Next(2) == 0;
        // Use the command's token type, or fall back to NPC's primary type
        ConnectionType tokenType = TokenTypeGranted ?? npc.LetterTokenTypes.FirstOrDefault();

        if (earnedToken && tokenType != default)
        {
            _tokenManager.AddTokensToNPC(tokenType, 1, _npcId);

            _messageSystem.AddSystemMessage(
                $"Had a pleasant time with {npc.Name} and strengthened your connection! (+1 {tokenType} token)",
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
            "Socialization completed",
            new
            {
                NPCName = npc.Name,
                EarnedToken = earnedToken,
                TokenType = tokenType,
                StaminaSpent = 1,
                TimeCost = 1  // Add time cost to result
            }
        );
    }

}