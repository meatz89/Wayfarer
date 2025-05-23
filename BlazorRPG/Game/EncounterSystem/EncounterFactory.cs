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
            TotalProgress = GetProgressThresholdForTier(commission.Tier),
            LocationName = location.Id,
            LocationSpotName = playerState.CurrentLocationSpot.Id,
            EncounterType = DetermineEncounterType(approachId),
            EncounterDifficulty = commission.Tier,
            SuccessThreshold = 10
        };

        // Always create exactly 5 stages for The Wayfarer's Resolve system
        encounter.Stages = GenerateUniversalFiveStageStructure();

        return encounter;
    }

    private List<EncounterStage> GenerateUniversalFiveStageStructure()
    {
        List<EncounterStage> stages = new List<EncounterStage>();

        // Always exactly 5 stages as per The Wayfarer's Resolve system
        for (int stageNum = 1; stageNum <= 5; stageNum++)
        {
            EncounterStage stage = new EncounterStage
            {
                StageNumber = stageNum,
                Description = GetUniversalStageDescription(stageNum),
                Options = new List<EncounterOption>() // Empty - populated by ChoiceCardSelector
            };
            stages.Add(stage);
        }

        return stages;
    }

    private string GetUniversalStageDescription(int stageNum)
    {
        // Following the tier structure from The Wayfarer's Resolve system
        return stageNum switch
        {
            1 => "Foundation - Initial assessment and preparation", // Foundation Tier
            2 => "Foundation - Building your approach and gathering resources", // Foundation Tier
            3 => "Development - Applying skills with increased intensity", // Development Tier
            4 => "Development - Major effort combining your capabilities", // Development Tier
            5 => "Execution - Final push to complete your objective", // Execution Tier
            _ => $"Stage {stageNum}"
        };
    }

    private int GetProgressThresholdForTier(int tier)
    {
        // Based on The Wayfarer's Resolve three-tier success structure
        // Basic Success: 10, Good Success: 14, Excellent Success: 18
        return tier switch
        {
            1 => 10, // Basic success threshold for Tier 1 commissions
            2 => 12, // Moderate threshold for Tier 2 commissions
            3 => 14, // Higher threshold for Tier 3 commissions
            _ => 10  // Default to basic threshold
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
            TotalProgress = 10, // Basic success threshold
            EncounterDifficulty = 1,
            EncounterType = CardTypes.Physical,
            Stages = GenerateUniversalFiveStageStructure() // Always 5 stages
        };

        return defaultEncounter;
    }
}