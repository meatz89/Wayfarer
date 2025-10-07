/// <summary>
/// Investigation discipline - categorical card property that determines specialization.
/// Cards with discipline matching location profile get specialist bonuses.
/// Parser must convert JSON strings to these enum values.
///
/// Parallel to ConversationalMove in Social system - this is a CARD PROPERTY,
/// not an environmental property. Locations have investigation profiles that
/// reference these disciplines.
/// </summary>
public enum InvestigationDiscipline
{
    /// <summary>
    /// Archives, books, documents, scholarly investigation
    /// </summary>
    Research,

    /// <summary>
    /// Perception, noticing details, surveillance, watching
    /// </summary>
    Observation,

    /// <summary>
    /// Logic, reasoning, connecting clues, analytical thinking
    /// </summary>
    Deduction,

    /// <summary>
    /// Questioning people, social investigation, interpersonal inquiry
    /// </summary>
    Interrogation,

    /// <summary>
    /// Stealth investigation, unauthorized access, covert examination
    /// </summary>
    Infiltration
}
