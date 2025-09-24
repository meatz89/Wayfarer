/// <summary>
/// Defines bonuses that cards gain when reaching specific levels
/// </summary>
public class CardLevelBonus
{
    /// <summary>
    /// Additional success rate percentage (e.g., 10 for +10%)
    /// </summary>
    public int? SuccessBonus { get; set; }

    /// <summary>
    /// Adds or changes persistence type when reaching this level
    /// </summary>
    public PersistenceType? AddPersistence { get; set; }

    /// <summary>
    /// Additional cards drawn on successful play
    /// </summary>
    public int? AddDrawOnSuccess { get; set; }

    /// <summary>
    /// If true, card does not force LISTEN on failure
    /// </summary>
    public bool? IgnoreFailureListen { get; set; }
}