/// <summary>
/// Defines the semantic type of a Situation within a Scene.
/// Used to mark Crisis moments that test player preparation.
/// </summary>
public enum SituationType
{
/// <summary>
/// Standard situation with normal costs and requirements.
/// Used for building stats, gathering resources, and scene progression.
/// </summary>
Normal,

/// <summary>
/// Crisis situation that tests player preparation.
/// Typically the final situation in a Scene with high stat requirements
/// or expensive alternatives, creating tension and strategic planning.
/// </summary>
Crisis
}
