using Xunit;

namespace Wayfarer.Tests.GameState;

public class CapacityBudgetTests
{
    [Fact]
    public void CanAddMoreLocations_EmptyVenue_ReturnsTrue()
    {
        Venue venue = new Venue("venue1", "Test Venue")
        {
            MaxLocations = 10
        };

        bool result = venue.CanAddMoreLocations();

        Assert.True(result);
    }

    [Fact]
    public void CanAddMoreLocations_BelowCapacity_ReturnsTrue()
    {
        Venue venue = new Venue("venue1", "Test Venue")
        {
            MaxLocations = 10,
            LocationIds = new List<string> { "loc1", "loc2", "loc3" }
        };

        bool result = venue.CanAddMoreLocations();

        Assert.True(result);
        Assert.Equal(3, venue.LocationIds.Count);
    }

    [Fact]
    public void CanAddMoreLocations_AtCapacity_ReturnsFalse()
    {
        Venue venue = new Venue("venue1", "Test Venue")
        {
            MaxLocations = 3,
            LocationIds = new List<string> { "loc1", "loc2", "loc3" }
        };

        bool result = venue.CanAddMoreLocations();

        Assert.False(result);
    }

    [Fact]
    public void CanAddMoreLocations_OverCapacity_ReturnsFalse()
    {
        Venue venue = new Venue("venue1", "Test Venue")
        {
            MaxLocations = 2,
            LocationIds = new List<string> { "loc1", "loc2", "loc3" }
        };

        bool result = venue.CanAddMoreLocations();

        Assert.False(result);
    }

    [Fact]
    public void AddOrUpdateLocation_NewLocation_AddsToVenueLocationIds()
    {
        GameWorld gameWorld = new GameWorld();
        Venue venue = new Venue("venue1", "Test Venue")
        {
            MaxLocations = 10
        };
        gameWorld.Venues.Add(venue);

        Location location = new Location("loc1", "Test Location")
        {
            VenueId = venue.Id,
            Venue = venue
        };

        gameWorld.AddOrUpdateLocation(location.Id, location);

        Assert.Contains(location, gameWorld.Locations);
        Assert.Contains(location.Id, venue.LocationIds);
    }

    [Fact]
    public void AddOrUpdateLocation_UpdateExisting_MaintainsVenueLocationIds()
    {
        GameWorld gameWorld = new GameWorld();
        Venue venue = new Venue("venue1", "Test Venue")
        {
            MaxLocations = 10
        };
        gameWorld.Venues.Add(venue);

        Location location = new Location("loc1", "Test Location")
        {
            VenueId = venue.Id,
            Venue = venue
        };
        gameWorld.Locations.Add(location);
        venue.LocationIds.Add(location.Id);

        Location updatedLocation = new Location("loc1", "Updated Location")
        {
            VenueId = venue.Id,
            Venue = venue
        };

        gameWorld.AddOrUpdateLocation(updatedLocation.Id, updatedLocation);

        Assert.Single(venue.LocationIds);
        Assert.Contains(location.Id, venue.LocationIds);
        Assert.Equal("Updated Location", gameWorld.Locations[0].Name);
    }

    [Fact]
    public void AddOrUpdateLocation_ChangeVenue_UpdatesBothVenues()
    {
        GameWorld gameWorld = new GameWorld();

        Venue oldVenue = new Venue("venue1", "Old Venue") { MaxLocations = 10 };
        Venue newVenue = new Venue("venue2", "New Venue") { MaxLocations = 10 };
        gameWorld.Venues.Add(oldVenue);
        gameWorld.Venues.Add(newVenue);

        Location location = new Location("loc1", "Test Location")
        {
            VenueId = oldVenue.Id,
            Venue = oldVenue
        };
        gameWorld.Locations.Add(location);
        oldVenue.LocationIds.Add(location.Id);

        Location movedLocation = new Location("loc1", "Test Location")
        {
            VenueId = newVenue.Id,
            Venue = newVenue
        };

        gameWorld.AddOrUpdateLocation(movedLocation.Id, movedLocation);

        Assert.DoesNotContain(location.Id, oldVenue.LocationIds);
        Assert.Contains(location.Id, newVenue.LocationIds);
    }

    [Fact]
    public void AddOrUpdateLocation_SkeletonReplacement_UpdatesInPlace()
    {
        GameWorld gameWorld = new GameWorld();
        Venue venue = new Venue("venue1", "Test Venue") { MaxLocations = 10 };
        gameWorld.Venues.Add(venue);

        Location skeleton = new Location("loc1", "Skeleton")
        {
            VenueId = venue.Id,
            Venue = venue,
            IsSkeleton = true
        };
        gameWorld.Locations.Add(skeleton);
        venue.LocationIds.Add(skeleton.Id);

        Location fullLocation = new Location("loc1", "Full Location")
        {
            VenueId = venue.Id,
            Venue = venue,
            IsSkeleton = false
        };

        gameWorld.AddOrUpdateLocation(fullLocation.Id, fullLocation);

        Assert.Single(gameWorld.Locations);
        Assert.Single(venue.LocationIds);
        Assert.Equal("Full Location", gameWorld.Locations[0].Name);
        Assert.False(gameWorld.Locations[0].IsSkeleton);
    }

    [Fact]
    public void CapacityBudget_CountsAllLocations_AuthoredAndGenerated()
    {
        Venue venue = new Venue("venue1", "Test Venue")
        {
            MaxLocations = 5,
            LocationIds = new List<string> { "authored1", "authored2", "generated1", "generated2" }
        };

        Assert.Equal(4, venue.LocationIds.Count);
        Assert.True(venue.CanAddMoreLocations());

        venue.LocationIds.Add("generated3");
        Assert.False(venue.CanAddMoreLocations());
    }

    [Fact]
    public void CapacityBudget_DerivedFromLocationIds_NotSeparateCounter()
    {
        Venue venue = new Venue("venue1", "Test Venue")
        {
            MaxLocations = 3,
            LocationIds = new List<string>()
        };

        Assert.Equal(0, venue.LocationIds.Count);
        Assert.True(venue.CanAddMoreLocations());

        venue.LocationIds.Add("loc1");
        Assert.Equal(1, venue.LocationIds.Count);
        Assert.True(venue.CanAddMoreLocations());

        venue.LocationIds.Add("loc2");
        venue.LocationIds.Add("loc3");
        Assert.Equal(3, venue.LocationIds.Count);
        Assert.False(venue.CanAddMoreLocations());
    }

    [Fact]
    public void AddOrUpdateLocation_PreservesObjectIdentity()
    {
        GameWorld gameWorld = new GameWorld();
        Venue venue = new Venue("venue1", "Test Venue") { MaxLocations = 10 };
        gameWorld.Venues.Add(venue);

        Location original = new Location("loc1", "Original")
        {
            VenueId = venue.Id,
            Venue = venue
        };
        gameWorld.Locations.Add(original);
        venue.LocationIds.Add(original.Id);

        Location reference = gameWorld.Locations[0];

        Location updated = new Location("loc1", "Updated")
        {
            VenueId = venue.Id,
            Venue = venue
        };

        gameWorld.AddOrUpdateLocation(updated.Id, updated);

        Assert.Same(reference, gameWorld.Locations[0]);
        Assert.Equal("Updated", reference.Name);
    }

    [Fact]
    public void BidirectionalRelationship_MaintainedThroughUpdates()
    {
        GameWorld gameWorld = new GameWorld();
        Venue venue = new Venue("venue1", "Test Venue") { MaxLocations = 10 };
        gameWorld.Venues.Add(venue);

        Location location1 = new Location("loc1", "Location 1") { VenueId = venue.Id, Venue = venue };
        Location location2 = new Location("loc2", "Location 2") { VenueId = venue.Id, Venue = venue };
        Location location3 = new Location("loc3", "Location 3") { VenueId = venue.Id, Venue = venue };

        gameWorld.AddOrUpdateLocation(location1.Id, location1);
        gameWorld.AddOrUpdateLocation(location2.Id, location2);
        gameWorld.AddOrUpdateLocation(location3.Id, location3);

        Assert.Equal(3, venue.LocationIds.Count);
        Assert.Contains("loc1", venue.LocationIds);
        Assert.Contains("loc2", venue.LocationIds);
        Assert.Contains("loc3", venue.LocationIds);

        foreach (Location loc in gameWorld.Locations)
        {
            Assert.Equal(venue, loc.Venue);
            Assert.Equal(venue.Id, loc.VenueId);
        }
    }
}
