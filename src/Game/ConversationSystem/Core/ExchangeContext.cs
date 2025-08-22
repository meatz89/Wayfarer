using System;

/// <summary>
/// Provides rich context for exchanges to ensure they feel like human interactions.
/// This bridges the mechanical exchange system with narrative depth.
/// </summary>
public class ExchangeContext
{
    private readonly NPCRelationshipTracker _relationshipTracker;
    private readonly WorldMemorySystem _worldMemory;
    private readonly ExchangeMemory _exchangeMemory;
    
    public ExchangeContext(
        NPCRelationshipTracker relationshipTracker,
        WorldMemorySystem worldMemory,
        ExchangeMemory exchangeMemory)
    {
        _relationshipTracker = relationshipTracker;
        _worldMemory = worldMemory;
        _exchangeMemory = exchangeMemory;
    }
    
    /// <summary>
    /// Determine if an exchange should be available based on relationship context
    /// </summary>
    public bool ShouldOfferExchange(string npcId, ExchangeCard exchange)
    {
        // Don't offer generous exchanges to someone who just betrayed us
        if (_relationshipTracker.GetLastInteractionOutcome(npcId) == "promise_broken" &&
            exchange.TemplateType == "trusted")
        {
            return false;
        }
        
        // Crisis exchanges only during emotional moments
        if (exchange.TemplateType == "reconciliation" &&
            !_worldMemory.HasRecentFailureWith(npcId))
        {
            return false;
        }
        
        // Some exchanges require minimum relationship
        if (exchange.TemplateType == "trusted" &&
            _relationshipTracker.GetSuccessfulDeliveries(npcId) < 5)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Get modified exchange based on relationship (returns new card with adjusted values)
    /// </summary>
    public ExchangeCard GetModifiedExchange(string npcId, ExchangeCard originalExchange)
    {
        var trustPattern = _relationshipTracker.GetTrustPattern(npcId);
        var modifiedCosts = new List<ResourceExchange>();
        
        foreach (var cost in originalExchange.Cost)
        {
            var newAmount = cost.Amount;
            
            // Trusted partners give better rates
            if (trustPattern == "reliable" && cost.Type == ResourceType.Coins)
            {
                newAmount = Math.Max(1, (int)(cost.Amount * 0.8));
            }
            
            // Broken trust means higher costs
            if (_relationshipTracker.HasBrokenPromisesBefore(npcId) && cost.Type == ResourceType.Coins)
            {
                newAmount = (int)(cost.Amount * 1.5);
            }
            
            modifiedCosts.Add(new ResourceExchange
            {
                Type = cost.Type,
                Amount = newAmount,
                IsAbsolute = cost.IsAbsolute
            });
        }
        
        // Return new exchange card with modified costs
        return new ExchangeCard
        {
            Id = originalExchange.Id,
            TemplateType = originalExchange.TemplateType,
            NPCPersonality = originalExchange.NPCPersonality,
            Cost = modifiedCosts,
            Reward = originalExchange.Reward,
        };
    }
    
    /// <summary>
    /// Get the emotional weight of an exchange
    /// </summary>
    public int GetEmotionalWeight(string npcId, ExchangeCard exchange)
    {
        var weight = 0;
        
        // Reconciliation exchanges are emotionally heavy
        if (exchange.TemplateType == "reconciliation")
            weight += 3;
            
        // Trusted exchanges with long relationships carry weight
        if (exchange.TemplateType == "trusted" && 
            _relationshipTracker.GetSuccessfulDeliveries(npcId) > 10)
            weight += 2;
            
        // Crisis history adds weight
        if (_exchangeMemory.HasCrisisHistory(npcId, exchange.TemplateType))
            weight += 2;
            
        return weight;
    }
    
    /// <summary>
    /// Process the aftermath of an exchange
    /// </summary>
    public void ProcessExchangeAftermath(
        string npcId, 
        ExchangeCard exchange,
        bool wasGenerous,
        EmotionalState npcState)
    {
        // Record the exchange in memory
        _exchangeMemory.RecordExchange(npcId, exchange.TemplateType, npcState, wasGenerous);
        
        // Update world memory for significant exchanges
        if (exchange.TemplateType == "reconciliation")
        {
            _worldMemory.RecordEvent(
                WorldEventType.TrustGained,
                npcId,
                "player",
                null
            );
        }
        
        // Track special moments
        if (npcState == EmotionalState.DESPERATE && wasGenerous)
        {
            // This creates a powerful memory - helping in desperate times
            _relationshipTracker.RecordPromise(npcId, "desperate_help");
            _relationshipTracker.RecordPromiseFulfilled(npcId, "desperate_help");
        }
    }
    
    /// <summary>
    /// Check if this exchange has special meaning
    /// </summary>
    public string GetSpecialMeaning(string npcId, ExchangeCard exchange)
    {
        // First exchange ever
        if (_relationshipTracker.GetSuccessfulDeliveries(npcId) == 0 &&
            _relationshipTracker.GetFailedDeliveries(npcId) == 0)
        {
            return "The beginning of your relationship";
        }
        
        // Exchange after betrayal
        if (_relationshipTracker.GetLastInteractionOutcome(npcId) == "promise_broken" &&
            exchange.TemplateType != "reconciliation")
        {
            return "A tentative step toward rebuilding trust";
        }
        
        // Generous exchange during crisis
        if (_exchangeMemory.GetMostSignificantExchange(npcId)?.EmotionalContext == EmotionalState.DESPERATE)
        {
            return "Echoes of past kindness resonate";
        }
        
        // Pattern of generosity
        if (_exchangeMemory.HasGenerosityPattern(npcId))
        {
            return "Your consistent kindness has become legend";
        }
        
        return null;
    }
}