
public class Knowledge
{
    public KnowledgeTypes KnowledgeType { get; set; }
    // You might add more properties here, like a description or a source

    public Knowledge(KnowledgeTypes knowledgeType)
    {
        KnowledgeType = knowledgeType;
    }
}
