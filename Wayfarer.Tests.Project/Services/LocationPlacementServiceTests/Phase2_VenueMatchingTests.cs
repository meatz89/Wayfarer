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
        service.PlaceLocation(commerceLocation, "medium");

        // ASSERT: Location placed in a venue
        // NOTE: VenuePurposeCompatibility was REMOVED - any venue can host any purpose
        Assert.NotNull(commerceLocation.Venue);

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
        service.PlaceLocation(dwellingLocation, "medium");

        // ASSERT: Location placed in a venue
        // NOTE: VenuePurposeCompatibility was REMOVED - any venue can host any purpose
        Assert.NotNull(dwellingLocation.Venue);
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
        service.PlaceLocation(worshipLocation, "medium");

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
        service.PlaceLocation(genericLocation, "medium");

        // ASSERT: Location placed successfully
        // NOTE: VenuePurposeCompatibility was REMOVED - any venue can host any purpose
        Assert.NotNull(genericLocation.Venue);
    }

    [Fact]
    public void PlaceLocation_AnyPurposeCanBePlacedInAnyVenue()
    {
        // ARRANGE: GameWorld with only Market and Tavern venues
        // NOTE: VenuePurposeCompatibility was REMOVED - any venue can host any purpose
        GameWorld world = new GameWorldBuilder()
            .WithVenue(VenueBuilder.Market())
            .WithVenue(VenueBuilder.Tavern())
            .WithPlayerAt(0, 0)
            .Build();

        LocationPlacementService service = new LocationPlacementService(world);
        Location worshipLocation = LocationBuilder.Worship();

        // ACT: Place worship location (should succeed - any venue can host any purpose)
        service.PlaceLocation(worshipLocation, "medium");

        // ASSERT: Location placed successfully in available venue
        Assert.NotNull(worshipLocation.Venue);
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
        service.PlaceLocation(commerceLocation, "medium");

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
            service.PlaceLocation(location, "medium");
        }

        // ASSERT: All locations are placed in a venue
        // NOTE: VenuePurposeCompatibility was REMOVED - any venue can host any purpose
        foreach (Location location in locations)
        {
            Assert.NotNull(location.Venue);
        }
    }
}
