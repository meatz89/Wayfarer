using System.Collections.Generic;

/// <summary>
/// Represents a card for resource exchanges.
/// Completely independent from ConversationCard - no inheritance, no shared mechanics.
/// Exchange cards are purely transactional with deterministic resource flows.
/// </summary>
public class ExchangeCard
{
    /// <summary>
    /// Unique identifier for this exchange card template.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Display name for this exchange.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Detailed description of what this exchange offers.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The type of exchange this card represents.
    /// </summary>
    public ExchangeType ExchangeType { get; set; }

    /// <summary>
    /// The NPC offering this exchange.
    /// Empty string for location-based or system exchanges.
    /// </summary>
    public string NpcId { get; set; }

    /// <summary>
    /// Cost structure for this exchange.
    /// All costs must be paid atomically.
    /// </summary>
    public ExchangeCostStructure Cost { get; set; } = new ExchangeCostStructure();

    /// <summary>
    /// Reward structure for this exchange.
    /// All rewards are granted atomically upon success.
    /// </summary>
    public ExchangeRewardStructure Reward { get; set; } = new ExchangeRewardStructure();

    /// <summary>
    /// Whether this exchange can only be performed once per game.
    /// </summary>
    public bool SingleUse { get; set; }

    /// <summary>
    /// Whether this exchange has been completed (for single-use exchanges).
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Success rate for this exchange (0-100).
    /// 100 means guaranteed success, lower values introduce risk.
    /// </summary>
    public int SuccessRate { get; set; } = 100;

    /// <summary>
    /// What happens if the exchange fails (for risky exchanges).
    /// Null means no penalty on failure.
    /// </summary>
    public ExchangeCostStructure FailurePenalty { get; set; }

    /// <summary>
    /// Display properties for UI rendering.
    /// </summary>
    public string IconId { get; set; }
    public string FlavorText { get; set; }

    /// <summary>
    /// Skeleton tracking - consistent with other entities.
    /// Skeletons are placeholder cards created when referenced but not yet defined.
    /// </summary>
    public bool IsSkeleton { get; set; }
    public string SkeletonSource { get; set; }

    /// <summary>
    /// Location requirement for this exchange.
    /// Null means available everywhere, otherwise must be at specified location.
    /// </summary>
    public string RequiredLocationId { get; set; }

    /// <summary>
    /// Time block requirements for this exchange.
    /// Empty list means available at all times.
    /// </summary>
    public List<TimeBlocks> AvailableTimeBlocks { get; set; } = new List<TimeBlocks>();

    /// <summary>
    /// Checks if this exchange is currently available.
    /// Does not check if player can afford it, only availability.
    /// </summary>
    public bool IsAvailable(string currentLocationId, TimeBlocks currentTimeBlock)
    {
        // Check if already completed for single-use exchanges
        if (SingleUse && IsCompleted)
            return false;

        // Check location requirement
        if (!string.IsNullOrEmpty(RequiredLocationId) &&
            currentLocationId != RequiredLocationId)
            return false;

        // Check time block requirement
        if (AvailableTimeBlocks.Count > 0 &&
            !AvailableTimeBlocks.Contains(currentTimeBlock))
            return false;

        return true;
    }

    /// <summary>
    /// Creates a deep clone of this exchange card.
    /// </summary>
    public ExchangeCard DeepClone()
    {
        return new ExchangeCard
        {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description,
            ExchangeType = this.ExchangeType,
            NpcId = this.NpcId,
            Cost = this.Cost?.DeepClone() ?? new ExchangeCostStructure(),
            Reward = this.Reward?.DeepClone() ?? new ExchangeRewardStructure(),
            SingleUse = this.SingleUse,
            IsCompleted = this.IsCompleted,
            SuccessRate = this.SuccessRate,
            FailurePenalty = this.FailurePenalty?.DeepClone(),
            IconId = this.IconId,
            FlavorText = this.FlavorText,
            IsSkeleton = this.IsSkeleton,
            SkeletonSource = this.SkeletonSource,
            RequiredLocationId = this.RequiredLocationId,
            AvailableTimeBlocks = new List<TimeBlocks>(this.AvailableTimeBlocks)
        };
    }

    /// <summary>
    /// Gets the exchange ratio as a readable string.
    /// </summary>
    public string GetExchangeRatio()
    {
        string costDesc = Cost?.GetDescription() ?? "Nothing";
        string rewardDesc = Reward?.GetDescription() ?? "Nothing";
        return $"{costDesc} → {rewardDesc}";
    }

    /// <summary>
    /// Determines if this is a risky exchange (can fail).
    /// </summary>
    public bool IsRisky()
    {
        return SuccessRate < 100;
    }
}