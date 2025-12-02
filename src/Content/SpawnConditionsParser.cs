
/// <summary>
/// Parser for converting SpawnConditionsDTO to SpawnConditions domain model
/// Validates enum values and converts collections
/// NO catalogue usage - SpawnConditions is pure data structure
/// </summary>
public static class SpawnConditionsParser
{
    /// <summary>
    /// Convert SpawnConditionsDTO to SpawnConditions domain entity
    /// Returns SpawnConditions.AlwaysEligible if dto is null (unconditional spawning)
    /// DDD pattern: Explicit sentinel value, not implicit null check
    /// </summary>
    public static SpawnConditions ParseSpawnConditions(SpawnConditionsDTO dto)
    {
        if (dto == null)
            return SpawnConditions.AlwaysEligible;

        // Parse CombinationLogic enum (defaults to AND if not specified)
        CombinationLogic combinationLogic = CombinationLogic.AND;
        if (!string.IsNullOrEmpty(dto.CombinationLogic))
        {
            if (!Enum.TryParse<CombinationLogic>(dto.CombinationLogic, true, out combinationLogic))
            {
                throw new InvalidOperationException($"SpawnConditions has invalid CombinationLogic value: '{dto.CombinationLogic}'. Must be 'AND' or 'OR'.");
            }
        }

        return new SpawnConditions
        {
            PlayerState = ParsePlayerStateConditions(dto.PlayerState),
            WorldState = ParseWorldStateConditions(dto.WorldState),
            EntityState = ParseEntityStateConditions(dto.EntityState),
            CombinationLogic = combinationLogic
        };
    }

    /// <summary>
    /// Parse PlayerStateConditions from DTO
    /// Validates MinStats dictionary for valid ScaleType enum values
    /// </summary>
    private static PlayerStateConditions ParsePlayerStateConditions(PlayerStateConditionsDTO dto)
    {
        if (dto == null)
            return new PlayerStateConditions();

        // Parse MinStats - DOMAIN COLLECTION PRINCIPLE: Set explicit properties via switch
        int? minMorality = null, minLawfulness = null, minMethod = null, minCaution = null, minTransparency = null, minFame = null;
        if (dto.MinStats != null)
        {
            foreach (StatThresholdDTO statDto in dto.MinStats)
            {
                if (!Enum.TryParse<ScaleType>(statDto.Stat, true, out ScaleType scaleType))
                {
                    throw new InvalidOperationException($"SpawnConditions.PlayerState.MinStats has invalid ScaleType key: '{statDto.Stat}'. Must be valid ScaleType enum value.");
                }
                switch (scaleType)
                {
                    case ScaleType.Morality: minMorality = statDto.Threshold; break;
                    case ScaleType.Lawfulness: minLawfulness = statDto.Threshold; break;
                    case ScaleType.Method: minMethod = statDto.Threshold; break;
                    case ScaleType.Caution: minCaution = statDto.Threshold; break;
                    case ScaleType.Transparency: minTransparency = statDto.Threshold; break;
                    case ScaleType.Fame: minFame = statDto.Threshold; break;
                }
            }
        }

        // Convert LocationVisits from DTO to domain entry
        List<LocationVisitEntry> locationVisits = new List<LocationVisitEntry>();
        if (dto.LocationVisits != null)
        {
            foreach (LocationVisitEntry entry in dto.LocationVisits)
            {
                locationVisits.Add(new LocationVisitEntry
                {
                    LocationId = entry.LocationId,
                    VisitCount = entry.VisitCount
                });
            }
        }

        return new PlayerStateConditions
        {
            MinMorality = minMorality,
            MinLawfulness = minLawfulness,
            MinMethod = minMethod,
            MinCaution = minCaution,
            MinTransparency = minTransparency,
            MinFame = minFame,
            RequiredItems = dto.RequiredItems ?? new List<string>(),
            LocationVisits = locationVisits
        };
    }

    /// <summary>
    /// Parse WorldStateConditions from DTO
    /// Validates Weather, TimeBlock, and LocationStates enum values
    /// </summary>
    private static WorldStateConditions ParseWorldStateConditions(WorldStateConditionsDTO dto)
    {
        if (dto == null)
            return new WorldStateConditions();

        // Parse optional Weather enum
        WeatherCondition? weather = null;
        if (!string.IsNullOrEmpty(dto.Weather))
        {
            if (!Enum.TryParse<WeatherCondition>(dto.Weather, true, out WeatherCondition weatherValue))
            {
                throw new InvalidOperationException($"SpawnConditions.WorldState.Weather has invalid value: '{dto.Weather}'. Must be valid WeatherCondition enum value.");
            }
            weather = weatherValue;
        }

        // Parse optional TimeBlock enum
        TimeBlocks? timeBlock = null;
        if (!string.IsNullOrEmpty(dto.TimeBlock))
        {
            if (!Enum.TryParse<TimeBlocks>(dto.TimeBlock, true, out TimeBlocks timeBlockValue))
            {
                throw new InvalidOperationException($"SpawnConditions.WorldState.TimeBlock has invalid value: '{dto.TimeBlock}'. Must be valid TimeBlocks enum value.");
            }
            timeBlock = timeBlockValue;
        }

        // Parse LocationStates list (StateType enum)
        List<StateType> locationStates = new List<StateType>();
        if (dto.LocationStates != null)
        {
            foreach (string stateString in dto.LocationStates)
            {
                if (!Enum.TryParse<StateType>(stateString, true, out StateType stateType))
                {
                    throw new InvalidOperationException($"SpawnConditions.WorldState.LocationStates has invalid StateType value: '{stateString}'. Must be valid StateType enum value.");
                }
                locationStates.Add(stateType);
            }
        }

        return new WorldStateConditions
        {
            Weather = weather,
            TimeBlock = timeBlock,
            MinDay = dto.MinDay,
            MaxDay = dto.MaxDay,
            LocationStates = locationStates
        };
    }

    /// <summary>
    /// Parse EntityStateConditions from DTO
    /// Converts DTO entry lists to domain entry lists
    /// </summary>
    private static EntityStateConditions ParseEntityStateConditions(EntityStateConditionsDTO dto)
    {
        if (dto == null)
            return new EntityStateConditions();

        // Convert NPCBond list from DTO to domain entry
        List<NPCBondEntry> npcBond = new List<NPCBondEntry>();
        if (dto.NPCBond != null)
        {
            foreach (NPCBondEntry entry in dto.NPCBond)
            {
                npcBond.Add(new NPCBondEntry
                {
                    NpcId = entry.NpcId,
                    BondStrength = entry.BondStrength
                });
            }
        }

        // Convert LocationReputation list from DTO to domain entry
        List<LocationReputationEntry> locationReputation = new List<LocationReputationEntry>();
        if (dto.LocationReputation != null)
        {
            foreach (LocationReputationEntry entry in dto.LocationReputation)
            {
                locationReputation.Add(new LocationReputationEntry
                {
                    LocationId = entry.LocationId,
                    ReputationScore = entry.Reputation
                });
            }
        }

        // Convert RouteTravelCount list from DTO to domain entry
        List<RouteTravelCountEntry> routeTravelCount = new List<RouteTravelCountEntry>();
        if (dto.RouteTravelCount != null)
        {
            foreach (RouteTravelCountEntry entry in dto.RouteTravelCount)
            {
                routeTravelCount.Add(new RouteTravelCountEntry
                {
                    RouteId = entry.RouteId,
                    TravelCount = entry.TravelCount
                });
            }
        }

        return new EntityStateConditions
        {
            NPCBond = npcBond,
            LocationReputation = locationReputation,
            RouteTravelCount = routeTravelCount,
            Properties = dto.Properties ?? new List<string>()
        };
    }
}
