using System;

/// <summary>
/// Provides rich context for exchanges to ensure they feel like human interactions.
/// This bridges the mechanical exchange system with narrative depth.
/// </summary>
public class ExchangeContext
{
    private readonly ExchangeMemory _exchangeMemory;
    
    public ExchangeContext(
        ExchangeMemory exchangeMemory)
    {
        _exchangeMemory = exchangeMemory;
    }
    
    /// <summary>
    /// Determine if an exchange should be available based on relationship context
    /// </summary>
    public bool ShouldOfferExchange(string npcId, ConversationCard exchange)
    {
        return true;
    }
    
    /// <summary>
    /// Get modified exchange based on relationship (returns new card with adjusted values)
    /// </summary>
    public ConversationCard GetModifiedExchange(string npcId, ConversationCard originalExchange)
    {
        var modifiedCosts = new List<ResourceExchange>();
        
        foreach (var cost in originalExchange.Context?.Cost ?? new List<ResourceExchange>())
        {
            var newAmount = cost.Amount;
            
            modifiedCosts.Add(new ResourceExchange
            {
                ResourceType = cost.ResourceType,
                Amount = newAmount,
                IsAbsolute = cost.IsAbsolute
            });
        }
        
        // Return new exchange card with modified costs
        return new ConversationCard
        {
            Id = originalExchange.Id,
            Template = originalExchange.Template,
            Context = new CardContext
            {
                Personality = originalExchange.Context?.Personality ?? PersonalityType.STEADFAST,
                EmotionalState = originalExchange.Context?.EmotionalState ?? EmotionalState.NEUTRAL,
                NPCName = originalExchange.Context?.NPCName,
                Cost = modifiedCosts,
                Reward = originalExchange.Context?.Reward ?? new List<ResourceExchange>(),
                // Copy other context properties
                ExchangeName = originalExchange.Context?.ExchangeName,
                ExchangeCost = originalExchange.Context?.ExchangeCost,
                ExchangeReward = originalExchange.Context?.ExchangeReward
            },
            Type = originalExchange.Type,
            Category = originalExchange.Category,
            Persistence = originalExchange.Persistence,
            Weight = originalExchange.Weight,
            BaseComfort = originalExchange.BaseComfort,
            IsExchange = true
        };
    }
    
    /// <summary>
    /// Get the emotional weight of an exchange
    /// </summary>
    public int GetEmotionalWeight(string npcId, ConversationCard exchange)
    {
        var weight = 0;
        
        // Reconciliation exchanges are emotionally heavy  
        if (exchange.Template.ToString().ToLower() == "reconciliation")
            weight += 3;
            
        if (_exchangeMemory.HasCrisisHistory(npcId, exchange.Template.ToString()))
            weight += 2;
            
        return weight;
    }
    
    /// <summary>
    /// Process the aftermath of an exchange
    /// </summary>
    public void ProcessExchangeAftermath(
        string npcId, 
        ConversationCard exchange,
        bool wasGenerous,
        EmotionalState npcState)
    {
        // Record the exchange in memory
        _exchangeMemory.RecordExchange(npcId, exchange.Template.ToString(), npcState, wasGenerous);
    }
    
    /// <summary>
    /// Check if this exchange has special meaning
    /// </summary>
    public string GetSpecialMeaning(string npcId, ConversationCard exchange)
    {
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