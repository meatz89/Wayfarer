using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Orchestrates all day transition activities in a deterministic, ordered manner.
/// Ensures all systems are properly notified and updated when a new day begins.
/// </summary>
public class DayTransitionOrchestrator
{
    private readonly List<IDayTransitionHandler> _handlers;
    private readonly ILogger<DayTransitionOrchestrator> _logger;
    private readonly TimeModel _timeModel;

    public DayTransitionOrchestrator(
        IEnumerable<IDayTransitionHandler> handlers,
        TimeModel timeModel,
        ILogger<DayTransitionOrchestrator> logger)
    {
        _handlers = handlers.OrderBy(h => h.Priority).ToList();
        _timeModel = timeModel ?? throw new ArgumentNullException(nameof(timeModel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes a new day transition, executing all handlers in priority order.
    /// </summary>
    public async Task<DayTransitionResult> ProcessNewDay()
    {
        DateTime startTime = DateTime.UtcNow;
        List<HandlerResult> results = new List<HandlerResult>();
        DayTransitionContext context = new DayTransitionContext
        {
            NewDay = _timeModel.CurrentDay,
            PreviousDay = _timeModel.CurrentDay - 1,
            StartingHour = _timeModel.CurrentHour,
            SharedData = new Dictionary<string, object>()
        };

        _logger.LogInformation("Starting day transition for day {NewDay}", context.NewDay);

        foreach (IDayTransitionHandler handler in _handlers)
        {
            string handlerName = handler.GetType().Name;

            try
            {
                _logger.LogDebug("Executing handler {HandlerName} with priority {Priority}",
                    handlerName, handler.Priority);

                HandlerResult result = await handler.ProcessDayTransition(context);
                results.Add(result);

                // Add handler output to shared context for subsequent handlers
                if (result.OutputData != null)
                {
                    foreach (KeyValuePair<string, object> kvp in result.OutputData)
                    {
                        context.SharedData[kvp.Key] = kvp.Value;
                    }
                }

                _logger.LogDebug("Handler {HandlerName} completed: {Success}",
                    handlerName, result.IsSuccess);

                // Stop processing if handler blocks subsequent handlers
                if (result.BlocksSubsequentHandlers)
                {
                    _logger.LogWarning("Handler {HandlerName} blocked subsequent handlers", handlerName);
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handler {HandlerName} failed during day transition", handlerName);

                results.Add(new HandlerResult
                {
                    HandlerName = handlerName,
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    BlocksSubsequentHandlers = false
                });

                // Continue with other handlers unless critical
                if (handler.IsCritical)
                {
                    _logger.LogError("Critical handler {HandlerName} failed, stopping day transition", handlerName);
                    break;
                }
            }
        }

        TimeSpan duration = DateTime.UtcNow - startTime;
        _logger.LogInformation("Day transition completed in {Duration}ms with {SuccessCount}/{TotalCount} successful handlers",
            duration.TotalMilliseconds, results.Count(r => r.IsSuccess), results.Count);

        return new DayTransitionResult(results, duration);
    }

    /// <summary>
    /// Registers a new handler dynamically.
    /// </summary>
    public void RegisterHandler(IDayTransitionHandler handler)
    {
        _handlers.Add(handler);
        _handlers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        _logger.LogInformation("Registered new day transition handler: {HandlerType}", handler.GetType().Name);
    }

    /// <summary>
    /// Gets information about registered handlers.
    /// </summary>
    public IReadOnlyList<DayTransitionHandlerInfo> GetHandlerInfo()
    {
        return _handlers.Select(h => new DayTransitionHandlerInfo
        {
            HandlerType = h.GetType().Name,
            Priority = h.Priority,
            IsCritical = h.IsCritical,
            Description = h.Description
        }).ToList();
    }
}

/// <summary>
/// Interface for components that need to process day transitions.
/// </summary>
public interface IDayTransitionHandler
{
    /// <summary>
    /// Priority for execution order (lower = earlier).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Whether failure of this handler should stop the day transition.
    /// </summary>
    bool IsCritical { get; }

    /// <summary>
    /// Human-readable description of what this handler does.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Processes the day transition.
    /// </summary>
    Task<HandlerResult> ProcessDayTransition(DayTransitionContext context);
}

/// <summary>
/// Context passed to day transition handlers.
/// </summary>
public class DayTransitionContext
{
    public int PreviousDay { get; init; }
    public int NewDay { get; init; }
    public int StartingHour { get; init; }
    public Dictionary<string, object> SharedData { get; init; }
}

/// <summary>
/// Result from a day transition handler.
/// </summary>
public class HandlerResult
{
    public string HandlerName { get; init; }
    public bool IsSuccess { get; init; }
    public string Message { get; init; }
    public string ErrorMessage { get; init; }
    public bool BlocksSubsequentHandlers { get; init; }
    public Dictionary<string, object> OutputData { get; init; }
}

/// <summary>
/// Overall result of a day transition.
/// </summary>
public class DayTransitionResult
{
    public IReadOnlyList<HandlerResult> HandlerResults { get; }
    public TimeSpan Duration { get; }
    public bool AllSuccessful { get; }
    public int SuccessfulCount { get; }
    public int FailedCount { get; }

    public DayTransitionResult(List<HandlerResult> results, TimeSpan duration)
    {
        HandlerResults = results;
        Duration = duration;
        SuccessfulCount = results.Count(r => r.IsSuccess);
        FailedCount = results.Count(r => !r.IsSuccess);
        AllSuccessful = FailedCount == 0;
    }

    public string GetSummary()
    {
        string summary = $"Day transition completed in {Duration.TotalMilliseconds:F0}ms\n";
        summary += $"Handlers: {SuccessfulCount} successful, {FailedCount} failed\n";

        if (FailedCount > 0)
        {
            summary += "Failed handlers:\n";
            foreach (HandlerResult? failed in HandlerResults.Where(r => !r.IsSuccess))
            {
                summary += $"  - {failed.HandlerName}: {failed.ErrorMessage}\n";
            }
        }

        return summary;
    }
}

/// <summary>
/// Information about a registered day transition handler.
/// </summary>
public class DayTransitionHandlerInfo
{
    public string HandlerType { get; init; }
    public int Priority { get; init; }
    public bool IsCritical { get; init; }
    public string Description { get; init; }
}