public class KnowledgePiece
{
    public KnowledgeTags Tag { get; }
    public KnowledgeCategories Category { get; }
    public int Level { get; set; }
    public List<string> UnlockedLocations { get; }
    public List<BasicActionTypes> UnlockedActions { get; }

    public KnowledgePiece(KnowledgeTags tag, KnowledgeCategories category)
    {
        Tag = tag;
        Category = category;
        Level = 1;
        UnlockedLocations = new List<string>();
        UnlockedActions = new List<BasicActionTypes>();
    }
}
