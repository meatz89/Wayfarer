public class CharacterRelationshipSystem
{
    private List<int> trustLevels = new();
    private List<KnowledgePiece> unlockedCharacterKnowledge = new();

    public int GetTrustLevel(CharacterNames character)
    {
        return 1;
    }

    public void ModifyTrust(CharacterNames character, int count)
    {
    }

    public bool HasUnlockedKnowledge(CharacterNames character, KnowledgePiece knowledge)
    {
        return true;
    }
}
