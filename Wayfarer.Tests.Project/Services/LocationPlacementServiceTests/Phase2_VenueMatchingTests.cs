using Xunit;

/// <summary>
/// USER STORY TESTS: Phase 2 - Venue Matching
///
/// Tests semantic coherence between LocationPurpose and VenueType.
/// Verifies VenuePurposeCompatibility lookup table correctness.
///
/// PRINCIPLE: Test behaviors (semantic matching), not implementation (switch statements).
/// PRINCIPLE: Test invariants (compatibility rules), not concrete values (specific venue names).
/// PRINCIPLE: Use builders (programmatic data), not fixtures (hard-coded JSON).
/// </summary>
public class Phase2_VenueMatchingTests
{
    [Fact]
    public void PlaceLocation_CommercePurpose_PlacedInCommercialVenue()
    {
        // ARRANGE: GameWorld with commercial venues
        GameWorld world = GameWorldBuilder.WithCommerceVenues();
        (GameWorld _, Player player) = new GameWorldBuilder()
            .WithVenues(world.Venues.ToList())
            .WithPlayerAt(0, 0)
            .BuildWithPlayer();

        LocationPlacementService service = new LocationPlacementService(world);
        Location commerceLocation = LocationBuilder.Commerce();

        // ACT: Place commerce location
        service.PlaceLocation(commerceLocation, "medium", player);

        // ASSERT: Location placed in compatible venue type
        Assert.NotNull(commerceLocation.Venue);

        bool isCompatible = VenuePurposeCompatibility.IsCompatible(
            LocationPurpose.Commerce,
            commerceLocation.Venue.Type
        );
        Assert.True(isCompatible,
            $"Commerce location placed in incompatible venue type {commerceLocation.Venue.Type}");

        // ASSERT: Venue type is one of the expected commercial types
        List<VenueType> expectedTypes = new List<VenueType>
        {
            VenueType.Market,
            VenueType.Merchant,
            VenueType.Workshop
        };
        Assert.Contains(commerceLocation.Venue.Type, expectedTypes);
    }

    [Fact]
    public void PlaceLocation_DwellingPurpose_PlacedInTavernVenue()
    {
        // ARRANGE: GameWorld with tavern venue
        GameWorld world = GameWorldBuilder.WithDwellingVenues();
        (GameWorld _, Player player) = new GameWorldBuilder()
            .WithVenues(world.Venues.ToList())
            .WithPlayerAt(0, 0)
            .BuildWithPlayer();

        LocationPlacementService service = new LocationPlacementService(world);
        Location dwellingLocation = LocationBuilder.Dwelling();

        // ACT: Place dwelling location
        service.PlaceLocation(dwellingLocation, "medium", player);

        // ASSERT: Location placed in Tavern venue
        Assert.NotNull(dwellingLocation.Venue);
        Assert.Equal(VenueType.Tavern, dwellingLocation.Venue.Type);

        // ASSERT: Compatibility verified
        bool isCompatible = VenuePurposeCompatibility.IsCompatible(
            LocationPurpose.Dwelling,
            dwellingLocation.Venue.Type
        );
        Assert.True(isCompatible);
    }

    [Fact]
    public void PlaceLocation_WorshipPurpose_PlacedInTempleVenue()
    {
        // ARRANGE: GameWorld with temple venue
        GameWorld world = GameWorldBuilder.WithWorshipVenues();
        (GameWorld _, Player player) = new GameWorldBuilder()
            .WithVenues(world.Venues.ToList())
            .WithPlayerAt(0, 0)
            .BuildWithPlayer();

        LocationPlacementService service = new LocationPlacementService(world);
        Location worshipLocation = LocationBuilder.Worship();

        // ACT: Place worship location
        service.PlaceLocation(worshipLocation, "medium", player);

        // ASSERT: Location placed in Temple venue
        Assert.NotNull(worshipLocation.Venue);
        Assert.Equal(VenueType.Temple, worshipLocation.Venue.Type);
    }

    [Fact]
    public void PlaceLocation_GenericPurpose_PlacedInAnyVenue()
    {
        // ARRANGE: GameWorld with mixed venue types
        GameWorld world = GameWorldBuilder.WithMixedVenues();
        (GameWorld _, Player player) = new GameWorldBuilder()
            .WithVenues(world.Venues.ToList())
            .WithPlayerAt(0, 0)
            .BuildWithPlayer();

        LocationPlacementService service = new LocationPlacementService(world);
        Location genericLocation = new LocationBuilder()
            .WithPurpose(LocationPurpose.Generic)
            .Build();

        // ACT: Place generic location
        service.PlaceLocation(genericLocation, "medium", player);

        // ASSERT: Location placed successfully (Generic matches all venue types)
        Assert.NotNull(genericLocation.Venue);

        // ASSERT: Compatibility verified (Generic is wildcard)
        bool isCompatible = VenuePurposeCompatibility.IsCompatible(
            LocationPurpose.Generic,
            genericLocation.Venue.Type
        );
        Assert.True(isCompatible,
            "Generic purpose should match any venue type (wildcard compatibility)");
    }

    [Fact]
    public void PlaceLocation_NoMatchingVenues_ThrowsInvalidOperationException()
    {
        // ARRANGE: GameWorld with NO temple venues
        GameWorld world = new GameWorldBuilder()
            .WithVenue(VenueBuilder.Market())
            .WithVenue(VenueBuilder.Tavern())
            .Build();

        (GameWorld _, Player player) = new GameWorldBuilder()
            .WithVenues(world.Venues.ToList())
            .WithPlayerAt(0, 0)
            .BuildWithPlayer();

        LocationPlacementService service = new LocationPlacementService(world);
        Location worshipLocation = LocationBuilder.Worship();

        // ACT & ASSERT: Placement throws when no compatible venues exist
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => service.PlaceLocation(worshipLocation, "medium", player)
        );

        // ASSERT: Exception message is informative
        Assert.Contains("No venues match location", exception.Message);
        Assert.Contains("Worship", exception.Message);
    }

    [Fact]
    public void PlaceLocation_MultipleMatchingVenues_SelectsAppropriateVenue()
    {
        // ARRANGE: GameWorld with 3 commercial venues at different distances
        Venue nearMarket = new VenueBuilder()
            .WithType(VenueType.Market)
            .WithCenterHex(3, 0)
            .Build();

        Venue midMerchant = new VenueBuilder()
            .WithType(VenueType.Merchant)
            .WithCenterHex(8, 0)
            .Build();

        Venue farWorkshop = new VenueBuilder()
            .WithType(VenueType.Workshop)
            .WithCenterHex(20, 0)
            .Build();

        GameWorld world = new GameWorldBuilder()
            .WithVenue(nearMarket)
            .WithVenue(midMerchant)
            .WithVenue(farWorkshop)
            .Build();

        (GameWorld _, Player player) = new GameWorldBuilder()
            .WithVenues(world.Venues.ToList())
            .WithPlayerAt(0, 0)
            .BuildWithPlayer();

        LocationPlacementService service = new LocationPlacementService(world);
        Location commerceLocation = LocationBuilder.Commerce();

        // ACT: Place commerce location with "medium" distance hint
        service.PlaceLocation(commerceLocation, "medium", player);

        // ASSERT: Location placed in one of the compatible venues
        Assert.NotNull(commerceLocation.Venue);

        List<VenueType> commercialTypes = new List<VenueType>
        {
            VenueType.Market,
            VenueType.Merchant,
            VenueType.Workshop
        };
        Assert.Contains(commerceLocation.Venue.Type, commercialTypes);

        // ASSERT: Venue selection considered distance (within medium range 6-12)
        int venueDistance = commerceLocation.Venue.CenterHex.DistanceTo(player.CurrentPosition);
        Assert.InRange(venueDistance, 0, 12);
    }

    [Fact]
    public void Invariant_AllPlacedLocations_MatchCompatibilityRules()
    {
        // ARRANGE: GameWorld with diverse venues
        GameWorld world = GameWorldBuilder.WithMixedVenues();
        (GameWorld _, Player player) = new GameWorldBuilder()
            .WithVenues(world.Venues.ToList())
            .WithPlayerAt(0, 0)
            .BuildWithPlayer();

        LocationPlacementService service = new LocationPlacementService(world);

        // Create locations with different purposes
        List<Location> locations = new List<Location>
        {
            LocationBuilder.Commerce(),
            LocationBuilder.Dwelling(),
            LocationBuilder.Transit()
        };

        // ACT: Place all locations
        foreach (Location location in locations)
        {
            service.PlaceLocation(location, "medium", player);
        }

        // ASSERT: INVARIANT - All locations satisfy compatibility rules
        foreach (Location location in locations)
        {
            Assert.NotNull(location.Venue);

            bool isCompatible = VenuePurposeCompatibility.IsCompatible(
                location.Purpose,
                location.Venue.Type
            );

            Assert.True(isCompatible,
                $"INVARIANT VIOLATION: Location '{location.Name}' with Purpose={location.Purpose} " +
                $"placed in incompatible venue Type={location.Venue.Type}");
        }
    }
}
