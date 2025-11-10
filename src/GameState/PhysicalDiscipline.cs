/// <summary>
/// Physical discipline - categorical card property that determines specialization.
/// Cards with discipline matching challenge type get specialist bonuses.
/// Parser must convert JSON strings to these enum values.
///
/// Parallel to ConversationalMove in Social system - this is a CARD PROPERTY,
/// not an environmental property. Challenge types reference these disciplines.
/// </summary>
public enum PhysicalDiscipline
{
/// <summary>
/// Offensive confrontations, fighting, combat techniques
/// </summary>
Combat,

/// <summary>
/// Climbing, jumping, running, acrobatic physical feats
/// </summary>
Athletics,

/// <summary>
/// Precision work, lockpicking, delicate manipulation
/// </summary>
Finesse,

/// <summary>
/// Sustained effort, resistance, stamina-based challenges
/// </summary>
Endurance,

/// <summary>
/// Raw power, breaking, forcing, overwhelming strength
/// </summary>
Strength
}
