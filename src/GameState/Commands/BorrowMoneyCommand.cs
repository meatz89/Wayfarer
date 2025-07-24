using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Command to borrow money from NPCs (costs connection tokens)
/// </summary>
public class BorrowMoneyCommand : BaseGameCommand
{
    private readonly string _npcId;
    private readonly NPCRepository _npcRepository;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly MessageSystem _messageSystem;
    private readonly GameConfiguration _config;


    public BorrowMoneyCommand(
        string npcId,
        NPCRepository npcRepository,
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem,
        GameConfiguration config)
    {
        _npcId = npcId ?? throw new ArgumentNullException(nameof(npcId));
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        Description = $"Borrow money from NPC {npcId}";
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

        TimeBlocks currentTime = gameWorld.CurrentTimeBlock;
        if (!npc.IsAvailableAtTime(player.CurrentLocationSpot.SpotID, currentTime))
        {
            return CommandValidationResult.Failure(
                $"{npc.Name} is not available at this time",
                true,
                "Try visiting at a different time");
        }

        // Check if NPC offers loans (must have trade or shadow tokens)
        bool canLend = npc.LetterTokenTypes.Contains(ConnectionType.Trade) ||
                      npc.LetterTokenTypes.Contains(ConnectionType.Shadow);

        if (!canLend)
        {
            return CommandValidationResult.Failure($"{npc.Name} doesn't offer loans");
        }

        // Time cost check removed - handled by executing service

        // Check token cost (2 tokens of NPC's type)
        int tokenCost = _config.Debt.BorrowMoneyCost;
        ConnectionType requiredType = npc.LetterTokenTypes.FirstOrDefault();
        int availableTokens = _tokenManager.GetTokenCount(requiredType);

        if (availableTokens < tokenCost)
        {
            return CommandValidationResult.Failure(
                $"Not enough {requiredType} tokens (need {tokenCost}, have {availableTokens})",
                true,
                $"Build relationship with {npc.Name} to earn more tokens");
        }

        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        NPC npc = _npcRepository.GetById(_npcId);
        Player player = gameWorld.GetPlayer();

        // Determine loan amount and cost
        int coinsReceived = _config.Debt.BorrowMoneyAmount;
        int tokenCost = _config.Debt.BorrowMoneyCost;
        ConnectionType tokenType = npc.LetterTokenTypes.FirstOrDefault();

        // For shadow NPCs, potentially offer more money
        if (tokenType == ConnectionType.Shadow)
        {
            coinsReceived = _config.Debt.IllegalWorkAmount; // More coins from shadow dealings
        }

        // Time spending handled by executing service
        _tokenManager.SpendTokens(tokenType, tokenCost, _npcId);

        // Add coins
        player.ModifyCoins(coinsReceived);

        // Narrative feedback based on NPC type
        if (tokenType == ConnectionType.Trade)
        {
            _messageSystem.AddSystemMessage(
                $"💰 {npc.Name} counts out {coinsReceived} coins. \"Pay me back when you can, friend.\"",
                SystemMessageTypes.Success
            );
        }
        else if (tokenType == ConnectionType.Shadow)
        {
            _messageSystem.AddSystemMessage(
                $"💰 {npc.Name} slides {coinsReceived} coins across. \"You owe me. Don't forget that.\"",
                SystemMessageTypes.Success
            );
        }

        _messageSystem.AddSystemMessage(
            $"  Spent {tokenCost} {tokenType} tokens as collateral",
            SystemMessageTypes.Info
        );

        // Add warning if tokens go negative (debt)
        Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(_npcId);
        if (npcTokens[tokenType] < 0)
        {
            _messageSystem.AddSystemMessage(
                $"⚠️ You now owe {npc.Name}. They'll expect repayment... with interest.",
                SystemMessageTypes.Warning
            );
        }

        return CommandResult.Success(
            "Money borrowed successfully",
            new
            {
                NPCName = npc.Name,
                CoinsReceived = coinsReceived,
                TokenType = tokenType.ToString(),
                TokensSpent = tokenCost,
                RemainingTokens = _tokenManager.GetTokenCount(tokenType),
                TimeCost = 1  // Add time cost to result
            }
        );
    }

}