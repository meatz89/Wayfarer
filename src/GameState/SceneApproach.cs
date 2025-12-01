/// <summary>
/// SceneApproach - A way to attempt a TravelScene.
/// Each approach has requirements, probability of success, and different outcomes.
/// Part of Physical challenge system (route obstacles).
/// </summary>
public class SceneApproach
{
    // HIGHLANDER: NO Id property - SceneApproach identified by object reference
    public string Name { get; set; }
    public string Description { get; set; }
    public int SuccessProbability { get; set; }

    // Requirements
    public int StaminaRequired { get; set; }
    // DOMAIN COLLECTION PRINCIPLE: List<T> instead of Dictionary
    public List<StatThresholdEntry> StatRequirements { get; set; } = new List<StatThresholdEntry>();

    // Outcomes
    public SceneOutcome SuccessOutcome { get; set; }
    public SceneOutcome FailureOutcome { get; set; }

    /// <summary>
    /// Check if player can use this approach (meets all requirements)
    /// </summary>
    public bool CanUseApproach(Player player, ItemRepository itemRepository)
    {
        if (player.Stamina < StaminaRequired)
            return false;

        foreach (StatThresholdEntry statReq in StatRequirements)
        {
            int currentLevel = statReq.Stat switch
            {
                PlayerStatType.Insight => player.Insight,
                PlayerStatType.Rapport => player.Rapport,
                PlayerStatType.Authority => player.Authority,
                PlayerStatType.Diplomacy => player.Diplomacy,
                PlayerStatType.Cunning => player.Cunning,
                _ => 0
            };
            if (currentLevel < statReq.Threshold)
                return false;
        }

        return true;
    }
}
