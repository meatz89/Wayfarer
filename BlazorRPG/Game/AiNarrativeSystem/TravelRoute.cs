public class TravelRoute
{
    public Location Origin { get; private set; }
    public Location Destination { get; private set; }
    public int BaseTimeCost { get; private set; }
    public int BaseEnergyCost { get; private set; }
    public int DangerLevel { get; private set; } // 0-10 scale
    public List<string> RequiredEquipment { get; private set; } = new List<string>();
    public List<TravelEncounterContext> PotentialEncounters { get; private set; } = new List<TravelEncounterContext>();

    // Knowledge level tracking - how well player knows this route
    public int KnowledgeLevel { get; private set; }

    // Increase knowledge when traveling this route
    public void IncreaseKnowledge()
    {
        if (KnowledgeLevel < 3)
            KnowledgeLevel++;
    }

    // Calculate actual costs based on knowledge level
    public int GetActualTimeCost()
    {
        // Knowledge reduces time cost
        return BaseTimeCost - KnowledgeLevel;
    }

    public int GetActualEnergyCost()
    {
        // Knowledge reduces energy cost
        return BaseEnergyCost - KnowledgeLevel;
    }

    // Check if player has required equipment
    public bool CanTravel(Player player)
    {
        foreach (string equipment in RequiredEquipment)
        {
            if (!player.HasItem(equipment))
                return false;
        }

        return true;
    }

    // Get a travel encounterContext if one should occur
    public TravelEncounterContext GetEncounter(int seed)
    {
        if (PotentialEncounters.Count == 0)
            return null;

        // Knowledge reduces encounterContext chance
        int encounterChance = 20 + (DangerLevel * 5) - (KnowledgeLevel * 10);

        // Determine if encounterContext happens
        Random random = new Random(seed);
        if (random.Next(100) < encounterChance)
        {
            // Select an encounterContext based on danger level
            List<TravelEncounterContext> appropriateEncounters = PotentialEncounters
                .Where(e => e.DangerLevel <= DangerLevel)
                .ToList();

            if (appropriateEncounters.Count > 0)
            {
                int index = random.Next(appropriateEncounters.Count);
                return appropriateEncounters[index];
            }
        }

        return null;
    }
}
