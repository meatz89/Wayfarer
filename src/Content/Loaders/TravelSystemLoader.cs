/// <summary>
/// Loads travel-related content: routes, path cards, events, collections.
/// COMPOSITION OVER INHERITANCE: Extracted from PackageLoader for single responsibility.
/// </summary>
public class TravelSystemLoader
{
    private readonly GameWorld _gameWorld;

    public TravelSystemLoader(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    /// <summary>
    /// Load all travel content from a package.
    /// Called by PackageLoader during package loading.
    /// </summary>
    public void LoadTravelContent(
        List<PathCardDTO> pathCardDtos,
        List<PathCardDTO> eventCardDtos,
        List<TravelEventDTO> travelEventDtos,
        List<PathCardCollectionDTO> collectionDtos,
        List<RouteDTO> routeDtos,
        PackageLoadResult result,
        bool allowSkeletons)
    {
        List<PathCardDTO> pathCardLookup = LoadPathCards(pathCardDtos, allowSkeletons);
        List<PathCardDTO> eventCardLookup = LoadEventCards(eventCardDtos, allowSkeletons);
        LoadTravelEvents(travelEventDtos, eventCardLookup, allowSkeletons);
        LoadEventCollections(collectionDtos, pathCardLookup, eventCardLookup, allowSkeletons);
        LoadRoutes(routeDtos, result, allowSkeletons);
    }

    /// <summary>
    /// Initialize the travel discovery system state after all content is loaded.
    /// Called ONCE after all packages loaded.
    /// </summary>
    public void InitializeTravelDiscoverySystem()
    {
        // Initialize PathCardDiscoveries from cards embedded in collections
        // First from path collections
        foreach (PathCardCollectionDTO collection in _gameWorld.AllPathCollections.Select(c => c.Collection))
        {
            foreach (PathCardDTO pathCard in collection.PathCards)
            {
                _gameWorld.SetPathCardDiscovered(pathCard, pathCard.StartsRevealed);
            }
        }

        // Also initialize discovery states for event cards in event collections
        foreach (PathCardCollectionDTO collection in _gameWorld.AllEventCollections.Select(c => c.Collection))
        {
            foreach (PathCardDTO eventCard in collection.EventCards)
            {
                _gameWorld.SetPathCardDiscovered(eventCard, eventCard.StartsRevealed);
            }
        }

        // Initialize EventDeckPositions for routes with event pools
        foreach (PathCollectionEntry entry in _gameWorld.AllEventCollections)
        {
            string routeId = entry.Collection.Id;
            string deckKey = $"route_{routeId}_events";
            _gameWorld.SetEventDeckPosition(deckKey, 0);
        }
    }

    private List<PathCardDTO> LoadPathCards(List<PathCardDTO> pathCardDtos, bool allowSkeletons)
    {
        if (pathCardDtos == null) return new List<PathCardDTO>();
        return pathCardDtos;
    }

    private List<PathCardDTO> LoadEventCards(List<PathCardDTO> eventCardDtos, bool allowSkeletons)
    {
        if (eventCardDtos == null) return new List<PathCardDTO>();
        return eventCardDtos;
    }

    private void LoadTravelEvents(List<TravelEventDTO> travelEventDtos, List<PathCardDTO> eventCardLookup, bool allowSkeletons)
    {
        if (travelEventDtos == null) return;

        foreach (TravelEventDTO dto in travelEventDtos)
        {
            // Embed actual event cards if this event has event card IDs
            if (dto.EventCardIds != null && dto.EventCards.Count == 0)
            {
                foreach (string cardId in dto.EventCardIds)
                {
                    PathCardDTO eventCard = eventCardLookup.FirstOrDefault(e => e.Name == cardId);
                    if (eventCard != null)
                    {
                        dto.EventCards.Add(eventCard);
                    }
                }
            }

            _gameWorld.AllTravelEvents.Add(new TravelEventEntry { TravelEvent = dto });
        }
    }

    private void LoadEventCollections(List<PathCardCollectionDTO> collectionDtos, List<PathCardDTO> pathCardLookup, List<PathCardDTO> eventCardLookup, bool allowSkeletons)
    {
        if (collectionDtos == null) return;

        foreach (PathCardCollectionDTO dto in collectionDtos)
        {
            // VALIDATION: Fail fast if required 'id' field is missing
            if (string.IsNullOrEmpty(dto.Id))
            {
                throw new InvalidOperationException(
                    "PathCardCollection missing required 'id' field. " +
                    "Check JSON - field name must be 'id' (lowercase), not 'collectionId'. " +
                    $"Collection data: PathCards={dto.PathCards?.Count ?? 0}, Events={dto.Events?.Count ?? 0}");
            }

            // Embed actual path cards if this collection has path card IDs
            if (dto.PathCards != null && dto.PathCards.Count == 0 && dto.PathCardIds != null)
            {
                foreach (string cardId in dto.PathCardIds)
                {
                    PathCardDTO pathCard = pathCardLookup.FirstOrDefault(p => p.Name == cardId);
                    if (pathCard != null)
                    {
                        dto.PathCards.Add(pathCard);
                    }
                }
            }

            // Determine if this is a path collection or event collection based on contents
            bool isEventCollection = (dto.Events != null && dto.Events.Count > 0);
            bool isPathCollection = (dto.PathCards != null && dto.PathCards.Count > 0);

            if (isEventCollection)
            {
                _gameWorld.AllEventCollections.Add(new PathCollectionEntry { Collection = dto });
            }
            else if (isPathCollection)
            {
                _gameWorld.AllPathCollections.Add(new PathCollectionEntry { Collection = dto });
            }
            else
            {
                _gameWorld.AllPathCollections.Add(new PathCollectionEntry { Collection = dto });
            }
        }
    }

    private void LoadRoutes(List<RouteDTO> routeDtos, PackageLoadResult result, bool allowSkeletons)
    {
        if (routeDtos == null) return;

        // Check for missing Locations and handle based on allowSkeletons
        foreach (RouteDTO dto in routeDtos)
        {
            // Check origin location
            Location originSpot = _gameWorld.Locations.FirstOrDefault(l => l.Name == dto.OriginSpotId);
            if (originSpot == null)
            {
                if (allowSkeletons)
                {
                    if (string.IsNullOrEmpty(dto.OriginVenueId))
                        throw new InvalidDataException($"Route '{dto.Name}' missing OriginVenueId - cannot create skeleton origin location");

                    originSpot = SkeletonGenerator.GenerateSkeletonSpot(
                        dto.OriginSpotId,
                        dto.OriginVenueId,
                        $"route_{dto.Name}_origin"
                    );
                    originSpot.Role = LocationRole.Connective;
                    _gameWorld.AddOrUpdateLocation(dto.OriginSpotId, originSpot);
                    _gameWorld.AddSkeleton(dto.OriginSpotId, "Location");
                }
                else
                {
                    throw new Exception($"[TravelSystemLoader] Route '{dto.Name}' references missing origin location '{dto.OriginSpotId}'.");
                }
            }

            // Check destination location
            Location destSpot = _gameWorld.Locations.FirstOrDefault(l => l.Name == dto.DestinationSpotId);
            if (destSpot == null)
            {
                if (allowSkeletons)
                {
                    if (string.IsNullOrEmpty(dto.DestinationVenueId))
                        throw new InvalidDataException($"Route '{dto.Name}' missing DestinationVenueId - cannot create skeleton destination location");

                    destSpot = SkeletonGenerator.GenerateSkeletonSpot(
                        dto.DestinationSpotId,
                        dto.DestinationVenueId,
                        $"route_{dto.Name}_destination"
                    );
                    destSpot.Role = LocationRole.Connective;
                    _gameWorld.AddOrUpdateLocation(dto.DestinationSpotId, destSpot);
                    _gameWorld.AddSkeleton(dto.DestinationSpotId, "Location");
                }
                else
                {
                    throw new Exception($"[TravelSystemLoader] Route '{dto.Name}' references missing destination location '{dto.DestinationSpotId}'.");
                }
            }
        }

        // BIDIRECTIONAL ROUTE PRINCIPLE: Routes are defined once but generate both directions
        foreach (RouteDTO dto in routeDtos)
        {
            RouteOption forwardRoute = RouteParser.ConvertRouteDTOToModel(dto, _gameWorld);
            _gameWorld.Routes.Add(forwardRoute);
            result.RoutesAdded.Add(forwardRoute);

            if (dto.CreateBidirectional)
            {
                RouteOption reverseRoute = RouteParser.GenerateReverseRoute(forwardRoute, _gameWorld);
                _gameWorld.Routes.Add(reverseRoute);
                result.RoutesAdded.Add(reverseRoute);
            }
        }
    }
}
