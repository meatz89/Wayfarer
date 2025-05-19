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

        if (commission.Type == CommissionTypes.Accumulative)
        {
            // For accumulative commissions, create a single-stage encounter
            ApproachDefinition approach = commission.Approaches.FirstOrDefault(a => a.Id == approachId);
            encounter.Stages = GenerateEncounterStagesForApproach(approach, commission.Tier, location);
        }
        else if (commission.Type == CommissionTypes.Sequential)
        {
            // For sequential commissions, start with the initial step
            CommissionStep initialStep = commission.InitialStep;
            ApproachDefinition approach = initialStep.Approaches.FirstOrDefault(a => a.Id == approachId);
            encounter.Stages = GenerateEncounterStagesForApproach(approach, commission.Tier, location);
        }

        return encounter;
    }

    private List<EncounterStage> GenerateEncounterStagesForApproach(
    ApproachDefinition approach,
    int tier,
    Location location)
    {
        // Get current time of day from the game state
        TimeWindowTypes timeOfDay = worldState.CurrentTimeWindow;

        int stageCount = 2 + (tier > 1 ? 1 : 0);
        List<EncounterStage> stages = new List<EncounterStage>();

        for (int stageNum = 1; stageNum <= stageCount; stageNum++)
        {
            int baseDifficulty = stageNum + (tier - 1);
            int baseProgress = stageNum * 2;

            EncounterStage stage = new EncounterStage
            {
                StageNumber = stageNum,
                Description = $"Stage {stageNum} of the {approach.Name} approach",
                Options = new List<EncounterOption>()
            };

            // Primary skill option with location property effects
            stage.Options.Add(CreateSkillCheckOption(
                $"primary_{approach.PrimarySkill.ToString().ToLower()}_{stageNum}",
                approach.PrimarySkill.ToString(),
                approach.PrimarySkill,
                baseDifficulty,
                baseProgress + 2,
                0,
                location));

            // Secondary skill option with location property effects
            stage.Options.Add(CreateSkillCheckOption(
                $"secondary_{approach.SecondarySkill.ToString().ToLower()}_{stageNum}",
                approach.SecondarySkill.ToString(),
                approach.SecondarySkill,
                baseDifficulty - 1,
                baseProgress,
                -1,
                location));

            // Safe option always available
            stage.Options.Add(new EncounterOption
            {
                Id = $"safe_option_{stageNum}",
                Name = "Cautious Approach",
                Description = "Take a safe but less effective approach",
                Skill = SkillTypes.None,
                Difficulty = 0,
                SuccessProgress = baseProgress - 2,
                FailureProgress = 0
            });

            stages.Add(stage);
        }

        return stages;
    }

    private EncounterOption CreateSkillCheckOption(
        string id,
        string skillName,
        SkillTypes skill,
        int baseDifficulty,
        int successProgress,
        int failureProgress,
        Location location)
    {
        // Apply location property modifiers to difficulty
        int locationModifier = GetLocationPropertyModifier(skill, location);
        int adjustedDifficulty = baseDifficulty + locationModifier;

        // Create appropriate description based on location modifiers
        string difficultyDescription = locationModifier switch
        {
            < 0 => $"(Favored by this location's properties)",
            > 0 => $"(Hindered by this location's properties)",
            _ => ""
        };

        return new EncounterOption
        {
            Id = id,
            Name = $"{skillName} Approach",
            Description = $"Use your {skillName} skill for this challenge {difficultyDescription}",
            Skill = skill,
            Difficulty = adjustedDifficulty,
            SuccessProgress = successProgress,
            FailureProgress = failureProgress,
            LocationModifier = locationModifier
        };
    }

    private int GetLocationPropertyModifier(SkillTypes skill, Location location)
    {
        // Call the static method from SkillCheckService
        return SkillCheckService.GetLocationPropertyModifier(skill, location);
    }

    private int GetLocationDifficultyModifier(SkillTypes skill, Location location)
    {
        int modifier = 0;

        // Apply modifiers based on location properties
        if (SkillCheckService.IsIntellectualSkill(skill))
        {
            // Intellectual skills are affected by Population and Illumination
            if (location.Population == Population.Crowded)
                modifier += 1; // Higher difficulty in crowds

            if (location.Illumination == Illumination.Dark)
                modifier += 1; // Higher difficulty in darkness

            if (location.Population == Population.Scholarly)
                modifier -= 1; // Lower difficulty in scholarly environment
        }
        else if (SkillCheckService.IsSocialSkill(skill))
        {
            // Social skills are affected by Population and Atmosphere
            if (location.Population == Population.Quiet)
                modifier += 1; // Higher difficulty in quiet environments

            if (location.Atmosphere == Atmosphere.Tense)
                modifier += 1; // Higher difficulty in tense atmosphere

            if (location.Population == Population.Crowded)
                modifier -= 1; // Lower difficulty in crowded environments
        }
        else if (SkillCheckService.IsPhysicalSkill(skill))
        {
            // Physical skills are affected by Physical property and Illumination
            if (location.Physical == Physical.Confined)
                modifier += 1; // Higher difficulty in confined spaces

            if (location.Illumination == Illumination.Dark)
                modifier += 1; // Higher difficulty in darkness

            if (location.Physical == Physical.Expansive)
                modifier -= 1; // Lower difficulty in open spaces
        }

        return modifier;
    }

    private CardTypes DetermineEncounterType(string approachId)
    {
        // Map approach ID to encounter type
        if (approachId.Contains("physical"))
            return CardTypes.Physical;

        if (approachId.Contains("intellectual"))
            return CardTypes.Intellectual;

        if (approachId.Contains("social"))
            return CardTypes.Social;

        return CardTypes.None;
    }

    public Encounter GetDefaultEncounterTemplate()
    {
        // Create a simple default encounter template for testing
        Encounter defaultEncounter = new Encounter
        {
            Id = "default_encounter",
            TotalProgress = 8,
            EncounterDifficulty = 1,
            Stages = new List<EncounterStage>
            {
                new EncounterStage
                {
                    StageNumber = 1,
                    Description = "A simple challenge",
                    Options = new List<EncounterOption>
                    {
                        new EncounterOption
                        {
                            Id = "default_option",
                            Name = "Basic Approach",
                            Skill = SkillTypes.None,
                            Difficulty = 0,
                            SuccessProgress = 2,
                            FailureProgress = 0
                        }
                    }
                }
            }
        };

        return defaultEncounter;
    }
}

