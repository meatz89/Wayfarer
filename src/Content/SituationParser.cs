using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Enums;

/// <summary>
/// Parser for converting SituationDTO to Situation domain model
/// </summary>
public static class SituationParser
{
    /// <summary>
    /// Convert a SituationDTO to a Situation domain model
    /// </summary>
    public static Situation ConvertDTOToSituation(SituationDTO dto, GameWorld gameWorld)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException("Situation DTO missing required 'Id' field");
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"Situation {dto.Id} missing required 'Name' field");
        if (string.IsNullOrEmpty(dto.SystemType))
            throw new InvalidOperationException($"Situation {dto.Id} missing required 'SystemType' field");
        if (string.IsNullOrEmpty(dto.DeckId))
            throw new InvalidOperationException($"Situation {dto.Id} missing required 'DeckId' field");

        // Parse system type
        if (!Enum.TryParse<TacticalSystemType>(dto.SystemType, true, out TacticalSystemType systemType))
        {
            throw new InvalidOperationException($"Situation {dto.Id} has invalid SystemType value: '{dto.SystemType}'");
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
        SituationCosts costs = ParseSituationCosts(dto.Costs);
        List<DifficultyModifier> difficultyModifiers = ParseDifficultyModifiers(dto.DifficultyModifiers);

        Situation situation = new Situation
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
            Costs = costs,
            DifficultyModifiers = difficultyModifiers,
            SituationCards = new List<SituationCard>(),
            // SituationRequirements system eliminated - situations always visible, difficulty varies
            ConsequenceType = consequenceType,
            SetsResolutionMethod = resolutionMethod,
            SetsRelationshipOutcome = relationshipOutcome,
            TransformDescription = dto.TransformDescription,
            PropertyReduction = propertyReduction
        };

        // Parse situation cards (victory conditions)
        if (dto.SituationCards != null && dto.SituationCards.Any())
        {
            foreach (SituationCardDTO situationCardDTO in dto.SituationCards)
            {
                SituationCard situationCard = ParseSituationCard(situationCardDTO, dto.Id);
                situation.SituationCards.Add(situationCard);
            }
        }
        return situation;
    }

    /// <summary>
    /// Parse a single situation card (victory condition)
    /// </summary>
    private static SituationCard ParseSituationCard(SituationCardDTO dto, string situationId)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException($"SituationCard in situation {situationId} missing required 'Id' field");

        SituationCard situationCard = new SituationCard
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            threshold = dto.threshold,
            Rewards = ParseSituationCardRewards(dto.Rewards),
            IsAchieved = false
        };

        return situationCard;
    }

    /// <summary>
    /// Parse situation card rewards
    /// Knowledge system eliminated - Understanding resource replaces Knowledge tokens
    /// </summary>
    private static SituationCardRewards ParseSituationCardRewards(SituationCardRewardsDTO dto)
    {
        if (dto == null)
            return new SituationCardRewards();

        SituationCardRewards rewards = new SituationCardRewards
        {
            Coins = dto.Coins,
            Progress = dto.Progress,
            Breakthrough = dto.Breakthrough,
            ObligationId = dto.ObligationId,
            Item = dto.Item,

            // Cube rewards (strong typing)
            InvestigationCubes = dto.InvestigationCubes,
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
    /// Parse situation costs from DTO
    /// </summary>
    private static SituationCosts ParseSituationCosts(SituationCostsDTO dto)
    {
        if (dto == null)
            return new SituationCosts();

        return new SituationCosts
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
