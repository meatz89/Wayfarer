public class KnowledgeRequirement
{
    public List<string> RequiredKnowledge { get; set; } = new List<string>();
    public List<string> RequiredSecrets { get; set; } = new List<string>();

    public bool MeetsRequirements(PlayerKnowledge playerKnowledge)
    {
        foreach (string knowledge in RequiredKnowledge)
        {
            if (!playerKnowledge.HasKnowledge(knowledge))
            {
                return false;
            }
        }

        foreach (string secret in RequiredSecrets)
        {
            if (!playerKnowledge.KnowsSecret(secret))
            {
                return false;
            }
        }

        return true;
    }

    public List<string> GetMissingRequirements(PlayerKnowledge playerKnowledge)
    {
        List<string> missing = new List<string>();

        foreach (string knowledge in RequiredKnowledge)
        {
            if (!playerKnowledge.HasKnowledge(knowledge))
            {
                missing.Add($"Knowledge: {knowledge}");
            }
        }

        foreach (string secret in RequiredSecrets)
        {
            if (!playerKnowledge.KnowsSecret(secret))
            {
                missing.Add($"Secret: {secret}");
            }
        }

        return missing;
    }
}
