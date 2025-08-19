using System;
using System.Collections.Generic;
using System.Linq;
public partial class DailyActivitiesManager
{
    private readonly GameWorld _gameWorld;
    private readonly LetterQueueManager _letterQueueManager;
    private readonly StandingObligationManager _obligationManager;
    private readonly MessageSystem _messageSystem;
    private readonly PatronLetterService _patronLetterService;
    private readonly ITimeManager _timeManager;

    // Track daily events for display
    public List<DailyEvent> DailyEvents { get; private set; } = new List<DailyEvent>();

    // Store last activity result
    private DailyActivityResult _lastActivityResult;

    public DailyActivitiesManager(
        GameWorld gameWorld,
        LetterQueueManager letterQueueManager,
        StandingObligationManager obligationManager,
        MessageSystem messageSystem,
        ITimeManager timeManager,
        PatronLetterService patronLetterService = null)
    {
        _gameWorld = gameWorld;
        _letterQueueManager = letterQueueManager;
        _obligationManager = obligationManager;
        _messageSystem = messageSystem;
        _timeManager = timeManager;
        _patronLetterService = patronLetterService;
    }

    // Process all daily activities and collect events
    public DailyActivityResult ProcessDailyActivities()
    {
        DailyEvents.Clear();
        DailyActivityResult result = new DailyActivityResult();

        // Announce daily activities processing
        _messageSystem.AddSystemMessage(
            "📜 Processing daily obligations and activities:",
            SystemMessageTypes.Info
        );

        // 1. Track letters that will expire
        Letter[] expiringLetters = GetLettersAboutToExpire();
        foreach (Letter letter in expiringLetters)
        {
            DailyEvents.Add(new DailyEvent
            {
                Type = DailyEventType.LetterExpired,
                Description = GetExpiredLetterNarrative(letter),
                TokenLoss = 2,
                TokenType = letter.TokenType,
                SenderName = letter.SenderName
            });
            result.ExpiredLetterCount++;
        }

        // 2. Process hourly deadlines (this actually removes expired letters)
        // Morning activities don't automatically process deadlines anymore
        // Deadlines are processed when time actually advances via ProcessTimeAdvancement

        // 3. Process obligations and get forced letters
        List<Letter> forcedLetters = _obligationManager.ProcessDailyObligations(_gameWorld.CurrentDay);
        foreach (Letter letter in forcedLetters)
        {
            int position = _letterQueueManager.AddLetterWithObligationEffects(letter);
            if (position > 0)
            {
                DailyEvents.Add(new DailyEvent
                {
                    Type = DailyEventType.ForcedLetterAdded,
                    Description = GetObligationLetterNarrative(letter, position),
                    LetterPosition = position,
                    SenderName = letter.SenderName
                });
                result.ForcedLetterCount++;
            }
        }

        // 4. Advance obligation time
        _obligationManager.AdvanceDailyTime();

        // 5. Check for patron letters
        if (_patronLetterService != null)
        {
            Letter patronLetter = _patronLetterService.CheckForPatronLetter();
            if (patronLetter != null)
            {
                // Add patron letter to queue - extreme debt gives it leverage
                int position = _letterQueueManager.AddLetterWithObligationEffects(patronLetter);

                if (position > 0)
                {
                    DailyEvents.Add(new DailyEvent
                    {
                        Type = DailyEventType.PatronLetterAdded,
                        Description = GetPatronLetterNarrative(patronLetter, position),
                        LetterPosition = position,
                        SenderName = "Your Patron"
                    });
                    result.PatronLetterCount = 1;
                }
            }
        }

        // 6. Generate regular daily letters
        int newLetterCount = _letterQueueManager.GenerateDailyLetters();
        if (newLetterCount > 0)
        {
            DailyEvents.Add(new DailyEvent
            {
                Type = DailyEventType.NewLettersAvailable,
                Description = GetNewLettersNarrative(newLetterCount)
            });
            result.NewLetterCount = newLetterCount;
        }

        // 7. Check for urgent letters needing attention
        Letter[] urgentLetters = _letterQueueManager.GetExpiringLetters(2);
        foreach (Letter letter in urgentLetters)
        {
            DailyEvents.Add(new DailyEvent
            {
                Type = DailyEventType.UrgentLetterWarning,
                Description = GetUrgentLetterNarrative(letter),
                LetterPosition = letter.QueuePosition
            });
            result.UrgentLetterCount++;
        }

        // Display daily summary
        DisplayDailySummary(result);

        // Store result for later retrieval
        _lastActivityResult = result;

        return result;
    }

    // Get letters that are about to expire (deadline = 0)
    private Letter[] GetLettersAboutToExpire()
    {
        return _gameWorld.GetPlayer().LetterQueue
            .Where(l => l != null && l.DeadlineInHours == 0)
            .ToArray();
    }

    // Check if it's time for daily activities
    public bool IsDailyActivityTime()
    {
        return _timeManager.GetCurrentTimeBlock() == TimeBlocks.Dawn;
    }

    // Clear daily events after display
    public void ClearDailyEvents()
    {
        DailyEvents.Clear();
    }

    // Display daily summary
    private void DisplayDailySummary(DailyActivityResult result)
    {
        if (result.ExpiredLetterCount > 0)
        {
            _messageSystem.AddSystemMessage(
                $"⏰ {result.ExpiredLetterCount} letter{(result.ExpiredLetterCount > 1 ? "s" : "")} expired overnight",
                SystemMessageTypes.Danger
            );
        }

        if (result.UrgentLetterCount > 0)
        {
            _messageSystem.AddSystemMessage(
                $"⚠️ {result.UrgentLetterCount} letter{(result.UrgentLetterCount > 1 ? "s need" : " needs")} urgent attention today!",
                SystemMessageTypes.Warning
            );
        }

        int queueStatus = _letterQueueManager.GetLetterCount();
        int maxQueue = 8; // Assuming max queue size is 8

        if (queueStatus >= maxQueue - 1)
        {
            _messageSystem.AddSystemMessage(
                $"📭 Your letter queue is nearly full ({queueStatus}/{maxQueue})",
                SystemMessageTypes.Warning
            );
        }

        _messageSystem.AddSystemMessage(
            "☀️ The day awaits your actions",
            SystemMessageTypes.Info
        );
    }

    // Narrative generation methods
    private string GetExpiredLetterNarrative(Letter letter)
    {
        string[] narratives = new string[]
        {
            $"The seal on {letter.SenderName}'s letter has faded completely. {letter.RecipientName} will never receive their message now.",
            $"You notice {letter.SenderName}'s letter has turned grey and brittle. Another promise broken, another relationship strained.",
            $"The deadline for {letter.RecipientName}'s letter passed in the night. You can almost feel {letter.SenderName}'s disappointment.",
            $"Dawn reveals the truth: {letter.SenderName}'s urgent message will never reach {letter.RecipientName}. The consequences begin to unfold."
        };

        Random random = new Random();
        return narratives[random.Next(narratives.Length)];
    }

    private string GetObligationLetterNarrative(Letter letter, int position)
    {
        string[] narratives = new string[]
        {
            $"Your standing obligation to {letter.SenderName} manifests as a letter that forces itself into position {position} of your queue.",
            $"The binding promise you made to {letter.SenderName} compels you - their letter appears at position {position}, immovable.",
            $"An oath sworn is a debt owed. {letter.SenderName}'s letter materializes at position {position}, bound by your word.",
            $"Your obligation to {letter.SenderName} weighs heavy this morning as their letter claims position {position} in your queue."
        };

        Random random = new Random();
        return narratives[random.Next(narratives.Length)];
    }

    private string GetNewLettersNarrative(int count)
    {
        if (count == 1)
        {
            string[] narratives = new string[]
            {
                "A single letter awaits at the morning board, its seal still fresh with dawn's dew.",
                "The letter board holds one new commission, delivered under cover of night.",
                "A lone envelope catches the morning light - someone needs your services."
            };
            Random random = new Random();
            return narratives[random.Next(narratives.Length)];
        }
        else
        {
            string[] narratives = new string[]
            {
                $"The letter board is heavy with {count} new commissions this morning. Word of your services spreads.",
                $"{count} sealed letters await your attention, each a promise waiting to be kept or broken.",
                $"Dawn reveals {count} fresh opportunities pinned to the board. Choose wisely - you cannot carry them all.",
                $"The night brought {count} new requests. Your skills as a letter carrier are becoming known."
            };
            Random random = new Random();
            return narratives[random.Next(narratives.Length)];
        }
    }

    private string GetUrgentLetterNarrative(Letter letter)
    {
        if (letter.DeadlineInHours == 1)
        {
            string[] narratives = new string[]
            {
                $"The letter for {letter.RecipientName} burns hot in your queue - tomorrow will be too late!",
                $"{letter.RecipientName}'s letter trembles with urgency. One more dawn and it becomes worthless paper.",
                $"Time runs out for {letter.RecipientName}'s message. By tomorrow's light, this promise will be ash."
            };
            Random random = new Random();
            return narratives[random.Next(narratives.Length)];
        }
        else
        {
            string[] narratives = new string[]
            {
                $"The seal on {letter.RecipientName}'s letter grows warm - only {letter.DeadlineInHours} days remain.",
                $"{letter.RecipientName} waits for word that grows more urgent by the hour. {letter.DeadlineInHours} days left.",
                $"The letter to {letter.RecipientName} weighs heavier each day. {letter.DeadlineInHours} sunrises remain."
            };
            Random random = new Random();
            return narratives[random.Next(narratives.Length)];
        }
    }

    private string GetPatronLetterNarrative(Letter letter, int position)
    {
        string[] narratives = new string[]
        {
            $"A courier in midnight blue delivers a gold-sealed letter. Your patron's will disrupts everything - it seizes position {position}.",
            $"The unmistakable weight of patronage arrives. Gold wax, no signature, position {position}. All other obligations must wait.",
            $"Your mysterious benefactor speaks. The letter's gold seal gleams as it claims position {position} in your queue.",
            $"Dawn brings a letter bearing the patron's seal. Without ceremony, it commands position {position}. Their needs supersede all else."
        };

        Random random = new Random();
        return narratives[random.Next(narratives.Length)];
    }
}

// Event types for daily activities
public enum DailyEventType
{
    LetterExpired,
    ForcedLetterAdded,
    NewLettersAvailable,
    UrgentLetterWarning,
    PatronLetterAdded
}

// Individual daily event
public class DailyEvent
{
    public DailyEventType Type { get; set; }
    public string Description { get; set; }
    public int? TokenLoss { get; set; }
    public ConnectionType? TokenType { get; set; }
    public string SenderName { get; set; }
    public int? LetterPosition { get; set; }
}

// Result of daily processing
public class DailyActivityResult
{
    public int ExpiredLetterCount { get; set; }
    public int ForcedLetterCount { get; set; }
    public int NewLetterCount { get; set; }
    public int UrgentLetterCount { get; set; }
    public int PatronLetterCount { get; set; }

    public bool HasEvents => ExpiredLetterCount > 0 || ForcedLetterCount > 0 ||
                            NewLetterCount > 0 || UrgentLetterCount > 0 || PatronLetterCount > 0;
}

// Extension to MorningActivitiesManager
public partial class DailyActivitiesManager
{
    // Get the result of the last daily activities processing
    public DailyActivityResult GetLastActivityResult()
    {
        return _lastActivityResult ?? new DailyActivityResult();
    }
}