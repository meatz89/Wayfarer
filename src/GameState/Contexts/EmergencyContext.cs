/// <summary>
/// Context for Emergency screens containing situation state and metadata.
/// Provides view model data for urgent situations demanding immediate response.
/// </summary>
public class EmergencyContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }

    // Emergency data
    // HIGHLANDER: EmergencyState is the mutable runtime state, Emergency is the immutable template
    public ActiveEmergencyState EmergencyState { get; set; }
    public EmergencySituation Emergency { get; set; }

    // Player resources
    public int CurrentStamina { get; set; }
    public int MaxStamina { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public int CurrentCoins { get; set; }
    public int CurrentFocus { get; set; }
    public int MaxFocus { get; set; }

    // Time pressure
    public int CurrentSegment { get; set; }
    public int ResponseDeadlineSegment { get; set; }
    public int SegmentsRemaining { get; set; }

    // Display info
    public string LocationName { get; set; }
    public string TimeDisplay { get; set; }

    public EmergencyContext()
    {
        IsValid = true;
        ErrorMessage = string.Empty;
    }

    // Helper methods for UI
    public List<EmergencyResponse> GetAffordableResponses()
    {
        if (Emergency == null) return new List<EmergencyResponse>();

        return Emergency.Responses
            .Where(r => CanAffordResponse(r))
            .ToList();
    }

    public List<EmergencyResponse> GetBlockedResponses()
    {
        if (Emergency == null) return new List<EmergencyResponse>();

        return Emergency.Responses
            .Where(r => !CanAffordResponse(r))
            .ToList();
    }

    public bool CanAffordResponse(EmergencyResponse response)
    {
        if (response.StaminaCost > CurrentStamina) return false;
        if (response.HealthCost > CurrentHealth) return false;
        if (response.CoinCost > CurrentCoins) return false;
        return true;
    }

    public string GetBlockReason(EmergencyResponse response)
    {
        List<string> reasons = new List<string>();

        if (response.StaminaCost > CurrentStamina)
            reasons.Add($"Need {response.StaminaCost} Stamina (have {CurrentStamina})");

        if (response.HealthCost > CurrentHealth)
            reasons.Add($"Need {response.HealthCost} Health (have {CurrentHealth})");

        if (response.CoinCost > CurrentCoins)
            reasons.Add($"Need {response.CoinCost} Coins (have {CurrentCoins})");

        return string.Join(", ", reasons);
    }

    public bool IsUrgent()
    {
        return SegmentsRemaining <= 1;
    }

    public bool IsExpired()
    {
        return SegmentsRemaining <= 0;
    }

    public string GetUrgencyLevel()
    {
        if (SegmentsRemaining <= 0) return "EXPIRED";
        if (SegmentsRemaining == 1) return "CRITICAL";
        if (SegmentsRemaining <= 2) return "URGENT";
        return "MODERATE";
    }
}
