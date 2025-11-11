/// <summary>
/// Ways to resolve legal and social penalties that result in state clearing
/// Used by StateClearingBehavior for penalty-based state clearing
/// </summary>
public enum PenaltyResolutionType
{
    /// <summary>
    /// Pay a monetary fine to clear legal penalty states
    /// </summary>
    PayFine,

    /// <summary>
    /// Repay an owed debt to clear social obligation states
    /// </summary>
    RepayDebt,

    /// <summary>
    /// Serve time penalty to clear criminal states
    /// </summary>
    ServeTime
}
