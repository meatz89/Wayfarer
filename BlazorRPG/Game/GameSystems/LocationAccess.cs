public class LocationAccess
{
    private List<KnowledgeFlags> requiredKnowledge = new();
    private List<LocationSpotTypes> spotAccess = new();

    public bool CanAccessLocation(LocationNames location, List<KnowledgeFlags> playerKnowledge)
    {
        return true;
    }

    public bool CanAccessSpot(LocationSpotTypes spot, List<KnowledgeFlags> playerKnowledge)
    {
        return true;
    }
}