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
        if (string.IsNullOrEmpty(dto.ChallengeTypeId))
            throw new InvalidOperationException($"Goal {dto.Id} missing required 'ChallengeTypeId' field");

        // Parse system type
        if (!Enum.TryParse<TacticalSystemType>(dto.SystemType, true, out TacticalSystemType systemType))
        {
            throw new InvalidOperationException($"Goal {dto.Id} has invalid SystemType value: '{dto.SystemType}'");
        }

        Goal goal = new Goal
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            SystemType = systemType,
            ChallengeTypeId = dto.ChallengeTypeId,
            LocationId = dto.LocationId,
            NpcId = dto.NpcId,
            NpcRequestId = dto.NpcRequestId,
            InvestigationId = dto.InvestigationId,
            IsIntroAction = dto.IsIntroAction,
            IsAvailable = dto.IsAvailable,
            IsCompleted = dto.IsCompleted,
            GoalCards = new List<GoalCard>(),
            Requirements = ParseGoalRequirements(dto.Requirements)
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

        Console.WriteLine($"[GoalParser] Parsed goal '{goal.Name}' ({goal.SystemType}) with {goal.GoalCards.Count} goal cards");
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
            MomentumThreshold = dto.MomentumThreshold,
            Weight = dto.Weight > 0 ? dto.Weight : 1, // Default weight to 1
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
            Tokens = dto.Tokens != null ? new Dictionary<string, int>(dto.Tokens) : new Dictionary<string, int>()
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
}
