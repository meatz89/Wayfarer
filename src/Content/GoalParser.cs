using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Enums;

/// <summary>
/// Parser for converting GoalDTO to Goal domain model
/// </summary>
public static class GoalParser
{
    /// <summary>
    /// Convert a GoalDTO to a Goal domain model
    /// </summary>
    public static Goal ConvertDTOToGoal(GoalDTO dto, GameWorld gameWorld)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException("Goal DTO missing required 'Id' field");
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"Goal {dto.Id} missing required 'Name' field");
        if (string.IsNullOrEmpty(dto.SystemType))
            throw new InvalidOperationException($"Goal {dto.Id} missing required 'SystemType' field");
        if (string.IsNullOrEmpty(dto.DeckId))
            throw new InvalidOperationException($"Goal {dto.Id} missing required 'DeckId' field");

        // Parse system type
        if (!Enum.TryParse<TacticalSystemType>(dto.SystemType, true, out TacticalSystemType systemType))
        {
            throw new InvalidOperationException($"Goal {dto.Id} has invalid SystemType value: '{dto.SystemType}'");
        }

        // Parse consequence type
        ConsequenceType consequenceType = ParseConsequenceType(dto.ConsequenceType);

        // Parse resolution method and relationship outcome
        ResolutionMethod resolutionMethod = ParseResolutionMethod(dto.ResolutionMethod);
        RelationshipOutcome relationshipOutcome = ParseRelationshipOutcome(dto.RelationshipOutcome);

        // Parse property requirements
        ObstaclePropertyRequirements propertyRequirements = ParsePropertyRequirements(dto.PropertyRequirements);

        // Parse property reduction
        ObstaclePropertyReduction propertyReduction = dto.PropertyReduction != null
            ? ObstacleParser.ConvertDTOToReduction(dto.PropertyReduction)
            : null;

        Goal goal = new Goal
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            SystemType = systemType,
            DeckId = dto.DeckId,
            PlacementLocationId = dto.PlacementLocationId,
            PlacementNpcId = dto.PlacementNpcId,
            InvestigationId = dto.InvestigationId,
            IsIntroAction = dto.IsIntroAction,
            IsAvailable = dto.IsAvailable,
            IsCompleted = dto.IsCompleted,
            DeleteOnSuccess = dto.DeleteOnSuccess,
            GoalCards = new List<GoalCard>(),
            Requirements = ParseGoalRequirements(dto.Requirements),
            ConsequenceType = consequenceType,
            SetsResolutionMethod = resolutionMethod,
            SetsRelationshipOutcome = relationshipOutcome,
            TransformDescription = dto.TransformDescription,
            PropertyRequirements = propertyRequirements,
            PropertyReduction = propertyReduction
        };

        // Parse goal cards (victory conditions)
        if (dto.GoalCards != null && dto.GoalCards.Any())
        {
            foreach (GoalCardDTO goalCardDTO in dto.GoalCards)
            {
                GoalCard goalCard = ParseGoalCard(goalCardDTO, dto.Id);
                goal.GoalCards.Add(goalCard);
            }
        }

        Console.WriteLine($"[GoalParser] Parsed goal '{goal.Name}' ({goal.SystemType}, Consequence: {consequenceType}) with {goal.GoalCards.Count} goal cards");
        return goal;
    }

    /// <summary>
    /// Parse a single goal card (victory condition)
    /// </summary>
    private static GoalCard ParseGoalCard(GoalCardDTO dto, string goalId)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException($"GoalCard in goal {goalId} missing required 'Id' field");

        GoalCard goalCard = new GoalCard
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            threshold = dto.threshold,
            Rewards = ParseGoalCardRewards(dto.Rewards),
            IsAchieved = false
        };

        return goalCard;
    }

    /// <summary>
    /// Parse goal card rewards
    /// </summary>
    private static GoalCardRewards ParseGoalCardRewards(GoalCardRewardsDTO dto)
    {
        if (dto == null)
            return new GoalCardRewards();

        GoalCardRewards rewards = new GoalCardRewards
        {
            Coins = dto.Coins,
            Progress = dto.Progress,
            Breakthrough = dto.Breakthrough,
            ObligationId = dto.ObligationId,
            Item = dto.Item,
            Knowledge = dto.Knowledge != null ? new List<string>(dto.Knowledge) : new List<string>(),
            Tokens = dto.Tokens != null ? new Dictionary<string, int>(dto.Tokens) : new Dictionary<string, int>(),
            ObstacleReduction = dto.ObstacleReduction != null ? ObstacleParser.ConvertDTOToReduction(dto.ObstacleReduction) : null
        };

        return rewards;
    }

    /// <summary>
    /// Parse goal requirements
    /// </summary>
    private static GoalRequirements ParseGoalRequirements(GoalRequirementsDTO dto)
    {
        if (dto == null)
            return null;

        GoalRequirements requirements = new GoalRequirements
        {
            RequiredKnowledge = dto.RequiredKnowledge != null ? new List<string>(dto.RequiredKnowledge) : new List<string>(),
            RequiredEquipment = dto.RequiredEquipment != null ? new List<string>(dto.RequiredEquipment) : new List<string>(),
            RequiredStats = new List<StatRequirement>(),
            MinimumLocationFamiliarity = dto.MinimumLocationFamiliarity,
            CompletedGoals = dto.CompletedGoals != null ? new List<string>(dto.CompletedGoals) : new List<string>()
        };

        // Parse required stats list (strongly-typed, no Dictionary)
        if (dto.RequiredStats != null)
        {
            foreach (StatRequirementDTO statDto in dto.RequiredStats)
            {
                if (Enum.TryParse<PlayerStatType>(statDto.StatType, true, out PlayerStatType statType))
                {
                    requirements.RequiredStats.Add(new StatRequirement
                    {
                        StatType = statType,
                        MinimumLevel = statDto.MinimumLevel
                    });
                }
                else
                {
                    Console.WriteLine($"[GoalParser] Warning: Unknown stat type '{statDto.StatType}' in requirements, skipping");
                }
            }
        }

        return requirements;
    }

    /// <summary>
    /// Parse consequence type from string
    /// </summary>
    private static ConsequenceType ParseConsequenceType(string consequenceTypeString)
    {
        if (string.IsNullOrEmpty(consequenceTypeString))
            return ConsequenceType.Grant;

        if (Enum.TryParse<ConsequenceType>(consequenceTypeString, true, out ConsequenceType consequenceType))
        {
            return consequenceType;
        }

        Console.WriteLine($"[GoalParser] Warning: Unknown ConsequenceType '{consequenceTypeString}', defaulting to Grant");
        return ConsequenceType.Grant;
    }

    /// <summary>
    /// Parse resolution method from string
    /// </summary>
    private static ResolutionMethod ParseResolutionMethod(string methodString)
    {
        if (string.IsNullOrEmpty(methodString))
            return ResolutionMethod.Unresolved;

        if (Enum.TryParse<ResolutionMethod>(methodString, true, out ResolutionMethod method))
        {
            return method;
        }

        Console.WriteLine($"[GoalParser] Warning: Unknown ResolutionMethod '{methodString}', defaulting to Unresolved");
        return ResolutionMethod.Unresolved;
    }

    /// <summary>
    /// Parse relationship outcome from string
    /// </summary>
    private static RelationshipOutcome ParseRelationshipOutcome(string outcomeString)
    {
        if (string.IsNullOrEmpty(outcomeString))
            return RelationshipOutcome.Neutral;

        if (Enum.TryParse<RelationshipOutcome>(outcomeString, true, out RelationshipOutcome outcome))
        {
            return outcome;
        }

        Console.WriteLine($"[GoalParser] Warning: Unknown RelationshipOutcome '{outcomeString}', defaulting to Neutral");
        return RelationshipOutcome.Neutral;
    }

    /// <summary>
    /// Parse obstacle property requirements
    /// </summary>
    private static ObstaclePropertyRequirements ParsePropertyRequirements(ObstaclePropertyRequirementsDTO dto)
    {
        if (dto == null)
            return null;

        return new ObstaclePropertyRequirements
        {
            MaxPhysicalDanger = dto.MaxPhysicalDanger,
            MaxMentalComplexity = dto.MaxMentalComplexity,
            MaxSocialDifficulty = dto.MaxSocialDifficulty
        };
    }
}
