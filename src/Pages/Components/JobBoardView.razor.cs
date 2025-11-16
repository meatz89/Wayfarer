using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Job Board View - Shows available delivery jobs at Commercial locations.
    /// Players can accept ONE active delivery job at a time.
    /// </summary>
    public class JobBoardViewBase : ComponentBase
    {
        [Inject] protected GameWorld GameWorld { get; set; }
        [Inject] protected TimeManager TimeManager { get; set; }

        [Parameter] public Location CurrentLocation { get; set; }
        [Parameter] public EventCallback<string> OnAcceptJob { get; set; }
        [Parameter] public EventCallback OnNavigateBack { get; set; }

        protected List<DeliveryJob> AvailableJobs { get; set; } = new();

        protected override void OnParametersSet()
        {
            // Get jobs available at current location
            if (CurrentLocation != null)
            {
                TimeBlocks currentTime = TimeManager.GetCurrentTimeBlock();
                AvailableJobs = GameWorld.GetJobsAvailableAt(CurrentLocation, currentTime);
            }
        }

        protected async Task HandleAcceptJob(DeliveryJob job)
        {
            await OnAcceptJob.InvokeAsync(job.Id);
        }

        protected string GetDifficultyClass(DifficultyTier tier)
        {
            return tier switch
            {
                DifficultyTier.Simple => "difficulty-simple",
                DifficultyTier.Moderate => "difficulty-moderate",
                DifficultyTier.Dangerous => "difficulty-dangerous",
                _ => ""
            };
        }
    }
}
