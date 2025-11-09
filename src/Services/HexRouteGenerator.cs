using System;
using System.Collections.Generic;
using System.Linq;

{
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
                throw new ArgumentException($"Location '{newLocation.Id}' has no HexPosition - cannot generate routes", nameof(newLocation));

            List<RouteOption> generatedRoutes = new List<RouteOption>();

            // Find all existing locations in DIFFERENT venues
            List<Location> otherLocations = _gameWorld.Locations
                .Where(loc => loc.Id != newLocation.Id &&
                             loc.VenueId != newLocation.VenueId && // Different venue = requires route
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
                    if (loc1.VenueId == loc2.VenueId)
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
            float pathCost,
            TransportType transportType)
        {
            // Generate unique route ID
            string routeId = $"route_{origin.Id}_{destination.Id}";

            // Calculate time segments based on path length and terrain
            int timeSegments = CalculateTimeSegments(hexPath, transportType);

            // Calculate stamina cost based on path difficulty
            int staminaCost = CalculateStaminaCost(hexPath, transportType);

            // Calculate coin cost based on transport type
            int coinCost = CalculateCoinCost(transportType, hexPath.Count);

            // Create route
            RouteOption route = new RouteOption
            {
                Id = routeId,
                Name = $"{origin.Name} to {destination.Name}",
                OriginLocationId = origin.Id,
                OriginLocation = origin,
                DestinationLocationId = destination.Id,
                DestinationLocation = destination,
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

            // Spawn scenes for Encounter segments
            SpawnEncounterScenes(route);

            return route;
        }

        /// <summary>
        /// Spawn scenes for all Encounter-type segments on the route
        /// Filters SceneTemplates by PlacementType=Route, terrain, and danger
        /// Creates Active scenes directly (not provisional - routes are permanent)
        /// </summary>
        private void SpawnEncounterScenes(RouteOption route)
        {
            if (route.Segments == null || route.Segments.Count == 0)
                return;

            // Get all SceneTemplates with Route placement
            List<SceneTemplate> routeTemplates = _gameWorld.SceneTemplates
                .Where(template => template.PlacementFilter != null &&
                                  template.PlacementFilter.PlacementType == PlacementType.Route)
                .ToList();

            if (routeTemplates.Count == 0)
                return; // No route scene templates available yet

            foreach (RouteSegment segment in route.Segments.Where(s => s.Type == SegmentType.Encounter))
            {
                // Get hex range for this segment to determine terrain/danger
                int hexesPerSegment = Math.Max(1, route.HexPath.Count / route.Segments.Count);
                int segmentIndex = segment.SegmentNumber - 1;
                int startHex = segmentIndex * hexesPerSegment;
                int endHex = (segmentIndex == route.Segments.Count - 1) ? route.HexPath.Count : (segmentIndex + 1) * hexesPerSegment;
                List<AxialCoordinates> segmentHexes = route.HexPath.Skip(startHex).Take(endHex - startHex).ToList();

                SegmentAnalysisResult analysis = AnalyzeSegmentHexes(segmentHexes);
                TerrainType dominantTerrain = analysis.DominantTerrain;
                int segmentDanger = analysis.AverageDanger;

                // Filter templates by terrain and danger
                List<SceneTemplate> matchingTemplates = FilterSceneTemplatesByTerrainAndDanger(
                    routeTemplates,
                    dominantTerrain,
                    segmentDanger
                );

                if (matchingTemplates.Count > 0)
                {
                    // Select random template (weighted by tier - lower tiers more common)
                    SceneTemplate selectedTemplate = SelectWeightedRandomTemplate(matchingTemplates);

                    // Spawn scene for this segment
                    Scene scene = SpawnActiveSceneForRoute(selectedTemplate, route, segment);

                    // Assign scene ID to segment
                    segment.MandatorySceneId = scene.Id;
                }
            }
        }

        /// <summary>
        /// Filter SceneTemplates by terrain and danger using PlacementFilter
        /// </summary>
        private List<SceneTemplate> FilterSceneTemplatesByTerrainAndDanger(
            List<SceneTemplate> templates,
            TerrainType segmentTerrain,
            int segmentDanger)
        {
            List<SceneTemplate> matching = new List<SceneTemplate>();

            foreach (SceneTemplate template in templates)
            {
                PlacementFilter filter = template.PlacementFilter;

                // Check terrain match (if filter specifies terrains)
                if (filter.TerrainTypes != null && filter.TerrainTypes.Count > 0)
                {
                    // Convert TerrainType to string for comparison
                    string terrainString = segmentTerrain.ToString();
                    if (!filter.TerrainTypes.Contains(terrainString))
                        continue; // Terrain doesn't match
                }

                // Check danger range (if filter specifies)
                if (filter.MinDangerRating.HasValue && segmentDanger < filter.MinDangerRating.Value)
                    continue; // Too safe for this template

                if (filter.MaxDangerRating.HasValue && segmentDanger > filter.MaxDangerRating.Value)
                    continue; // Too dangerous for this template

                matching.Add(template);
            }

            return matching;
        }

        /// <summary>
        /// Select random template with tier-based weighting (lower tiers more common)
        /// </summary>
        private SceneTemplate SelectWeightedRandomTemplate(List<SceneTemplate> templates)
        {
            if (templates.Count == 1)
                return templates[0];

            // Weight calculation: Tier 0 = 8x, Tier 1 = 4x, Tier 2 = 2x, Tier 3+ = 1x
            int totalWeight = 0;
            List<int> weights = new List<int>();

            foreach (SceneTemplate template in templates)
            {
                int weight = template.Tier switch
                {
                    0 => 8, // Safety net scenes very common
                    1 => 4, // Low complexity common
                    2 => 2, // Standard complexity moderate
                    _ => 1  // High complexity rare
                };
                weights.Add(weight);
                totalWeight += weight;
            }

            // Random selection weighted by tier
            Random random = new Random();
            int roll = random.Next(totalWeight);
            int cumulative = 0;

            for (int i = 0; i < templates.Count; i++)
            {
                cumulative += weights[i];
                if (roll < cumulative)
                    return templates[i];
            }

            // Fallback (shouldn't reach here)
            return templates[0];
        }

        /// <summary>
        /// Spawn Active scene directly for route (non-provisional)
        /// Route scenes are permanent, not choice-dependent like location/NPC scenes
        /// </summary>
        private Scene SpawnActiveSceneForRoute(SceneTemplate template, RouteOption route, RouteSegment segment)
        {
            // Generate unique Scene ID
            string sceneId = $"scene_{template.Id}_{route.Id}_seg{segment.SegmentNumber}";

            // Create Scene directly as Active (skip provisional step)
            Scene scene = new Scene
            {
                Id = sceneId,
                TemplateId = template.Id,
                Template = template,
                PlacementType = PlacementType.Route,
                PlacementId = route.Id,
                State = SceneState.Active, // Active immediately, not provisional
                Archetype = template.Archetype,
                DisplayName = template.DisplayNameTemplate,
                IntroNarrative = template.IntroNarrativeTemplate,
                SpawnRules = template.SpawnRules
            };

            // Create Situations from SituationTemplates
            foreach (SituationTemplate sitTemplate in template.SituationTemplates)
            {
                Situation situation = InstantiateSituation(sitTemplate, scene, route);
                scene.Situations.Add(situation);
            }

            // Set CurrentSituation to first Situation (direct object reference)
            scene.CurrentSituation = scene.Situations.FirstOrDefault();

            // Add to GameWorld.Scenes (permanent storage)
            _gameWorld.Scenes.Add(scene);

            return scene;
        }

        /// <summary>
        /// Instantiate Situation from SituationTemplate for route scene
        /// Simplified version without placeholder replacement (routes have minimal dynamic context)
        /// </summary>
        private Situation InstantiateSituation(SituationTemplate template, Scene parentScene, RouteOption route)
        {
            string situationId = $"situation_{template.Id}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

            Situation situation = new Situation
            {
                Id = situationId,
                Name = template.Id, // Use template ID as name
                Description = template.NarrativeTemplate ?? "",
                InstantiationState = InstantiationState.Deferred, // Starts deferred, instantiates when player enters segment
                Template = template,
                SystemType = TacticalSystemType.Physical, // Route encounters default to Physical
                ParentScene = parentScene,
                Tier = parentScene.Template?.Tier ?? 1,
                Repeatable = false // Route encounters are one-time
            };

            return situation;
        }

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
                    PathCollectionId = null, // Self-sufficient: FixedPath uses NarrativeDescription only, no PathCards needed
                    MandatorySceneId = null, // Will be populated by scene spawning
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

            // Very high danger: 30% of segments (spec recommendation)
            return Math.Min((int)Math.Ceiling(timeSegments * 0.3), timeSegments - 2);
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
                float spacing = (float)availableCount / encounterCount;
                for (int i = 0; i < encounterCount; i++)
                {
                    int position = availableStart + (int)Math.Round(i * spacing + spacing / 2);
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
        /// Calculate time segments from hex path
        /// Based on specification: TimeSegments = PathLength × TerrainMultipliers
        /// </summary>
        private int CalculateTimeSegments(List<AxialCoordinates> hexPath, TransportType transportType)
        {
            if (hexPath == null || hexPath.Count == 0)
                return 1;

            float totalTime = 0;

            foreach (AxialCoordinates coords in hexPath)
            {
                Hex hex = _gameWorld.WorldHexGrid.GetHex(coords);
                if (hex != null)
                {
                    totalTime += GetTerrainTimeMultiplier(hex.Terrain, transportType);
                }
            }

            // Convert to time segments (round up, minimum 1)
            return Math.Max(1, (int)Math.Ceiling(totalTime / 3.0f)); // Divide by 3 for segment granularity
        }

        /// <summary>
        /// Calculate stamina cost based on terrain difficulty
        /// </summary>
        private int CalculateStaminaCost(List<AxialCoordinates> hexPath, TransportType transportType)
        {
            if (hexPath == null || hexPath.Count == 0)
                return 1;

            float totalStamina = 0;

            foreach (AxialCoordinates coords in hexPath)
            {
                Hex hex = _gameWorld.WorldHexGrid.GetHex(coords);
                if (hex != null)
                {
                    // Base stamina from terrain difficulty
                    float terrainStamina = GetTerrainTimeMultiplier(hex.Terrain, transportType) * 0.5f;
                    totalStamina += terrainStamina;
                }
            }

            return Math.Max(1, (int)Math.Ceiling(totalStamina));
        }

        /// <summary>
        /// Calculate coin cost based on transport type and distance
        /// </summary>
        private int CalculateCoinCost(TransportType transportType, int pathLength)
        {
            int baseCost = transportType switch
            {
                TransportType.Walking => 0,      // Free
                TransportType.Cart => 5,         // Moderate fee
                TransportType.Horseback => 10,   // Higher fee
                TransportType.Boat => 8,         // Moderate fee
                _ => 0
            };

            // Scale with distance
            return baseCost + (pathLength / 5); // +1 coin per 5 hexes
        }

        /// <summary>
        /// Get terrain time multiplier (same as pathfinding movement cost)
        /// From specification lines 181-187
        /// </summary>
        private float GetTerrainTimeMultiplier(TerrainType terrain, TransportType transportType)
        {
            switch (terrain)
            {
                case TerrainType.Plains:
                    return 1.0f;
                case TerrainType.Road:
                    return 0.8f;
                case TerrainType.Forest:
                    return transportType == TransportType.Cart ? 2.0f : 1.5f;
                case TerrainType.Mountains:
                    return 2.0f;
                case TerrainType.Swamp:
                    return 2.5f;
                case TerrainType.Water:
                    return 0.9f;
                case TerrainType.Impassable:
                    return float.PositiveInfinity;
                default:
                    return 1.0f;
            }
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
}
