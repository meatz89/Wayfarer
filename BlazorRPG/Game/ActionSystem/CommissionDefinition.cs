public class CommissionDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public CommissionTypes Type { get; set; }
    public int ProgressThreshold { get; set; }
    public int ExpirationDays { get; set; }
    public int ReputationRequirement { get; set; }
    public string InitialLocationId { get; set; }
    public int SilverReward { get; set; }
    public int ReputationReward { get; set; }
    public int InsightPointReward { get; set; }
    public int Tier { get; set; } = 1;
    public List<ApproachDefinition> Approaches { get; set; } = new List<ApproachDefinition>();
    public CommissionStep InitialStep { get; set; }

    public int CurrentProgress { get; set; } = 0;
    public int CurrentStepIndex { get; set; } = 0;
    public List<CommissionStep> CompletedSteps { get; set; } = new List<CommissionStep>();

    public CommissionStep CurrentStep()
    {
        // For SEQUENTIAL commissions, return the initial step
        if (Type == CommissionTypes.Sequential)
        {
            return InitialStep;
        }

        // For ACCUMULATIVE commissions, create a virtual step
        // that references the commission's location
        return new CommissionStep
        {
            Name = this.Name,
            Description = this.Description,
            LocationId = this.InitialLocationId,
            Approaches = this.Approaches,
            ProgressGoal = this.ProgressThreshold
        };
    }

    // Check if commission is complete
    public bool IsComplete()
    {
        return CurrentProgress >= ProgressThreshold;
    }

    // Add progress and handle step transitions
    public void AddProgress(int progress, GameState gameState)
    {
        CurrentProgress += progress;

        // Handle step transitions for sequential commissions
        if (Type == CommissionTypes.Sequential && InitialStep != null)
        {
            // If we've reached the current step's goal
            if (CurrentProgress >= InitialStep.ProgressGoal)
            {
                // Store completed step
                CompletedSteps.Add(InitialStep);

                // Generate next step if we haven't reached total threshold
                if (CurrentProgress < ProgressThreshold)
                {
                    InitialStep = GenerateNextStep(gameState);
                }
            }
        }
    }

    // Generate the next step based on completed steps and approach used
    private CommissionStep GenerateNextStep(GameState gameState)
    {
        // For a POC, we can implement a simple step generation
        // In a full implementation, this would use more sophisticated logic
        // based on your procedural generation approach

        CommissionStep previousStep = CompletedSteps.Last();
        string nextLocationId = DetermineNextLocationId(previousStep, gameState);

        CommissionStep nextStep = new CommissionStep
        {
            Name = $"Continue {Name}",
            Description = $"Follow up on your findings from {previousStep.Name}.",
            LocationId = nextLocationId,
            ProgressGoal = previousStep.ProgressGoal + 5, // Increase difficulty
            Approaches = GenerateApproachesForStep(previousStep)
        };

        return nextStep;
    }

    private string DetermineNextLocationId(CommissionStep previousStep, GameState gameState)
    {
        // Simple implementation - alternate between locations
        // In a full implementation, this would use more sophisticated logic
        if (previousStep.LocationId == InitialLocationId)
        {
            // Find a connected location
            return gameState.WorldState.locations
                .FirstOrDefault(l => l.Id != InitialLocationId &&
                                   l.ConnectedTo.Contains(InitialLocationId))?.Id ?? InitialLocationId;
        }
        return InitialLocationId; // Return to starting location
    }

    private List<ApproachDefinition> GenerateApproachesForStep(CommissionStep previousStep)
    {
        // Create approaches that follow logically from previous step
        // This is a simple implementation - a full implementation would be more sophisticated
        List<ApproachDefinition> approaches = new List<ApproachDefinition>();

        // For each card type, create a relevant approach
        approaches.Add(new ApproachDefinition
        {
            Id = $"physical_followup_{Guid.NewGuid()}",
            Name = "Physical Approach",
            Description = "Use physical means to follow up on your findings.",
            RequiredCardType = CardTypes.Physical,
            PrimarySkill = SkillTypes.Strength,
            SecondarySkill = SkillTypes.Endurance
        });

        approaches.Add(new ApproachDefinition
        {
            Id = $"intellectual_followup_{Guid.NewGuid()}",
            Name = "Intellectual Approach",
            Description = "Apply analytical thinking to advance your investigation.",
            RequiredCardType = CardTypes.Intellectual,
            PrimarySkill = SkillTypes.Analysis,
            SecondarySkill = SkillTypes.Observation
        });

        approaches.Add(new ApproachDefinition
        {
            Id = $"social_followup_{Guid.NewGuid()}",
            Name = "Social Approach",
            Description = "Use interpersonal skills to gather more information.",
            RequiredCardType = CardTypes.Social,
            PrimarySkill = SkillTypes.Charm,
            SecondarySkill = SkillTypes.Persuasion
        });

        return approaches;
    }
}