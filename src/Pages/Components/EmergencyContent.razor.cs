using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Emergency situation screen component that handles urgent situations demanding immediate response.
    /// Interrupts normal gameplay at sync points with time-sensitive decisions.
    ///
    /// ARCHITECTURAL PRINCIPLES:
    /// - Follows the authoritative parent pattern - receives GameScreen via CascadingParameter
    /// - EmergencyContext passed as Parameter from parent (created by GameOrchestrator)
    /// - All game state mutations go through GameOrchestrator
    /// - Time pressure mechanics with consequences for ignoring
    /// </summary>
    public class EmergencyContentBase : ComponentBase
    {
        [Parameter] public EmergencyContext Context { get; set; }
        [Parameter] public EventCallback OnEmergencyEnd { get; set; }
        [CascadingParameter] protected GameScreenBase GameScreen { get; set; }

        [Inject] protected GameOrchestrator GameOrchestrator { get; set; }

        protected async Task HandleSelectResponse(EmergencyResponse response)
        {
            if (Context == null || !Context.IsValid) return;

            EmergencyResult result = GameOrchestrator.SelectEmergencyResponse(Context.EmergencyState, response);

            if (!result.Success)
            {
                // Show error message through GameOrchestrator message system
                GameOrchestrator.GetMessageSystem().AddSystemMessage(
                    result.Message ?? "Failed to select response",
                    SystemMessageTypes.Danger);
                return;
            }

            // Show result narrative
            if (!string.IsNullOrEmpty(result.Message))
            {
                GameOrchestrator.GetMessageSystem().AddSystemMessage(
                    result.Message,
                    SystemMessageTypes.Info);
            }

            // Emergency resolved, return to location
            await OnEmergencyEnd.InvokeAsync();
        }

        protected async Task HandleIgnore()
        {
            if (Context == null || !Context.IsValid) return;

            EmergencyResult result = GameOrchestrator.IgnoreEmergency(Context.EmergencyState);

            if (!result.Success)
            {
                GameOrchestrator.GetMessageSystem().AddSystemMessage(
                    result.Message ?? "Failed to ignore emergency",
                    SystemMessageTypes.Danger);
                return;
            }

            // Show result narrative
            if (!string.IsNullOrEmpty(result.Message))
            {
                GameOrchestrator.GetMessageSystem().AddSystemMessage(
                    result.Message,
                    SystemMessageTypes.Warning);
            }

            // Emergency resolved (ignored), return to location
            await OnEmergencyEnd.InvokeAsync();
        }

        protected async Task HandleReturn()
        {
            await OnEmergencyEnd.InvokeAsync();
        }
    }
}
