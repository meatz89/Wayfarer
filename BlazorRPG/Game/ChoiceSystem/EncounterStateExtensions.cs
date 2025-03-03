/// <summary>
/// Extension methods for the encounter state
/// </summary>
public static class EncounterStateExtensions
{
    /// <summary>
    /// Get a tag value from the appropriate dictionary
    /// </summary>
    public static int GetTagValue(this EncounterState state, ApproachTypes tag)
    {
        return state.ApproachTypesDic.ContainsKey(tag) ? state.ApproachTypesDic[tag] : 0;
    }

    /// <summary>
    /// Get a tag value from the appropriate dictionary
    /// </summary>
    public static int GetTagValue(this EncounterState state, FocusTypes tag)
    {
        return state.FocusTypesDic.ContainsKey(tag) ? state.FocusTypesDic[tag] : 0;
    }

    /// <summary>
    /// Calculate the momentum to pressure ratio
    /// </summary>
    public static float MomentumToPressureRatio(this EncounterState state)
    {
        return state.Pressure > 0 ? (float)state.Momentum / state.Pressure : float.MaxValue;
    }
}