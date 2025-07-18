using System;
using System.Collections.Generic;
using System.Linq;
public class MorningActivitiesManager
{
    private readonly GameWorld _gameWorld;
    private readonly LetterQueueManager _letterQueueManager;
    private readonly StandingObligationManager _obligationManager;
    private readonly MessageSystem _messageSystem;
    
    // Track morning events for display
    public List<MorningEvent> MorningEvents { get; private set; } = new List<MorningEvent>();
    
    public MorningActivitiesManager(
        GameWorld gameWorld, 
        LetterQueueManager letterQueueManager,
        StandingObligationManager obligationManager,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld;
        _letterQueueManager = letterQueueManager;
        _obligationManager = obligationManager;
        _messageSystem = messageSystem;
    }
    
    // Process all morning activities and collect events
    public MorningActivityResult ProcessMorningActivities()
    {
        MorningEvents.Clear();
        var result = new MorningActivityResult();
        
        // 1. Track letters that will expire
        var expiringLetters = GetLettersAboutToExpire();
        foreach (var letter in expiringLetters)
        {
            MorningEvents.Add(new MorningEvent
            {
                Type = MorningEventType.LetterExpired,
                Description = GetExpiredLetterNarrative(letter),
                TokenLoss = 2,
                TokenType = letter.TokenType,
                SenderName = letter.SenderName
            });
            result.ExpiredLetterCount++;
        }
        
        // 2. Process daily deadlines (this actually removes expired letters)
        _letterQueueManager.ProcessDailyDeadlines();
        
        // 3. Process obligations and get forced letters
        var forcedLetters = _obligationManager.ProcessDailyObligations(_gameWorld.CurrentDay);
        foreach (var letter in forcedLetters)
        {
            var position = _letterQueueManager.AddLetterWithObligationEffects(letter);
            if (position > 0)
            {
                MorningEvents.Add(new MorningEvent
                {
                    Type = MorningEventType.ForcedLetterAdded,
                    Description = GetObligationLetterNarrative(letter, position),
                    LetterPosition = position,
                    SenderName = letter.SenderName
                });
                result.ForcedLetterCount++;
            }
        }
        
        // 4. Advance obligation time
        _obligationManager.AdvanceDailyTime();
        
        // 5. Generate regular daily letters
        var newLetterCount = _letterQueueManager.GenerateDailyLetters();
        if (newLetterCount > 0)
        {
            MorningEvents.Add(new MorningEvent
            {
                Type = MorningEventType.NewLettersAvailable,
                Description = GetNewLettersNarrative(newLetterCount)
            });
            result.NewLetterCount = newLetterCount;
        }
        
        // 6. Check for urgent letters needing attention
        var urgentLetters = _letterQueueManager.GetExpiringLetters(2);
        foreach (var letter in urgentLetters)
        {
            MorningEvents.Add(new MorningEvent
            {
                Type = MorningEventType.UrgentLetterWarning,
                Description = GetUrgentLetterNarrative(letter),
                LetterPosition = letter.QueuePosition
            });
            result.UrgentLetterCount++;
        }
        
        return result;
    }
    
    // Get letters that are about to expire (deadline = 0)
    private Letter[] GetLettersAboutToExpire()
    {
        return _gameWorld.GetPlayer().LetterQueue
            .Where(l => l != null && l.Deadline == 0)
            .ToArray();
    }
    
    // Check if it's morning (dawn)
    public bool IsMorningTime()
    {
        return _gameWorld.TimeManager.GetCurrentTimeBlock() == TimeBlocks.Dawn;
    }
    
    // Clear morning events after display
    public void ClearMorningEvents()
    {
        MorningEvents.Clear();
    }
    
    // Narrative generation methods
    private string GetExpiredLetterNarrative(Letter letter)
    {
        var narratives = new string[]
        {
            $"The seal on {letter.SenderName}'s letter has faded completely. {letter.RecipientName} will never receive their message now.",
            $"You notice {letter.SenderName}'s letter has turned grey and brittle. Another promise broken, another relationship strained.",
            $"The deadline for {letter.RecipientName}'s letter passed in the night. You can almost feel {letter.SenderName}'s disappointment.",
            $"Dawn reveals the truth: {letter.SenderName}'s urgent message will never reach {letter.RecipientName}. The consequences begin to unfold."
        };
        
        var random = new Random();
        return narratives[random.Next(narratives.Length)];
    }
    
    private string GetObligationLetterNarrative(Letter letter, int position)
    {
        var narratives = new string[]
        {
            $"Your standing obligation to {letter.SenderName} manifests as a letter that forces itself into position {position} of your queue.",
            $"The binding promise you made to {letter.SenderName} compels you - their letter appears at position {position}, immovable.",
            $"An oath sworn is a debt owed. {letter.SenderName}'s letter materializes at position {position}, bound by your word.",
            $"Your obligation to {letter.SenderName} weighs heavy this morning as their letter claims position {position} in your queue."
        };
        
        var random = new Random();
        return narratives[random.Next(narratives.Length)];
    }
    
    private string GetNewLettersNarrative(int count)
    {
        if (count == 1)
        {
            var narratives = new string[]
            {
                "A single letter awaits at the morning board, its seal still fresh with dawn's dew.",
                "The letter board holds one new commission, delivered under cover of night.",
                "A lone envelope catches the morning light - someone needs your services."
            };
            var random = new Random();
            return narratives[random.Next(narratives.Length)];
        }
        else
        {
            var narratives = new string[]
            {
                $"The letter board is heavy with {count} new commissions this morning. Word of your services spreads.",
                $"{count} sealed letters await your attention, each a promise waiting to be kept or broken.",
                $"Dawn reveals {count} fresh opportunities pinned to the board. Choose wisely - you cannot carry them all.",
                $"The night brought {count} new requests. Your reputation as a letter carrier grows, for better or worse."
            };
            var random = new Random();
            return narratives[random.Next(narratives.Length)];
        }
    }
    
    private string GetUrgentLetterNarrative(Letter letter)
    {
        if (letter.Deadline == 1)
        {
            var narratives = new string[]
            {
                $"The letter for {letter.RecipientName} burns hot in your queue - tomorrow will be too late!",
                $"{letter.RecipientName}'s letter trembles with urgency. One more dawn and it becomes worthless paper.",
                $"Time runs out for {letter.RecipientName}'s message. By tomorrow's light, this promise will be ash."
            };
            var random = new Random();
            return narratives[random.Next(narratives.Length)];
        }
        else
        {
            var narratives = new string[]
            {
                $"The seal on {letter.RecipientName}'s letter grows warm - only {letter.Deadline} days remain.",
                $"{letter.RecipientName} waits for word that grows more urgent by the hour. {letter.Deadline} days left.",
                $"The letter to {letter.RecipientName} weighs heavier each day. {letter.Deadline} sunrises remain."
            };
            var random = new Random();
            return narratives[random.Next(narratives.Length)];
        }
    }
}

// Event types for morning activities
public enum MorningEventType
{
    LetterExpired,
    ForcedLetterAdded,
    NewLettersAvailable,
    UrgentLetterWarning
}

// Individual morning event
public class MorningEvent
{
    public MorningEventType Type { get; set; }
    public string Description { get; set; }
    public int? TokenLoss { get; set; }
    public ConnectionType? TokenType { get; set; }
    public string SenderName { get; set; }
    public int? LetterPosition { get; set; }
}

// Result of morning processing
public class MorningActivityResult
{
    public int ExpiredLetterCount { get; set; }
    public int ForcedLetterCount { get; set; }
    public int NewLetterCount { get; set; }
    public int UrgentLetterCount { get; set; }
    
    public bool HasEvents => ExpiredLetterCount > 0 || ForcedLetterCount > 0 || 
                            NewLetterCount > 0 || UrgentLetterCount > 0;
}