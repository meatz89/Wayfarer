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


    public WorkCommand(
        string npcId,
        NPCRepository npcRepository,
        IGameRuleEngine ruleEngine,
        GameConfiguration gameConfiguration,
        MessageSystem messageSystem)
    {
        _npcId = npcId ?? throw new ArgumentNullException(nameof(npcId));
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
        _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
        _gameConfiguration = gameConfiguration ?? throw new ArgumentNullException(nameof(gameConfiguration));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));

        Description = $"Work for NPC {npcId}";
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
        TimeBlocks currentTime = gameWorld.TimeManager.GetCurrentTimeBlock();
        if (!npc.IsAvailableAtTime(player.CurrentLocationSpot.SpotID, currentTime))
        {
            return CommandValidationResult.Failure(
                $"{npc.Name} is not available at this time",
                true,
                "Try visiting at a different time");
        }

        // Check resource costs (1 hour, 1 stamina)
        if (!gameWorld.TimeManager.CanPerformAction(1))
        {
            return CommandValidationResult.Failure(
                "Not enough time remaining",
                true,
                "Rest or wait until tomorrow");
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

        // Get work reward from configuration based on NPC profession
        WorkReward reward = GetWorkReward(npc);

        // Apply costs
        int hoursSpent = 1;
        int staminaSpent = 1;
        gameWorld.TimeManager.SpendHours(hoursSpent);
        player.ModifyStamina(-staminaSpent);

        // Apply rewards
        int coinsEarned = reward.BaseCoins + reward.BonusCoins;
        player.ModifyCoins(coinsEarned);

        // Success message
        string message = $"Worked for {npc.Name} and earned {coinsEarned} coins";
        if (reward.BonusCoins > 0)
        {
            message += $" (including {reward.BonusCoins} bonus)";
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
                HoursSpent = hoursSpent
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