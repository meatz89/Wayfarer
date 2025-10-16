using System.Text.Json;


public class TravelObstacleParser
{
    public TravelObstacle ParseTravelObstacle(TravelObstacleDTO dto)
    {
        TravelObstacle obstacle = new TravelObstacle
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            VenueId = dto.VenueId,
            RouteId = dto.RouteId
        };

        if (Enum.TryParse<ObstacleType>(dto.Type, out ObstacleType obstacleType))
        {
            obstacle.Type = obstacleType;
        }

        if (dto.Approaches != null)
        {
            foreach (ObstacleApproachDTO approachDto in dto.Approaches)
            {
                obstacle.Approaches.Add(ParseApproach(approachDto));
            }
        }

        return obstacle;
    }

    private ObstacleApproach ParseApproach(ObstacleApproachDTO dto)
    {
        ObstacleApproach approach = new ObstacleApproach
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            SuccessProbability = dto.SuccessProbability,
            StaminaRequired = dto.StaminaRequired
        };

        // Knowledge system eliminated - no knowledge requirements

        if (dto.EquipmentRequirements != null)
        {
            approach.EquipmentRequirements = ParseEquipmentRequirement(dto.EquipmentRequirements);
        }

        if (dto.StatRequirements != null)
        {
            approach.StatRequirements = new Dictionary<PlayerStatType, int>();
            foreach (KeyValuePair<string, int> kvp in dto.StatRequirements)
            {
                if (Enum.TryParse<PlayerStatType>(kvp.Key, out PlayerStatType statType))
                {
                    approach.StatRequirements[statType] = kvp.Value;
                }
            }
        }

        if (dto.SuccessOutcome != null)
        {
            approach.SuccessOutcome = ParseOutcome(dto.SuccessOutcome);
        }

        if (dto.FailureOutcome != null)
        {
            approach.FailureOutcome = ParseOutcome(dto.FailureOutcome);
        }

        return approach;
    }

    private ObstacleOutcome ParseOutcome(ObstacleOutcomeDTO dto)
    {
        ObstacleOutcome outcome = new ObstacleOutcome
        {
            Description = dto.Description,
            TimeSegmentCost = dto.TimeSegmentCost,
            StaminaCost = dto.StaminaCost,
            HealthChange = dto.HealthChange
            // Knowledge system eliminated - no knowledge rewards
        };

        if (dto.RouteImprovement != null)
        {
            outcome.RouteImprovement = ParseRouteImprovement(dto.RouteImprovement);
        }

        return outcome;
    }

    private RouteImprovement ParseRouteImprovement(RouteImprovementDTO dto)
    {
        return new RouteImprovement
        {
            Description = dto.Description,
            TimeReduction = dto.TimeReduction,
            StaminaReduction = dto.StaminaReduction
        };
    }

    // Knowledge parsing deleted - Knowledge system eliminated

    private EquipmentRequirement ParseEquipmentRequirement(EquipmentRequirementDTO dto)
    {
        return new EquipmentRequirement
        {
            RequiredEquipment = dto.RequiredEquipment ?? new List<string>()
        };
    }
}
