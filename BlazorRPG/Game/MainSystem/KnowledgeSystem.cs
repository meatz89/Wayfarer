public class KnowledgeSystem
{
    private readonly GameState gameState;

    private List<Knowledge> knowledgePoints = new();
    private List<Knowledge> unlockedKnowledge = new();

    public bool HasKnowledge(Knowledge flag) => unlockedKnowledge.Contains(flag);
    public void UnlockKnowledge(Knowledge flag)
    {

    }
}
