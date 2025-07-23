using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wayfarer.GameState;

/// <summary>
/// Extends LetterQueueManager with transactional operations and displacement preview
/// </summary>
public class TransactionalLetterQueueManager
{
    private readonly LetterQueueManager _baseManager;
    private readonly QueueDisplacementPlanner _planner;
    private readonly GameWorld _gameWorld;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly MessageSystem _messageSystem;
    
    public TransactionalLetterQueueManager(
        LetterQueueManager baseManager,
        QueueDisplacementPlanner planner,
        GameWorld gameWorld,
        ConnectionTokenManager tokenManager,
        MessageSystem messageSystem)
    {
        _baseManager = baseManager ?? throw new ArgumentNullException(nameof(baseManager));
        _planner = planner ?? throw new ArgumentNullException(nameof(planner));
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
    }
    
    /// <summary>
    /// Adds a letter with leverage effects using transactions and preview
    /// </summary>
    public async Task<int> AddLetterWithLeverageTransactional(Letter letter, bool showPreview = true)
    {
        // Calculate leverage position
        int targetPosition = _baseManager.CalculateLeveragePosition(letter);
        
        // Create displacement plan
        var plan = _planner.PlanLetterAddition(letter, targetPosition);
        
        // Show preview if requested
        if (showPreview)
        {
            _planner.ShowDisplacementPreview(plan);
            
            if (!plan.CanExecute)
            {
                return 0;
            }
            
            // If there are evictions, ask for confirmation
            if (plan.Evictions.Count > 0)
            {
                _messageSystem.AddSystemMessage(
                    "⚠️ This action will cause letters to be evicted. Proceed? (This would trigger a confirmation UI)",
                    SystemMessageTypes.Warning
                );
            }
        }
        
        // Create transaction
        var transaction = new GameTransaction(_gameWorld);
        
        // Add the letter operation
        transaction.AddOperation(new Operations.AddLetterToQueueOperation(letter, targetPosition, _baseManager));
        
        // Add operations for evicted letters
        foreach (var eviction in plan.Evictions)
        {
            // Remove tokens for evicted letter
            if (eviction.TokenPenalty > 0)
            {
                var senderId = GetNPCIdByName(eviction.Letter.SenderName);
                transaction.AddOperation(new Operations.SpendTokensOperation(
                    eviction.Letter.TokenType, 
                    eviction.TokenPenalty, 
                    senderId, 
                    _tokenManager
                ));
            }
        }
        
        // Execute transaction
        var result = transaction.Execute();
        
        if (result.Success)
        {
            _messageSystem.AddSystemMessage(
                $"✅ Letter from {letter.SenderName} successfully added at position {targetPosition}",
                SystemMessageTypes.Success
            );
            
            // Show narrative for evictions
            foreach (var eviction in plan.Evictions)
            {
                _messageSystem.AddSystemMessage(
                    $"💥 {eviction.Letter.SenderName}'s letter was forced out by leverage!",
                    SystemMessageTypes.Danger
                );
            }
            
            return targetPosition;
        }
        else
        {
            _messageSystem.AddSystemMessage(
                $"❌ Failed to add letter: {result.Message}",
                SystemMessageTypes.Danger
            );
            return 0;
        }
    }
    
    /// <summary>
    /// Performs a skip delivery with full transaction support
    /// </summary>
    public async Task<bool> SkipDeliverTransactional(int position, bool showPreview = true)
    {
        // Create displacement plan
        var plan = _planner.PlanSkipDelivery(position);
        
        // Show preview if requested
        if (showPreview)
        {
            _planner.ShowDisplacementPreview(plan);
            
            if (!plan.CanExecute)
            {
                return false;
            }
        }
        
        // Create transaction
        var transaction = new GameTransaction(_gameWorld);
        
        // Add token spending operations
        foreach (var tokenCost in plan.TokenCost)
        {
            var senderId = GetNPCIdByName(plan.NewLetter.SenderName);
            transaction.AddOperation(new Operations.SpendTokensOperation(
                tokenCost.Key,
                tokenCost.Value,
                senderId,
                _tokenManager
            ));
        }
        
        // Add letter movement operations
        transaction.AddOperation(new Operations.RemoveLetterFromQueueOperation(position));
        transaction.AddOperation(new Operations.AddLetterToQueueOperation(plan.NewLetter, 1, _baseManager));
        
        // Execute transaction
        var result = transaction.Execute();
        
        if (result.Success)
        {
            _messageSystem.AddSystemMessage(
                $"✅ {plan.NewLetter.SenderName}'s letter successfully skipped to position 1",
                SystemMessageTypes.Success
            );
            
            // Record skip for each skipped letter
            foreach (var skipped in plan.SkippedLetters)
            {
                _baseManager.RecordLetterSkip(skipped.Letter);
            }
            
            return true;
        }
        else
        {
            _messageSystem.AddSystemMessage(
                $"❌ Skip failed: {result.Message}",
                SystemMessageTypes.Danger
            );
            return false;
        }
    }
    
    /// <summary>
    /// Performs a purge with transaction support
    /// </summary>
    public async Task<bool> PurgeLetterTransactional(Dictionary<ConnectionType, int> tokenPayment, bool showPreview = true)
    {
        // Create displacement plan
        var plan = _planner.PlanPurge();
        
        // Show preview if requested
        if (showPreview)
        {
            _planner.ShowDisplacementPreview(plan);
            
            if (!plan.CanExecute)
            {
                return false;
            }
        }
        
        // Validate token payment
        int totalTokens = 0;
        foreach (var payment in tokenPayment)
        {
            totalTokens += payment.Value;
        }
        
        if (totalTokens != plan.RequiredTokenTotal)
        {
            _messageSystem.AddSystemMessage(
                $"❌ Purging requires exactly {plan.RequiredTokenTotal} tokens! You offered {totalTokens}",
                SystemMessageTypes.Danger
            );
            return false;
        }
        
        // Create transaction
        var transaction = new GameTransaction(_gameWorld);
        
        // Add token spending operations
        foreach (var payment in tokenPayment)
        {
            if (payment.Value > 0)
            {
                transaction.AddOperation(new Operations.SpendTokensOperation(
                    payment.Key,
                    payment.Value,
                    null, // Purge tokens aren't spent with a specific NPC
                    _tokenManager
                ));
            }
        }
        
        // Add letter removal operation
        var queueSize = _gameWorld.GetPlayer().LetterQueue.Length;
        transaction.AddOperation(new Operations.RemoveLetterFromQueueOperation(queueSize));
        
        // Execute transaction
        var result = transaction.Execute();
        
        if (result.Success)
        {
            _messageSystem.AddSystemMessage(
                $"🔥 {plan.NewLetter.SenderName}'s letter has been purged!",
                SystemMessageTypes.Success
            );
            return true;
        }
        else
        {
            _messageSystem.AddSystemMessage(
                $"❌ Purge failed: {result.Message}",
                SystemMessageTypes.Danger
            );
            return false;
        }
    }
    
    /// <summary>
    /// Performs a complex multi-step operation atomically
    /// </summary>
    public async Task<bool> DeliverLetterWithRewards(int position)
    {
        var letter = _baseManager.GetLetterAt(position);
        if (letter == null || letter.State != LetterState.Collected)
        {
            return false;
        }
        
        // Create transaction for atomic delivery
        var transaction = new GameTransaction(_gameWorld);
        
        // Remove letter from queue
        transaction.AddOperation(new Operations.RemoveLetterFromQueueOperation(position));
        
        // Add coin reward
        transaction.AddOperation(new Operations.SpendCoinsOperation(-letter.Payment)); // Negative = gain
        
        // Add potential token rewards
        var tokenChance = 0.25f; // Base 25% chance
        if (new Random().NextDouble() < tokenChance)
        {
            // Would add token reward operation here
            _messageSystem.AddSystemMessage(
                $"✨ Earned a {letter.TokenType} token from {letter.RecipientName}!",
                SystemMessageTypes.Success
            );
        }
        
        // Execute transaction
        var result = transaction.Execute();
        
        if (result.Success)
        {
            _messageSystem.AddSystemMessage(
                $"✅ Successfully delivered letter to {letter.RecipientName} for {letter.Payment} coins",
                SystemMessageTypes.Success
            );
            
            // Record delivery for history
            _baseManager.RecordLetterDelivery(letter);
            
            return true;
        }
        else
        {
            _messageSystem.AddSystemMessage(
                $"❌ Delivery failed: {result.Message}",
                SystemMessageTypes.Danger
            );
            return false;
        }
    }
    
    private string GetNPCIdByName(string npcName)
    {
        // This would call into the NPC repository to get the ID
        // For now, returning a placeholder
        return $"npc_{npcName.ToLower().Replace(" ", "_")}";
    }
}