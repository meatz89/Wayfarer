using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Subsystems.LocationSubsystem
{
    /// <summary>
    /// Manages spot-specific operations within locations.
    /// Handles spot discovery, properties, and area navigation.
    /// </summary>
    public class LocationSpotManager
    {
        private readonly GameWorld _gameWorld;
        private readonly LocationManager _locationManager;

        public LocationSpotManager(GameWorld gameWorld, LocationManager locationManager)
        {
            _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
            _locationManager = locationManager ?? throw new ArgumentNullException(nameof(locationManager));
        }

        /// <summary>
        /// Find a spot within a location by name or ID.
        /// </summary>
        public LocationSpot FindSpotInLocation(Location location, string spotIdentifier)
        {
            if (location == null || string.IsNullOrEmpty(spotIdentifier)) return null;

            LocationSpot targetSpot = null;

            // First check AvailableSpots in the Location object
            if (location.AvailableSpots != null)
            {
                targetSpot = location.AvailableSpots.FirstOrDefault(s =>
                    s.Name.Equals(spotIdentifier, StringComparison.OrdinalIgnoreCase) ||
                    s.SpotID.Equals(spotIdentifier, StringComparison.OrdinalIgnoreCase));
            }

            // If not found, check the location spot repository for spots in this location
            if (targetSpot == null)
            {
                List<LocationSpot> spotsInLocation = GetSpotsForLocation(location.Id);
                targetSpot = spotsInLocation?.FirstOrDefault(s =>
                    s.Name.Equals(spotIdentifier, StringComparison.OrdinalIgnoreCase) ||
                    s.SpotID.Equals(spotIdentifier, StringComparison.OrdinalIgnoreCase));
            }

            return targetSpot;
        }

        /// <summary>
        /// Get all spots for a specific location.
        /// </summary>
        public List<LocationSpot> GetSpotsForLocation(string locationId)
        {
            if (string.IsNullOrEmpty(locationId)) return new List<LocationSpot>();

            return _gameWorld.WorldState.locationSpots
                .Where(s => s.LocationId.Equals(locationId, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Build the location path hierarchy for display.
        /// </summary>
        public List<string> BuildLocationPath(Location location, LocationSpot spot)
        {
            List<string> path = new List<string>();

            if (location != null)
            {
                // Build path from location hierarchy
                // For now, use location name as the primary path element
                path.Add(location.Name);
            }

            if (spot != null && spot.Name != location?.Name)
            {
                path.Add(spot.Name);
            }

            return path;
        }

        /// <summary>
        /// Get location traits for display based on current time.
        /// </summary>
        public List<string> GetLocationTraits(Location location, LocationSpot spot, TimeBlocks currentTime)
        {
            if (location == null) return new List<string>();

            // Use LocationTraitsParser for systematic trait generation from JSON data
            return LocationTraitsParser.ParseLocationTraits(location, currentTime);
        }

        /// <summary>
        /// Get all areas within a location for navigation.
        /// </summary>
        public List<AreaWithinLocationViewModel> GetAreasWithinLocation(
            Location location,
            LocationSpot currentSpot,
            TimeBlocks currentTime,
            NPCRepository npcRepository)
        {
            List<AreaWithinLocationViewModel> areas = new List<AreaWithinLocationViewModel>();

            if (location == null) return areas;

            // Get all spots in the same location
            List<LocationSpot> spots = GetSpotsForLocation(location.Id);

            foreach (LocationSpot spot in spots)
            {
                // Skip the current spot - don't show it in the list
                if (spot.SpotID == currentSpot?.SpotID)
                    continue;

                // Get NPCs at this spot
                List<NPC> npcsAtSpot = npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTime);
                List<string> npcNames = npcsAtSpot.Select(n => n.Name).ToList();

                // Build detail string with NPCs if present
                string detail = GetSpotDetail(spot);
                if (npcNames.Any())
                {
                    detail = $"{detail} • {string.Join(", ", npcNames)}";
                }

                areas.Add(new AreaWithinLocationViewModel
                {
                    Name = spot.Name,
                    Detail = detail,
                    SpotId = spot.SpotID,
                    IsCurrent = false, // Never current since we skip the current spot
                    IsTravelHub = IsSpotTravelHub(spot, location)
                });
            }

            return areas;
        }

        /// <summary>
        /// Get a brief detail description for a spot.
        /// </summary>
        public string GetSpotDetail(LocationSpot spot)
        {
            if (spot == null) return "";

            // Generate detail based on spot ID (using categorical approach)
            return spot.SpotID switch
            {
                "marcus_stall" => "Cloth merchant's stall",
                "central_fountain" => "Gathering place",
                "north_entrance" => "To Noble District",
                "main_hall" => "Common room",
                "bar_counter" => "Bertram's domain",
                "corner_table" => "Private conversations",
                _ => GenerateSpotDetail(spot)
            };
        }

        /// <summary>
        /// Generate a detail description for a spot based on its properties.
        /// </summary>
        private string GenerateSpotDetail(LocationSpot spot)
        {
            if (spot == null) return "";

            SpotDescriptionGenerator descGenerator = new SpotDescriptionGenerator();
            TimeBlocks currentTime = TimeBlocks.Morning; // Default for brief descriptions
            List<SpotPropertyType> activeProperties = spot.GetActiveProperties(currentTime);
            return descGenerator.GenerateBriefDescription(activeProperties);
        }

        /// <summary>
        /// Check if a spot is a travel hub.
        /// </summary>
        public bool IsSpotTravelHub(LocationSpot spot, Location location)
        {
            if (spot == null || location == null) return false;

            return spot.SpotID == location.TravelHubSpotId ||
                   spot.DomainTags?.Contains("Crossroads") == true;
        }

        /// <summary>
        /// Get active properties for a spot at a specific time.
        /// </summary>
        public List<SpotPropertyType> GetActiveSpotProperties(LocationSpot spot, TimeBlocks timeBlock)
        {
            if (spot == null) return new List<SpotPropertyType>();
            return spot.GetActiveProperties(timeBlock);
        }

        /// <summary>
        /// Check if a spot has a specific property at a given time.
        /// </summary>
        public bool SpotHasProperty(LocationSpot spot, SpotPropertyType property, TimeBlocks timeBlock)
        {
            List<SpotPropertyType> activeProperties = GetActiveSpotProperties(spot, timeBlock);
            return activeProperties.Contains(property);
        }

        /// <summary>
        /// Get all spots with a specific property in a location.
        /// </summary>
        public List<LocationSpot> GetSpotsWithProperty(string locationId, SpotPropertyType property, TimeBlocks timeBlock)
        {
            List<LocationSpot> spots = GetSpotsForLocation(locationId);
            return spots.Where(s => SpotHasProperty(s, property, timeBlock)).ToList();
        }

        /// <summary>
        /// Check if player has discovered a spot.
        /// </summary>
        public bool IsSpotDiscovered(string spotId)
        {
            if (string.IsNullOrEmpty(spotId)) return false;

            Player player = _gameWorld.GetPlayer();
            return player.LocationActionAvailability?.Contains(spotId) == true;
        }

        /// <summary>
        /// Mark a spot as discovered by the player.
        /// </summary>
        public void MarkSpotDiscovered(string spotId)
        {
            if (string.IsNullOrEmpty(spotId)) return;

            Player player = _gameWorld.GetPlayer();
            if (!player.LocationActionAvailability.Contains(spotId))
            {
                player.LocationActionAvailability.Add(spotId);
            }
        }

        /// <summary>
        /// Get the default entrance spot for a location.
        /// </summary>
        public LocationSpot GetDefaultEntranceSpot(string locationId)
        {
            Location location = _locationManager.GetLocation(locationId);
            if (location == null) return null;

            // First try to get the designated travel hub
            if (!string.IsNullOrEmpty(location.TravelHubSpotId))
            {
                LocationSpot travelHub = _locationManager.GetSpot(locationId, location.TravelHubSpotId);
                if (travelHub != null) return travelHub;
            }

            // Then look for spots tagged as crossroads
            List<LocationSpot> spots = GetSpotsForLocation(locationId);
            LocationSpot? crossroads = spots.FirstOrDefault(s => s.DomainTags?.Contains("Crossroads") == true);
            if (crossroads != null) return crossroads;

            // Finally, just return the first available spot
            return spots.FirstOrDefault();
        }

        /// <summary>
        /// Get accessible spots from the current spot.
        /// </summary>
        public List<LocationSpot> GetAccessibleSpotsFromCurrent(LocationSpot currentSpot)
        {
            if (currentSpot == null) return new List<LocationSpot>();

            // For now, all spots in the same location are accessible
            // This could be enhanced with specific connectivity rules
            return GetSpotsForLocation(currentSpot.LocationId)
                .Where(s => s.SpotID != currentSpot.SpotID)
                .ToList();
        }

        /// <summary>
        /// Get spots that provide a specific service.
        /// </summary>
        public List<LocationSpot> GetServiceSpots(string locationId, ServiceTypes service, TimeBlocks timeBlock)
        {
            List<LocationSpot> spots = GetSpotsForLocation(locationId);
            List<LocationSpot> serviceSpots = new List<LocationSpot>();

            foreach (LocationSpot spot in spots)
            {
                // Check if spot has properties that indicate this service
                List<SpotPropertyType> properties = GetActiveSpotProperties(spot, timeBlock);

                bool hasService = service switch
                {
                    ServiceTypes.Market => properties.Contains(SpotPropertyType.Commercial),
                    _ => false
                };

                if (hasService)
                {
                    serviceSpots.Add(spot);
                }
            }

            return serviceSpots;
        }
    }
}