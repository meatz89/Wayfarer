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
    public Dictionary<PlayerStatType, int> StatRequirements { get; set; } = new Dictionary<PlayerStatType, int>();

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

        foreach (KeyValuePair<PlayerStatType, int> statReq in StatRequirements)
        {
            int currentLevel = statReq.Key switch
            {
                PlayerStatType.Insight => player.Insight,
                PlayerStatType.Rapport => player.Rapport,
                PlayerStatType.Authority => player.Authority,
                PlayerStatType.Diplomacy => player.Diplomacy,
                PlayerStatType.Cunning => player.Cunning,
                _ => 0
            };
            if (currentLevel < statReq.Value)
                return false;
        }

        return true;
    }
}
