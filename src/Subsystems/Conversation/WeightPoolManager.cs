using System;

// WeightPoolManager handles weight pool persistence and capacity management
public class WeightPoolManager
{
    private int currentSpentWeight = 0;
    private int baseCapacity = 5;
    private AtmosphereManager atmosphereManager;

    public WeightPoolManager(AtmosphereManager atmosphereManager)
    {
        this.atmosphereManager = atmosphereManager;
    }

    public int CurrentSpentWeight => currentSpentWeight;
    public int CurrentCapacity => GetEffectiveCapacity();
    public int AvailableWeight => Math.Max(0, CurrentCapacity - currentSpentWeight);

    // Get effective capacity including atmosphere bonuses
    private int GetEffectiveCapacity()
    {
        int capacity = baseCapacity;
        
        // Prepared atmosphere adds +1 capacity
        capacity += atmosphereManager.GetWeightCapacityBonus();
        
        return capacity;
    }

    // Set base capacity from emotional state
    public void SetBaseCapacity(EmotionalState state)
    {
        baseCapacity = state switch
        {
            EmotionalState.DESPERATE => 3,
            EmotionalState.TENSE => 4,
            EmotionalState.NEUTRAL => 5,
            EmotionalState.OPEN => 5,
            EmotionalState.CONNECTED => 6,
            _ => 5
        };
    }

    // Check if player can afford card
    public bool CanAffordCard(int cardWeight)
    {
        return AvailableWeight >= cardWeight;
    }

    // Spend weight for playing a card
    public bool SpendWeight(int amount)
    {
        if (amount > AvailableWeight)
        {
            return false;
        }

        currentSpentWeight += amount;
        return true;
    }

    // Refresh weight pool (on LISTEN action)
    public void RefreshPool()
    {
        currentSpentWeight = 0;
    }

    // Add weight to pool (from card effects)
    public void AddWeight(int amount)
    {
        // Reduce spent weight (effectively adding to available pool)
        currentSpentWeight = Math.Max(0, currentSpentWeight - amount);
    }

    // Set weight to maximum capacity (from observation cards)
    public void SetToMaximum()
    {
        currentSpentWeight = 0;
    }

    // Get weight status for UI display
    public string GetWeightStatus()
    {
        return $"{AvailableWeight}/{CurrentCapacity}";
    }

    // Check if pool is completely depleted
    public bool IsPoolDepleted()
    {
        return AvailableWeight <= 0;
    }

    // Reset for new conversation
    public void Reset()
    {
        currentSpentWeight = 0;
        baseCapacity = 5; // Default neutral capacity
    }

    // Get visual representation for UI (dots or bars)
    public WeightPoolDisplay GetDisplayInfo()
    {
        return new WeightPoolDisplay
        {
            Available = AvailableWeight,
            Capacity = CurrentCapacity,
            Spent = currentSpentWeight,
            HasPreparedBonus = atmosphereManager.CurrentConversationAtmosphere == ConversationAtmosphere.Prepared
        };
    }
}

// Display information for UI
public class WeightPoolDisplay
{
    public int Available { get; set; }
    public int Capacity { get; set; }
    public int Spent { get; set; }
    public bool HasPreparedBonus { get; set; }
}