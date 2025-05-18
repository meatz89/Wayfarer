public class EncounterFactory
{
    private readonly LocationRepository _locationRepository;

    public EncounterFactory(LocationRepository locationRepository)
    {
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

            // Primary skill option
            stage.Options.Add(new EncounterOption
            {
                Id = $"primary_{approach.PrimarySkill.ToString().ToLower()}_{stageNum}",
                Name = $"{approach.PrimarySkill} Approach",
                Description = $"Use your {approach.PrimarySkill} skill for this challenge",
                Skill = approach.PrimarySkill,
                Difficulty = baseDifficulty,
                SuccessProgress = baseProgress + 2,
                FailureProgress = 0
            });

            // Secondary skill option
            stage.Options.Add(new EncounterOption
            {
                Id = $"secondary_{approach.SecondarySkill.ToString().ToLower()}_{stageNum}",
                Name = $"{approach.SecondarySkill} Approach",
                Description = $"Use your {approach.SecondarySkill} skill for this challenge",
                Skill = approach.SecondarySkill,
                Difficulty = baseDifficulty - 1,
                SuccessProgress = baseProgress,
                FailureProgress = -1
            });

            // Safe option
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

