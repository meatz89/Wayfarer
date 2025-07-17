using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;

namespace Wayfarer.GameState
{
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
                    Description = $"Letter from {letter.SenderName} to {letter.RecipientName} expired",
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
                        Description = $"Obligation letter from {letter.SenderName} added at position {position}",
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
                    Description = $"{newLetterCount} new letters arrived overnight"
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
                    Description = $"URGENT: Letter to {letter.RecipientName} expires in {letter.Deadline} days!",
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
}