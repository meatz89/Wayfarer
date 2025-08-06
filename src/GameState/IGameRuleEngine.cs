using System.Collections.Generic;

/// <summary>
/// Central interface for all game mechanics and rule calculations.
/// This separates game logic from UI and infrastructure concerns.
/// </summary>
public interface IGameRuleEngine
{
    // Letter queue mechanics
    int CalculateLeverage(string npcId, ConnectionType tokenType);
    int CalculateLetterPosition(Letter letter, int npcTokenBalance);
    int CalculateSkipCost(int fromPosition, Letter letter);
    bool CanSkipToPosition(Letter letter, int targetPosition);

    // Token economy
    int GetTokenThresholdForCategory(LetterCategory category);
    bool HasRelationshipForCategory(string npcId, LetterCategory category);
    LetterCategory DetermineLetterCategory(int tokenCount);

    // Travel mechanics
    int CalculateTravelStamina(RouteOption route);
    bool CanTravel(Player player, RouteOption route);

    // Time management
    TimeBlocks GetTimeBlock(int hour);
    int GetActiveHoursRemaining(int currentHour);
    bool IsNPCAvailable(NPC npc, TimeBlocks timeBlock);

    // Stamina and recovery
    int CalculateRestRecovery(string lodgingType);
    int CalculateMaxStamina(Player player);

    // Debt and emergency actions
    bool ShouldOfferEmergencyActions(Player player);
    int CalculateDebtLeverage(int tokenBalance);

    // Letter payments and deadlines
    (int min, int max) GetPaymentRangeForCategory(LetterCategory category);
    int CalculateLateDeliveryPenalty(Letter letter, int daysLate);
    int CalculateChainLetterBonus(Letter letter);

    // Patron mechanics  
    bool ShouldGeneratePatronLetter(int daysSinceLastLetter);
    int DeterminePatronLetterPosition();
}

/// <summary>
/// Result of action validation with detailed reasons
/// </summary>
public class ActionValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationFailure> Failures { get; set; } = new();

    public void AddFailure(string reason, bool canBeRemedied = false)
    {
        Failures.Add(new ValidationFailure { Reason = reason, CanBeRemedied = canBeRemedied });
    }
}

public class ValidationFailure
{
    public string Reason { get; set; }
    public bool CanBeRemedied { get; set; }
}