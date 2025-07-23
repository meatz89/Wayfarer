using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Wayfarer.Core.Repositories.Implementation
{
    /// <summary>
    /// Concrete implementation of ILocationSpotRepository
    /// </summary>
    public class LocationSpotRepositoryImpl : BaseRepository<LocationSpot>, ILocationSpotRepository
    {
        private readonly INPCRepository _npcRepository;

        public LocationSpotRepositoryImpl(
            IWorldStateAccessor worldState, 
            INPCRepository npcRepository,
            ILogger<LocationSpotRepositoryImpl> logger) 
            : base(worldState, logger)
        {
            _npcRepository = npcRepository;
        }

        protected override List<LocationSpot> GetCollection()
        {
            return _worldState.LocationSpots;
        }

        protected override string GetEntityId(LocationSpot entity)
        {
            return entity?.SpotID;
        }

        protected override string EntityTypeName => "LocationSpot";

        public IEnumerable<LocationSpot> GetSpotsForLocation(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId))
            {
                return Enumerable.Empty<LocationSpot>();
            }

            return GetAll().Where(s => s.LocationId == locationId);
        }

        public LocationSpot GetBySpotId(string spotId)
        {
            return GetById(spotId);
        }

        public IEnumerable<LocationSpot> GetSpotsByType(LocationSpotType spotType)
        {
            return GetAll().Where(s => s.SpotType == spotType);
        }

        public IEnumerable<LocationSpot> GetSpotsWithNPCs()
        {
            var allNPCs = _npcRepository.GetAll();
            var spotsWithNPCs = allNPCs
                .Where(n => !string.IsNullOrWhiteSpace(n.SpotId))
                .Select(n => n.SpotId)
                .Distinct();

            return GetAll().Where(s => spotsWithNPCs.Contains(s.SpotID));
        }
    }
}