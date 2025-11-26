using Xunit;

/// <summary>
/// MANDATORY TEST COVERAGE: LocationCapability Flags enum bitwise operations
///
/// This test file should have been written BEFORE the LocationCapability refactoring was committed.
/// Tests ALL critical operations on the Flags enum across the entire system:
/// - Bitwise operations (HasFlag, OR, AND)
/// - EnumParser integration (string parsing to enum)
/// - LocationParser capability parsing from JSON
/// - EntityResolver capability matching in PlacementFilter queries (find-only)
/// - Edge cases and boundary conditions
///
/// PRINCIPLE: Test behaviors (bitwise logic), not implementation (switch statements).
/// PRINCIPLE: Test invariants (flag combinations work correctly), not concrete values (specific flag numbers).
/// PRINCIPLE: Use Theory tests for comprehensive coverage across all 31 capability values.
/// </summary>
public class LocationCapabilityTests
{
    // ==================== SECTION 1: BASIC ENUM OPERATIONS ====================

    [Fact]
    public void LocationCapability_None_HasValueZero()
    {
        // ARRANGE & ACT
        LocationCapability none = LocationCapability.None;

        // ASSERT: None flag has value 0 (default empty state)
        Assert.Equal(0, (int)none);
    }

    [Fact]
    public void LocationCapability_AllFlagsUnique_NoDuplicateBits()
    {
        // ARRANGE: Get all capability flags except None
        LocationCapability[] allFlags = Enum.GetValues<LocationCapability>()
            .Where(cap => cap != LocationCapability.None)
            .ToArray();

        // ACT: Verify each flag has unique bit pattern
        HashSet<int> bitValues = new HashSet<int>();
        foreach (LocationCapability flag in allFlags)
        {
            bitValues.Add((int)flag);
        }

        // ASSERT: All flags have unique values (no collision)
        Assert.Equal(allFlags.Length, bitValues.Count);
    }

    // ==================== SECTION 2: BITWISE OR OPERATIONS (COMBINING FLAGS) ====================

    [Fact]
    public void LocationCapability_BitwiseOR_CombinesTwoFlags()
    {
        // ACT: Combine two capabilities using bitwise OR
        LocationCapability combined = LocationCapability.Crossroads | LocationCapability.Commercial;

        // ASSERT: Result contains both flags
        Assert.True(combined.HasFlag(LocationCapability.Crossroads));
        Assert.True(combined.HasFlag(LocationCapability.Commercial));
    }

    [Fact]
    public void LocationCapability_BitwiseOR_CombinesMultipleFlags()
    {
        // ACT: Combine four capabilities
        LocationCapability combined = LocationCapability.Indoor
            | LocationCapability.Social
            | LocationCapability.Commercial
            | LocationCapability.Market;

        // ASSERT: Result contains all four flags
        Assert.True(combined.HasFlag(LocationCapability.Indoor));
        Assert.True(combined.HasFlag(LocationCapability.Social));
        Assert.True(combined.HasFlag(LocationCapability.Commercial));
        Assert.True(combined.HasFlag(LocationCapability.Market));
    }

    [Fact]
    public void LocationCapability_BitwiseOR_WithNone_PreservesFlags()
    {
        // ACT: Combine capability with None
        LocationCapability result = LocationCapability.Tavern | LocationCapability.None;

        // ASSERT: None has no effect (identity operation)
        Assert.Equal(LocationCapability.Tavern, result);
    }

    [Fact]
    public void LocationCapability_BitwiseOR_Idempotent_DuplicateFlagIgnored()
    {
        // ACT: Combine same flag multiple times
        LocationCapability result = LocationCapability.Temple
            | LocationCapability.Temple
            | LocationCapability.Temple;

        // ASSERT: Result equals single flag (idempotent operation)
        Assert.Equal(LocationCapability.Temple, result);
    }

    [Fact]
    public void LocationCapability_BitwiseOR_ChainedCombinations()
    {
        // ACT: Build combined capabilities progressively
        LocationCapability capabilities = LocationCapability.None;
        capabilities |= LocationCapability.Crossroads;
        capabilities |= LocationCapability.Transit;
        capabilities |= LocationCapability.Urban;

        // ASSERT: All flags present
        Assert.True(capabilities.HasFlag(LocationCapability.Crossroads));
        Assert.True(capabilities.HasFlag(LocationCapability.Transit));
        Assert.True(capabilities.HasFlag(LocationCapability.Urban));
    }

    // ==================== SECTION 3: BITWISE AND OPERATIONS (CHECKING REQUIREMENTS) ====================

    [Fact]
    public void LocationCapability_BitwiseAND_ChecksSingleRequirement()
    {
        // ARRANGE: Location with multiple capabilities
        LocationCapability location = LocationCapability.Indoor | LocationCapability.Social | LocationCapability.Tavern;
        LocationCapability required = LocationCapability.Tavern;

        // ACT: Check if location has required capability
        bool hasCapability = (location & required) == required;

        // ASSERT: Location has Tavern capability
        Assert.True(hasCapability);
    }

    [Fact]
    public void LocationCapability_BitwiseAND_ChecksMultipleRequirements()
    {
        // ARRANGE: Location with multiple capabilities
        LocationCapability location = LocationCapability.Indoor
            | LocationCapability.Social
            | LocationCapability.Commercial
            | LocationCapability.Market;

        LocationCapability required = LocationCapability.Indoor | LocationCapability.Market;

        // ACT: Check if location has ALL required capabilities
        bool hasAllCapabilities = (location & required) == required;

        // ASSERT: Location has both Indoor AND Market
        Assert.True(hasAllCapabilities);
    }

    [Fact]
    public void LocationCapability_BitwiseAND_FailsWhenMissingRequirement()
    {
        // ARRANGE: Location WITHOUT all required capabilities
        LocationCapability location = LocationCapability.Indoor | LocationCapability.Social;
        LocationCapability required = LocationCapability.Indoor | LocationCapability.Commercial;

        // ACT: Check if location has ALL required capabilities
        bool hasAllCapabilities = (location & required) == required;

        // ASSERT: Location missing Commercial capability
        Assert.False(hasAllCapabilities);
    }

    [Fact]
    public void LocationCapability_BitwiseAND_NoneRequirement_AlwaysMatches()
    {
        // ARRANGE: Location with any capabilities
        LocationCapability location = LocationCapability.Temple | LocationCapability.Indoor;
        LocationCapability required = LocationCapability.None;

        // ACT: Check if location has None requirement
        bool hasNone = (location & required) == required;

        // ASSERT: None requirement always satisfied (0 == 0)
        Assert.True(hasNone);
    }

    [Fact]
    public void LocationCapability_BitwiseAND_EmptyLocation_OnlyMatchesNone()
    {
        // ARRANGE: Location with no capabilities
        LocationCapability location = LocationCapability.None;
        LocationCapability required = LocationCapability.Commercial;

        // ACT: Check if empty location has Commercial
        bool hasCapability = (location & required) == required;

        // ASSERT: Empty location doesn't match any requirement except None
        Assert.False(hasCapability);
    }

    [Fact]
    public void LocationCapability_BitwiseAND_PartialMatch_StillFails()
    {
        // ARRANGE: Location with some but not all required capabilities
        LocationCapability location = LocationCapability.Crossroads | LocationCapability.Urban;
        LocationCapability required = LocationCapability.Crossroads | LocationCapability.Transit | LocationCapability.TransitHub;

        // ACT: Check for ALL requirements
        bool hasAll = (location & required) == required;

        // ASSERT: Partial match is NOT sufficient (must have ALL)
        Assert.False(hasAll);
    }

    // ==================== SECTION 4: HASFLAG METHOD (ALTERNATIVE CHECK) ====================

    [Fact]
    public void LocationCapability_HasFlag_DetectsSingleFlag()
    {
        // ARRANGE: Location with multiple capabilities
        LocationCapability capabilities = LocationCapability.Market | LocationCapability.Commercial | LocationCapability.Indoor;

        // ACT & ASSERT: HasFlag detects each individual flag
        Assert.True(capabilities.HasFlag(LocationCapability.Market));
        Assert.True(capabilities.HasFlag(LocationCapability.Commercial));
        Assert.True(capabilities.HasFlag(LocationCapability.Indoor));
    }

    [Fact]
    public void LocationCapability_HasFlag_DetectsCombinedFlags()
    {
        // ARRANGE: Location capabilities
        LocationCapability capabilities = LocationCapability.Crossroads | LocationCapability.Transit | LocationCapability.Urban;

        // ACT: Check for combined flags
        LocationCapability requiredCombo = LocationCapability.Crossroads | LocationCapability.Transit;
        bool hasCombo = capabilities.HasFlag(requiredCombo);

        // ASSERT: HasFlag works with combined requirements
        Assert.True(hasCombo);
    }

    [Fact]
    public void LocationCapability_HasFlag_ReturnsFalseForMissingFlag()
    {
        // ARRANGE: Location without specific capability
        LocationCapability capabilities = LocationCapability.Indoor | LocationCapability.Social;

        // ACT & ASSERT: HasFlag returns false for missing capability
        Assert.False(capabilities.HasFlag(LocationCapability.Commercial));
        Assert.False(capabilities.HasFlag(LocationCapability.Outdoor));
    }

    [Fact]
    public void LocationCapability_HasFlag_NoneFlag_AlwaysTrue()
    {
        // ARRANGE: Any capability combination
        LocationCapability capabilities = LocationCapability.Temple | LocationCapability.Prestigious;

        // ACT & ASSERT: HasFlag(None) always returns true
        Assert.True(capabilities.HasFlag(LocationCapability.None));
    }

    // ==================== SECTION 5: ENUMPARSER INTEGRATION ====================

    [Theory]
    [InlineData("Crossroads", LocationCapability.Crossroads)]
    [InlineData("TransitHub", LocationCapability.TransitHub)]
    [InlineData("Gateway", LocationCapability.Gateway)]
    [InlineData("Commercial", LocationCapability.Commercial)]
    [InlineData("Market", LocationCapability.Market)]
    [InlineData("Tavern", LocationCapability.Tavern)]
    [InlineData("SleepingSpace", LocationCapability.SleepingSpace)]
    [InlineData("Indoor", LocationCapability.Indoor)]
    [InlineData("Outdoor", LocationCapability.Outdoor)]
    [InlineData("Social", LocationCapability.Social)]
    [InlineData("Temple", LocationCapability.Temple)]
    [InlineData("Noble", LocationCapability.Noble)]
    [InlineData("Water", LocationCapability.Water)]
    [InlineData("River", LocationCapability.River)]
    [InlineData("Official", LocationCapability.Official)]
    [InlineData("Authority", LocationCapability.Authority)]
    [InlineData("Guarded", LocationCapability.Guarded)]
    [InlineData("Checkpoint", LocationCapability.Checkpoint)]
    [InlineData("Wealthy", LocationCapability.Wealthy)]
    [InlineData("Prestigious", LocationCapability.Prestigious)]
    [InlineData("Urban", LocationCapability.Urban)]
    [InlineData("Rural", LocationCapability.Rural)]
    [InlineData("ViewsMainEntrance", LocationCapability.ViewsMainEntrance)]
    [InlineData("ViewsBackAlley", LocationCapability.ViewsBackAlley)]
    public void EnumParser_TryParse_ValidCapabilityString_ReturnsTrue(string capabilityString, LocationCapability expected)
    {
        // ACT: Parse capability string
        bool success = EnumParser.TryParse<LocationCapability>(capabilityString, out LocationCapability result);

        // ASSERT: Parsing succeeds and returns correct capability
        Assert.True(success);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("crossroads")] // lowercase
    [InlineData("COMMERCIAL")] // uppercase
    [InlineData("TaVeRn")] // mixed case
    public void EnumParser_TryParse_CaseInsensitive_ReturnsTrue(string capabilityString)
    {
        // ACT: Parse with different casing
        bool success = EnumParser.TryParse<LocationCapability>(capabilityString, out LocationCapability result);

        // ASSERT: Case-insensitive parsing succeeds
        Assert.True(success);
        Assert.NotEqual(LocationCapability.None, result);
    }

    [Theory]
    [InlineData("InvalidCapability")]
    [InlineData("NotACapability")]
    [InlineData("")]
    [InlineData("   ")]
    public void EnumParser_TryParse_InvalidString_ReturnsFalse(string invalidString)
    {
        // ACT: Parse invalid capability string
        bool success = EnumParser.TryParse<LocationCapability>(invalidString, out LocationCapability result);

        // ASSERT: Parsing fails and returns None (default)
        Assert.False(success);
        Assert.Equal(LocationCapability.None, result);
    }

    [Fact]
    public void EnumParser_TryParse_NullString_ReturnsFalse()
    {
        // ACT: Parse null string
        bool success = EnumParser.TryParse<LocationCapability>(null, out LocationCapability result);

        // ASSERT: Parsing fails gracefully
        Assert.False(success);
        Assert.Equal(LocationCapability.None, result);
    }

    // ==================== SECTION 6: LOCATIONPARSER CAPABILITY PARSING ====================

    [Fact]
    public void LocationParser_ParseCapabilities_SingleCapability_SetCorrectly()
    {
        // ARRANGE: DTO with single capability (all required fields provided)
        LocationDTO dto = CreateValidLocationDTO("test_location", "Test Location");
        dto.Capabilities = new List<string> { "Commercial" };

        GameWorld world = new GameWorld();

        // ACT: Parse DTO to Location
        Location location = LocationParser.ConvertDTOToLocation(dto, world);

        // ASSERT: Capability parsed and set correctly
        Assert.True(location.Capabilities.HasFlag(LocationCapability.Commercial));
        Assert.Equal(LocationCapability.Commercial, location.Capabilities);
    }

    [Fact]
    public void LocationParser_ParseCapabilities_MultipleCapabilities_CombinedWithOR()
    {
        // ARRANGE: DTO with multiple capabilities
        LocationDTO dto = CreateValidLocationDTO("tavern_location", "The Prancing Pony");
        dto.Capabilities = new List<string> { "Indoor", "Social", "Tavern", "SleepingSpace" };

        GameWorld world = new GameWorld();

        // ACT: Parse DTO to Location
        Location location = LocationParser.ConvertDTOToLocation(dto, world);

        // ASSERT: All capabilities combined using bitwise OR
        Assert.True(location.Capabilities.HasFlag(LocationCapability.Indoor));
        Assert.True(location.Capabilities.HasFlag(LocationCapability.Social));
        Assert.True(location.Capabilities.HasFlag(LocationCapability.Tavern));
        Assert.True(location.Capabilities.HasFlag(LocationCapability.SleepingSpace));

        // ASSERT: Verify combined value matches bitwise OR
        LocationCapability expected = LocationCapability.Indoor
            | LocationCapability.Social
            | LocationCapability.Tavern
            | LocationCapability.SleepingSpace;
        Assert.Equal(expected, location.Capabilities);
    }

    [Fact]
    public void LocationParser_ParseCapabilities_EmptyList_SetsNone()
    {
        // ARRANGE: DTO with empty capabilities list
        LocationDTO dto = CreateValidLocationDTO("empty_location", "Empty Location");
        dto.Capabilities = new List<string>();

        GameWorld world = new GameWorld();

        // ACT: Parse DTO to Location
        Location location = LocationParser.ConvertDTOToLocation(dto, world);

        // ASSERT: Capabilities defaults to None when empty
        Assert.Equal(LocationCapability.None, location.Capabilities);
    }

    [Fact]
    public void LocationParser_ParseCapabilities_NullList_SetsNone()
    {
        // ARRANGE: DTO with null capabilities
        LocationDTO dto = CreateValidLocationDTO("null_location", "Null Location");
        dto.Capabilities = null;

        GameWorld world = new GameWorld();

        // ACT: Parse DTO to Location
        Location location = LocationParser.ConvertDTOToLocation(dto, world);

        // ASSERT: Capabilities defaults to None when null
        Assert.Equal(LocationCapability.None, location.Capabilities);
    }

    [Fact]
    public void LocationParser_ParseCapabilities_InvalidCapabilityString_Skipped()
    {
        // ARRANGE: DTO with mix of valid and invalid capabilities
        LocationDTO dto = CreateValidLocationDTO("mixed_location", "Mixed Location");
        dto.Capabilities = new List<string> { "Commercial", "InvalidCapability", "Market" };

        GameWorld world = new GameWorld();

        // ACT: Parse DTO to Location
        Location location = LocationParser.ConvertDTOToLocation(dto, world);

        // ASSERT: Valid capabilities parsed, invalid ones skipped
        Assert.True(location.Capabilities.HasFlag(LocationCapability.Commercial));
        Assert.True(location.Capabilities.HasFlag(LocationCapability.Market));

        // ASSERT: Only valid capabilities present
        LocationCapability expected = LocationCapability.Commercial | LocationCapability.Market;
        Assert.Equal(expected, location.Capabilities);
    }

    /// <summary>
    /// Helper: Create a valid LocationDTO with all required fields for capability parsing tests.
    /// Tests focus on capability behavior; other fields use minimal valid values.
    /// </summary>
    private static LocationDTO CreateValidLocationDTO(string id, string name)
    {
        return new LocationDTO
        {
            Id = id,
            Name = name,
            LocationType = "Generic",
            Privacy = "Public",
            Safety = "Neutral",
            Activity = "Moderate",
            Purpose = "Generic",
            ObligationProfile = "Research"
        };
    }

    // ==================== SECTION 7: ENTITYRESOLVER CAPABILITY MATCHING ====================

    [Fact]
    public void EntityResolver_FindLocation_RequiredCapabilities_MatchesSingleCapability()
    {
        // ARRANGE: GameWorld with location having Commercial capability
        Location commercialLocation = new Location("Market Square")
        {
            Capabilities = LocationCapability.Commercial
        };

        GameWorld world = new GameWorld();
        world.Locations.Add(commercialLocation);

        EntityResolver resolver = new EntityResolver(world);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            RequiredCapabilities = LocationCapability.Commercial
        };

        // ACT: Find location with Commercial capability
        Location result = resolver.FindLocation(filter, null);

        // ASSERT: Existing location matched
        Assert.Same(commercialLocation, result);
    }

    [Fact]
    public void EntityResolver_FindLocation_RequiredCapabilities_MatchesMultipleCapabilities()
    {
        // ARRANGE: GameWorld with location having Indoor + Social capabilities
        Location tavernLocation = new Location("The Rusty Anchor")
        {
            Capabilities = LocationCapability.Indoor | LocationCapability.Social | LocationCapability.Tavern
        };

        GameWorld world = new GameWorld();
        world.Locations.Add(tavernLocation);

        EntityResolver resolver = new EntityResolver(world);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            RequiredCapabilities = LocationCapability.Indoor | LocationCapability.Social
        };

        // ACT: Find location with both Indoor AND Social
        Location result = resolver.FindLocation(filter, null);

        // ASSERT: Location has ALL required capabilities (bitwise AND check)
        Assert.Same(tavernLocation, result);
    }

    [Fact]
    public void EntityResolver_FindLocation_RequiredCapabilities_RejectsMissingCapability()
    {
        // ARRANGE: GameWorld with location missing one required capability
        Location indoorLocation = new Location("Private Room")
        {
            Capabilities = LocationCapability.Indoor // Missing Social
        };

        GameWorld world = new GameWorld();
        world.Locations.Add(indoorLocation);

        EntityResolver resolver = new EntityResolver(world);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            RequiredCapabilities = LocationCapability.Indoor | LocationCapability.Social
        };

        // ACT: Find location (no match, returns null - find-only)
        Location result = resolver.FindLocation(filter, null);

        // ASSERT: No match found (existing location missing Social capability)
        Assert.Null(result);
    }

    [Fact]
    public void EntityResolver_FindLocation_RequiredCapabilitiesNone_MatchesAnyLocation()
    {
        // ARRANGE: GameWorld with location having any capabilities
        Location anyLocation = new Location("Some Place")
        {
            Capabilities = LocationCapability.Temple | LocationCapability.Indoor
        };

        GameWorld world = new GameWorld();
        world.Locations.Add(anyLocation);

        EntityResolver resolver = new EntityResolver(world);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            RequiredCapabilities = LocationCapability.None // No capability requirement
        };

        // ACT: Find location with None requirement
        Location result = resolver.FindLocation(filter, null);

        // ASSERT: Any location matches when requirement is None
        Assert.NotNull(result);
    }

    [Fact]
    public void EntityResolver_FindLocation_RequiredCapabilities_SubsetMatch_Succeeds()
    {
        // ARRANGE: Location has MORE capabilities than required (superset)
        Location versatileLocation = new Location("Grand Hall")
        {
            Capabilities = LocationCapability.Indoor
                | LocationCapability.Social
                | LocationCapability.Gathering
                | LocationCapability.Prestigious
        };

        GameWorld world = new GameWorld();
        world.Locations.Add(versatileLocation);

        EntityResolver resolver = new EntityResolver(world);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            RequiredCapabilities = LocationCapability.Indoor | LocationCapability.Social
        };

        // ACT: Find location (requires subset of location's capabilities)
        Location result = resolver.FindLocation(filter, null);

        // ASSERT: Location matches because it has ALL required capabilities (and more)
        Assert.Same(versatileLocation, result);
    }

    [Fact]
    public void EntityResolver_FindLocation_EmptyWorld_ReturnsNull()
    {
        // ARRANGE: EntityResolver with empty GameWorld
        GameWorld world = new GameWorld();
        EntityResolver resolver = new EntityResolver(world);

        PlacementFilter filter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            RequiredCapabilities = LocationCapability.Crossroads | LocationCapability.Transit | LocationCapability.Urban
        };

        // ACT: Find location (no existing match, returns null - find-only)
        Location result = resolver.FindLocation(filter, null);

        // ASSERT: No match found (EntityResolver is find-only, doesn't create)
        Assert.Null(result);
    }

    // ==================== SECTION 8: EDGE CASES AND BOUNDARY CONDITIONS ====================

    [Fact]
    public void LocationCapability_AllFlagsSet_HasAllCapabilities()
    {
        // ARRANGE: Create capability with ALL flags set
        LocationCapability allCapabilities = LocationCapability.Crossroads
            | LocationCapability.TransitHub
            | LocationCapability.Gateway
            | LocationCapability.Transit
            | LocationCapability.Transport
            | LocationCapability.Commercial
            | LocationCapability.Market
            | LocationCapability.Tavern
            | LocationCapability.SleepingSpace
            | LocationCapability.Restful
            | LocationCapability.LodgingProvider
            | LocationCapability.Service
            | LocationCapability.Rest
            | LocationCapability.Indoor
            | LocationCapability.Outdoor
            | LocationCapability.Social
            | LocationCapability.Gathering
            | LocationCapability.Temple
            | LocationCapability.Noble
            | LocationCapability.Water
            | LocationCapability.River
            | LocationCapability.Official
            | LocationCapability.Authority
            | LocationCapability.Guarded
            | LocationCapability.Checkpoint
            | LocationCapability.Wealthy
            | LocationCapability.Prestigious
            | LocationCapability.Urban
            | LocationCapability.Rural
            | LocationCapability.ViewsMainEntrance
            | LocationCapability.ViewsBackAlley;

        // ACT & ASSERT: Verify all flags present
        Assert.True(allCapabilities.HasFlag(LocationCapability.Crossroads));
        Assert.True(allCapabilities.HasFlag(LocationCapability.Market));
        Assert.True(allCapabilities.HasFlag(LocationCapability.Temple));
        Assert.True(allCapabilities.HasFlag(LocationCapability.ViewsBackAlley));
    }

    [Fact]
    public void LocationCapability_MaxBitPosition_DoesNotExceedIntSize()
    {
        // ARRANGE: Get highest bit position capability
        LocationCapability highest = LocationCapability.ViewsBackAlley;

        // ACT: Verify bit position within int32 range
        int bitPosition = 30; // ViewsBackAlley = 1 << 30
        int expectedValue = 1 << bitPosition;

        // ASSERT: Bit position valid for int (0-30 for signed 32-bit)
        Assert.Equal(expectedValue, (int)highest);
        Assert.True(bitPosition < 31); // Avoid sign bit
    }

    [Theory]
    [InlineData(LocationCapability.Indoor, LocationCapability.Outdoor)] // Mutually exclusive conceptually
    [InlineData(LocationCapability.Urban, LocationCapability.Rural)] // Mutually exclusive conceptually
    public void LocationCapability_ConceptuallyExclusive_CanStillCombineTechnically(LocationCapability cap1, LocationCapability cap2)
    {
        // ACT: Combine conceptually exclusive capabilities (technically allowed)
        LocationCapability combined = cap1 | cap2;

        // ASSERT: Flags system doesn't enforce conceptual exclusivity (content design responsibility)
        Assert.True(combined.HasFlag(cap1));
        Assert.True(combined.HasFlag(cap2));
    }

    [Fact]
    public void LocationCapability_BitwiseOperations_AssociativeLaw()
    {
        // ARRANGE: Three capabilities
        LocationCapability a = LocationCapability.Commercial;
        LocationCapability b = LocationCapability.Market;
        LocationCapability c = LocationCapability.Indoor;

        // ACT: Combine in different orders
        LocationCapability result1 = (a | b) | c;
        LocationCapability result2 = a | (b | c);

        // ASSERT: Associative law holds (order doesn't matter)
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void LocationCapability_BitwiseOperations_CommutativeLaw()
    {
        // ARRANGE: Two capabilities
        LocationCapability a = LocationCapability.Temple;
        LocationCapability b = LocationCapability.Prestigious;

        // ACT: Combine in both orders
        LocationCapability result1 = a | b;
        LocationCapability result2 = b | a;

        // ASSERT: Commutative law holds
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void LocationCapability_ToString_ReturnsReadableString()
    {
        // ARRANGE: Single capability
        LocationCapability capability = LocationCapability.Commercial;

        // ACT: Convert to string
        string result = capability.ToString();

        // ASSERT: ToString returns enum name (for debugging/logging)
        Assert.Equal("Commercial", result);
    }

    [Fact]
    public void LocationCapability_CombinedFlags_ToString_ShowsAllFlags()
    {
        // ARRANGE: Combined capabilities
        LocationCapability combined = LocationCapability.Indoor | LocationCapability.Social | LocationCapability.Tavern;

        // ACT: Convert to string
        string result = combined.ToString();

        // ASSERT: ToString shows all flag names (for debugging)
        Assert.Contains("Indoor", result);
        Assert.Contains("Social", result);
        Assert.Contains("Tavern", result);
    }
}
