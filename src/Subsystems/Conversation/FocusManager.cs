using System;

// FocusManager handles focus persistence and capacity management
public class FocusManager
{
    private int currentSpentFocus = 0;
    private int baseCapacity = 5;
    private AtmosphereManager atmosphereManager;

    public FocusManager(AtmosphereManager atmosphereManager)
    {
        this.atmosphereManager = atmosphereManager;
    }

    public int CurrentSpentFocus => currentSpentFocus;
    public int CurrentCapacity => GetEffectiveCapacity();
    public int AvailableFocus => Math.Max(0, CurrentCapacity - currentSpentFocus);

    // Get effective capacity including atmosphere bonuses
    private int GetEffectiveCapacity()
    {
        int capacity = baseCapacity;

        // Prepared atmosphere adds +1 capacity
        capacity += atmosphereManager.GetFocusCapacityBonus();

        return capacity;
    }

    // Set base capacity from connection state
    public void SetBaseCapacity(ConnectionState state)
    {
        baseCapacity = state switch
        {
            ConnectionState.DISCONNECTED => 3,
            ConnectionState.GUARDED => 4,
            ConnectionState.NEUTRAL => 5,
            ConnectionState.RECEPTIVE => 5,
            ConnectionState.TRUSTING => 6,
            _ => 5
        };
    }

    // Check if player can afford card
    public bool CanAffordCard(int cardFocus)
    {
        return AvailableFocus >= cardFocus;
    }

    // Spend focus for playing a card
    public bool SpendFocus(int amount)
    {
        if (amount > AvailableFocus)
        {
            return false;
        }

        currentSpentFocus += amount;
        return true;
    }

    // Refresh focus (on LISTEN action)
    public void RefreshPool()
    {
        currentSpentFocus = 0;
    }

    // Add focus to pool (from card effects)
    public void AddFocus(int amount)
    {
        // Reduce spent focus (effectively adding to available pool)
        currentSpentFocus = Math.Max(0, currentSpentFocus - amount);
    }

    // Set focus to maximum capacity (from observation cards)
    public void SetToMaximum()
    {
        currentSpentFocus = 0;
    }

    // Get focus status for UI display
    public string GetFocusStatus()
    {
        return $"{AvailableFocus}/{CurrentCapacity}";
    }

    // Check if pool is completely depleted
    public bool IsPoolDepleted()
    {
        return AvailableFocus <= 0;
    }

    // Deplete all remaining focus (for ForceListen failure effect)
    public void DepleteFocus()
    {
        currentSpentFocus = CurrentCapacity;
    }

    // Reset for new conversation
    public void Reset()
    {
        currentSpentFocus = 0;
        baseCapacity = 5; // Default neutral capacity
    }

    // Get visual representation for UI (dots or bars)
    public FocusDisplay GetDisplayInfo()
    {
        return new FocusDisplay
        {
            Available = AvailableFocus,
            Capacity = CurrentCapacity,
            Spent = currentSpentFocus,
            HasPreparedBonus = atmosphereManager.CurrentAtmosphere == AtmosphereType.Prepared
        };
    }
}

// Display information for UI
public class FocusDisplay
{
    public int Available { get; set; }
    public int Capacity { get; set; }
    public int Spent { get; set; }
    public bool HasPreparedBonus { get; set; }
}