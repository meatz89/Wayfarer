/// <summary>
/// Obligation discipline - categorical card property that determines specialization.
/// Cards with discipline matching Venue profile get specialist bonuses.
/// Parser must convert JSON strings to these enum values.
///
/// Parallel to ConversationalMove in Social system - this is a CARD PROPERTY,
/// not an environmental property. Locations have obligation profiles that
/// reference these disciplines.
/// </summary>
public enum ObligationDiscipline
{
/// <summary>
/// Archives, books, documents, scholarly obligation
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
/// Questioning people, social obligation, interpersonal inquiry
/// </summary>
Interrogation,

/// <summary>
/// Stealth obligation, unauthorized access, covert examination
/// </summary>
Infiltration
}
