/// <summary>
/// Determines how a Scene is presented to the player when they arrive at a location.
/// Controls whether scene appears as menu option (Atmospheric) or takes over full screen (Modal).
/// </summary>
public enum PresentationMode
{
    /// <summary>
    /// Scene appears as a choice in the normal atmospheric menu (existing behavior).
    /// Player can freely browse other options and choose when to engage.
    /// </summary>
    Atmospheric,

    /// <summary>
    /// Scene immediately takes over the full screen on location entry (Sir Brante forced moment).
    /// Player must engage with the scene's situations before returning to normal gameplay.
    /// </summary>
    Modal
}
