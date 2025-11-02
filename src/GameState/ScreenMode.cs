/// <summary>
/// Screen-level navigation modes for the game.
/// Used by IntentResult to communicate screen-level navigation from backend to UI.
/// </summary>
public enum ScreenMode
{
    Location,
    Scene,           // Full-screen scene display (Sir Brante forced moment)
    Exchange,
    Travel,
    SocialChallenge,
    MentalChallenge,
    PhysicalChallenge,
    ConversationTree,
    Observation,
    Emergency,
    HexMap
}
