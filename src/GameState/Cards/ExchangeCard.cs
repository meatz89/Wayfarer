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
    public string FlavorText { get; set; }

    /// <summary>
    /// Skeleton tracking - consistent with other entities.
    /// Skeletons are placeholder cards created when referenced but not yet defined.
    /// </summary>
    public bool IsSkeleton { get; set; }
    public string SkeletonSource { get; set; }

    /// <summary>
    /// Venue requirement for this exchange.
    /// Null means available everywhere, otherwise must be at specified location.
    /// </summary>
    public string RequiredVenueId { get; set; }

    /// <summary>
    /// Time block requirements for this exchange.
    /// Empty list means available at all times.
    /// </summary>
    public List<TimeBlocks> AvailableTimeBlocks { get; set; } = new List<TimeBlocks>();

    // VALIDATION REQUIREMENTS

    /// <summary>
    /// Domain tags required for this exchange (e.g., "market", "tavern").
    /// Empty list means no domain restrictions.
    /// </summary>
    public List<string> RequiredDomains { get; set; } = new List<string>();

    /// <summary>
    /// Minimum relationship tier required to access this exchange.
    /// 0 means no relationship requirement.
    /// </summary>
    public int MinimumRelationshipTier { get; set; }

    /// <summary>
    /// Connection state required for this exchange (e.g., must be TRUSTING).
    /// Null means no specific state required.
    /// </summary>
    public ConnectionState? RequiredConnectionState { get; set; }

    /// <summary>
    /// Whether this exchange requires patience from the NPC.
    /// If true and NPC has no patience, exchange is unavailable.
    /// </summary>
    public bool RequiresPatience { get; set; }

    // EXCHANGE EFFECTS

    /// <summary>
    /// Exchange ID that becomes unlocked after completing this exchange.
    /// Null means no exchange is unlocked.
    /// </summary>
    public string UnlocksExchangeId { get; set; }

    /// <summary>
    /// Story event ID that triggers when this exchange completes.
    /// Null means no story event.
    /// </summary>
    public string TriggerEvent { get; set; }

    /// <summary>
    /// Whether completing this exchange affects the relationship state.
    /// </summary>
    public bool AffectsRelationship { get; set; }

    /// <summary>
    /// Relationship flow modifier applied when exchange completes.
    /// Positive values improve relationship, negative values harm it.
    /// </summary>
    public int FlowModifier { get; set; }

    /// <summary>
    /// Connection state that results from completing this exchange.
    /// Null means no state change.
    /// </summary>
    public ConnectionState? ConnectionStateChange { get; set; }

    /// <summary>
    /// Whether this exchange consumes NPC patience.
    /// </summary>
    public bool ConsumesPatience { get; set; }

    /// <summary>
    /// Amount of patience consumed by this exchange.
    /// Only relevant if ConsumesPatience is true.
    /// </summary>
    public int PatienceCost { get; set; }

    /// <summary>
    /// Whether this exchange advances time.
    /// </summary>
    public bool AdvancesTime { get; set; }

    /// <summary>
    /// Number of hours to advance time when exchange completes.
    /// Only relevant if AdvancesTime is true.
    /// </summary>
    public int TimeAdvancementHours { get; set; }

    // RUNTIME TRACKING

    /// <summary>
    /// Number of times this exchange has been used.
    /// Tracked per save game.
    /// </summary>
    public int TimesUsed { get; set; }

    /// <summary>
    /// Maximum number of times this exchange can be used.
    /// 0 or negative means unlimited uses (unless SingleUse is true).
    /// </summary>
    public int MaxUses { get; set; }

    /// <summary>
    /// Checks if this exchange is currently available.
    /// Does not check if player can afford it, only availability.
    /// </summary>
    public bool IsAvailable(string currentVenueId, TimeBlocks currentTimeBlock)
    {
        // Check if already completed for single-use exchanges
        if (SingleUse && IsCompleted)
            return false;

        // Check Venue requirement
        if (!string.IsNullOrEmpty(RequiredVenueId) &&
            currentVenueId != RequiredVenueId)
            return false;

        // Check time block requirement
        if (AvailableTimeBlocks.Count > 0 &&
            !AvailableTimeBlocks.Contains(currentTimeBlock))
            return false;

        return true;
    }

    /// <summary>
    /// Gets the exchange ratio as a readable string.
    /// </summary>
    public string GetExchangeRatio()
    {
        string costDesc = Cost.GetDescription();
        string rewardDesc = Reward.GetDescription();
        return $"{costDesc} → {rewardDesc}";
    }

    /// <summary>
    /// Determines if this is a risky exchange (can fail).
    /// </summary>
    public bool IsRisky()
    {
        return SuccessRate < 100;
    }

    /// <summary>
    /// Gets cost resources as a flat list.
    /// </summary>
    public List<ResourceAmount> GetCostAsList()
    {
        return Cost.Resources;
    }

    /// <summary>
    /// Gets reward resources as a flat list.
    /// </summary>
    public List<ResourceAmount> GetRewardAsList()
    {
        return Reward.Resources;
    }

    /// <summary>
    /// Gets item rewards granted by this exchange.
    /// </summary>
    public List<string> GetItemRewards()
    {
        return Reward.ItemIds;
    }

    /// <summary>
    /// Checks if player can afford this exchange.
    /// </summary>
    public bool CanAfford(PlayerResourceState playerResources)
    {
        return Cost.CanAfford(playerResources);
    }

    /// <summary>
    /// Gets narrative description of the exchange.
    /// </summary>
    public string GetNarrativeContext()
    {
        string costDesc = Cost.GetDescription();
        string rewardDesc = Reward.GetDescription();
        return $"Trading {costDesc} for {rewardDesc}";
    }

    /// <summary>
    /// Checks if this exchange has been exhausted (can no longer be used).
    /// </summary>
    public bool IsExhausted()
    {
        if (SingleUse && IsCompleted)
            return true;

        if (MaxUses > 0 && TimesUsed >= MaxUses)
            return true;

        return false;
    }

    /// <summary>
    /// Records that this exchange was used once.
    /// </summary>
    public void RecordUse()
    {
        TimesUsed++;
        if (SingleUse)
        {
            IsCompleted = true;
        }
    }
}