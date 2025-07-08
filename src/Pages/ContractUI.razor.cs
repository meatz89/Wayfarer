using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages
{
    public partial class ContractUIBase : ComponentBase
    {
        [Inject] public GameWorld GameWorld { get; set; }
        [Inject] public ContractRepository ContractRepository { get; set; }

        public List<Contract> AvailableContracts => ContractRepository.GetAvailableContracts(GameWorld.CurrentDay, GameWorld.CurrentTimeBlock);

        public void CompleteContract(Contract contract)
        {
            // Apply contract completion effects
            GameWorld.GetPlayer().ModifyCoins(contract.Payment);

            // If this contract unlocks others, make them available
            if (contract.UnlocksContractIds.Any())
            {
                foreach (string contractId in contract.UnlocksContractIds)
                {
                    Contract unlockedContract = ContractRepository.GetContract(contractId);
                    if (unlockedContract != null)
                    {
                        unlockedContract.StartDay = GameWorld.CurrentDay;
                        unlockedContract.DueDay = GameWorld.CurrentDay + 5; // Arbitrary due date
                    }
                }
            }

            // If this contract locks others, make them unavailable
            if (contract.LocksContractIds.Any())
            {
                foreach (string contractId in contract.LocksContractIds)
                {
                    Contract lockedContract = ContractRepository.GetContract(contractId);
                    if (lockedContract != null)
                    {
                        lockedContract.IsFailed = true;
                    }
                }
            }

            // Mark contract as completed
            contract.IsCompleted = true;

            // Refresh UI
            StateHasChanged();
        }
    }
}