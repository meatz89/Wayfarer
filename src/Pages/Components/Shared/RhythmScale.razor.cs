using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components.Shared
{
    public class RhythmScaleBase : ComponentBase
    {
        [Parameter] public string LeftLabel { get; set; }
        [Parameter] public string RightLabel { get; set; }
        [Parameter] public int CurrentValue { get; set; }
        [Parameter] public int MinValue { get; set; } = -5;
        [Parameter] public int MaxValue { get; set; } = 5;

        protected string GetSegmentClass(int segmentValue)
        {
            // Simple logic: <0 negative, =0 neutral, >0 positive
            if (segmentValue < 0) return "negative";
            if (segmentValue == 0) return "neutral";
            return "positive";
        }
    }
}
