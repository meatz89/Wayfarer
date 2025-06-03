public class OpportunityDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public OpportunityTypes Type { get; set; }
    public int ProgressThreshold { get; set; }
    public int ExpirationDays { get; set; }
    public int ReputationRequirement { get; set; }
    public string InitialLocationId { get; set; }
    public int SilverReward { get; set; }
    public int ReputationReward { get; set; }
    public int InsightPointReward { get; set; }
    public int Tier { get; set; } = 1;
    public List<ApproachDefinition> Approaches { get; set; } = new List<ApproachDefinition>();
    public Opportunitiestep InitialStep { get; set; }

    public int CurrentProgress { get; set; } = 0;
    public int CurrentStepIndex { get; set; } = 0;
    public List<Opportunitiestep> CompletedSteps { get; set; } = new List<Opportunitiestep>();

    public Opportunitiestep CurrentStep()
    {
        // For SEQUENTIAL Opportunities, return the initial step
        if (Type == OpportunityTypes.Sequential)
        {
            return InitialStep;
        }

        // For ACCUMULATIVE Opportunities, create a virtual step
        // that references the opportunity's location
        return new Opportunitiestep
        {
            Name = this.Name,
            Description = this.Description,
            LocationId = this.InitialLocationId,
            Approaches = this.Approaches,
            ProgressGoal = this.ProgressThreshold
        };
    }

    // Check if opportunity is complete
    public bool IsComplete()
    {
        return CurrentProgress >= ProgressThreshold;
    }

    // Add progress and handle step transitions
    public void AddProgress(int progress, GameWorld gameWorld)
    {
        CurrentProgress += progress;

        if (Type == OpportunityTypes.Sequential && InitialStep != null)
        {
            if (CurrentProgress >= InitialStep.ProgressGoal)
            {
                CompletedSteps.Add(InitialStep);

                if (CurrentProgress < ProgressThreshold)
                {
                    InitialStep = GenerateNextStep(gameWorld);
                }
            }
        }
    }

    // Generate the next step based on completed steps and approach used
    private Opportunitiestep GenerateNextStep(GameWorld gameWorld)
    {
        // For a POC, we can implement a simple step generation
        // In a full implementation, this would use more sophisticated logic
        // based on your procedural generation approach

        Opportunitiestep previousStep = CompletedSteps.Last();
        string nextLocationId = DetermineNextLocationId(previousStep, gameWorld);

        Opportunitiestep nextStep = new Opportunitiestep
        {
            Name = $"Continue {Name}",
            Description = $"Follow up on your findings from {previousStep.Name}.",
            LocationId = nextLocationId,
            ProgressGoal = previousStep.ProgressGoal + 5, // Increase difficulty
            Approaches = GenerateApproachesForStep(previousStep)
        };

        return nextStep;
    }

    private string DetermineNextLocationId(Opportunitiestep previousStep, GameWorld gameWorld)
    {
        // Simple implementation - alternate between locations
        // In a full implementation, this would use more sophisticated logic
        if (previousStep.LocationId == InitialLocationId)
        {
            return gameWorld.WorldState.locations
                .FirstOrDefault(l => l.Id != InitialLocationId &&
                                   l.ConnectedTo.Contains(InitialLocationId))?.Id ?? InitialLocationId;
        }
        return InitialLocationId;
    }

    private List<ApproachDefinition> GenerateApproachesForStep(Opportunitiestep previousStep)
    {
        // Create approaches that follow logically from previous step
        // This is a simple implementation - a full implementation would be more sophisticated
        List<ApproachDefinition> approaches = new List<ApproachDefinition>();

        approaches.Add(new ApproachDefinition
        {
            Id = $"physical_followup_{Guid.NewGuid()}",
            Name = "Physical Approach",
            Description = "Use physical means to follow up on your findings.",
            RequiredCardType = SkillCategories.Physical
        });

        approaches.Add(new ApproachDefinition
        {
            Id = $"intellectual_followup_{Guid.NewGuid()}",
            Name = "Intellectual Approach",
            Description = "Apply analytical thinking to advance your investigation.",
            RequiredCardType = SkillCategories.Intellectual
        });

        approaches.Add(new ApproachDefinition
        {
            Id = $"social_followup_{Guid.NewGuid()}",
            Name = "Social Approach",
            Description = "Use interpersonal skills to gather more information.",
            RequiredCardType = SkillCategories.Social
        });

        return approaches;
    }
}