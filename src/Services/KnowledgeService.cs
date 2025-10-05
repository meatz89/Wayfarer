/// <summary>
/// Service for managing player knowledge and secrets
/// NOT investigation-specific - used by conversations and other systems
/// </summary>
public class KnowledgeService
{
    public void GrantKnowledge(Player player, string knowledgeId)
    {
        if (player?.Knowledge != null && !string.IsNullOrEmpty(knowledgeId))
        {
            player.Knowledge.AddKnowledge(knowledgeId);
        }
    }

    public void GrantSecret(Player player, string secretId)
    {
        if (player?.Knowledge != null && !string.IsNullOrEmpty(secretId))
        {
            player.Knowledge.AddSecret(secretId);
        }
    }

    public bool HasKnowledge(Player player, string knowledgeId)
    {
        return player?.Knowledge?.HasKnowledge(knowledgeId) ?? false;
    }

    public bool KnowsSecret(Player player, string secretId)
    {
        return player?.Knowledge?.KnowsSecret(secretId) ?? false;
    }
}
