public class LocationAccess
{
    private List<KnowledgeFlags> requiredKnowledge = new();
    private List<LocationSpotNames> spotAccess = new();

    public bool CanAccessLocation(LocationNames location, List<KnowledgeFlags> playerKnowledge)
    {
        return true;
    }

    public bool CanAccessSpot(LocationSpotNames spot, List<KnowledgeFlags> playerKnowledge)
    {
        return true;
    }
}