using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    public class ObligationActivationModalBase : ComponentBase
    {
        [Parameter] public ObligationActivationResult Data { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        protected async Task CloseModal()
        {
            await OnClose.InvokeAsync();
        }
    }
}
