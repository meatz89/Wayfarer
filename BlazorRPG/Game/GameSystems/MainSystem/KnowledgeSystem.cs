public class KnowledgeSystem
{
    private readonly GameState gameState;
    private List<KnowledgeFlags> knowledgePoints = new();
    private List<KnowledgeFlags> unlockedKnowledge = new();

    public bool HasKnowledge(KnowledgeFlags flag) => unlockedKnowledge.Contains(flag);
    public void UnlockKnowledge(KnowledgeFlags flag)
    {

    }
}

public enum KnowledgeFlags
{
    SafeRoute,
    LoadingTechnique,
    GATHERingSpot,
    StorageSpot,
    Schedule,
    Preference,
    Motivation
}
