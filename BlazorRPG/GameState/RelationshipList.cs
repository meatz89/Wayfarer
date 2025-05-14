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
        foreach (Relationship relationship in _relationships)
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

    internal RelationshipList Clone()
    {
        // Create a new empty RelationshipList
        RelationshipList clone = new RelationshipList();

        // Create a new list for the clone's relationships
        clone._relationships = new List<Relationship>();

        // Create new Relationship objects for each relationship in the original list
        foreach (Relationship relationship in _relationships)
        {
            // Create a new Relationship with the same values
            Relationship relationshipCopy = new Relationship
            {
                Character = relationship.Character,
                Level = relationship.Level
                // Copy any other properties that Relationship might have
            };

            // Add the copied relationship to the clone's list
            clone._relationships.Add(relationshipCopy);
        }

        return clone;
    }
}