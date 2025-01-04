public class CharacterRelationshipSystem
{
    private List<int> trustLevels = new();
    private List<KnowledgeFlags> unlockedCharacterKnowledge = new();

    public int GetTrustLevel(CharacterNames character)
    {
        return 1;
    }

    public void ModifyTrust(CharacterNames character, int count)
    {
    }

    public bool HasUnlockedKnowledge(CharacterNames character, KnowledgeFlags knowledge)
    {
        return true;
    }
}
