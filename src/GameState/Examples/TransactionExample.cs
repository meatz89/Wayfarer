using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wayfarer.GameState.Operations;

namespace Wayfarer.GameState.Examples;

/// <summary>
/// Example demonstrating how to use the transaction system and displacement preview
/// </summary>
public class TransactionExample
{
    private readonly TransactionalLetterQueueManager _transactionalQueue;
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    
    public TransactionExample(
        TransactionalLetterQueueManager transactionalQueue,
        GameWorld gameWorld,
        MessageSystem messageSystem)
    {
        _transactionalQueue = transactionalQueue;
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
    }
    
    /// <summary>
    /// Example 1: Complex work action with multiple effects
    /// </summary>
    public async Task<bool> PerformWorkAction(ActionOption workAction)
    {
        _messageSystem.AddSystemMessage("Starting complex work action...", SystemMessageTypes.Info);
        
        // Create a transaction for all work effects
        var transaction = new GameTransaction(_gameWorld);
        
        // 1. Spend time (3 hours)
        transaction.AddOperation(new AdvanceTimeOperation(3, _gameWorld.TimeManager));
        
        // 2. Spend stamina (-1)
        transaction.AddOperation(new ModifyStaminaOperation(-1));
        
        // 3. Gain coins (+4)
        transaction.AddOperation(new SpendCoinsOperation(-4)); // Negative = gain
        
        // 4. Potentially gain a token (would be handled separately with token manager)
        
        // Validate the transaction
        var validation = transaction.Validate();
        if (!validation.IsValid)
        {
            _messageSystem.AddSystemMessage("Cannot perform work:", SystemMessageTypes.Danger);
            foreach (var failure in validation.FailedOperations)
            {
                _messageSystem.AddSystemMessage($"  • {failure}", SystemMessageTypes.Warning);
            }
            return false;
        }
        
        // Execute the transaction
        var result = transaction.Execute();
        
        if (result.Success)
        {
            _messageSystem.AddSystemMessage("✅ Work completed successfully!", SystemMessageTypes.Success);
            _messageSystem.AddSystemMessage("  • Spent 3 hours and 1 stamina", SystemMessageTypes.Info);
            _messageSystem.AddSystemMessage("  • Earned 4 coins", SystemMessageTypes.Success);
            return true;
        }
        else
        {
            _messageSystem.AddSystemMessage($"❌ Work failed: {result.Message}", SystemMessageTypes.Danger);
            return false;
        }
    }
    
    /// <summary>
    /// Example 2: Adding a high-leverage letter that causes displacement
    /// </summary>
    public async Task<bool> AddHighLeverageLetter()
    {
        // Create a letter from a creditor (negative token balance)
        var letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderName = "Marcus the Merchant",
            RecipientName = "Lady Blackwood",
            TokenType = ConnectionType.Trade,
            Payment = 5,
            Deadline = 3,
            IsFromPatron = false
        };
        
        _messageSystem.AddSystemMessage("📨 A letter arrives from your creditor...", SystemMessageTypes.Warning);
        
        // Add with preview - this will show displacement effects
        var position = await _transactionalQueue.AddLetterWithLeverageTransactional(letter, showPreview: true);
        
        return position > 0;
    }
    
    /// <summary>
    /// Example 3: Skip delivery with preview
    /// </summary>
    public async Task<bool> SkipLetterWithPreview(int position)
    {
        _messageSystem.AddSystemMessage($"Attempting to skip letter at position {position}...", SystemMessageTypes.Info);
        
        // This will show:
        // - Token cost
        // - Which letters will be skipped
        // - Relationship impact
        var success = await _transactionalQueue.SkipDeliverTransactional(position, showPreview: true);
        
        return success;
    }
    
    /// <summary>
    /// Example 4: Complex patron letter insertion with cascading effects
    /// </summary>
    public async Task<bool> HandlePatronLetterArrival()
    {
        var patronLetter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderName = "Your Mysterious Patron",
            RecipientName = "The Shadow Broker",
            TokenType = ConnectionType.Shadow,
            Payment = 10,
            Deadline = 2,
            IsFromPatron = true,
            IsPatronLetter = true,
            PatronQueuePosition = 2 // Wants position 2
        };
        
        _messageSystem.AddSystemMessage("🌟 A GOLD-SEALED LETTER ARRIVES!", SystemMessageTypes.Warning);
        
        // Create a transaction for patron letter handling
        var transaction = new GameTransaction(_gameWorld);
        
        // This would use a specialized patron letter operation
        // For now, we'll use the transactional manager
        var position = await _transactionalQueue.AddLetterWithLeverageTransactional(patronLetter, showPreview: true);
        
        if (position > 0)
        {
            _messageSystem.AddSystemMessage("The patron's will is absolute.", SystemMessageTypes.Info);
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Example 5: Attempting an invalid transaction
    /// </summary>
    public async Task DemonstrateFailedTransaction()
    {
        _messageSystem.AddSystemMessage("Attempting impossible action...", SystemMessageTypes.Info);
        
        var transaction = new GameTransaction(_gameWorld);
        
        // Try to spend more coins than we have
        transaction.AddOperation(new SpendCoinsOperation(1000));
        
        // Try to spend tokens we don't have
        transaction.AddOperation(new SpendTokensOperation(
            ConnectionType.Noble, 
            10, 
            "duke_aldric",
            null // Would need actual token manager
        ));
        
        // Validation will fail
        var validation = transaction.Validate();
        if (!validation.IsValid)
        {
            _messageSystem.AddSystemMessage("Transaction validation failed:", SystemMessageTypes.Danger);
            foreach (var failure in validation.FailedOperations)
            {
                _messageSystem.AddSystemMessage($"  • {failure}", SystemMessageTypes.Warning);
            }
        }
        
        // Even if we try to execute, it will fail safely
        var result = transaction.Execute();
        _messageSystem.AddSystemMessage($"Result: {result.Message}", SystemMessageTypes.Info);
    }
    
    /// <summary>
    /// Example 6: Purge with token selection
    /// </summary>
    public async Task<bool> PurgeBottomLetter()
    {
        _messageSystem.AddSystemMessage("Considering desperate measures...", SystemMessageTypes.Warning);
        
        // Player chooses which tokens to spend (must total 3)
        var tokenPayment = new Dictionary<ConnectionType, int>
        {
            { ConnectionType.Common, 2 },
            { ConnectionType.Trade, 1 }
        };
        
        // This will show what letter will be purged and confirm token cost
        var success = await _transactionalQueue.PurgeLetterTransactional(tokenPayment, showPreview: true);
        
        if (success)
        {
            _messageSystem.AddSystemMessage("The deed is done. The letter is gone.", SystemMessageTypes.Info);
        }
        
        return success;
    }
}