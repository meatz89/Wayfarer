using System.Collections.Generic;

/// <summary>
/// Central interface for all game mechanics and rule calculations.
/// This separates game logic from UI and infrastructure concerns.
/// </summary>
public interface IGameRuleEngine
{
    // DeliveryObligation queue mechanics
    int CalculateLeverage(string npcId, ConnectionType tokenType);
    int CalculateLetterPosition(DeliveryObligation letter, int npcTokenBalance);
    int CalculateSkipCost(int fromPosition, DeliveryObligation letter);
    bool CanSkipToPosition(DeliveryObligation letter, int targetPosition);

    // Token economy
    int GetTokenThresholdForCategory(LetterCategory category);
    bool HasRelationshipForCategory(string npcId, LetterCategory category);
    LetterCategory DetermineLetterCategory(int tokenCount);

    // Travel mechanics
    int CalculateTravelStamina(RouteOption route);
    bool CanTravel(Player player, RouteOption route);

    // Time management
    TimeBlocks GetTimeBlock(int segment);
    int GetActiveSegmentsRemaining(int currentSegment);
    bool IsNPCAvailable(NPC npc, TimeBlocks timeBlock);

    // Stamina and recovery
    int CalculateRestRecovery(string lodgingType);
    int CalculateMaxStamina(Player player);

    // Debt and emergency actions
    bool ShouldOfferEmergencyActions(Player player);
    int CalculateDebtLeverage(int tokenBalance);

    // Letter payments and deadlines
    PaymentRange GetPaymentRangeForCategory(LetterCategory category);
    int CalculateLateDeliveryPenalty(DeliveryObligation letter, int daysLate);

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