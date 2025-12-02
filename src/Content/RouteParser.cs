/// <summary>
/// Parser for RouteOption domain entities from RouteDTO.
/// HIGHLANDER: Resolves string IDs to object references at parse-time.
/// </summary>
public static class RouteParser
{
    /// <summary>
    /// Convert RouteDTO to RouteOption domain entity.
    /// HIGHLANDER: Resolves all ID references to object references at parse-time.
    /// </summary>
    public static RouteOption ConvertRouteDTOToModel(RouteDTO dto, GameWorld gameWorld)
    {
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidDataException("Route missing required field 'Name'");
        if (string.IsNullOrEmpty(dto.OriginSpotId))
            throw new InvalidDataException($"Route '{dto.Name}' missing required field 'OriginSpotId'");
        if (string.IsNullOrEmpty(dto.DestinationSpotId))
            throw new InvalidDataException($"Route '{dto.Name}' missing required field 'DestinationSpotId'");
        if (string.IsNullOrEmpty(dto.Description))
            throw new InvalidDataException($"Route '{dto.Name}' missing required field 'Description'");

        // HIGHLANDER: LINQ queries on already-parsed locations
        RouteOption route = new RouteOption
        {
            Name = dto.Name,
            OriginLocation = gameWorld.Locations.FirstOrDefault(l => l.Name == dto.OriginSpotId),
            DestinationLocation = gameWorld.Locations.FirstOrDefault(l => l.Name == dto.DestinationSpotId),
            Method = Enum.TryParse<TravelMethods>(dto.Method, out TravelMethods method) ? method : TravelMethods.Walking,
            BaseCoinCost = dto.BaseCoinCost,
            BaseStaminaCost = dto.BaseStaminaCost,
            TravelTimeSegments = dto.TravelTimeSegments,
            Description = dto.Description,
            MaxItemCapacity = dto.MaxItemCapacity > 0 ? dto.MaxItemCapacity : 3
        };

        // Parse terrain categories
        if (dto.TerrainCategories != null)
        {
            foreach (string category in dto.TerrainCategories)
            {
                if (Enum.TryParse<TerrainCategory>(category, out TerrainCategory terrain))
                {
                    route.TerrainCategories.Add(terrain);
                }
            }
        }

        // Parse travel path cards system properties
        route.StartingStamina = dto.StartingStamina;

        // Parse route segments
        if (dto.Segments != null)
        {
            foreach (RouteSegmentDTO segmentDto in dto.Segments)
            {
                RouteSegment segment = ParseRouteSegment(segmentDto, gameWorld);
                route.Segments.Add(segment);
            }
        }

        return route;
    }

    /// <summary>
    /// Parse a single RouteSegment from DTO.
    /// HIGHLANDER: Resolves collection IDs to object references at parse-time.
    /// </summary>
    private static RouteSegment ParseRouteSegment(RouteSegmentDTO segmentDto, GameWorld gameWorld)
    {
        // Parse segment type
        SegmentType segmentType = SegmentType.FixedPath;
        if (!string.IsNullOrEmpty(segmentDto.Type))
        {
            Enum.TryParse<SegmentType>(segmentDto.Type, out segmentType);
        }

        RouteSegment segment = new RouteSegment
        {
            SegmentNumber = segmentDto.SegmentNumber,
            Type = segmentType,
            NarrativeDescription = segmentDto.NarrativeDescription
        };

        // HIGHLANDER: Resolve IDs to object references at parse-time
        if (segmentType == SegmentType.FixedPath)
        {
            if (!string.IsNullOrEmpty(segmentDto.PathCollectionId))
            {
                segment.PathCollection = gameWorld.GetPathCollection(segmentDto.PathCollectionId);
            }
        }
        else if (segmentType == SegmentType.Event)
        {
            if (!string.IsNullOrEmpty(segmentDto.EventCollectionId))
            {
                segment.EventCollection = gameWorld.GetPathCollection(segmentDto.EventCollectionId);
            }
        }
        else if (segmentType == SegmentType.Encounter)
        {
            if (!string.IsNullOrEmpty(segmentDto.MandatorySceneId))
            {
                segment.MandatorySceneTemplate = gameWorld.SceneTemplates
                    .FirstOrDefault(t => t.Id == segmentDto.MandatorySceneId);
            }
        }

        return segment;
    }

    /// <summary>
    /// Generate the reverse route from a forward route.
    /// BIDIRECTIONAL ROUTE GENERATION: Ensures travel is always bidirectional.
    /// </summary>
    public static RouteOption GenerateReverseRoute(RouteOption forwardRoute, GameWorld gameWorld)
    {
        string originLocationName = forwardRoute.OriginLocation.Name;
        string destLocationName = forwardRoute.DestinationLocation.Name;

        RouteOption reverseRoute = new RouteOption
        {
            Name = $"Return to {originLocationName}",
            OriginLocation = forwardRoute.DestinationLocation,
            DestinationLocation = forwardRoute.OriginLocation,
            Method = forwardRoute.Method,
            BaseCoinCost = forwardRoute.BaseCoinCost,
            BaseStaminaCost = forwardRoute.BaseStaminaCost,
            TravelTimeSegments = forwardRoute.TravelTimeSegments,
            DepartureTime = forwardRoute.DepartureTime,
            MaxItemCapacity = forwardRoute.MaxItemCapacity,
            Description = $"Return journey from {destLocationName} to {originLocationName}",
            RouteType = forwardRoute.RouteType,
            HasPermitUnlock = forwardRoute.HasPermitUnlock,
            StartingStamina = forwardRoute.StartingStamina
        };

        // Copy terrain categories
        reverseRoute.TerrainCategories.AddRange(forwardRoute.TerrainCategories);

        // Copy weather modifications - DOMAIN COLLECTION PRINCIPLE: Explicit properties for fixed enum
        reverseRoute.ClearWeatherModification = forwardRoute.ClearWeatherModification;
        reverseRoute.RainWeatherModification = forwardRoute.RainWeatherModification;
        reverseRoute.SnowWeatherModification = forwardRoute.SnowWeatherModification;
        reverseRoute.FogWeatherModification = forwardRoute.FogWeatherModification;
        reverseRoute.StormWeatherModification = forwardRoute.StormWeatherModification;

        // Reverse the segments order for the return journey
        List<RouteSegment> reversedSegments = forwardRoute.Segments.OrderByDescending(s => s.SegmentNumber).ToList();
        int segmentNumber = 1;
        foreach (RouteSegment originalSegment in reversedSegments)
        {
            RouteSegment reverseSegment = new RouteSegment
            {
                SegmentNumber = segmentNumber++,
                Type = originalSegment.Type,
                PathCollection = originalSegment.PathCollection,
                EventCollection = originalSegment.EventCollection,
                MandatorySceneTemplate = originalSegment.MandatorySceneTemplate
            };
            reverseRoute.Segments.Add(reverseSegment);
        }

        // Copy route-level event pool if exists
        PathCollectionEntry forwardEntry = gameWorld.AllEventCollections.FirstOrDefault(x => x.Collection.Id == forwardRoute.Name);
        if (forwardEntry != null)
        {
            PathCollectionEntry reverseEntry = new PathCollectionEntry
            {
                Collection = forwardEntry.Collection
            };
            gameWorld.AllEventCollections.Add(reverseEntry);
        }

        return reverseRoute;
    }
}
