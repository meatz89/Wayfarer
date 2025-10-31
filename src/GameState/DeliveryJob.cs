using Wayfarer.GameState.Enums;

/// <summary>
/// Represents a procedurally generated delivery job connecting two Commercial locations.
/// Jobs are generated at parse time from available routes by DeliveryJobCatalog.
/// Players can accept ONE active delivery job at a time.
/// </summary>
public class DeliveryJob
{
    // ============================================
    // IDENTITY
    // ============================================

    /// <summary>
    /// Unique identifier for this job.
    /// Format: "delivery_{originId}_to_{destinationId}"
    /// </summary>
    public string Id { get; set; } = "";

    // ============================================
    // ROUTING
    // ============================================

    /// <summary>
    /// Location where this job is offered (job board location).
    /// Must be a Commercial location.
    /// </summary>
    public string OriginLocationId { get; set; } = "";

    /// <summary>
    /// Location where delivery must be completed.
    /// Must be a Commercial location.
    /// </summary>
    public string DestinationLocationId { get; set; } = "";

    /// <summary>
    /// Route connecting origin to destination.
    /// Used for calculating payment and displaying route info.
    /// </summary>
    public string RouteId { get; set; } = "";

    // ============================================
    // ECONOMICS
    // ============================================

    /// <summary>
    /// Payment in coins awarded on delivery completion.
    /// Formula: RoomCost + TravelCost + DifficultyBonus
    /// Calculated at parse time by DeliveryJobCatalog.
    /// </summary>
    public int Payment { get; set; }

    // ============================================
    // NARRATIVE
    // ============================================

    /// <summary>
    /// Short cargo description for narrative flavor.
    /// Examples: "a letter", "a sack of grain", "valuable documents"
    /// Procedurally generated based on difficulty tier.
    /// </summary>
    public string CargoDescription { get; set; } = "";

    /// <summary>
    /// Full job description displayed in job board.
    /// Format: "Deliver {cargo} to {destination}"
    /// </summary>
    public string JobDescription { get; set; } = "";

    // ============================================
    // DIFFICULTY
    // ============================================

    /// <summary>
    /// Categorical difficulty tier.
    /// Determines payment bonus and cargo type.
    /// Simple: short route, low danger, +3 coins
    /// Moderate: medium route, medium danger, +5 coins
    /// Dangerous: long route, high danger, +10 coins
    /// </summary>
    public DifficultyTier DifficultyTier { get; set; }

    // ============================================
    // AVAILABILITY
    // ============================================

    /// <summary>
    /// Can this job currently be accepted?
    /// Set to false after acceptance to prevent duplicate assignments.
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Time blocks when this job is offered at the job board.
    /// Empty list = available at all times.
    /// Typically: Morning, Midday (jobs refresh daily).
    /// </summary>
    public List<TimeBlocks> AvailableAt { get; set; } = new();

    // ============================================
    // INITIALIZATION (LET IT CRASH PHILOSOPHY)
    // ============================================

    /// <summary>
    /// Entity initialization - all collections initialized inline.
    /// Parser must set all required properties (Id, locations, payment, etc.).
    /// No null collections - violations throw exceptions.
    /// </summary>
    public DeliveryJob()
    {
        // Collections initialized inline (LET IT CRASH philosophy)
        AvailableAt = new List<TimeBlocks>();
    }
}
