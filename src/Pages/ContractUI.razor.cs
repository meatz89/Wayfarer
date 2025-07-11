using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages
{
    public class ContractUIBase : ComponentBase
    {
        [Inject] public GameWorld GameWorld { get; set; }
        [Inject] public GameWorldManager GameWorldManager { get; set; }
        [Inject] public ContractRepository ContractRepository { get; set; }

        public List<Contract> AvailableContracts
        {
            get
            {
                return ContractRepository.GetAvailableContracts(GameWorld.CurrentDay, GameWorld.CurrentTimeBlock);
            }
        }

        public void CompleteContract(Contract contract)
        {
            GameWorldManager.CompleteContract(contract);

            // Refresh UI
            StateHasChanged();
        }
    }
}