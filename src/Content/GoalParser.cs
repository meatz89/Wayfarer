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

        // Parse property reduction
        ObstaclePropertyReduction propertyReduction = dto.PropertyReduction != null
            ? ObstacleParser.ConvertDTOToReduction(dto.PropertyReduction)
            : null;

        // Parse costs and difficulty modifiers
        GoalCosts costs = ParseGoalCosts(dto.Costs);
        List<DifficultyModifier> difficultyModifiers = ParseDifficultyModifiers(dto.DifficultyModifiers);

        Goal goal = new Goal
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            SystemType = systemType,
            DeckId = dto.DeckId,
            PlacementLocationId = dto.PlacementLocationId,
            PlacementNpcId = dto.PlacementNpcId,
            ObligationId = dto.ObligationId,
            IsIntroAction = dto.IsIntroAction,
            IsAvailable = dto.IsAvailable,
            IsCompleted = dto.IsCompleted,
            DeleteOnSuccess = dto.DeleteOnSuccess,
            BaseDifficulty = dto.BaseDifficulty,
            Costs = costs,
            DifficultyModifiers = difficultyModifiers,
            GoalCards = new List<GoalCard>(),
            // GoalRequirements system eliminated - goals always visible, difficulty varies
            ConsequenceType = consequenceType,
            SetsResolutionMethod = resolutionMethod,
            SetsRelationshipOutcome = relationshipOutcome,
            TransformDescription = dto.TransformDescription,
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
    /// Knowledge system eliminated - Understanding resource replaces Knowledge tokens
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

            // Cube rewards (strong typing)
            XXXOBLIGATIONCUBESXXX = dto.XXXOBLIGATIONCUBESXXX,
            StoryCubes = dto.StoryCubes,
            ExplorationCubes = dto.ExplorationCubes,

            // Core Loop reward types
            EquipmentId = dto.EquipmentId,
            CreateObligationData = dto.CreateObligationData != null
                ? new CreateObligationReward
                {
                    PatronNpcId = dto.CreateObligationData.PatronNpcId,
                    StoryCubesGranted = dto.CreateObligationData.StoryCubesGranted,
                    RewardCoins = dto.CreateObligationData.RewardCoins
                }
                : null,
            RouteSegmentUnlock = dto.RouteSegmentUnlock != null
                ? new RouteSegmentUnlock
                {
                    RouteId = dto.RouteSegmentUnlock.RouteId,
                    SegmentPosition = dto.RouteSegmentUnlock.SegmentPosition,
                    PathId = dto.RouteSegmentUnlock.PathId
                }
                : null,

            ObstacleReduction = dto.ObstacleReduction != null ? ObstacleParser.ConvertDTOToReduction(dto.ObstacleReduction) : null
        };

        return rewards;
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
        return RelationshipOutcome.Neutral;
    }

    /// <summary>
    /// Parse goal costs from DTO
    /// </summary>
    private static GoalCosts ParseGoalCosts(GoalCostsDTO dto)
    {
        if (dto == null)
            return new GoalCosts();

        return new GoalCosts
        {
            Time = dto.Time,
            Focus = dto.Focus,
            Stamina = dto.Stamina,
            Coins = dto.Coins
        };
    }

    /// <summary>
    /// Parse difficulty modifiers list from DTOs
    /// </summary>
    private static List<DifficultyModifier> ParseDifficultyModifiers(List<DifficultyModifierDTO> dtos)
    {
        if (dtos == null || !dtos.Any())
            return new List<DifficultyModifier>();

        List<DifficultyModifier> modifiers = new List<DifficultyModifier>();
        foreach (DifficultyModifierDTO dto in dtos)
        {
            DifficultyModifier modifier = ParseDifficultyModifier(dto);
            if (modifier != null)
            {
                modifiers.Add(modifier);
            }
        }

        return modifiers;
    }

    /// <summary>
    /// Parse single difficulty modifier from DTO
    /// </summary>
    private static DifficultyModifier ParseDifficultyModifier(DifficultyModifierDTO dto)
    {
        if (dto == null)
            return null;

        // Parse modifier type
        if (!Enum.TryParse<ModifierType>(dto.Type, true, out ModifierType modifierType))
        {
            return null;
        }

        return new DifficultyModifier
        {
            Type = modifierType,
            Context = dto.Context,
            Threshold = dto.Threshold,
            Effect = dto.Effect
        };
    }
}
