using System;
using System.Collections.Generic;

namespace Wayfarer.Subsystems.TravelSubsystem
{
    /// <summary>
    /// Calculates travel times and costs between locations.
    /// </summary>
    public class TravelTimeCalculator
    {
        private readonly GameWorld _gameWorld;

        // Travel time matrix in minutes
        private static readonly Dictionary<(string, string), int> TravelTimes = new Dictionary<(string, string), int>
        {
            // Your Room connections (very close to Market Square)
            { ("your_room", "market_square"), 10 },
            { ("market_square", "your_room"), 10 },
            
            // Market Square as central hub
            { ("market_square", "noble_district"), 30 },
            { ("noble_district", "market_square"), 30 },
            { ("market_square", "merchant_row"), 15 },
            { ("merchant_row", "market_square"), 15 },
            { ("market_square", "city_gates"), 30 },
            { ("city_gates", "market_square"), 30 },
            { ("market_square", "riverside"), 45 },
            { ("riverside", "market_square"), 45 },
            
            // Non-hub connections (longer)
            { ("noble_district", "merchant_row"), 45 },
            { ("merchant_row", "noble_district"), 45 },
            { ("noble_district", "city_gates"), 60 },
            { ("city_gates", "noble_district"), 60 },
            { ("noble_district", "riverside"), 60 },
            { ("riverside", "noble_district"), 60 },
            { ("merchant_row", "city_gates"), 30 },
            { ("city_gates", "merchant_row"), 30 },
            { ("merchant_row", "riverside"), 60 },
            { ("riverside", "merchant_row"), 60 },
            { ("city_gates", "riverside"), 30 },
            { ("riverside", "city_gates"), 30 },
            
            // Your Room to other locations (must go through Market Square)
            { ("your_room", "noble_district"), 40 },
            { ("noble_district", "your_room"), 40 },
            { ("your_room", "merchant_row"), 25 },
            { ("merchant_row", "your_room"), 25 },
            { ("your_room", "city_gates"), 40 },
            { ("city_gates", "your_room"), 40 },
            { ("your_room", "riverside"), 55 },
            { ("riverside", "your_room"), 55 },
        };

        public TravelTimeCalculator(GameWorld gameWorld)
        {
            _gameWorld = gameWorld;
        }

        /// <summary>
        /// Get base travel time between two locations in minutes.
        /// </summary>
        public int GetBaseTravelTime(string fromLocationId, string toLocationId)
        {
            // Same location = no travel time
            if (fromLocationId == toLocationId)
            {
                return 0;
            }

            // Look up travel time
            (string fromLocationId, string toLocationId) key = (fromLocationId, toLocationId);
            if (TravelTimes.TryGetValue(key, out int time))
            {
                return time;
            }

            // No fallback - route must be defined
            Console.WriteLine($"[TravelTimeCalculator] Warning: No travel time defined for {fromLocationId} -> {toLocationId}");
            return 60; // Default to 1 hour if not found
        }

        /// <summary>
        /// Calculate actual travel time with transport method modifier.
        /// </summary>
        public int CalculateTravelTime(string fromLocationId, string toLocationId, TravelMethods transportMethod)
        {
            int baseTime = GetBaseTravelTime(fromLocationId, toLocationId);

            // Apply transport method modifier
            double modifier = GetTransportModifier(transportMethod);
            int actualTime = (int)(baseTime * modifier);

            // Apply weather effects if any
            actualTime = ApplyWeatherEffects(actualTime);

            return Math.Max(5, actualTime); // Minimum 5 minutes travel time
        }

        /// <summary>
        /// Get transport method speed modifier.
        /// </summary>
        private double GetTransportModifier(TravelMethods transportMethod)
        {
            return transportMethod switch
            {
                TravelMethods.Walking => 1.0,      // Base speed
                TravelMethods.Horseback => 0.5,    // Twice as fast
                TravelMethods.Carriage => 0.7,     // Moderate speed boost
                TravelMethods.Cart => 1.3,          // Slower due to cargo
                TravelMethods.Boat => 0.8,          // Good for water routes
                _ => 1.0
            };
        }

        /// <summary>
        /// Apply weather effects to travel time.
        /// </summary>
        private int ApplyWeatherEffects(int baseTime)
        {
            WeatherCondition weather = _gameWorld.WorldState.CurrentWeather;

            return weather switch
            {
                WeatherCondition.Rain => (int)(baseTime * 1.2),      // 20% slower in rain
                WeatherCondition.Snow => (int)(baseTime * 1.5),      // 50% slower in snow
                WeatherCondition.Storm => (int)(baseTime * 2.0),     // Double time in storm
                _ => baseTime                                         // No effect
            };
        }

        /// <summary>
        /// Calculate coin cost for travel.
        /// </summary>
        public int CalculateTravelCost(RouteOption route, TravelMethods transportMethod)
        {
            int baseCost = route.BaseCoinCost;

            // Apply transport method cost modifier
            baseCost = transportMethod switch
            {
                TravelMethods.Walking => 0,                  // Free
                TravelMethods.Horseback => baseCost * 2,     // More expensive
                TravelMethods.Carriage => baseCost * 3,      // Most expensive
                TravelMethods.Cart => baseCost,              // Standard cost
                TravelMethods.Boat => baseCost * 2,          // Moderate cost
                _ => baseCost
            };

            return baseCost;
        }

        /// <summary>
        /// Get all travel times from a specific location.
        /// </summary>
        public Dictionary<string, int> GetTravelTimesFrom(string locationId)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach (KeyValuePair<(string, string), int> kvp in TravelTimes)
            {
                if (kvp.Key.Item1 == locationId)
                {
                    result[kvp.Key.Item2] = kvp.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// Check if travel is possible given current time constraints.
        /// </summary>
        public bool CanTravelInTime(int travelTimeMinutes, int availableMinutes)
        {
            return travelTimeMinutes <= availableMinutes;
        }
    }
}