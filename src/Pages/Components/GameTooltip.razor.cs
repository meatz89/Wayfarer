using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    public partial class GameTooltip : ComponentBase, IDisposable
    {
        [Parameter] public string Content { get; set; } = "";
        [Parameter] public RenderFragment? ContentFragment { get; set; }
        [Parameter] public RenderFragment ChildContent { get; set; } = null!;
        [Parameter] public string Position { get; set; } = "top"; // top, bottom, left, right
        [Parameter] public int Delay { get; set; } = 300; // milliseconds
        [Parameter] public string CssClass { get; set; } = "";

        private bool IsVisible { get; set; }
        private Timer? _showTimer;
        private Timer? _hideTimer;

        private void ShowTooltip()
        {
            _hideTimer?.Dispose();
            _hideTimer = null;

            if (_showTimer != null)
            {
                return; // Already showing or about to show
            }

            _showTimer = new Timer((_) =>
            {
                InvokeAsync(() =>
                {
                    IsVisible = true;
                    StateHasChanged();
                });
                _showTimer?.Dispose();
                _showTimer = null;
            }, null, Delay, Timeout.Infinite);
        }

        private void HideTooltip()
        {
            _showTimer?.Dispose();
            _showTimer = null;

            if (_hideTimer != null)
            {
                return; // Already hiding
            }

            _hideTimer = new Timer((_) =>
            {
                InvokeAsync(() =>
                {
                    IsVisible = false;
                    StateHasChanged();
                });
                _hideTimer?.Dispose();
                _hideTimer = null;
            }, null, 100, Timeout.Infinite); // Small delay for hide to prevent flicker
        }

        private string GetPositionClass()
        {
            return $"tooltip-{Position.ToLower()} {CssClass}".Trim();
        }

        private string GetPositionStyle()
        {
            // Additional positioning logic can be added here if needed
            return "";
        }

        public void Dispose()
        {
            _showTimer?.Dispose();
            _hideTimer?.Dispose();
        }
    }
}