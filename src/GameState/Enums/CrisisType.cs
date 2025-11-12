/// <summary>
/// Type of personal crisis an NPC is experiencing.
/// Determines narrative framing for social challenges with NPCs in DISCONNECTED state.
/// Derived from NPC data at parse-time or spawning, not runtime detection.
/// </summary>
public enum CrisisType
{
    /// <summary>
    /// No crisis - NPC is not in distress
    /// </summary>
    None,

    /// <summary>
    /// Forced marriage crisis - NPC facing unwanted arranged marriage
    /// </summary>
    ForcedMarriage,

    /// <summary>
    /// Financial troubles - NPC struggling with debt or money problems
    /// </summary>
    FinancialTroubles,

    /// <summary>
    /// Family crisis - NPC dealing with family conflicts or children issues
    /// </summary>
    FamilyCrisis,

    /// <summary>
    /// Personal troubles - Generic crisis for DISCONNECTED NPCs without specific type
    /// </summary>
    PersonalTroubles
}
