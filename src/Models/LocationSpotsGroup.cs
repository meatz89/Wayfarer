using System.Collections.Generic;
using Wayfarer.Content;

namespace Wayfarer.Models
{
    /// <summary>
    /// Strongly-typed grouping of location spots by location ID.
    /// Replaces tuple usage in PackageLoader location validation.
    /// </summary>
    public class LocationSpotsGroup
    {
        /// <summary>
        /// The location ID this group represents
        /// </summary>
        public string LocationId { get; init; }

        /// <summary>
        /// The spots belonging to this location
        /// </summary>
        public List<LocationSpot> Spots { get; init; }

        /// <summary>
        /// Create a location spots group
        /// </summary>
        public LocationSpotsGroup(string locationId, List<LocationSpot> spots)
        {
            LocationId = locationId;
            Spots = spots ?? new List<LocationSpot>();
        }

        /// <summary>
        /// Create a new group with a single spot
        /// </summary>
        public static LocationSpotsGroup CreateWithSpot(string locationId, LocationSpot spot)
        {
            return new LocationSpotsGroup(locationId, new List<LocationSpot> { spot });
        }
    }
}