using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components.Shared
{
    public class ActionEconomyDisplayBase : ComponentBase
    {
        [Parameter] public string Label { get; set; }
        [Parameter] public int CurrentValue { get; set; }
        [Parameter] public int MaxValue { get; set; }
        [Parameter] public string TooltipContent { get; set; }
    }
}
