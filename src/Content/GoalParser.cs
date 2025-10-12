using System;
using System.Collections.Generic;
using System.Linq;

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

        // Parse effect type
        GoalEffectType effectType = ParseEffectType(dto.EffectType);

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
            LocationId = dto.LocationId,
            NpcId = dto.NpcId,
            InvestigationId = dto.InvestigationId,
            IsIntroAction = dto.IsIntroAction,
            IsAvailable = dto.IsAvailable,
            IsCompleted = dto.IsCompleted,
            DeleteOnSuccess = dto.DeleteOnSuccess,
            GoalCards = new List<GoalCard>(),
            Requirements = ParseGoalRequirements(dto.Requirements),
            EffectType = effectType,
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

        Console.WriteLine($"[GoalParser] Parsed goal '{goal.Name}' ({goal.SystemType}, Effect: {effectType}) with {goal.GoalCards.Count} goal cards");
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
            RequiredStats = new Dictionary<PlayerStatType, int>(),
            MinimumLocationFamiliarity = dto.MinimumLocationFamiliarity,
            CompletedGoals = dto.CompletedGoals != null ? new List<string>(dto.CompletedGoals) : new List<string>()
        };

        // Parse required stats dictionary (string keys to PlayerStatType enum)
        if (dto.RequiredStats != null)
        {
            foreach (KeyValuePair<string, int> stat in dto.RequiredStats)
            {
                if (Enum.TryParse<PlayerStatType>(stat.Key, true, out PlayerStatType statType))
                {
                    requirements.RequiredStats[statType] = stat.Value;
                }
                else
                {
                    Console.WriteLine($"[GoalParser] Warning: Unknown stat type '{stat.Key}' in requirements, skipping");
                }
            }
        }

        return requirements;
    }

    /// <summary>
    /// Parse effect type from string
    /// </summary>
    private static GoalEffectType ParseEffectType(string effectTypeString)
    {
        if (string.IsNullOrEmpty(effectTypeString))
            return GoalEffectType.None;

        if (Enum.TryParse<GoalEffectType>(effectTypeString, true, out GoalEffectType effectType))
        {
            return effectType;
        }

        Console.WriteLine($"[GoalParser] Warning: Unknown EffectType '{effectTypeString}', defaulting to None");
        return GoalEffectType.None;
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
            MaxSocialDifficulty = dto.MaxSocialDifficulty,
            MaxStaminaCost = dto.MaxStaminaCost,
            MaxTimeCost = dto.MaxTimeCost
        };
    }
}
