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
    // DOMAIN COLLECTION PRINCIPLE: Explicit properties for fixed enum (PlayerStatType)
    public int InsightRequired { get; set; }
    public int RapportRequired { get; set; }
    public int AuthorityRequired { get; set; }
    public int DiplomacyRequired { get; set; }
    public int CunningRequired { get; set; }

    public int GetStatRequired(PlayerStatType stat) => stat switch
    {
        PlayerStatType.Insight => InsightRequired,
        PlayerStatType.Rapport => RapportRequired,
        PlayerStatType.Authority => AuthorityRequired,
        PlayerStatType.Diplomacy => DiplomacyRequired,
        PlayerStatType.Cunning => CunningRequired,
        _ => 0
    };

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

        // Check explicit stat requirements
        if (player.Insight < InsightRequired) return false;
        if (player.Rapport < RapportRequired) return false;
        if (player.Authority < AuthorityRequired) return false;
        if (player.Diplomacy < DiplomacyRequired) return false;
        if (player.Cunning < CunningRequired) return false;

        return true;
    }
}
