using Microsoft.AspNetCore.Components;
using System;

namespace Wayfarer.Pages.Components.Shared
{
    public class ResourceBarBase : ComponentBase
    {
        [Parameter] public string Label { get; set; }
        [Parameter] public int CurrentValue { get; set; }
        [Parameter] public int MaxValue { get; set; }
        [Parameter] public string BarType { get; set; } = "normal"; // normal, progress, consequence
        [Parameter] public string TooltipContent { get; set; }
        [Parameter] public bool ShowMaxValue { get; set; } = false;

        protected string BarTypeClass => string.IsNullOrEmpty(BarType) || BarType == "normal" ? "" : $"{BarType}-fill";

        protected double PercentageWidth
        {
            get
            {
                if (MaxValue <= 0) return 0;
                return Math.Min(100, (CurrentValue / (double)MaxValue) * 100);
            }
        }
    }
}
