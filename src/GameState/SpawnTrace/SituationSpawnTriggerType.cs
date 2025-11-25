/// <summary>
/// Describes what triggered a situation spawn event
/// </summary>
public enum SituationSpawnTriggerType
{
    /// <summary>Initial situation in scene (parse-time)</summary>
    InitialScene,

    /// <summary>Spawned by parent situation success</summary>
    SuccessSpawn,

    /// <summary>Spawned by parent situation failure</summary>
    FailureSpawn
}
