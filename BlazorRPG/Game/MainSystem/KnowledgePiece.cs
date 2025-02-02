public class KnowledgePiece
{
    public KnowledgeTags Tag { get; }
    public KnowledgeCategories Category { get; }
    public int Level { get; private set; }
    public List<LocationNames> UnlockedLocations { get; }
    public List<BasicActionTypes> UnlockedActions { get; }

    public KnowledgePiece(KnowledgeTags tag, KnowledgeCategories category)
    {
        Tag = tag;
        Category = category;
        Level = 1;
        UnlockedLocations = new List<LocationNames>();
        UnlockedActions = new List<BasicActionTypes>();
    }
}
