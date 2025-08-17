using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates contextual environmental hints based on game state
/// These are the subtle peripheral awareness elements shown in conversations
/// </summary>
public class EnvironmentalHintSystem
{
    private readonly GameWorld _gameWorld;
    private readonly ITimeManager _timeManager;
    private readonly LetterQueueManager _letterQueueManager;
    private readonly Dictionary<string, List<EnvironmentalHint>> _locationHints;

    public EnvironmentalHintSystem(
        GameWorld gameWorld,
        ITimeManager timeManager,
        LetterQueueManager letterQueueManager)
    {
        _gameWorld = gameWorld;
        _timeManager = timeManager;
        _letterQueueManager = letterQueueManager;
        _locationHints = InitializeHints();
    }

    /// <summary>
    /// Get the highest priority environmental hint for the current location and state
    /// </summary>
    public string GetEnvironmentalHint(string locationId)
    {
        if (!_locationHints.ContainsKey(locationId))
            return null;

        List<EnvironmentalHint> hints = _locationHints[locationId];
        GameStateContext gameState = new GameStateContext
        {
            CurrentHour = _timeManager.GetCurrentTimeHours(),
            CurrentMinute = _timeManager.GetCurrentMinutes(),
            HasUrgentLetters = HasUrgentLetters(),
            QueuePressure = GetQueuePressure(),
            PlayerDebt = GetPlayerDebt()
        };

        // Evaluate all hints and find highest priority match
        EnvironmentalHint bestHint = null;
        int highestPriority = -1;

        foreach (EnvironmentalHint hint in hints)
        {
            if (hint.Condition(gameState) && hint.Priority > highestPriority)
            {
                bestHint = hint;
                highestPriority = hint.Priority;
            }
        }

        return bestHint?.Text;
    }

    /// <summary>
    /// Get deadline pressure text for peripheral display
    /// </summary>
    public string GetDeadlinePressure()
    {
        Letter? mostUrgent = _letterQueueManager.GetActiveLetters()
            .Where(l => l != null)
            .OrderBy(l => l.DeadlineInHours)
            .FirstOrDefault();

        if (mostUrgent == null) return null;

        if (mostUrgent.DeadlineInHours <= 0)
            return $"💀 {mostUrgent.RecipientName}: EXPIRED!";

        if (mostUrgent.DeadlineInHours <= 3)
        {
            int hours = mostUrgent.DeadlineInHours;
            int minutes = (int)((mostUrgent.DeadlineInHours - hours) * 60);
            return $"⚡ {mostUrgent.RecipientName}: {hours}h {minutes}m";
        }

        return null; // Don't show if not urgent
    }

    private Dictionary<string, List<EnvironmentalHint>> InitializeHints()
    {
        return new Dictionary<string, List<EnvironmentalHint>>
        {
            ["market_square"] = new List<EnvironmentalHint>
            {
                new EnvironmentalHint
                {
                    Text = "Guards shifting nervously...",
                    Priority = 8,
                    Condition = ctx => ctx.CurrentHour >= 14 && ctx.CurrentHour <= 18
                },
                new EnvironmentalHint
                {
                    Text = "Merchants packing up their stalls",
                    Priority = 6,
                    Condition = ctx => ctx.CurrentHour >= 17 && ctx.CurrentHour <= 19
                },
                new EnvironmentalHint
                {
                    Text = "Church bells tolling the hour",
                    Priority = 4,
                    Condition = ctx => ctx.CurrentMinute == 0
                },
                new EnvironmentalHint
                {
                    Text = "A courier hurries past, clutching letters",
                    Priority = 5,
                    Condition = ctx => ctx.QueuePressure > 0.7f
                }
            },
            ["noble_district"] = new List<EnvironmentalHint>
            {
                new EnvironmentalHint
                {
                    Text = "Carriages rattling over cobblestones",
                    Priority = 5,
                    Condition = ctx => ctx.CurrentHour >= 10 && ctx.CurrentHour <= 16
                },
                new EnvironmentalHint
                {
                    Text = "Servants gossiping by the gates",
                    Priority = 6,
                    Condition = ctx => ctx.CurrentHour >= 7 && ctx.CurrentHour <= 9
                },
                new EnvironmentalHint
                {
                    Text = "Windows lighting up as dusk falls",
                    Priority = 7,
                    Condition = ctx => ctx.CurrentHour >= 18 && ctx.CurrentHour <= 20
                }
            },
            ["merchant_row"] = new List<EnvironmentalHint>
            {
                new EnvironmentalHint
                {
                    Text = "Coins clinking in purses",
                    Priority = 4,
                    Condition = ctx => ctx.CurrentHour >= 9 && ctx.CurrentHour <= 17
                },
                new EnvironmentalHint
                {
                    Text = "Debt collectors making rounds",
                    Priority = 8,
                    Condition = ctx => ctx.PlayerDebt > 0 && ctx.CurrentHour >= 10
                },
                new EnvironmentalHint
                {
                    Text = "Fresh bread scent wafting from bakeries",
                    Priority = 3,
                    Condition = ctx => ctx.CurrentHour >= 6 && ctx.CurrentHour <= 8
                }
            },
            ["riverside"] = new List<EnvironmentalHint>
            {
                new EnvironmentalHint
                {
                    Text = "Fog rolling in from the water",
                    Priority = 6,
                    Condition = ctx => ctx.CurrentHour <= 8 || ctx.CurrentHour >= 19
                },
                new EnvironmentalHint
                {
                    Text = "Dock workers unloading cargo",
                    Priority = 5,
                    Condition = ctx => ctx.CurrentHour >= 6 && ctx.CurrentHour <= 18
                },
                new EnvironmentalHint
                {
                    Text = "Gulls crying overhead",
                    Priority = 3,
                    Condition = ctx => true // Always present
                }
            },
            ["city_gates"] = new List<EnvironmentalHint>
            {
                new EnvironmentalHint
                {
                    Text = "Travelers arriving with road dust",
                    Priority = 6,
                    Condition = ctx => ctx.CurrentHour >= 8 && ctx.CurrentHour <= 12
                },
                new EnvironmentalHint
                {
                    Text = "Gates preparing to close for night",
                    Priority = 9,
                    Condition = ctx => ctx.CurrentHour >= 20 && ctx.CurrentHour <= 21
                },
                new EnvironmentalHint
                {
                    Text = "Guards checking papers carefully",
                    Priority = 7,
                    Condition = ctx => ctx.HasUrgentLetters
                }
            },
            ["your_room"] = new List<EnvironmentalHint>
            {
                new EnvironmentalHint
                {
                    Text = "Letters piling on your desk",
                    Priority = 8,
                    Condition = ctx => ctx.QueuePressure > 0.5f
                },
                new EnvironmentalHint
                {
                    Text = "Morning light through the window",
                    Priority = 4,
                    Condition = ctx => ctx.CurrentHour >= 6 && ctx.CurrentHour <= 9
                },
                new EnvironmentalHint
                {
                    Text = "Candle burning low",
                    Priority = 6,
                    Condition = ctx => ctx.CurrentHour >= 20 || ctx.CurrentHour <= 5
                }
            }
        };
    }

    private bool HasUrgentLetters()
    {
        return _letterQueueManager.GetActiveLetters()
            .Any(l => l != null && l.DeadlineInHours <= 3);
    }

    private float GetQueuePressure()
    {
        Letter[] letters = _letterQueueManager.GetActiveLetters();
        return letters.Count(l => l != null) / 8.0f;
    }

    private int GetPlayerDebt()
    {
        // Sum all negative token balances
        Dictionary<ConnectionType, int> tokens = _gameWorld.GetPlayer().ConnectionTokens;
        int totalDebt = 0;

        foreach (int tokenValue in tokens.Values)
        {
            if (tokenValue < 0)
                totalDebt += Math.Abs(tokenValue);
        }

        return totalDebt;
    }
}

/// <summary>
/// Represents a single environmental hint with its display conditions
/// </summary>
public class EnvironmentalHint
{
    public string Text { get; set; }
    public int Priority { get; set; } // 0-10, higher = more important
    public Func<GameStateContext, bool> Condition { get; set; }
}

/// <summary>
/// Context for evaluating hint conditions
/// </summary>
public class GameStateContext
{
    public int CurrentHour { get; set; }
    public int CurrentMinute { get; set; }
    public bool HasUrgentLetters { get; set; }
    public float QueuePressure { get; set; }
    public int PlayerDebt { get; set; }
}