using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Observation scene screen component that handles scene investigation with multiple examination points.
    /// Player has limited resources and must prioritize what to examine.
    ///
    /// ARCHITECTURAL PRINCIPLES:
    /// - Follows the authoritative parent pattern - receives GameScreen via CascadingParameter
    /// - ObservationContext passed as Parameter from parent (created by GameOrchestrator)
    /// - All game state mutations go through GameOrchestrator
    /// - Strategic resource management with progressive revelation mechanics
    /// </summary>
    public class ObservationContentBase : ComponentBase
    {
        [Parameter] public ObservationContext Context { get; set; }
        [Parameter] public EventCallback OnObservationEnd { get; set; }
        [CascadingParameter] protected GameScreenBase GameScreen { get; set; }

        [Inject] protected GameOrchestrator GameOrchestrator { get; set; }

        protected async Task HandleExaminePoint(ExaminationPoint point)
        {
            if (Context == null || !Context.IsValid) return;

            ObservationResult result = await GameOrchestrator.ExaminePoint(Context.Scene, point);

            if (!result.Success)
            {
                // Show error message through GameOrchestrator message system
                GameOrchestrator.GetMessageSystem().AddSystemMessage(
                    result.Message ?? "Failed to examine point",
                    SystemMessageTypes.Danger);
                return;
            }

            // Refresh context to reflect changes
            Context = GameOrchestrator.CreateObservationContext(Context.Scene);

            // Show results through message system (knowledge gained, items found, etc. are already shown by facade)
            if (result.SceneCompleted)
            {
                GameOrchestrator.GetMessageSystem().AddSystemMessage(
                    "Scene investigation complete!",
                    SystemMessageTypes.Success);
            }

            StateHasChanged();
        }

        protected async Task HandleReturn()
        {
            await OnObservationEnd.InvokeAsync();
        }
    }
}
