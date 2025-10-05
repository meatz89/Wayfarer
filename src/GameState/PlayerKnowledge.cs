public class PlayerKnowledge
{
    private List<string> _acquiredKnowledge = new List<string>();
    private List<string> _knownSecrets = new List<string>();

    public bool HasKnowledge(string knowledgeId)
    {
        return _acquiredKnowledge.Contains(knowledgeId);
    }

    public bool KnowsSecret(string secretId)
    {
        return _knownSecrets.Contains(secretId);
    }

    public void AddKnowledge(string knowledgeId)
    {
        if (!string.IsNullOrEmpty(knowledgeId) && !_acquiredKnowledge.Contains(knowledgeId))
        {
            _acquiredKnowledge.Add(knowledgeId);
        }
    }

    public void AddSecret(string secretId)
    {
        if (!string.IsNullOrEmpty(secretId) && !_knownSecrets.Contains(secretId))
        {
            _knownSecrets.Add(secretId);
        }
    }

    public List<string> GetAllKnowledge()
    {
        return new List<string>(_acquiredKnowledge);
    }

    public List<string> GetAllSecrets()
    {
        return new List<string>(_knownSecrets);
    }
}
