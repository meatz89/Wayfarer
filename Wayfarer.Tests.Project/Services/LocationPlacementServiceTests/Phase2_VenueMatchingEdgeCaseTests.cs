using Xunit;

/// <summary>
/// EDGE CASE TESTS: Phase 2 - Venue Matching (Boundary Conditions)
///
/// Tests edge cases and boundary conditions for venue-location placement:
/// - Capacity enforcement (full venues → uses next available)
/// - Distance fallback logic (no venues in range → ignores constraint)
/// - Additional purpose coverage (Transit, Defense, Worship, Learning, Entertainment)
/// - Exhaustive compatibility validation (theory-based enum coverage)
///
/// PRINCIPLE: Test edge cases that could break in production.
/// PRINCIPLE: Use theory tests for exhaustive enum validation.
/// PRINCIPLE: Test behaviors (capacity enforcement), not implementation (venue.LocationIds.Count).
/// </summary>
public class Phase2_VenueMatchingEdgeCaseTests
{
    [Fact]
    public void PlaceLocation_VenueAtCapacity_UsesNextAvailableVenue()
    {
        // ARRANGE: Two commercial venues - first has small capacity, second has large capacity
        // Both within "near" distance range (2-5 hexes from player at origin)
        Venue nearMarket = new VenueBuilder()
            .WithType(VenueType.Market)
            .WithMaxLocations(2)  // Small capacity (will fill quickly)
            .WithCenterHex(3, 0)  // Distance 3 from player
            .Build();

        Venue farMerchant = new VenueBuilder()
            .WithType(VenueType.Merchant)
            .WithMaxLocations(10)  // Large capacity
            .WithCenterHex(5, 0)  // Distance 5 from player
            .Build();

        (GameWorld world, Player player) = new GameWorldBuilder()
            .WithVenue(nearMarket)
            .WithVenue(farMerchant)
            .WithPlayerAt(0, 0)
            .BuildWithPlayer();

        LocationPlacementService service = new LocationPlacementService(world);

        // Fill nearMarket to capacity (2 locations)
        // Use "near" distance hint (2-5 hexes) to include nearMarket at distance 5
        Location shop1 = LocationBuilder.Commerce();
        Location shop2 = LocationBuilder.Commerce();

        service.PlaceLocation(shop1, "near");
        world.AddOrUpdateLocation(shop1.Name, shop1);  // Add to GameWorld for capacity tracking

        service.PlaceLocation(shop2, "near");
        world.AddOrUpdateLocation(shop2.Name, shop2);  // Add to GameWorld for capacity tracking

        // Verify nearMarket is at capacity via query (AFTER adding to GameWorld)
        int nearMarketCount = world.Locations.Count(loc => loc.Venue == nearMarket);
        int farMerchantCount = world.Locations.Count(loc => loc.Venue == farMerchant);
        Assert.Equal(2, nearMarketCount);
        Assert.Equal(0, farMerchantCount);

        // ACT: Place third commerce location (nearMarket is full)
        Location shop3 = LocationBuilder.Commerce();
        service.PlaceLocation(shop3, "near");  // Same distance hint - both venues in range

        // ASSERT: Third location placed in farMerchant (NOT nearMarket which is full)
        Assert.NotNull(shop3.Venue);
        Assert.Equal(VenueType.Merchant, shop3.Venue.Type);
        Assert.Equal(farMerchant.Name, shop3.Venue.Name);

        // ASSERT: Capacity enforcement worked - nearMarket still at 2, farMerchant has 1
        world.AddOrUpdateLocation(shop3.Name, shop3);  // Add to GameWorld for final verification
        int finalNearMarketCount = world.Locations.Count(loc => loc.Venue == nearMarket);
        int finalFarMerchantCount = world.Locations.Count(loc => loc.Venue == farMerchant);
        Assert.Equal(2, finalNearMarketCount);
        Assert.Equal(1, finalFarMerchantCount);
    }

    [Fact]
    public void PlaceLocation_AllMatchingVenuesAtCapacity_ThrowsInvalidOperationException()
    {
        // ARRANGE: Two commercial venues, BOTH at capacity
        Venue market = new VenueBuilder()
            .WithType(VenueType.Market)
            .WithMaxLocations(1)
            .Build();

        Venue merchant = new VenueBuilder()
            .WithType(VenueType.Merchant)
            .WithMaxLocations(1)
            .Build();

        GameWorld world = new GameWorldBuilder()
            .WithVenue(market)
            .WithVenue(merchant)
            .Build();

        (GameWorld _, Player player) = new GameWorldBuilder()
            .WithVenues(world.Venues.ToList())
            .WithPlayerAt(0, 0)
            .BuildWithPlayer();

        LocationPlacementService service = new LocationPlacementService(world);

        // Fill both venues to capacity
        Location commerce1 = LocationBuilder.Commerce();
        Location commerce2 = LocationBuilder.Commerce();
        service.PlaceLocation(commerce1, "medium");
        world.AddOrUpdateLocation(commerce1.Name, commerce1);
        service.PlaceLocation(commerce2, "medium");
        world.AddOrUpdateLocation(commerce2.Name, commerce2);

        // Verify both at capacity via query
        int marketCount = world.Locations.Count(loc => loc.Venue == market);
        int merchantCount = world.Locations.Count(loc => loc.Venue == merchant);
        Assert.Equal(1, marketCount);
        Assert.Equal(1, merchantCount);

        // ACT & ASSERT: Placement fails when all matching venues at capacity
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => service.PlaceLocation(LocationBuilder.Commerce(), "medium")
        );

        // ASSERT: Exception message is informative
        Assert.Contains("All matching venues at capacity", exception.Message);
        Assert.Contains("Commerce", exception.Message);
    }

    [Fact]
    public void PlaceLocation_NoVenuesInRange_IgnoresDistanceConstraint()
    {
        // ARRANGE: Commercial venue is FAR (beyond "near" range of 2-5)
        Venue farMarket = new VenueBuilder()
            .WithType(VenueType.Market)
            .WithCenterHex(20, 0)  // Distance 20 from origin
            .Build();

        GameWorld world = new GameWorldBuilder()
            .WithVenue(farMarket)
            .Build();

        (GameWorld _, Player player) = new GameWorldBuilder()
            .WithVenues(world.Venues.ToList())
            .WithPlayerAt(0, 0)
            .BuildWithPlayer();

        LocationPlacementService service = new LocationPlacementService(world);
        Location commerceLocation = LocationBuilder.Commerce();

        // ACT: Place with "near" hint (range 2-5), but only venue is at distance 20
        service.PlaceLocation(commerceLocation, "near");

        // ASSERT: Location placed anyway (fallback ignores distance constraint)
        Assert.NotNull(commerceLocation.Venue);
        Assert.Equal(farMarket.Name, commerceLocation.Venue.Name);

        // ASSERT: Venue is indeed beyond "near" range (demonstrates fallback behavior)
        int actualDistance = commerceLocation.Venue.CenterHex.DistanceTo(player.CurrentPosition);
        Assert.True(actualDistance > 5, $"Venue should be beyond 'near' range (2-5), actual: {actualDistance}");
    }

    [Fact]
    public void PlaceLocation_TransitPurpose_PlacedInWilderness()
    {
        // ARRANGE: GameWorld with wilderness venue (for roads/paths)
        GameWorld world = new GameWorldBuilder()
            .WithVenue(VenueBuilder.Wilderness())
            .Build();

        (GameWorld _, Player player) = new GameWorldBuilder()
            .WithVenues(world.Venues.ToList())
            .WithPlayerAt(0, 0)
            .BuildWithPlayer();

        LocationPlacementService service = new LocationPlacementService(world);
        Location transitLocation = LocationBuilder.Transit();

        // ACT: Place transit location (roads, paths, outdoor routes)
        service.PlaceLocation(transitLocation, "medium");

        // ASSERT: Location placed in a venue
        // NOTE: VenuePurposeCompatibility was REMOVED - any venue can host any purpose
        Assert.NotNull(transitLocation.Venue);
    }

    [Fact]
    public void PlaceLocation_DefensePurpose_PlacedInFortressOrGuard()
    {
        // ARRANGE: GameWorld with defense venues
        List<Venue> venues = new List<Venue>
        {
            new VenueBuilder().WithType(VenueType.Fortress).Build(),
            new VenueBuilder().WithType(VenueType.Guard).Build()
        };

        GameWorld world = new GameWorldBuilder()
            .WithVenues(venues)
            .Build();

        (GameWorld _, Player player) = new GameWorldBuilder()
            .WithVenues(world.Venues.ToList())
            .WithPlayerAt(0, 0)
            .BuildWithPlayer();

        LocationPlacementService service = new LocationPlacementService(world);
        Location defenseLocation = new LocationBuilder()
            .WithPurpose(LocationPurpose.Defense)
            .Build();

        // ACT: Place defense location
        service.PlaceLocation(defenseLocation, "medium");

        // ASSERT: Location placed in defense venue (Fortress or Guard)
        Assert.NotNull(defenseLocation.Venue);
        List<VenueType> defenseTypes = new List<VenueType> { VenueType.Fortress, VenueType.Guard };
        Assert.Contains(defenseLocation.Venue.Type, defenseTypes);
    }

    [Fact]
    public void PlaceLocation_LearningPurpose_PlacedInAcademy()
    {
        // ARRANGE: GameWorld with academy venue
        GameWorld world = new GameWorldBuilder()
            .WithVenue(new VenueBuilder().WithType(VenueType.Academy).Build())
            .Build();

        (GameWorld _, Player player) = new GameWorldBuilder()
            .WithVenues(world.Venues.ToList())
            .WithPlayerAt(0, 0)
            .BuildWithPlayer();

        LocationPlacementService service = new LocationPlacementService(world);
        Location learningLocation = new LocationBuilder()
            .WithPurpose(LocationPurpose.Learning)
            .Build();

        // ACT: Place learning location
        service.PlaceLocation(learningLocation, "medium");

        // ASSERT: Location placed in Academy venue
        Assert.NotNull(learningLocation.Venue);
        Assert.Equal(VenueType.Academy, learningLocation.Venue.Type);
    }

    [Fact]
    public void PlaceLocation_EntertainmentPurpose_PlacedInTheaterOrArena()
    {
        // ARRANGE: GameWorld with entertainment venues
        List<Venue> venues = new List<Venue>
        {
            new VenueBuilder().WithType(VenueType.Theater).Build(),
            new VenueBuilder().WithType(VenueType.Arena).Build()
        };

        GameWorld world = new GameWorldBuilder()
            .WithVenues(venues)
            .Build();

        (GameWorld _, Player player) = new GameWorldBuilder()
            .WithVenues(world.Venues.ToList())
            .WithPlayerAt(0, 0)
            .BuildWithPlayer();

        LocationPlacementService service = new LocationPlacementService(world);
        Location entertainmentLocation = new LocationBuilder()
            .WithPurpose(LocationPurpose.Entertainment)
            .Build();

        // ACT: Place entertainment location
        service.PlaceLocation(entertainmentLocation, "medium");

        // ASSERT: Location placed in entertainment venue (Theater or Arena)
        Assert.NotNull(entertainmentLocation.Venue);
        List<VenueType> entertainmentTypes = new List<VenueType> { VenueType.Theater, VenueType.Arena };
        Assert.Contains(entertainmentLocation.Venue.Type, entertainmentTypes);
    }

    // NOTE: VenuePurposeCompatibility_ValidatesCorrectly test REMOVED
    // VenuePurposeCompatibility was DELETED - any venue type can host any location purpose

    [Fact]
    public void Invariant_VenueCapacity_NeverExceeded()
    {
        // INVARIANT: venue.LocationIds.Count MUST NEVER exceed venue.MaxLocations

        // ARRANGE: Venue with small capacity
        Venue market = new VenueBuilder()
            .WithType(VenueType.Market)
            .WithMaxLocations(3)
            .Build();

        GameWorld world = new GameWorldBuilder()
            .WithVenue(market)
            .Build();

        (GameWorld _, Player player) = new GameWorldBuilder()
            .WithVenues(world.Venues.ToList())
            .WithPlayerAt(0, 0)
            .BuildWithPlayer();

        LocationPlacementService service = new LocationPlacementService(world);

        // ACT: Place 3 locations (fill to capacity)
        Location commerce1 = LocationBuilder.Commerce();
        Location commerce2 = LocationBuilder.Commerce();
        Location commerce3 = LocationBuilder.Commerce();
        service.PlaceLocation(commerce1, "medium");
        world.AddOrUpdateLocation(commerce1.Name, commerce1);
        service.PlaceLocation(commerce2, "medium");
        world.AddOrUpdateLocation(commerce2.Name, commerce2);
        service.PlaceLocation(commerce3, "medium");
        world.AddOrUpdateLocation(commerce3.Name, commerce3);

        // ASSERT: INVARIANT - Capacity equals max (not exceeded)
        int marketCount = world.Locations.Count(loc => loc.Venue == market);
        Assert.Equal(3, marketCount);
        Assert.Equal(market.MaxLocations, marketCount);

        // ACT: Attempt to place 4th location (should fail, not exceed capacity)
        Assert.Throws<InvalidOperationException>(
            () => service.PlaceLocation(LocationBuilder.Commerce(), "medium")
        );

        // ASSERT: INVARIANT - Capacity STILL equals max (never exceeded)
        int finalMarketCount = world.Locations.Count(loc => loc.Venue == market);
        Assert.Equal(3, finalMarketCount);
        Assert.True(finalMarketCount <= market.MaxLocations,
            "INVARIANT VIOLATION: Venue capacity exceeded MaxLocations");
    }
}
