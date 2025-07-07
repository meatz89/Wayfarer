public class RouteOption
{
    public string Name { get; set; }
    public int CoinCost { get; set; }
    public int StaminaCost { get; set; }
    public int TimeBlockCost { get; set; }
    public TimeBlocks? DepartureTime { get; set; } // Null means departs anytime
    public bool IsDiscovered { get; set; } = true;

    public Location Origin { get; set; }
    public Location Destination { get; set; }
    public int BaseTimeCost { get; set; }
    public int BaseEnergyCost { get; set; }
    public int DangerLevel { get; set; } // 0-10 scale
    public List<string> RequiredEquipment { get; set; } = new List<string>();
    public List<EncounterContext> PotentialEncounters { get; set; } = new List<EncounterContext>();

    public int KnowledgeLevel { get; private set; }

    public void IncreaseKnowledge()
    {
        if (KnowledgeLevel < 3)
            KnowledgeLevel++;
    }

    public int GetActualTimeCost()
    {
        return BaseTimeCost - KnowledgeLevel;
    }

    public int GetActualEnergyCost()
    {
        return BaseEnergyCost - KnowledgeLevel;
    }

    public bool CanTravel(Player player)
    {
        foreach (string equipment in RequiredEquipment)
        {
            if (!player.HasItem(equipment))
                return false;
        }

        return true;
    }

    public EncounterContext GetEncounter(int seed)
    {
        if (PotentialEncounters.Count == 0)
            return null;

        int encounterChance = 20 + (DangerLevel * 5) - (KnowledgeLevel * 10);

        Random random = new Random(seed);
        if (random.Next(100) < encounterChance)
        {
            List<EncounterContext> appropriateEncounters = PotentialEncounters
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
