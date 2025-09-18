using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Subsystems.LocationSubsystem
{
    /// <summary>
    /// Manages core location operations and state.
    /// Incorporates all functionality from LocationRepository.
    /// </summary>
    public class LocationManager
    {
        private readonly GameWorld _gameWorld;

        public LocationManager(GameWorld gameWorld)
        {
            _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        }

        /// <summary>
        /// Get the player's current location based on their current spot.
        /// </summary>
        public Location GetCurrentLocation()
        {
            Player player = _gameWorld.GetPlayer();
            if (player.CurrentLocationSpot == null) return null;
            return GetLocation(player.CurrentLocationSpot.LocationId);
        }

        /// <summary>
        /// Get the player's current location spot.
        /// </summary>
        public LocationSpot GetCurrentLocationSpot()
        {
            return _gameWorld.GetPlayer().CurrentLocationSpot;
        }

        /// <summary>
        /// Get a location by its ID.
        /// </summary>
        public Location GetLocation(string locationId)
        {
            if (string.IsNullOrEmpty(locationId)) return null;

            Location location = _gameWorld.WorldState.locations.FirstOrDefault(l =>
                l.Id.Equals(locationId, StringComparison.OrdinalIgnoreCase));

            return location;
        }

        /// <summary>
        /// Get a location by its name.
        /// </summary>
        public Location GetLocationByName(string locationName)
        {
            if (string.IsNullOrEmpty(locationName)) return null;

            Location location = _gameWorld.WorldState.locations.FirstOrDefault(l =>
                l.Name.Equals(locationName, StringComparison.OrdinalIgnoreCase));

            return location;
        }

        /// <summary>
        /// Get all locations in the world.
        /// </summary>
        public List<Location> GetAllLocations()
        {
            return _gameWorld.WorldState.locations ?? new List<Location>();
        }

        /// <summary>
        /// Get all spots for a specific location.
        /// </summary>
        public List<LocationSpot> GetSpotsForLocation(string locationId)
        {
            if (string.IsNullOrEmpty(locationId)) return new List<LocationSpot>();
            // Get from GameWorld's primary Spots dictionary
            return _gameWorld.Spots.Values()
                .Where(s => s.LocationId.Equals(locationId, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Get a specific spot within a location.
        /// </summary>
        public LocationSpot GetSpot(string locationId, string spotId)
        {
            Location location = GetLocation(locationId);
            // Get spot directly from GameWorld's primary storage
            return _gameWorld.GetSpot(spotId);
        }

        /// <summary>
        /// Get locations connected to the specified location.
        /// </summary>
        public List<Location> GetConnectedLocations(string currentLocationId)
        {
            if (string.IsNullOrEmpty(currentLocationId)) return new List<Location>();

            return _gameWorld.WorldState.locations
                .Where(l => l.ConnectedLocationIds?.Contains(currentLocationId) == true)
                .ToList();
        }

        /// <summary>
        /// Add a new location to the world.
        /// </summary>
        public void AddLocation(Location location)
        {
            if (location == null) throw new ArgumentNullException(nameof(location));

            if (_gameWorld.WorldState.locations.Any(l =>
                l.Id.Equals(location.Id, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Location '{location.Id}' already exists.");
            }

            _gameWorld.WorldState.locations.Add(location);
        }

        /// <summary>
        /// Add a new location spot to the world.
        /// </summary>
        public void AddLocationSpot(LocationSpot spot)
        {
            if (spot == null) throw new ArgumentNullException(nameof(spot));

            if (_gameWorld.WorldState.locationSpots.Any(s =>
                s.LocationId.Equals(spot.LocationId, StringComparison.OrdinalIgnoreCase) &&
                s.SpotID.Equals(spot.SpotID, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Spot '{spot.SpotID}' already exists in '{spot.LocationId}'.");
            }

            _gameWorld.WorldState.locationSpots.Add(spot);
        }

        /// <summary>
        /// Set the player's current location and spot.
        /// </summary>
        public void SetCurrentLocation(Location location, LocationSpot spot)
        {
            if (location == null) throw new ArgumentNullException(nameof(location));

            // Validate that the spot belongs to the location
            if (spot != null && !spot.LocationId.Equals(location.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Spot {spot.SpotID} does not belong to location {location.Id}");
            }

            _gameWorld.GetPlayer().CurrentLocationSpot = spot;
        }

        /// <summary>
        /// Set the player's current spot directly.
        /// </summary>
        public void SetCurrentSpot(LocationSpot spot)
        {
            _gameWorld.GetPlayer().CurrentLocationSpot = spot;
        }

        /// <summary>
        /// Record that the player has visited a location.
        /// </summary>
        public void RecordLocationVisit(string locationId)
        {
            if (string.IsNullOrEmpty(locationId)) return;
            _gameWorld.WorldState.RecordLocationVisit(locationId);
        }

        /// <summary>
        /// Check if this is the player's first visit to a location.
        /// </summary>
        public bool IsFirstVisit(string locationId)
        {
            if (string.IsNullOrEmpty(locationId)) return false;
            return _gameWorld.WorldState.IsFirstVisit(locationId);
        }

        /// <summary>
        /// Check if a location exists.
        /// </summary>
        public bool LocationExists(string locationId)
        {
            return GetLocation(locationId) != null;
        }

        /// <summary>
        /// Check if a spot exists within a location.
        /// </summary>
        public bool SpotExists(string locationId, string spotId)
        {
            return GetSpot(locationId, spotId) != null;
        }

        /// <summary>
        /// Get all location spots in the world (from WorldState).
        /// </summary>
        public List<LocationSpot> GetAllLocationSpots()
        {
            return _gameWorld.WorldState.locationSpots ?? new List<LocationSpot>();
        }

        /// <summary>
        /// Get location spots filtered by location ID.
        /// </summary>
        public List<LocationSpot> GetLocationSpotsForLocation(string locationId)
        {
            if (string.IsNullOrEmpty(locationId)) return new List<LocationSpot>();

            return _gameWorld.WorldState.locationSpots
                .Where(s => s.LocationId.Equals(locationId, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Check if player knows a specific location spot.
        /// </summary>
        public bool IsSpotKnown(string spotId)
        {
            if (string.IsNullOrEmpty(spotId)) return false;

            Player player = _gameWorld.GetPlayer();
            return player.LocationActionAvailability?.Contains(spotId) == true;
        }

        /// <summary>
        /// Get the travel hub spot for a location.
        /// </summary>
        public LocationSpot GetTravelHubSpot(string locationId)
        {
            Location location = GetLocation(locationId);
            if (location == null) return null;

            // Look for spots with Crossroads property
            List<LocationSpot> spots = GetSpotsForLocation(locationId);
            return spots.FirstOrDefault(s => s.SpotProperties?.Contains(SpotPropertyType.Crossroads) == true);
        }

        /// <summary>
        /// Check if a spot is the travel hub for its location.
        /// </summary>
        public bool IsTravelHub(LocationSpot spot)
        {
            if (spot == null) return false;

            Location location = GetLocation(spot.LocationId);
            if (location == null) return false;

            // Travel happens at any spot with Crossroads property
            return spot.SpotProperties?.Contains(SpotPropertyType.Crossroads) == true;
        }
    }
}