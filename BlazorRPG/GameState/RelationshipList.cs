public class RelationshipList
{
    private List<Relationship> _relationships = new List<Relationship>();

    public int GetLevel(string characterName)
    {
        foreach (Relationship relationship in _relationships)
        {
            if (relationship.Character == characterName)
                return relationship.Level;
        }
        return 0;
    }

    public void SetLevel(string characterName, int level)
    {
        foreach (var relationship in _relationships)
        {
            if (relationship.Character == characterName)
            {
                relationship.Level = level;
                return;
            }
        }

        // Add new relationship if not found
        _relationships.Add(new Relationship { Character = characterName, Level = level });
    }

    public List<Relationship> GetAllRelationships()
    {
        return new List<Relationship>(_relationships);
    }
}