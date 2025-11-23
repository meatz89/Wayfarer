/// <summary>
/// Describes what triggered a scene spawn event
/// </summary>
enum SpawnTriggerType
{
    /// <summary>Game start, no parent (authored content)</summary>
    Initial,

    /// <summary>Authored tutorial progression</summary>
    Tutorial,

    /// <summary>Spawned by choice reward</summary>
    ChoiceReward,

    /// <summary>Spawned by situation success</summary>
    SituationSuccess,

    /// <summary>Spawned by situation failure</summary>
    SituationFailure,

    /// <summary>Spawned during day/time advancement</summary>
    DayTransition
}
