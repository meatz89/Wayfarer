/// <summary>
/// Loads spatial hierarchy: Region → District → Venue → Location.
/// COMPOSITION OVER INHERITANCE: Extracted from PackageLoader for single responsibility.
/// DOMAIN COLLECTION PRINCIPLE: Uses LINQ queries instead of Dictionary lookups.
/// </summary>
public class SpatialHierarchyLoader
{
    private readonly GameWorld _gameWorld;
    private readonly LocationPlayabilityValidator _locationValidator;
    private readonly LocationPlacementService _locationPlacementService;

    public SpatialHierarchyLoader(
        GameWorld gameWorld,
        LocationPlayabilityValidator locationValidator,
        LocationPlacementService locationPlacementService)
    {
        _gameWorld = gameWorld;
        _locationValidator = locationValidator;
        _locationPlacementService = locationPlacementService;
    }

    /// <summary>
    /// Load all spatial hierarchy content from a package.
    /// Called by PackageLoader during package loading.
    /// </summary>
    public void LoadSpatialContent(
        List<RegionDTO> regionDtos,
        List<DistrictDTO> districtDtos,
        List<VenueDTO> venueDtos,
        List<LocationDTO> locationDtos,
        PackageLoadResult result,
        bool allowSkeletons)
    {
        // Load in strict dependency order
        LoadRegions(regionDtos, allowSkeletons);
        LoadDistricts(districtDtos, allowSkeletons);
        LoadVenues(venueDtos, result, allowSkeletons);
        LoadLocations(locationDtos, result, allowSkeletons);
    }

    /// <summary>
    /// Load regions - NO Dictionary, parse directly to GameWorld.Regions
    /// DOMAIN COLLECTION PRINCIPLE: Regions go directly to GameWorld, no intermediate lookup
    /// </summary>
    private void LoadRegions(List<RegionDTO> regionDtos, bool allowSkeletons)
    {
        if (regionDtos == null) return;

        foreach (RegionDTO dto in regionDtos)
        {
            Region region = new Region
            {
                Name = dto.Name,
                Description = dto.Description,
                Government = dto.Government,
                Culture = dto.Culture,
                Population = dto.Population,
                MajorExports = dto.MajorExports,
                MajorImports = dto.MajorImports
            };
            _gameWorld.Regions.Add(region);
        }
    }

    /// <summary>
    /// Load districts with LINQ-based region resolution (NO Dictionary)
    /// DOMAIN COLLECTION PRINCIPLE: Replace Dictionary lookup with LINQ FirstOrDefault
    /// </summary>
    private void LoadDistricts(List<DistrictDTO> districtDtos, bool allowSkeletons)
    {
        if (districtDtos == null) return;

        foreach (DistrictDTO dto in districtDtos)
        {
            // DOMAIN COLLECTION PRINCIPLE: LINQ query instead of Dictionary lookup
            Region region = null;
            if (!string.IsNullOrEmpty(dto.RegionName))
            {
                region = _gameWorld.Regions.FirstOrDefault(r => r.Name == dto.RegionName);
            }

            District district = new District
            {
                Name = dto.Name,
                Description = dto.Description,
                Region = region,
                DistrictType = dto.DistrictType,
                DangerLevel = dto.DangerLevel,
                Characteristics = dto.Characteristics
            };
            _gameWorld.Districts.Add(district);

            // Bidirectional link
            if (region != null && !region.Districts.Contains(district))
            {
                region.Districts.Add(district);
            }
        }
    }

    /// <summary>
    /// Load venues with LINQ-based district resolution (NO Dictionary)
    /// DOMAIN COLLECTION PRINCIPLE: Replace Dictionary lookup with LINQ FirstOrDefault
    /// </summary>
    private void LoadVenues(List<VenueDTO> venueDtos, PackageLoadResult result, bool allowSkeletons)
    {
        if (venueDtos == null) return;

        foreach (VenueDTO dto in venueDtos)
        {
            // DOMAIN COLLECTION PRINCIPLE: LINQ query instead of Dictionary lookup
            District district = null;
            if (!string.IsNullOrEmpty(dto.DistrictName))
            {
                district = _gameWorld.Districts.FirstOrDefault(d => d.Name == dto.DistrictName);
            }

            Venue existing = _gameWorld.Venues.FirstOrDefault(v => v.Name == dto.Name);

            if (existing != null)
            {
                // UPDATE existing venue in-place
                existing.Name = dto.Name;
                existing.Description = dto.Description;
                existing.District = district;

                VenueType venueType = VenueType.Wilderness;
                if (!string.IsNullOrEmpty(dto.LocationType))
                {
                    if (!Enum.TryParse<VenueType>(dto.LocationType, true, out venueType))
                    {
                        throw new InvalidDataException($"Venue '{dto.Name}' has invalid LocationType '{dto.LocationType}'.");
                    }
                }
                existing.Type = venueType;
                existing.IsSkeleton = false;

                SkeletonRegistryEntry skeletonEntry = _gameWorld.SkeletonRegistry.FirstOrDefault(x => x.SkeletonKey == dto.Name);
                if (skeletonEntry != null)
                {
                    _gameWorld.SkeletonRegistry.Remove(skeletonEntry);
                }

                result.VenuesAdded.Add(existing);

                if (district != null && !district.Venues.Contains(existing))
                {
                    district.Venues.Add(existing);
                }
            }
            else
            {
                Venue venue = VenueParser.ConvertDTOToVenue(dto);
                venue.District = district;
                _gameWorld.Venues.Add(venue);
                result.VenuesAdded.Add(venue);

                if (district != null && !district.Venues.Contains(venue))
                {
                    district.Venues.Add(venue);
                }
            }
        }
    }

    /// <summary>
    /// Load locations (spots) within venues.
    /// </summary>
    private void LoadLocations(List<LocationDTO> spotDtos, PackageLoadResult result, bool allowSkeletons)
    {
        if (spotDtos == null) return;

        foreach (LocationDTO dto in spotDtos)
        {
            Location existingSkeleton = _gameWorld.Locations.FirstOrDefault(l => l.Name == dto.Name);
            if (existingSkeleton != null && existingSkeleton.IsSkeleton)
            {
                SkeletonRegistryEntry spotSkeletonEntry = _gameWorld.SkeletonRegistry.FirstOrDefault(x => x.SkeletonKey == dto.Name);
                if (spotSkeletonEntry != null)
                {
                    _gameWorld.SkeletonRegistry.Remove(spotSkeletonEntry);
                }
            }

            Location location = LocationParser.ConvertDTOToLocation(dto, _gameWorld);

            if (location.Venue != null)
            {
                if (!_gameWorld.CanVenueAddMoreLocations(location.Venue))
                {
                    int currentCount = _gameWorld.GetLocationCountInVenue(location.Venue);
                    throw new InvalidOperationException(
                        $"Venue '{location.Venue.Name}' has reached capacity " +
                        $"({currentCount}/{location.Venue.MaxLocations} locations). " +
                        $"Cannot add location '{location.Name}'.");
                }

                _gameWorld.AddOrUpdateLocation(location.Name, location);
                result.LocationsAdded.Add(location);
            }
            else
            {
                _gameWorld.AddOrUpdateLocation(location.Name, location);
                result.LocationsAdded.Add(location);
            }
        }
    }
}
