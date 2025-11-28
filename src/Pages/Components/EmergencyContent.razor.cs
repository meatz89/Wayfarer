using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Emergency situation screen component that handles urgent situations demanding immediate response.
    /// Interrupts normal gameplay at sync points with time-sensitive decisions.
    ///
    /// ARCHITECTURAL PRINCIPLES:
    /// - Follows the authoritative parent pattern - receives GameScreen via CascadingParameter
    /// - EmergencyContext passed as Parameter from parent (created by GameFacade)
    /// - All game state mutations go through GameFacade
    /// - Time pressure mechanics with consequences for ignoring
    /// </summary>
    public class EmergencyContentBase : ComponentBase
    {
        [Parameter] public EmergencyContext Context { get; set; }
        [Parameter] public EventCallback OnEmergencyEnd { get; set; }
        [CascadingParameter] protected GameScreenBase GameScreen { get; set; }

        [Inject] protected GameFacade GameFacade { get; set; }

        protected async Task HandleSelectResponse(EmergencyResponse response)
        {
            if (Context == null || !Context.IsValid) return;

            EmergencyResult result = await GameFacade.SelectEmergencyResponse(Context.Emergency, response);

            if (!result.Success)
            {
                // Show error message through GameFacade message system
                GameFacade.GetMessageSystem().AddSystemMessage(
                    result.Message ?? "Failed to select response",
                    SystemMessageTypes.Danger);
                return;
            }

            // Show result narrative
            if (!string.IsNullOrEmpty(result.Message))
            {
                GameFacade.GetMessageSystem().AddSystemMessage(
                    result.Message,
                    SystemMessageTypes.Info);
            }

            // Emergency resolved, return to location
            await OnEmergencyEnd.InvokeAsync();
        }

        protected async Task HandleIgnore()
        {
            if (Context == null || !Context.IsValid) return;

            EmergencyResult result = await GameFacade.IgnoreEmergency(Context.Emergency);

            if (!result.Success)
            {
                GameFacade.GetMessageSystem().AddSystemMessage(
                    result.Message ?? "Failed to ignore emergency",
                    SystemMessageTypes.Danger);
                return;
            }

            // Show result narrative
            if (!string.IsNullOrEmpty(result.Message))
            {
                GameFacade.GetMessageSystem().AddSystemMessage(
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
