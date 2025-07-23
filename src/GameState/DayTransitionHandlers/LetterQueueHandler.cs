using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Wayfarer.GameState.DayTransitionHandlers;

/// <summary>
/// Handles letter queue processing during day transitions.
/// </summary>
public class LetterQueueHandler : IDayTransitionHandler
{
    private readonly LetterQueueManager _letterQueueManager;
    private readonly StandingObligationManager _obligationManager;
    private readonly ILogger<LetterQueueHandler> _logger;

    public int Priority => 100; // Early in the process
    public bool IsCritical => true; // Letter processing is critical
    public string Description => "Process letter deadlines and generate new letters";

    public LetterQueueHandler(
        LetterQueueManager letterQueueManager,
        StandingObligationManager obligationManager,
        ILogger<LetterQueueHandler> logger)
    {
        _letterQueueManager = letterQueueManager;
        _obligationManager = obligationManager;
        _logger = logger;
    }

    public async Task<HandlerResult> ProcessDayTransition(DayTransitionContext context)
    {
        try
        {
            // Process letter deadline countdown
            _letterQueueManager.ProcessDailyDeadlines();
            _logger.LogDebug("Processed letter deadlines for day {Day}", context.NewDay);

            // Process standing obligations and generate forced letters
            var forcedLetters = _obligationManager.ProcessDailyObligations(context.NewDay);
            _logger.LogDebug("Generated {Count} forced letters from obligations", forcedLetters.Count);

            // Add forced letters to the queue with obligation effects
            foreach (var letter in forcedLetters)
            {
                _letterQueueManager.AddLetterWithObligationEffects(letter);
            }

            // Advance obligation time tracking
            _obligationManager.AdvanceDailyTime();

            // Generate new letters for the day
            _letterQueueManager.GenerateDailyLetters();
            _logger.LogDebug("Generated daily letters");

            return new HandlerResult
            {
                HandlerName = nameof(LetterQueueHandler),
                Success = true,
                Message = $"Processed {forcedLetters.Count} obligation letters and generated daily letters",
                OutputData = new()
                {
                    ["forced_letter_count"] = forcedLetters.Count,
                    ["total_letters_in_queue"] = _letterQueueManager.GetAllLetters().Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process letter queue during day transition");
            return new HandlerResult
            {
                HandlerName = nameof(LetterQueueHandler),
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}