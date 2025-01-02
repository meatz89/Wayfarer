public class ActionValidator
{
    private readonly KnowledgeSystem knowledgeSystem;
    private readonly CharacterRelationshipSystem relationshipSystem;
    private readonly LocationAccess locationAccess;

    public bool CanExecuteAction(BasicAction action, CharacterNames? character = null)
    {
        return true;
    }

    public List<string> GetBlockingRequirements(BasicAction action)
    {
        return new List<string>();
    }

}
