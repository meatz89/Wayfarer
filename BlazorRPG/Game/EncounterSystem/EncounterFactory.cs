public class EncounterFactory
{
    private readonly LocationRepository _locationRepository;
    private readonly WorldState worldState;

    public EncounterFactory(
        GameState gameState,
        LocationRepository locationRepository)
    {
        this.worldState = gameState.WorldState;
        _locationRepository = locationRepository;
    }

    public Encounter CreateEncounterFromCommission(
        CommissionDefinition commission,
        string approachId,
        PlayerState playerState,
        Location location)
    {
        Encounter encounter = new Encounter
        {
            Id = $"{commission.Id}_{approachId}",
            CommissionId = commission.Id,
            ApproachId = approachId,
            TotalProgress = commission.ProgressThreshold,
            LocationName = location.Id,
            LocationSpotName = playerState.CurrentLocationSpot.Id,
            EncounterType = DetermineEncounterType(approachId),
            EncounterDifficulty = commission.Tier
        };

        // Create the encounter stages structure (choices come from NarrativeChoiceRepository)
        encounter.Stages = GenerateEncounterStageStructure(commission.Tier);

        return encounter;
    }

    private List<EncounterStage> GenerateEncounterStageStructure(int tier)
    {
        int stageCount = CalculateStageCount(tier);
        List<EncounterStage> stages = new List<EncounterStage>();

        for (int stageNum = 1; stageNum <= stageCount; stageNum++)
        {
            EncounterStage stage = new EncounterStage
            {
                StageNumber = stageNum,
                Description = GetStageDescription(stageNum, stageCount),
                Options = new List<EncounterOption>() // Empty - populated by ChoiceCardSelector
            };

            stages.Add(stage);
        }

        return stages;
    }

    private int CalculateStageCount(int tier)
    {
        return tier switch
        {
            1 => 2, // Simple encounters
            2 => 3, // Standard encounters  
            _ => 3  // Complex encounters
        };
    }

    private string GetStageDescription(int stageNum, int totalStages)
    {
        return stageNum switch
        {
            1 => "Initial approach and preparation",
            2 when totalStages == 2 => "Final execution and resolution",
            2 when totalStages > 2 => "Development and major effort",
            3 => "Final push and completion",
            _ => $"Stage {stageNum} of {totalStages}"
        };
    }

    private CardTypes DetermineEncounterType(string approachId)
    {
        if (approachId.Contains("physical", StringComparison.OrdinalIgnoreCase))
            return CardTypes.Physical;

        if (approachId.Contains("intellectual", StringComparison.OrdinalIgnoreCase))
            return CardTypes.Intellectual;

        if (approachId.Contains("social", StringComparison.OrdinalIgnoreCase))
            return CardTypes.Social;

        return CardTypes.Physical; // Default fallback
    }

    public Encounter GetDefaultEncounterTemplate()
    {
        Encounter defaultEncounter = new Encounter
        {
            Id = "default_encounter",
            TotalProgress = 8,
            EncounterDifficulty = 1,
            EncounterType = CardTypes.Physical,
            Stages = GenerateEncounterStageStructure(1)
        };

        return defaultEncounter;
    }
}