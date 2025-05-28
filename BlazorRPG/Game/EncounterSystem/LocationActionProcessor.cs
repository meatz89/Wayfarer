using System;
using System.Numerics;

public class LocationActionProcessor
{
    private LocationRepository _locationRepository;
    private WorldState worldState;

    public LocationActionProcessor(
        GameWorld gameState,
        LocationRepository locationRepository)
    {
        this.worldState = gameState.WorldState;
        _locationRepository = locationRepository;
    }

    public EncounterContext InitializeEncounter(LocationSpot spot, LocationAction action, Player player)
    {
        EncounterContext encounterContext = new EncounterContext
        {
            LocationName = spot.Name,
            LocationDescription = spot.GetCurrentDescription(),
            ActionName = action.Name,
            SkillCategory = action.RequiredCardType, // Physical/Intellectual/Social
            ObjectiveDescription = action.ObjectiveDescription,
            PlayerSkillCards = player.GetCardsOfType(action.RequiredCardType),
            StartingFocusPoints = CalculateFocusPoints(action.Complexity),
            TargetNPC = spot.PrimaryNPC,
            LocationProperties = spot.GetCurrentProperties()
        };

        return encounterContext;
    }

    private int CalculateFocusPoints(int complexity)
    {
        return complexity; // For simplicity, using complexity directly as focus points
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
                Options = new List<EncounterChoice>() // Empty - populated by ChoiceCardSelector
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

    private ActionTypes DetermineSkillCategory(string approachId)
    {
        if (approachId.Contains("physical", StringComparison.OrdinalIgnoreCase))
            return ActionTypes.Physical;
        if (approachId.Contains("intellectual", StringComparison.OrdinalIgnoreCase))
            return ActionTypes.Intellectual;
        if (approachId.Contains("social", StringComparison.OrdinalIgnoreCase))
            return ActionTypes.Social;

        return ActionTypes.Physical; // Default fallback
    }

    public Encounter GetDefaultEncounterTemplate()
    {
        Encounter defaultEncounter = new Encounter
        {
            Id = "default_encounter",
            TotalProgress = 10, // Basic success threshold
            EncounterDifficulty = 1,
            SkillCategory = ActionTypes.Physical,
            Stages = GenerateUniversalFiveStageStructure() // Always 5 stages
        };

        return defaultEncounter;
    }
}