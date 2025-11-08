using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    public class ObligationProgressModalBase : ComponentBase
    {
        [Parameter] public ObligationProgressResult Data { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        protected async Task CloseModal()
        {
            await OnClose.InvokeAsync();
        }

        protected double GetProgressPercent()
        {
            if (Data.TotalSituationCount == 0) return 0;
            return ((double)Data.CompletedSituationCount / Data.TotalSituationCount) * 100.0;
        }
    }
}
