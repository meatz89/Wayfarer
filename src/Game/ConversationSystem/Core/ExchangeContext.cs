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
    public bool ShouldOfferExchange(string npcId, ExchangeCard exchange)
    {
        return true;
    }
    
    /// <summary>
    /// Get modified exchange based on relationship (returns new card with adjusted values)
    /// </summary>
    public ExchangeCard GetModifiedExchange(string npcId, ExchangeCard originalExchange)
    {
        var modifiedCosts = new List<ResourceExchange>();
        
        foreach (var cost in originalExchange.Cost)
        {
            var newAmount = cost.Amount;
            
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
    }
    
    /// <summary>
    /// Check if this exchange has special meaning
    /// </summary>
    public string GetSpecialMeaning(string npcId, ExchangeCard exchange)
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