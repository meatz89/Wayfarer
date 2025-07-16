using System;
using System.Linq;
using Wayfarer.GameState;
using Wayfarer.Content;

namespace Wayfarer.GameState
{
    public class LetterQueueManager
    {
        private readonly GameWorld _gameWorld;
        private readonly LetterTemplateRepository _letterTemplateRepository;
        private readonly NPCRepository _npcRepository;
        private readonly Random _random = new Random();
        
        public LetterQueueManager(GameWorld gameWorld, LetterTemplateRepository letterTemplateRepository, NPCRepository npcRepository)
        {
            _gameWorld = gameWorld;
            _letterTemplateRepository = letterTemplateRepository;
            _npcRepository = npcRepository;
        }
        
        // Get the player's letter queue
        public Letter[] GetPlayerQueue()
        {
            return _gameWorld.GetPlayer().LetterQueue;
        }
        
        // Add letter to queue at specific position
        public bool AddLetterToQueue(Letter letter, int position)
        {
            if (letter == null || position < 1 || position > 8) return false;
            
            var queue = _gameWorld.GetPlayer().LetterQueue;
            if (queue[position - 1] != null) return false; // Position occupied
            
            queue[position - 1] = letter;
            letter.QueuePosition = position;
            return true;
        }
        
        // Add letter to first available slot (for minimal POC)
        public int AddLetterToFirstEmpty(Letter letter)
        {
            if (letter == null) return 0;
            
            var queue = _gameWorld.GetPlayer().LetterQueue;
            for (int i = 0; i < 8; i++)
            {
                if (queue[i] == null)
                {
                    queue[i] = letter;
                    letter.QueuePosition = i + 1;
                    return i + 1;
                }
            }
            return 0; // Queue full
        }
        
        // Remove letter from queue
        public bool RemoveLetterFromQueue(int position)
        {
            if (position < 1 || position > 8) return false;
            
            var queue = _gameWorld.GetPlayer().LetterQueue;
            var letter = queue[position - 1];
            if (letter == null) return false;
            
            letter.QueuePosition = 0;
            queue[position - 1] = null;
            
            // Don't shift for minimal POC
            return true;
        }
        
        // Get letter at position
        public Letter GetLetterAt(int position)
        {
            if (position < 1 || position > 8) return null;
            return _gameWorld.GetPlayer().LetterQueue[position - 1];
        }
        
        // Check if position 1 has a letter
        public bool CanDeliverFromPosition1()
        {
            return GetLetterAt(1) != null;
        }
        
        // Process daily deadline countdown
        public void ProcessDailyDeadlines()
        {
            var queue = _gameWorld.GetPlayer().LetterQueue;
            for (int i = 0; i < 8; i++)
            {
                var letter = queue[i];
                if (letter != null)
                {
                    letter.Deadline--;
                    if (letter.Deadline <= 0)
                    {
                        // Just remove for now
                        RemoveLetterFromQueue(i + 1);
                    }
                }
            }
        }
        
        // Get expiring letters
        public Letter[] GetExpiringLetters(int daysThreshold)
        {
            return _gameWorld.GetPlayer().LetterQueue
                .Where(l => l != null && l.Deadline <= daysThreshold)
                .OrderBy(l => l.Deadline)
                .ToArray();
        }
        
        // Check if queue is full
        public bool IsQueueFull()
        {
            return _gameWorld.GetPlayer().LetterQueue.All(slot => slot != null);
        }
        
        // Get queue fill status
        public int GetLetterCount()
        {
            return _gameWorld.GetPlayer().LetterQueue.Count(slot => slot != null);
        }
        
        // Skip letter delivery by spending tokens
        public bool TrySkipDeliver(int position)
        {
            if (position <= 1 || position > 8) return false;
            
            var letter = GetLetterAt(position);
            if (letter == null) return false;
            
            // Check if position 1 is occupied (can't skip to occupied slot)
            if (GetLetterAt(1) != null) return false;
            
            // Calculate token cost: position - 1 (skip from position 3 costs 2 tokens)
            int tokenCost = position - 1;
            
            // Validate token availability through ConnectionTokenManager
            var tokenManager = new ConnectionTokenManager(_gameWorld);
            if (!tokenManager.HasTokens(letter.TokenType, tokenCost))
            {
                return false;
            }
            
            // Spend the tokens
            if (!tokenManager.SpendTokens(letter.TokenType, tokenCost))
            {
                return false;
            }
            
            // Move letter to position 1
            var queue = _gameWorld.GetPlayer().LetterQueue;
            queue[0] = letter; // Position 1 is index 0
            queue[position - 1] = null; // Clear original position
            letter.QueuePosition = 1;
            
            return true;
        }
        
        // Generate 1-2 daily letters from available NPCs and templates
        public void GenerateDailyLetters()
        {
            // Generate 1-2 letters per day
            int lettersToGenerate = _random.Next(1, 3);
            
            for (int i = 0; i < lettersToGenerate; i++)
            {
                // Check if queue has space
                if (IsQueueFull())
                {
                    break; // Stop generating if queue is full
                }
                
                // Get a random template
                var template = _letterTemplateRepository.GetRandomTemplate();
                if (template == null) continue;
                
                // Get random NPCs for sender and recipient
                var allNpcs = _npcRepository.GetAllNPCs();
                if (allNpcs.Count < 2) continue; // Need at least 2 NPCs
                
                var sender = allNpcs[_random.Next(allNpcs.Count)];
                var recipient = allNpcs[_random.Next(allNpcs.Count)];
                
                // Ensure sender and recipient are different
                if (sender.ID == recipient.ID && allNpcs.Count > 1)
                {
                    recipient = allNpcs.Where(n => n.ID != sender.ID).First();
                }
                
                // Generate letter from template
                var letter = _letterTemplateRepository.GenerateLetterFromTemplate(template, sender.Name, recipient.Name);
                if (letter != null)
                {
                    // Add to first empty slot
                    AddLetterToFirstEmpty(letter);
                }
            }
        }
    }
}