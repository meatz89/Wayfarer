/// <summary>
/// Types of states that can affect the player
/// Fixed list of 20 states across 3 categories (Physical, Mental, Social)
/// </summary>
public enum StateType
{
// Physical States (8 types)
/// <summary>Suffering from a serious injury that impairs movement and action</summary>
Wounded,
/// <summary>Completely drained of energy, struggling to perform basic tasks</summary>
Exhausted,
/// <summary>Afflicted by illness that weakens the body</summary>
Sick,
/// <summary>Minor injury that causes discomfort but doesn't prevent action</summary>
Injured,
/// <summary>Desperately hungry, body weakening from lack of food</summary>
Starving,
/// <summary>Equipped with weapons, ready for confrontation</summary>
Armed,
/// <summary>Well-supplied with food and resources for travel</summary>
Provisioned,
/// <summary>Fully recovered and energized</summary>
Rested,

// Mental States (5 types)
/// <summary>Mind clouded, struggling to think clearly or make decisions</summary>
Confused,
/// <summary>Psychological scarring from a disturbing experience</summary>
Traumatized,
/// <summary>Mind sharp and creative, seeing connections others miss</summary>
Inspired,
/// <summary>Intensely concentrated, able to tackle complex problems</summary>
Focused,
/// <summary>Fixated on a particular goal or mystery to the exclusion of all else</summary>
Obsessed,

// Social States (7 types)
/// <summary>Sought by authorities, must avoid official attention</summary>
Wanted,
/// <summary>Renowned for recent deeds, recognized positively by others</summary>
Celebrated,
/// <summary>Rejected by the community, socially isolated</summary>
Shunned,
/// <summary>Public embarrassment that affects confidence and social standing</summary>
Humiliated,
/// <summary>Concealing true identity behind a false persona</summary>
Disguised,
/// <summary>Owing a significant favor or debt to someone</summary>
Indebted,
/// <summary>Earned the confidence of important individuals or groups</summary>
Trusted
}
