using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Wayfarer.Pages
{
    public partial class LoadingIndicator : ComponentBase
    {
        [Inject] private IJSRuntime JSRuntime { get; set; }

        [Parameter] public bool IsVisible { get; set; }
        [Parameter] public string Message { get; set; }
        [Parameter] public bool ShowProgress { get; set; }
        [Parameter] public int Progress { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (IsVisible)
            {
                await JSRuntime.InvokeVoidAsync("showLoadingAnimation");
            }
        }
    }
}