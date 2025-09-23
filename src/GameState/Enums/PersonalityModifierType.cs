/// <summary>
/// Types of personality modifiers that affect conversation mechanics
/// Each personality type has one unique modifier that changes gameplay
/// </summary>
public enum PersonalityModifierType
{
    None,
    AscendingFocusRequired,  // Proud - Cards must be played in ascending focus order each turn
    MomentumLossDoubled,     // Devoted - All momentum losses doubled (momentum erosion, card effects, etc.)
    HighestFocusBonus,       // Mercantile - Your highest focus card each turn gains +30% success
    RepeatFocusPenalty,      // Cunning - Playing same focus as previous card costs -2 rapport
    RapportChangeCap         // Steadfast - All rapport changes capped at ±2
}