using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Wayfarer.Core.Repositories.Implementation
{
    /// <summary>
    /// Concrete implementation of ILocationRepository
    /// </summary>
    public class LocationRepositoryImpl : BaseRepository<Location>, ILocationRepository
    {
        public LocationRepositoryImpl(IWorldStateAccessor worldState, ILogger<LocationRepositoryImpl> logger) 
            : base(worldState, logger)
        {
        }

        protected override List<Location> GetCollection()
        {
            return _worldState.Locations;
        }

        protected override string GetEntityId(Location entity)
        {
            return entity?.LocationID;
        }

        protected override string EntityTypeName => "Location";

        public IEnumerable<Location> GetLocationsByRegion(string region)
        {
            if (string.IsNullOrWhiteSpace(region))
            {
                return Enumerable.Empty<Location>();
            }

            return GetAll().Where(l => l.Region == region);
        }

        public IEnumerable<Location> GetLocationsWithRouteType(RouteType routeType)
        {
            return GetAll().Where(l => l.Connections != null && 
                l.Connections.Any(c => c.RouteTypes.Contains(routeType)));
        }

        public bool LocationExists(string locationId)
        {
            return Exists(locationId);
        }
    }
}