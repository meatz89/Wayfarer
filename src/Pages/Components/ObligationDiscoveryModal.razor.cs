using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    public class ObligationDiscoveryModalBase : ComponentBase
    {
        [Parameter] public ObligationDiscoveryResult Data { get; set; }
        [Parameter] public EventCallback OnBeginIntro { get; set; }
        [Parameter] public EventCallback OnDismiss { get; set; }

        protected async Task BeginIntroAction()
        {
            await OnBeginIntro.InvokeAsync();
        }

        protected async Task DismissModal()
        {
            await OnDismiss.InvokeAsync();
        }
    }
}
