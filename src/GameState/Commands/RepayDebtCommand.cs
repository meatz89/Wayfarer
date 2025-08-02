using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Command to repay debt to an NPC
/// </summary>
public class RepayDebtCommand : BaseGameCommand
{
    private readonly string _npcId;
    private readonly int _amount;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;

    public RepayDebtCommand(
        string npcId,
        int amount,
        NPCRepository npcRepository,
        MessageSystem messageSystem)
    {
        _npcId = npcId ?? throw new ArgumentNullException(nameof(npcId));
        _amount = amount;
        _npcRepository = npcRepository ?? throw new ArgumentNullException(nameof(npcRepository));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));

        Description = $"Repay {amount} coins to {npcId}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        var player = gameWorld.GetPlayer();
        
        // Check if player has debt with this NPC
        var debt = player.ActiveDebts.FirstOrDefault(d => d.CreditorId == _npcId && !d.IsPaid);
        if (debt == null)
        {
            return CommandValidationResult.Failure("No debt with this NPC");
        }
        
        // Check if player has enough coins
        if (player.Coins < _amount)
        {
            return CommandValidationResult.Failure(
                $"Not enough coins (need {_amount}, have {player.Coins})",
                true,
                "Earn more coins through work or deliveries");
        }
        
        // Check if NPC is at current location
        var npc = _npcRepository.GetById(_npcId);
        if (npc == null)
        {
            return CommandValidationResult.Failure("NPC not found");
        }
        
        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        var player = gameWorld.GetPlayer();
        var npc = _npcRepository.GetById(_npcId);
        var debt = player.ActiveDebts.FirstOrDefault(d => d.CreditorId == _npcId && !d.IsPaid);
        
        if (debt == null)
        {
            return CommandResult.Failure("Debt not found");
        }
        
        // Calculate total owed
        int totalOwed = debt.GetTotalOwed(gameWorld.CurrentDay);
        
        // Deduct payment from player
        player.ModifyCoins(-_amount);
        
        if (_amount >= totalOwed)
        {
            // Full payment
            debt.IsPaid = true;
            int overpayment = _amount - totalOwed;
            
            if (overpayment > 0)
            {
                player.ModifyCoins(overpayment);
                _messageSystem.AddSystemMessage(
                    $"💰 Debt to {npc.Name} fully repaid! Returned {overpayment} coins overpayment.",
                    SystemMessageTypes.Success
                );
            }
            else
            {
                _messageSystem.AddSystemMessage(
                    $"💰 Debt to {npc.Name} fully repaid!",
                    SystemMessageTypes.Success
                );
            }
        }
        else
        {
            // Partial payment - reduce principal
            int remainingPayment = _amount;
            int accruedInterest = totalOwed - debt.Principal;
            
            // Pay interest first
            if (remainingPayment >= accruedInterest)
            {
                remainingPayment -= accruedInterest;
                debt.Principal -= remainingPayment;
                
                _messageSystem.AddSystemMessage(
                    $"💰 Paid {_amount} coins to {npc.Name}. Remaining debt: {debt.Principal} coins.",
                    SystemMessageTypes.Info
                );
            }
            else
            {
                // Only paid part of interest
                _messageSystem.AddSystemMessage(
                    $"💰 Paid {_amount} coins toward interest. Total debt still: {totalOwed - _amount} coins.",
                    SystemMessageTypes.Warning
                );
            }
        }
        
        return CommandResult.Success(
            "Payment processed",
            new
            {
                NPCName = npc.Name,
                AmountPaid = _amount,
                RemainingDebt = debt.IsPaid ? 0 : debt.GetTotalOwed(gameWorld.CurrentDay),
                DebtCleared = debt.IsPaid
            }
        );
    }
}