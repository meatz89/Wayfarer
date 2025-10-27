
/// <summary>
/// Status of an NPC request
/// </summary>
public enum SituationStatus
{
    /// <summary>
    /// Request is available to attempt
    /// </summary>
    Available,

    /// <summary>
    /// Request has been completed (any card was successfully played)
    /// </summary>
    Completed,

    /// <summary>
    /// Request was failed and is no longer available
    /// </summary>
    Failed
}
