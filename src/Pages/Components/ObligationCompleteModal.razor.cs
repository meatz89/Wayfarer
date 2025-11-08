using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    public class ObligationCompleteModalBase : ComponentBase
    {
        [Parameter] public ObligationCompleteResult Data { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        protected async Task CloseModal()
        {
            await OnClose.InvokeAsync();
        }
    }
}
