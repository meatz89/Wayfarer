/// <summary>
/// Determines how situations within a Scene progress after each choice execution.
/// Controls whether player returns to menu (Breathe) or continues to next situation (Cascade).
/// </summary>
public enum ProgressionMode
{
    /// <summary>
    /// After each situation resolves, player returns to atmospheric menu (scene decides when to end).
    /// Allows player to take breaks, check resources, and choose when to continue.
    /// </summary>
    Breathe,

    /// <summary>
    /// After each situation resolves, next situation immediately appears (continuous flow).
    /// Creates pressure and momentum, situations cascade until scene completion.
    /// </summary>
    Cascade
}
