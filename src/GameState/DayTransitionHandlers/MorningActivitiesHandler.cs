using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Wayfarer.GameState.DayTransitionHandlers;

/// <summary>
/// Handles morning activities during day transitions.
/// </summary>
public class MorningActivitiesHandler : IDayTransitionHandler
{
    private readonly MorningActivitiesManager _morningActivitiesManager;
    private readonly ILogger<MorningActivitiesHandler> _logger;

    public int Priority => 200; // After letter processing
    public bool IsCritical => false; // Morning activities are optional
    public string Description => "Process morning activities and events";

    public MorningActivitiesHandler(
        MorningActivitiesManager morningActivitiesManager,
        ILogger<MorningActivitiesHandler> logger)
    {
        _morningActivitiesManager = morningActivitiesManager;
        _logger = logger;
    }

    public async Task<HandlerResult> ProcessDayTransition(DayTransitionContext context)
    {
        try
        {
            // Process morning activities
            var morningResult = _morningActivitiesManager.ProcessMorningActivities();
            
            _logger.LogDebug("Processed morning activities: {EventCount} events", 
                morningResult?.Events?.Count ?? 0);

            return new HandlerResult
            {
                HandlerName = nameof(MorningActivitiesHandler),
                Success = true,
                Message = morningResult?.HasEvents == true 
                    ? $"Processed {morningResult.Events.Count} morning events" 
                    : "No morning events",
                OutputData = new()
                {
                    ["morning_result"] = morningResult,
                    ["has_events"] = morningResult?.HasEvents ?? false
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process morning activities during day transition");
            return new HandlerResult
            {
                HandlerName = nameof(MorningActivitiesHandler),
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}