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

        // Resolve targetObstacleIndex to actual Obstacle reference
        if (dto.TargetObstacleIndex.HasValue)
        {
            int targetIndex = dto.TargetObstacleIndex.Value;

            // Location-based goal (Mental/Physical challenges)
            if (!string.IsNullOrEmpty(goal.LocationId))
            {
                LocationEntry locationEntry = gameWorld.Locations.FirstOrDefault(l => l.LocationId == goal.LocationId);
                if (locationEntry != null && locationEntry.location != null)
                {
                    if (targetIndex >= 0 && targetIndex < locationEntry.location.Obstacles.Count)
                    {
                        goal.TargetObstacle = locationEntry.location.Obstacles[targetIndex];
                        Console.WriteLine($"[GoalParser] Goal '{goal.Name}' targets obstacle '{goal.TargetObstacle.Name}' at location '{goal.LocationId}'");
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Goal '{goal.Id}' has targetObstacleIndex {targetIndex} but location '{goal.LocationId}' " +
                            $"only has {locationEntry.location.Obstacles.Count} obstacles");
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Goal '{goal.Id}' references location '{goal.LocationId}' which was not found in GameWorld");
                }
            }
            // NPC-based goal (Social challenges)
            else if (!string.IsNullOrEmpty(goal.NpcId))
            {
                NPC npc = gameWorld.NPCs.FirstOrDefault(n => n.ID == goal.NpcId);
                if (npc != null)
                {
                    if (targetIndex >= 0 && targetIndex < npc.Obstacles.Count)
                    {
                        goal.TargetObstacle = npc.Obstacles[targetIndex];
                        Console.WriteLine($"[GoalParser] Goal '{goal.Name}' targets obstacle '{goal.TargetObstacle.Name}' for NPC '{goal.NpcId}'");
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Goal '{goal.Id}' has targetObstacleIndex {targetIndex} but NPC '{goal.NpcId}' " +
                            $"only has {npc.Obstacles.Count} obstacles");
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Goal '{goal.Id}' references NPC '{goal.NpcId}' which was not found in GameWorld");
                }
            }
            // Investigation-based goals with obstacles will be handled in investigation spawning system (Tasks 21-23)
            else if (!string.IsNullOrEmpty(goal.InvestigationId))
            {
                Console.WriteLine($"[GoalParser] Goal '{goal.Name}' has targetObstacleIndex but is investigation-based - " +
                    "obstacle resolution will happen when investigation spawns obstacles");
            }
            else
            {
                throw new InvalidOperationException(
                    $"Goal '{goal.Id}' has targetObstacleIndex but no LocationId, NpcId, or InvestigationId");
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
}
