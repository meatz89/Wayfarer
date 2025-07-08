using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages
{
    public partial class ContractUIBase : ComponentBase
    {
        [Inject] public GameWorld GameWorld { get; set; }
        [Inject] public GameWorldManager GameWorldManager { get; set; }
        // ✅ ARCHITECTURAL COMPLIANCE: ContractRepository allowed for read-only UI data binding
        [Inject] public ContractRepository ContractRepository { get; set; }

        public List<Contract> AvailableContracts => ContractRepository.GetAvailableContracts(GameWorld.CurrentDay, GameWorld.CurrentTimeBlock);

        public void CompleteContract(Contract contract)
        {
            // ✅ ARCHITECTURAL COMPLIANCE: Route through GameWorldManager gateway
            GameWorldManager.CompleteContract(contract);

            // Refresh UI
            StateHasChanged();
        }
    }
}