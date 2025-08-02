using System;
using System.Threading.Tasks;

/// <summary>
/// Command to work for an NPC to earn coins
/// </summary>
public class WorkCommand : BaseGameCommand
{
    private readonly string _npcId;
    private readonly NPCRepository _npcRepository;
    private readonly IGameRuleEngine _ruleEngine;
    private readonly GameConfiguration _gameConfiguration;
    private readonly MessageSystem _messageSystem;
    private readonly ConnectionTokenManager _tokenManager;


    public WorkCommand(
        string npcId,
        NPCRepository npcRepository,
        IGameRuleEngine ruleEngine,
        GameConfiguration gameConfiguration,
        MessageSystem messageSystem,
        ConnectionTokenManager tokenManager)
    {
        _npcId = npcId ?? throw new ArgumentNullException(nameof(npcId));
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
        _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
        _gameConfiguration = gameConfiguration ?? throw new ArgumentNullException(nameof(gameConfiguration));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));

        Description = $"Work for NPC {npcId}";
        
        // Determine token type based on work context
        NPC npc = _npcRepository.GetById(_npcId);
        if (npc != null)
        {
            // Dock work grants Trade tokens (loading/unloading cargo)
            if (npc.Profession == Professions.Dock_Boss || npc.Profession == Professions.Soldier)
            {
                TokenTypeGranted = ConnectionType.Commerce;
            }
            // Most other work grants Common tokens (everyday labor)
            else if (npc.Profession == Professions.Craftsman || npc.Profession == Professions.Innkeeper ||
                     npc.Profession == Professions.TavernKeeper || npc.Profession == Professions.Merchant)
            {
                TokenTypeGranted = ConnectionType.Trust;
            }
            // Noble work might grant Noble tokens
            else if (npc.Profession == Professions.Noble || npc.Profession == Professions.Scholar)
            {
                TokenTypeGranted = ConnectionType.Status;
            }
            // Otherwise fall back to NPC's primary type
        }
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        // Validate NPC exists and offers work
        NPC npc = _npcRepository.GetById(_npcId);
        if (npc == null)
        {
            return CommandValidationResult.Failure("NPC not found");
        }

        if (!npc.OffersWork)
        {
            return CommandValidationResult.Failure($"{npc.Name} doesn't offer work");
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

        // Check resource costs (1 stamina) - time check handled by executing service

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

        // Get work reward from configuration based on NPC profession
        WorkReward reward = GetWorkReward(npc);

        // Apply costs
        int hoursSpent = 1;
        int staminaSpent = 1;
        player.ModifyStamina(-staminaSpent);
        // Time spending is handled by the executing service

        // Apply rewards
        int coinsEarned = reward.BaseCoins + reward.BonusCoins;
        player.ModifyCoins(coinsEarned);

        // 50% chance to earn a token (same as socializing/delivery)
        bool earnedToken = new Random().Next(2) == 0;
        ConnectionType tokenType = TokenTypeGranted ?? npc.LetterTokenTypes.FirstOrDefault();
        
        if (earnedToken && tokenType != default)
        {
            _tokenManager.AddTokensToNPC(tokenType, 1, _npcId);
        }

        // Success message
        string message = $"Worked for {npc.Name} and earned {coinsEarned} coins";
        if (reward.BonusCoins > 0)
        {
            message += $" (including {reward.BonusCoins} bonus)";
        }
        if (earnedToken)
        {
            message += $". Your diligent work strengthened your connection! (+1 {tokenType} token)";
        }

        _messageSystem.AddSystemMessage(message, SystemMessageTypes.Success);

        return CommandResult.Success(
            "Work completed successfully",
            new
            {
                NPCName = npc.Name,
                CoinsEarned = coinsEarned,
                BaseCoins = reward.BaseCoins,
                BonusCoins = reward.BonusCoins,
                StaminaSpent = staminaSpent,
                TimeCost = hoursSpent,
                EarnedToken = earnedToken,
                TokenType = tokenType
            }
        );
    }

    private WorkReward GetWorkReward(NPC npc)
    {
        // Check for special rewards first
        if (_gameConfiguration.WorkRewards.SpecialRewards.ContainsKey(npc.ID))
        {
            return _gameConfiguration.WorkRewards.SpecialRewards[npc.ID];
        }

        // Check by profession
        if (_gameConfiguration.WorkRewards.ByProfession.ContainsKey(npc.Profession))
        {
            return _gameConfiguration.WorkRewards.ByProfession[npc.Profession];
        }

        // Return default reward
        return _gameConfiguration.WorkRewards.DefaultReward;
    }
}