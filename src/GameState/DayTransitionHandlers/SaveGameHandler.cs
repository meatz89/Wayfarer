using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Wayfarer.GameState.DayTransitionHandlers;

/// <summary>
/// Handles automatic game saving during day transitions.
/// </summary>
public class SaveGameHandler : IDayTransitionHandler
{
    private readonly GameWorldManager _gameWorldManager;
    private readonly ILogger<SaveGameHandler> _logger;

    public int Priority => 900; // Late in the process, after all changes
    public bool IsCritical => false; // Save failures shouldn't block day transition
    public string Description => "Auto-save game state";

    public SaveGameHandler(
        GameWorldManager gameWorldManager,
        ILogger<SaveGameHandler> logger)
    {
        _gameWorldManager = gameWorldManager;
        _logger = logger;
    }

    public async Task<HandlerResult> ProcessDayTransition(DayTransitionContext context)
    {
        try
        {
            // Perform auto-save
            await Task.Run(() => _gameWorldManager.SaveGame());
            
            _logger.LogInformation("Auto-saved game state for day {Day}", context.NewDay);

            return new HandlerResult
            {
                HandlerName = nameof(SaveGameHandler),
                Success = true,
                Message = $"Auto-saved game state for day {context.NewDay}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to auto-save during day transition");
            
            // Don't block day transition for save failures
            return new HandlerResult
            {
                HandlerName = nameof(SaveGameHandler),
                Success = false,
                ErrorMessage = $"Auto-save failed: {ex.Message}",
                BlocksSubsequentHandlers = false
            };
        }
    }
}