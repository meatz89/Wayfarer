using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    public class InvestigationProgressModalBase : ComponentBase
    {
        [Parameter] public InvestigationProgressResult Data { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        protected async Task CloseModal()
        {
            await OnClose.InvokeAsync();
        }

        protected double GetProgressPercent()
        {
            if (Data.TotalGoalCount == 0) return 0;
            return ((double)Data.CompletedGoalCount / Data.TotalGoalCount) * 100.0;
        }
    }
}
