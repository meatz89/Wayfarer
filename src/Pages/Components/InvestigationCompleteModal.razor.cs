using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Wayfarer.Pages.Components
{
    public class InvestigationCompleteModalBase : ComponentBase
    {
        [Parameter] public InvestigationCompleteResult Data { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        protected async Task CloseModal()
        {
            await OnClose.InvokeAsync();
        }
    }
}
