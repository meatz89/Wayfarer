using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Services
{
    /// <summary>
    /// A* pathfinding service for hex-based travel system.
    /// Generates shortest valid path between two hex coordinates with terrain/transport compatibility.
    /// Uses hex distance heuristic for optimal pathfinding performance.
    /// </summary>
    public class PathfindingService
    {
        /// <summary>
        /// Find shortest path between two coordinates using A* algorithm
        /// Returns null if no valid path exists
        /// </summary>
        public static PathfindingResult FindPath(
            AxialCoordinates start,
            AxialCoordinates goal,
            HexMap hexMap,
            TransportType transportType)
        {
            if (hexMap == null)
                throw new ArgumentNullException(nameof(hexMap), "HexMap cannot be null");

            // Validate start and goal hexes exist
            Hex startHex = hexMap.GetHex(start);
            Hex goalHex = hexMap.GetHex(goal);

            if (startHex == null)
                throw new ArgumentException($"Start coordinates ({start.Q}, {start.R}) do not exist in hex map");

            if (goalHex == null)
                throw new ArgumentException($"Goal coordinates ({goal.Q}, {goal.R}) do not exist in hex map");

            // Check if goal is reachable with transport type
            if (!IsTerrainTraversable(goalHex.Terrain, transportType))
            {
                return PathfindingResult.NoPathFound($"Goal hex terrain '{goalHex.Terrain}' is not traversable by {transportType}");
            }

            // A* data structures
            PriorityQueue<AxialCoordinates> openSet = new PriorityQueue<AxialCoordinates>();
            Dictionary<AxialCoordinates, AxialCoordinates> cameFrom = new Dictionary<AxialCoordinates, AxialCoordinates>();
            Dictionary<AxialCoordinates, float> gScore = new Dictionary<AxialCoordinates, float>();
            Dictionary<AxialCoordinates, float> fScore = new Dictionary<AxialCoordinates, float>();

            // Initialize start node
            gScore[start] = 0;
            fScore[start] = HeuristicCost(start, goal);
            openSet.Enqueue(start, fScore[start]);

            // A* main loop
            while (openSet.Count > 0)
            {
                AxialCoordinates current = openSet.Dequeue();

                // Goal reached - reconstruct path
                if (current.Equals(goal))
                {
                    List<AxialCoordinates> path = ReconstructPath(cameFrom, current);
                    float totalCost = gScore[current];
                    int dangerRating = CalculateDangerRating(path, hexMap);

                    return PathfindingResult.Success(path, totalCost, dangerRating);
                }

                // Explore neighbors
                AxialCoordinates[] neighbors = current.GetNeighbors();
                foreach (AxialCoordinates neighbor in neighbors)
                {
                    Hex neighborHex = hexMap.GetHex(neighbor);

                    // Skip if hex doesn't exist
                    if (neighborHex == null)
                        continue;

                    // Skip if terrain is not traversable by transport type
                    if (!IsTerrainTraversable(neighborHex.Terrain, transportType))
                        continue;

                    // Calculate movement cost to this neighbor
                    float movementCost = GetTerrainMovementCost(neighborHex.Terrain, transportType);
                    float tentativeGScore = gScore[current] + movementCost;

                    // If this path to neighbor is better than any previous one
                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = tentativeGScore + HeuristicCost(neighbor, goal);

                        // Add to open set if not already present
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Enqueue(neighbor, fScore[neighbor]);
                        }
                    }
                }
            }

            // No path found
            return PathfindingResult.NoPathFound($"No valid path exists from ({start.Q}, {start.R}) to ({goal.Q}, {goal.R}) for {transportType}");
        }

        /// <summary>
        /// Heuristic cost estimate (hex distance) - admissible and consistent for A*
        /// </summary>
        private static float HeuristicCost(AxialCoordinates from, AxialCoordinates to)
        {
            return from.DistanceTo(to);
        }

        /// <summary>
        /// Check if terrain is traversable by transport type
        /// Based on transport capabilities and terrain restrictions
        /// </summary>
        private static bool IsTerrainTraversable(TerrainType terrain, TransportType transportType)
        {
            // Impassable blocks all transport
            if (terrain == TerrainType.Impassable)
                return false;

            switch (transportType)
            {
                case TransportType.Walking:
                    // Walking: All except Water, Impassable
                    return terrain != TerrainType.Water;

                case TransportType.Cart:
                    // Cart: Road, Plains, Forest (limited)
                    return terrain == TerrainType.Road ||
                           terrain == TerrainType.Plains ||
                           terrain == TerrainType.Forest;

                case TransportType.Horseback:
                    // Horseback: Road, Plains, Forest
                    return terrain == TerrainType.Road ||
                           terrain == TerrainType.Plains ||
                           terrain == TerrainType.Forest;

                case TransportType.Boat:
                    // Boat: Water only
                    return terrain == TerrainType.Water;

                default:
                    throw new ArgumentException($"Unknown transport type: {transportType}");
            }
        }

        /// <summary>
        /// Get terrain movement cost multiplier for pathfinding
        /// Lower values = faster movement (preferred paths)
        /// Based on specification terrain multipliers (line 181-187)
        /// </summary>
        private static float GetTerrainMovementCost(TerrainType terrain, TransportType transportType)
        {
            // Base terrain costs
            switch (terrain)
            {
                case TerrainType.Plains:
                    return 1.0f;

                case TerrainType.Road:
                    return 0.8f; // Fastest

                case TerrainType.Forest:
                    // Cart struggles in forest
                    return transportType == TransportType.Cart ? 2.0f : 1.5f;

                case TerrainType.Mountains:
                    return 2.0f;

                case TerrainType.Swamp:
                    return 2.5f; // Slowest passable terrain

                case TerrainType.Water:
                    // Only traversable by boat
                    return 0.9f;

                case TerrainType.Impassable:
                    return float.PositiveInfinity; // Blocks path

                default:
                    throw new ArgumentException($"Unknown terrain type: {terrain}");
            }
        }

        /// <summary>
        /// Calculate total danger rating from hex path
        /// Sum of danger levels along path (0-10 per hex)
        /// </summary>
        private static int CalculateDangerRating(List<AxialCoordinates> path, HexMap hexMap)
        {
            int totalDanger = 0;

            foreach (AxialCoordinates coords in path)
            {
                Hex hex = hexMap.GetHex(coords);
                if (hex != null)
                {
                    totalDanger += hex.DangerLevel;
                }
            }

            return totalDanger;
        }

        /// <summary>
        /// Reconstruct path from A* came-from chain
        /// Returns path from start to current (inclusive)
        /// </summary>
        private static List<AxialCoordinates> ReconstructPath(
            Dictionary<AxialCoordinates, AxialCoordinates> cameFrom,
            AxialCoordinates current)
        {
            List<AxialCoordinates> path = new List<AxialCoordinates> { current };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current); // Prepend to maintain start → goal order
            }

            return path;
        }
    }

    /// <summary>
    /// Result of pathfinding operation
    /// Contains path, cost metrics, or failure reason
    /// </summary>
    public class PathfindingResult
    {
        public bool IsSuccess { get; private set; }
        public List<AxialCoordinates> Path { get; private set; }
        public float TotalCost { get; private set; }
        public int DangerRating { get; private set; }
        public string FailureReason { get; private set; }

        private PathfindingResult(bool isSuccess, List<AxialCoordinates> path, float totalCost, int dangerRating, string failureReason)
        {
            IsSuccess = isSuccess;
            Path = path;
            TotalCost = totalCost;
            DangerRating = dangerRating;
            FailureReason = failureReason;
        }

        public static PathfindingResult Success(List<AxialCoordinates> path, float totalCost, int dangerRating)
        {
            return new PathfindingResult(true, path, totalCost, dangerRating, null);
        }

        public static PathfindingResult NoPathFound(string reason)
        {
            return new PathfindingResult(false, new List<AxialCoordinates>(), 0, 0, reason);
        }
    }

    /// <summary>
    /// Simple priority queue for A* algorithm
    /// Min-heap based on priority value
    /// </summary>
    internal class PriorityQueue<T>
    {
        private List<(T item, float priority)> _elements = new List<(T, float)>();

        public int Count => _elements.Count;

        public void Enqueue(T item, float priority)
        {
            _elements.Add((item, priority));
            // Bubble up
            int childIndex = _elements.Count - 1;
            while (childIndex > 0)
            {
                int parentIndex = (childIndex - 1) / 2;
                if (_elements[childIndex].priority >= _elements[parentIndex].priority)
                    break;

                (_elements[childIndex], _elements[parentIndex]) = (_elements[parentIndex], _elements[childIndex]);
                childIndex = parentIndex;
            }
        }

        public T Dequeue()
        {
            if (_elements.Count == 0)
                throw new InvalidOperationException("Priority queue is empty");

            T result = _elements[0].item;

            // Move last element to root
            int lastIndex = _elements.Count - 1;
            _elements[0] = _elements[lastIndex];
            _elements.RemoveAt(lastIndex);

            // Bubble down
            if (_elements.Count > 0)
            {
                int parentIndex = 0;
                while (true)
                {
                    int leftChild = parentIndex * 2 + 1;
                    int rightChild = parentIndex * 2 + 2;
                    int smallestIndex = parentIndex;

                    if (leftChild < _elements.Count && _elements[leftChild].priority < _elements[smallestIndex].priority)
                        smallestIndex = leftChild;

                    if (rightChild < _elements.Count && _elements[rightChild].priority < _elements[smallestIndex].priority)
                        smallestIndex = rightChild;

                    if (smallestIndex == parentIndex)
                        break;

                    (_elements[parentIndex], _elements[smallestIndex]) = (_elements[smallestIndex], _elements[parentIndex]);
                    parentIndex = smallestIndex;
                }
            }

            return result;
        }

        public bool Contains(T item)
        {
            return _elements.Any(e => EqualityComparer<T>.Default.Equals(e.item, item));
        }
    }
}
