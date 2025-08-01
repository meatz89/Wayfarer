using System;
using System.Threading.Tasks;

/// <summary>
/// Command to keep a secret for an NPC, building trust connection
/// This represents any confidential task or information the NPC shares
/// </summary>
public class KeepSecretCommand : BaseGameCommand
{
    private readonly string _npcId;
    private readonly NPCRepository _npcRepository;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly MessageSystem _messageSystem;


    public KeepSecretCommand(
        string npcId,
        NPCRepository npcRepository,
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem)
    {
        _npcId = npcId ?? throw new ArgumentNullException(nameof(npcId));
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));

        Description = $"Keep a secret for NPC {npcId}";
        
        // Keeping secrets builds deep trust
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

        // Check if have strong enough relationship (need at least 3 tokens)
        Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(_npcId);
        int currentTokens = npcTokens.Values.Sum();
        if (currentTokens < 3)
        {
            return CommandValidationResult.Failure(
                $"{npc.Name} doesn't trust you enough to share secrets",
                true,
                "Build more connection first (need 3+ tokens)");
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

        // No resource cost - keeping secrets is about trust, not resources

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        NPC npc = _npcRepository.GetById(_npcId);
        Player player = gameWorld.GetPlayer();

        // Time spending handled by executing service (1 hour for the conversation)
        // No stamina cost - this is a conversation action

        // Always grants a token - if they trust you with a secret, you've earned trust
        ConnectionType tokenType = TokenTypeGranted.Value;
        _tokenManager.AddTokensToNPC(tokenType, 1, _npcId);

        // Generate contextual secret based on NPC profession
        string secretContext = GetSecretContext(npc);

        _messageSystem.AddSystemMessage(
            $"{npc.Name} confides in you: \"{secretContext}\" You promise to keep their secret. (+1 {tokenType} token)",
            SystemMessageTypes.Success
        );

        return CommandResult.Success(
            "Secret kept successfully",
            new
            {
                NPCName = npc.Name,
                EarnedToken = true,
                TokenType = tokenType,
                TimeCost = 1,
                SecretContext = secretContext
            }
        );
    }

    private string GetSecretContext(NPC npc)
    {
        return npc.Profession switch
        {
            Professions.Dock_Boss => "My daughter is sick, but I can't afford to miss work",
            Professions.Merchant => "I'm expecting a valuable shipment that's not on the books",
            Professions.Status => "There are political moves being made against my house",
            Professions.Innkeeper => "Some of my guests are not who they claim to be",
            Professions.TavernKeeper => "There's been talk of trouble brewing in the back rooms",
            Professions.Scholar => "I've discovered something that could change everything",
            Professions.Craftsman => "I've been commissioned for items I shouldn't be making",
            Professions.Soldier => "My orders conflict with what I know is right",
            _ => "I need someone I can trust with this information"
        };
    }
}