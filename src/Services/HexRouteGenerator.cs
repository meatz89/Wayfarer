using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Procedural route generator using hex-based pathfinding.
/// Generates routes between locations based on hex grid topology.
/// Calculates route properties (danger, time, transport) from hex path terrain.
///
/// Architecture: Routes connect Location → Location (never Venue → Venue)
/// Venue membership determines travel cost rules (same venue = free, different venue = route)
/// </summary>
public class HexRouteGenerator
{
    private readonly GameWorld _gameWorld;

    public HexRouteGenerator(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Generate routes between a new location and all other locations with different venue membership
    /// Called when new location created to establish connectivity
    /// Routes only generated between locations in DIFFERENT venues
    /// </summary>
    public List<RouteOption> GenerateRoutesForNewLocation(Location newLocation)
    {
        if (newLocation == null)
            throw new ArgumentNullException(nameof(newLocation));

        if (!newLocation.HexPosition.HasValue)
            throw new ArgumentException($"Location '{newLocation.Name}' has no HexPosition - cannot generate routes", nameof(newLocation));

        List<RouteOption> generatedRoutes = new List<RouteOption>();

        // Find all existing locations in DIFFERENT venues
        List<Location> otherLocations = _gameWorld.Locations
            .Where(loc => loc != newLocation && // Object reference comparison
                         loc.Venue != newLocation.Venue && // Object reference comparison
                         loc.HexPosition.HasValue)
            .ToList();

        foreach (Location destination in otherLocations)
        {
            // Try all transport types (start with most restrictive for optimal pathing)
            TransportType[] transportPriority = new[]
            {
                TransportType.Walking,   // Most universal
                TransportType.Horseback, // Faster but terrain-restricted
                TransportType.Cart,      // Efficient but limited
                TransportType.Boat       // Water only
            };

            RouteOption route = null;

            foreach (TransportType transport in transportPriority)
            {
                PathfindingResult pathResult = PathfindingService.FindPath(
                    newLocation.HexPosition.Value,
                    destination.HexPosition.Value,
                    _gameWorld.WorldHexGrid,
                    transport
                );

                if (pathResult.IsSuccess)
                {
                    route = CreateRouteFromPath(
                        newLocation,
                        destination,
                        pathResult.Path,
                        pathResult.DangerRating,
                        pathResult.TotalCost,
                        transport
                    );

                    generatedRoutes.Add(route);
                    break; // Use first successful transport type
                }
            }

            // No valid path found with any transport type
            if (route == null)
            {
                // Log or track unreachable locations (optional)
            }
        }

        return generatedRoutes;
    }

    /// <summary>
    /// Generate all routes between existing locations
    /// Used for initial world setup or full route regeneration
    /// Only generates routes between locations in different venues
    /// </summary>
    public List<RouteOption> GenerateAllRoutes()
    {
        List<RouteOption> allRoutes = new List<RouteOption>();

        // Get all locations with hex positions
        List<Location> locationsWithPosition = _gameWorld.Locations
            .Where(loc => loc.HexPosition.HasValue)
            .ToList();

        // Generate routes between all pairs (avoiding duplicates)
        for (int i = 0; i < locationsWithPosition.Count; i++)
        {
            for (int j = i + 1; j < locationsWithPosition.Count; j++)
            {
                Location loc1 = locationsWithPosition[i];
                Location loc2 = locationsWithPosition[j];

                // Only generate routes between different venues
                if (loc1.Venue == loc2.Venue) // Object reference comparison
                    continue; // Same venue = instant travel, no route needed

                // Try pathfinding with Walking (most universal)
                PathfindingResult pathResult = PathfindingService.FindPath(
                    loc1.HexPosition.Value,
                    loc2.HexPosition.Value,
                    _gameWorld.WorldHexGrid,
                    TransportType.Walking
                );

                if (pathResult.IsSuccess)
                {
                    // Create route from loc1 to loc2
                    RouteOption route = CreateRouteFromPath(
                        loc1,
                        loc2,
                        pathResult.Path,
                        pathResult.DangerRating,
                        pathResult.TotalCost,
                        TransportType.Walking
                    );

                    allRoutes.Add(route);

                    // Generate reverse route (B → A) for bidirectional travel
                    RouteOption reverseRoute = CreateRouteFromPath(
                        loc2,  // Swap: destination becomes origin
                        loc1,  // Swap: origin becomes destination
                        pathResult.Path.ToList().AsEnumerable().Reverse().ToList(),  // Reverse hex path
                        pathResult.DangerRating,
                        pathResult.TotalCost,
                        TransportType.Walking
                    );
                    allRoutes.Add(reverseRoute);
                }
            }
        }

        return allRoutes;
    }

    /// <summary>
    /// Create RouteOption from pathfinding result
    /// Calculates route properties from hex path (danger, time, costs)
    /// </summary>
    private RouteOption CreateRouteFromPath(
        Location origin,
        Location destination,
        List<AxialCoordinates> hexPath,
        int dangerRating,
        int pathCost,
        TransportType transportType)
    {
        // Calculate time segments based on path length and terrain
        int timeSegments = CalculateTimeSegments(hexPath, transportType);

        // Calculate stamina cost based on path difficulty
        int staminaCost = CalculateStaminaCost(hexPath, transportType);

        // Calculate coin cost based on transport type
        int coinCost = CalculateCoinCost(transportType, hexPath.Count);

        // Create route
        RouteOption route = new RouteOption
        {
            Name = $"{origin.Name} to {destination.Name}",
            OriginLocation = origin, // Object reference ONLY
            DestinationLocation = destination, // Object reference ONLY
            Method = ConvertTransportToTravelMethod(transportType),
            BaseCoinCost = coinCost,
            BaseStaminaCost = staminaCost,
            TravelTimeSegments = timeSegments,
            Description = GenerateRouteDescription(origin, destination, dangerRating, transportType),
            HexPath = hexPath,
            DangerRating = dangerRating,
            RouteType = DetermineRouteType(dangerRating)
        };

        // Generate route segments with encounters
        route.Segments = GenerateRouteSegments(hexPath, dangerRating, timeSegments, transportType);

        // Assign SceneTemplates to Encounter segments (template-only - Scene spawned when player reaches segment)
        AssignMandatorySceneTemplates(route, dangerRating);

        return route;
    }

    /// <summary>
    /// Assign MandatorySceneTemplate to Encounter segments at route generation time
    /// Templates are filtered by StoryCategory.Service (transactional encounters)
    /// Actual Scene spawning happens in TravelManager when player reaches segment
    /// DDR-007: Template selection is deterministic based on route and segment properties
    /// </summary>
    private void AssignMandatorySceneTemplates(RouteOption route, int dangerRating)
    {
        if (route.Segments == null || route.Segments.Count == 0)
            return;

        // Filter eligible templates by Category (Service = transactional encounters)
        List<SceneTemplate> eligibleTemplates = _gameWorld.SceneTemplates
            .Where(t => t.Category == StoryCategory.Service)
            .ToList();

        if (eligibleTemplates.Count == 0)
            return; // No Service scene templates available yet

        foreach (RouteSegment segment in route.Segments.Where(s => s.Type == SegmentType.Encounter))
        {
            // Filter by tier matching danger (within 1 tier)
            List<SceneTemplate> matching = FilterTemplatesByDanger(eligibleTemplates, dangerRating);
            if (matching.Count > 0)
            {
                segment.MandatorySceneTemplate = SelectDeterministicTemplate(matching, route.Name, segment.SegmentNumber);
            }
            else if (eligibleTemplates.Count > 0)
            {
                // Fallback: use any eligible template if no tier match
                segment.MandatorySceneTemplate = SelectDeterministicTemplate(eligibleTemplates, route.Name, segment.SegmentNumber);
            }
        }
    }

    /// <summary>
    /// Filter templates by danger rating category.
    /// Templates are already filtered by Category=Service.
    /// Returns all eligible templates - danger-based selection uses weighted randomization.
    /// </summary>
    private List<SceneTemplate> FilterTemplatesByDanger(List<SceneTemplate> templates, int dangerRating)
    {
        // All templates in pool are already filtered by Category=Service
        // Scene difficulty now scales via Location.Difficulty at choice generation time (arc42 §8.28)
        // Return all eligible templates - deterministic selection handles distribution
        return templates;
    }

    /// <summary>
    /// Select template based on segment number.
    /// arc42 §8.3: No hashing - use simple index arithmetic with Template.Id ordering.
    /// Templates have IDs (immutable archetypes allowed per §8.3).
    /// </summary>
    private SceneTemplate SelectDeterministicTemplate(List<SceneTemplate> templates, string routeName, int segmentNumber)
    {
        if (templates.Count == 1)
            return templates[0];

        // Sort templates by Id for consistent ordering (Template IDs allowed per arc42 §8.3)
        List<SceneTemplate> sortedTemplates = templates.OrderBy(t => t.Id).ToList();

        // Simple index selection: segment number determines which template
        int templateIndex = (segmentNumber - 1) % sortedTemplates.Count;

        return sortedTemplates[templateIndex];
    }

    // NOTE: SpawnActiveSceneForRoute() and InstantiateSituation() DELETED
    // Scene spawning for Encounter segments moved to TravelManager.SpawnEncounterScene()
    // Uses SceneInstantiator.ActivateScene() for proper entity resolution

    /// <summary>
    /// Generate route segments with danger-based encounter distribution
    /// Specification: Number of segments = TimeSegments, encounters distributed based on danger
    /// </summary>
    private List<RouteSegment> GenerateRouteSegments(
        List<AxialCoordinates> hexPath,
        int dangerRating,
        int timeSegments,
        TransportType transportType)
    {
        if (hexPath == null || hexPath.Count == 0 || timeSegments < 1)
            return new List<RouteSegment>();

        List<RouteSegment> segments = new List<RouteSegment>();

        // Calculate number of encounters based on danger rating
        int encounterCount = CalculateEncounterCount(dangerRating, timeSegments);

        // Determine encounter positions (avoid first and last segments)
        List<int> encounterPositions = DetermineEncounterPositions(timeSegments, encounterCount);

        // Divide hex path into segment ranges
        int hexesPerSegment = Math.Max(1, hexPath.Count / timeSegments);

        // Generate segments
        for (int i = 0; i < timeSegments; i++)
        {
            int segmentNumber = i + 1;
            bool isEncounter = encounterPositions.Contains(segmentNumber);

            // Get hex range for this segment
            int startHex = i * hexesPerSegment;
            int endHex = (i == timeSegments - 1) ? hexPath.Count : (i + 1) * hexesPerSegment;
            List<AxialCoordinates> segmentHexes = hexPath.Skip(startHex).Take(endHex - startHex).ToList();

            // Determine dominant terrain and danger for this segment
            SegmentAnalysisResult analysis = AnalyzeSegmentHexes(segmentHexes);
            TerrainType dominantTerrain = analysis.DominantTerrain;
            int segmentDanger = analysis.AverageDanger;

            RouteSegment segment = new RouteSegment
            {
                SegmentNumber = segmentNumber,
                Type = isEncounter ? SegmentType.Encounter : SegmentType.FixedPath,
                PathCollection = isEncounter ? null : GeneratePathCardsForSegment(dominantTerrain, segmentDanger, segmentNumber),
                // MandatorySceneTemplate: null initially, populated by scene spawning logic
                NarrativeDescription = GenerateSegmentDescription(dominantTerrain, segmentDanger)
            };

            segments.Add(segment);
        }

        return segments;
    }

    /// <summary>
    /// Calculate number of encounters based on danger rating
    /// Implements danger-based distribution algorithm
    /// </summary>
    private int CalculateEncounterCount(int dangerRating, int timeSegments)
    {
        if (dangerRating < 20)
            return 0; // Safe routes: no mandatory encounters

        if (dangerRating < 40)
            return 1; // Low danger: 1 encounter

        if (dangerRating < 60)
            return Math.Min(2, timeSegments - 2); // Medium danger: 2 encounters (if space)

        if (dangerRating < 80)
            return Math.Min(3, timeSegments - 2); // High danger: 3 encounters (if space)

        // DDR-007: Very high danger uses integer division (1 encounter per 3 segments)
        return Math.Min((timeSegments + 2) / 3, timeSegments - 2);
    }

    /// <summary>
    /// Determine positions for encounters within segment range
    /// Avoids first and last segments, spreads evenly
    /// </summary>
    private List<int> DetermineEncounterPositions(int totalSegments, int encounterCount)
    {
        List<int> positions = new List<int>();

        if (encounterCount == 0 || totalSegments < 3)
            return positions; // No room for encounters

        // Available positions: segments 2 through (totalSegments - 1)
        // Segment numbering is 1-based
        int availableStart = 2;
        int availableEnd = totalSegments; // Inclusive, but we want to avoid last
        int availableCount = availableEnd - availableStart; // Segments 2 to N-1

        if (availableCount < 1)
            return positions; // Not enough segments

        // Distribute evenly through available range
        if (encounterCount == 1)
        {
            // Place in middle
            positions.Add(availableStart + availableCount / 2);
        }
        else
        {
            // Distribute evenly
            int spacing = availableCount / encounterCount;
            for (int i = 0; i < encounterCount; i++)
            {
                int position = availableStart + (i * spacing + spacing / 2);
                // Ensure within bounds
                position = Math.Max(availableStart, Math.Min(availableEnd - 1, position));
                if (!positions.Contains(position))
                    positions.Add(position);
            }
        }

        return positions;
    }

    /// <summary>
    /// Analyze hex range to determine dominant terrain and average danger
    /// </summary>
    private SegmentAnalysisResult AnalyzeSegmentHexes(List<AxialCoordinates> segmentHexes)
    {
        if (segmentHexes == null || segmentHexes.Count == 0)
            return new SegmentAnalysisResult(TerrainType.Plains, 0);

        // Count terrain types
        Dictionary<TerrainType, int> terrainCounts = new Dictionary<TerrainType, int>();
        int totalDanger = 0;
        int validHexCount = 0;

        foreach (AxialCoordinates coords in segmentHexes)
        {
            Hex hex = _gameWorld.WorldHexGrid.GetHex(coords);
            if (hex != null)
            {
                // Count terrain
                if (!terrainCounts.ContainsKey(hex.Terrain))
                    terrainCounts[hex.Terrain] = 0;
                terrainCounts[hex.Terrain]++;

                // Sum danger
                totalDanger += hex.DangerLevel;
                validHexCount++;
            }
        }

        // Find dominant terrain (most common)
        TerrainType dominantTerrain = TerrainType.Plains;
        int maxCount = 0;
        foreach (KeyValuePair<TerrainType, int> kvp in terrainCounts)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                dominantTerrain = kvp.Key;
            }
        }

        // Calculate average danger
        int averageDanger = validHexCount > 0 ? totalDanger / validHexCount : 0;

        return new SegmentAnalysisResult(dominantTerrain, averageDanger);
    }

    /// <summary>
    /// Generate narrative description for segment based on terrain and danger
    /// </summary>
    private string GenerateSegmentDescription(TerrainType terrain, int danger)
    {
        string terrainDesc = terrain switch
        {
            TerrainType.Plains => "Open Plains",
            TerrainType.Road => "Well-Traveled Road",
            TerrainType.Forest => "Dense Forest",
            TerrainType.Mountains => "Mountain Pass",
            TerrainType.Swamp => "Treacherous Swamp",
            TerrainType.Water => "Water Crossing",
            _ => "Unknown Terrain"
        };

        string dangerDesc = danger < 3 ? "(Safe)" :
                           danger < 6 ? "(Caution Advised)" :
                           danger < 8 ? "(Dangerous)" :
                           "(Very Dangerous)";

        return $"{terrainDesc} {dangerDesc}";
    }

    /// <summary>
    /// Generate PathCardCollectionDTO for a FixedPath segment with 2 path options
    /// Safe path: slower (2 segments), no stamina cost
    /// Fast path: quicker (1 segment), stamina cost based on danger
    /// Both start face-down (StartsRevealed = false) for discovery mechanic
    /// </summary>
    private PathCardCollectionDTO GeneratePathCardsForSegment(TerrainType terrain, int danger, int segmentNumber)
    {
        PathCardNames names = GetTerrainPathNames(terrain);
        int fastPathStaminaCost = CalculateFastPathStaminaCost(danger);

        PathCardDTO safePath = new PathCardDTO
        {
            Id = $"procedural_safe_{segmentNumber}",
            Name = names.SafeName,
            StartsRevealed = false,
            StaminaCost = 0,
            TravelTimeSegments = 2,
            NarrativeText = names.SafeDescription
        };

        PathCardDTO fastPath = new PathCardDTO
        {
            Id = $"procedural_fast_{segmentNumber}",
            Name = names.FastName,
            StartsRevealed = false,
            StaminaCost = fastPathStaminaCost,
            TravelTimeSegments = 1,
            NarrativeText = names.FastDescription
        };

        return new PathCardCollectionDTO
        {
            Id = $"procedural_collection_{segmentNumber}",
            Name = $"Segment {segmentNumber} Paths",
            PathCards = new List<PathCardDTO> { safePath, fastPath }
        };
    }

    /// <summary>
    /// Get terrain-appropriate path names for procedural path cards
    /// </summary>
    private PathCardNames GetTerrainPathNames(TerrainType terrain)
    {
        return terrain switch
        {
            TerrainType.Forest => new PathCardNames(
                "Forest Trail", "A well-worn path through the trees.",
                "Through the Undergrowth", "Push through the dense brush - faster but exhausting."),
            TerrainType.Plains => new PathCardNames(
                "Caravan Road", "Follow the established trade route.",
                "Cross-Country", "Cut across the open fields directly."),
            TerrainType.Mountains => new PathCardNames(
                "Mountain Pass", "The long but steady switchback trail.",
                "Ridge Climb", "Scale the rocky ridge for a shortcut."),
            TerrainType.Swamp => new PathCardNames(
                "Raised Pathway", "Stick to the elevated boardwalk.",
                "Wade Through", "Trudge directly through the muck."),
            TerrainType.Road => new PathCardNames(
                "Main Road", "Continue along the well-maintained road.",
                "Side Track", "Take a lesser-known parallel path."),
            TerrainType.Water => new PathCardNames(
                "Bridge Crossing", "Use the established crossing point.",
                "Ford the River", "Wade across at a shallow point."),
            _ => new PathCardNames(
                "Main Path", "The obvious route forward.",
                "Shortcut", "A quicker but more demanding alternative.")
        };
    }

    /// <summary>
    /// Calculate stamina cost for fast path based on danger level
    /// Danger 0-2: 1 stamina, Danger 3-4: 2 stamina, Danger 5+: 2 stamina
    /// </summary>
    private int CalculateFastPathStaminaCost(int danger)
    {
        if (danger <= 2) return 1;
        return 2;
    }

    /// <summary>
    /// Helper struct for terrain-based path naming
    /// </summary>
    private readonly struct PathCardNames
    {
        public string SafeName { get; }
        public string SafeDescription { get; }
        public string FastName { get; }
        public string FastDescription { get; }

        public PathCardNames(string safeName, string safeDesc, string fastName, string fastDesc)
        {
            SafeName = safeName;
            SafeDescription = safeDesc;
            FastName = fastName;
            FastDescription = fastDesc;
        }
    }

    /// <summary>
    /// Calculate time segments from hex path (DDR-007: flat segment costs)
    /// </summary>
    private int CalculateTimeSegments(List<AxialCoordinates> hexPath, TransportType transportType)
    {
        if (hexPath == null || hexPath.Count == 0)
            return 1;

        int totalSegments = 0;

        foreach (AxialCoordinates coords in hexPath)
        {
            Hex hex = _gameWorld.WorldHexGrid.GetHex(coords);
            if (hex != null)
            {
                totalSegments += GetTerrainSegmentCost(hex.Terrain, transportType);
            }
        }

        // DDR-007: Total is already in segments, minimum 1
        return Math.Max(1, totalSegments);
    }

    /// <summary>
    /// Calculate stamina cost based on terrain difficulty (DDR-007: flat stamina costs)
    /// </summary>
    private int CalculateStaminaCost(List<AxialCoordinates> hexPath, TransportType transportType)
    {
        if (hexPath == null || hexPath.Count == 0)
            return 1;

        int totalStamina = 0;

        foreach (AxialCoordinates coords in hexPath)
        {
            Hex hex = _gameWorld.WorldHexGrid.GetHex(coords);
            if (hex != null)
            {
                totalStamina += GetTerrainStaminaCost(hex.Terrain, transportType);
            }
        }

        // DDR-007: Total is already flat stamina, minimum 1
        return Math.Max(1, totalStamina);
    }

    /// <summary>
    /// Calculate coin cost based on transport type (DDR-007: flat costs)
    /// </summary>
    private int CalculateCoinCost(TransportType transportType, int pathLength)
    {
        // DDR-007: Flat coin cost per transport type
        return transportType switch
        {
            TransportType.Walking => 0,        // Free
            TransportType.Cart => 5,           // 5 coins
            TransportType.Horseback => 8,      // 8 coins
            TransportType.Boat => 6,           // 6 coins
            _ => 0
        };
    }

    /// <summary>
    /// Get terrain segment cost (DDR-007: flat segment values)
    /// Each terrain type costs a fixed number of segments to traverse
    /// </summary>
    private int GetTerrainSegmentCost(TerrainType terrain, TransportType transportType)
    {
        return terrain switch
        {
            TerrainType.Plains => 1,           // Easy terrain: 1 segment
            TerrainType.Road => 1,             // Fast terrain: 1 segment
            TerrainType.Forest => transportType == TransportType.Cart ? 3 : 2,  // Moderate: 2 segments (3 for cart)
            TerrainType.Mountains => 3,        // Difficult terrain: 3 segments
            TerrainType.Swamp => 3,            // Difficult terrain: 3 segments
            TerrainType.Water => 1,            // Easy for boats: 1 segment
            TerrainType.Impassable => 99,      // Effectively impassable
            _ => 1
        };
    }

    /// <summary>
    /// Get terrain stamina cost (DDR-007: flat stamina values)
    /// Each terrain type costs a fixed amount of stamina to traverse
    /// </summary>
    private int GetTerrainStaminaCost(TerrainType terrain, TransportType transportType)
    {
        return terrain switch
        {
            TerrainType.Plains => 1,           // Easy: 1 stamina
            TerrainType.Road => 0,             // Roads are easy: 0 stamina
            TerrainType.Forest => 2,           // Moderate: 2 stamina
            TerrainType.Mountains => 3,        // Hard: 3 stamina
            TerrainType.Swamp => 3,            // Hard: 3 stamina
            TerrainType.Water => 1,            // Easy for boats: 1 stamina
            TerrainType.Impassable => 99,      // Effectively impassable
            _ => 1
        };
    }

    /// <summary>
    /// Convert TransportType to TravelMethods enum
    /// </summary>
    private TravelMethods ConvertTransportToTravelMethod(TransportType transportType)
    {
        return transportType switch
        {
            TransportType.Walking => TravelMethods.Walking,
            TransportType.Cart => TravelMethods.Cart,
            TransportType.Horseback => TravelMethods.Horseback,
            TransportType.Boat => TravelMethods.Boat,
            _ => TravelMethods.Walking
        };
    }

    /// <summary>
    /// Determine route type based on danger rating
    /// For hex-generated routes, default to Knowledge (learn once, know forever)
    /// </summary>
    private RouteType DetermineRouteType(int dangerRating)
    {
        // Hex-generated routes are knowledge-based (discover once, available forever)
        // Alternative types (Seal, Service) are for manually-authored special routes
        return RouteType.Knowledge;
    }

    /// <summary>
    /// Generate descriptive text for route
    /// </summary>
    private string GenerateRouteDescription(Location origin, Location destination, int dangerRating, TransportType transportType)
    {
        string safetyDesc = dangerRating < 10 ? "safe" :
                          dangerRating < 30 ? "moderately dangerous" :
                          "very dangerous";

        string transportDesc = transportType switch
        {
            TransportType.Walking => "on foot",
            TransportType.Cart => "by cart",
            TransportType.Horseback => "on horseback",
            TransportType.Boat => "by boat",
            _ => "by travel"
        };

        return $"Travel from {origin.Name} to {destination.Name} {transportDesc}. This route is {safetyDesc}.";
    }
}
