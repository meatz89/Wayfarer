public class CharacterRelationshipSystem
{
    private List<int> trustLevels = new();
    private List<KnowledgePiece> unlockedCharacterKnowledge = new();

    public int GetTrustLevel(string character)
    {
        return 1;
    }

    public void ModifyTrust(string character, int count)
    {
    }

    public bool HasUnlockedKnowledge(string character, KnowledgePiece knowledge)
    {
        return true;
    }
}
