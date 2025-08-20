/// <summary>
/// Categorical severity levels for narrative events.
/// Backend determines importance, frontend translates to UI styling/messaging.
/// </summary>
public enum NarrativeSeverity
{
    Info,       // Neutral information
    Success,    // Positive outcome
    Warning,    // Caution needed
    Danger,     // Critical situation
    Celebration // Major positive milestone
}

/// <summary>
/// Categorical types for special letter delivery events.
/// Backend sets these categories, frontend translates to narrative text.
/// </summary>
public enum SpecialLetterEventType
{
    // Introduction letter events
    IntroductionLetterGenerated,    // DeliveryObligation successfully generated and added to satchel
    NPCIntroduced,
    IntroductionLetterIncomplete,
    IntroductionTargetNotFound,

    // Access permit events
    AccessPermitGenerated,          // DeliveryObligation successfully generated and added to satchel
    LocationAccessGranted,
    RouteAccessGranted,
    AccessPermitIncomplete,
    AccessTargetNotFound,


    // Token bonus events
    SpecialLetterTokenBonus,

    // General events
    SpecialLetterDelivered,
    SpecialLetterProcessingError
}

/// <summary>
/// Categorical data for special letter events.
/// Backend provides structured data, frontend translates to user-facing text.
/// </summary>
public class SpecialLetterEvent
{
    public SpecialLetterEventType EventType { get; set; }
    public LetterSpecialType LetterType { get; set; }
    public string TargetNPCId { get; set; } = string.Empty;
    public string TargetLocationId { get; set; } = string.Empty;
    public string TargetRouteId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public ConnectionType TokenType { get; set; }
    public int TokenAmount { get; set; }
    public NarrativeSeverity Severity { get; set; } = NarrativeSeverity.Info;
    
    // Additional fields for letter generation events
    public int Position { get; set; }        // Position in satchel (always 0 for special letters)
    public int TokenCost { get; set; }       // Tokens spent to generate the letter
}