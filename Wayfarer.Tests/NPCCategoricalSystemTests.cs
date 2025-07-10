using Xunit;

namespace Wayfarer.Tests;

public class NPCCategoricalSystemTests
{
    [Fact]
    public void NPC_Should_Have_Profession_And_Social_Class_Properties()
    {
        // Arrange & Act
        NPC npc = new NPC
        {
            ID = "test_npc",
            Name = "Test NPC",
            Profession = Professions.Merchant,
            SocialClass = Social_Class.Merchant
        };

        // Assert
        Assert.Equal(Professions.Merchant, npc.Profession);
        Assert.Equal(Social_Class.Merchant, npc.SocialClass);
    }

    [Fact]
    public void NPC_Should_Have_Schedule_For_Availability()
    {
        // Arrange & Act
        NPC npc = new NPC
        {
            ID = "test_npc",
            Name = "Test NPC",
            AvailabilitySchedule = Schedule.Morning
        };

        // Assert
        Assert.Equal(Schedule.Morning, npc.AvailabilitySchedule);
    }

    [Theory]
    [InlineData(Schedule.Morning, TimeBlocks.Morning, true)]
    [InlineData(Schedule.Morning, TimeBlocks.Afternoon, false)]
    [InlineData(Schedule.Afternoon, TimeBlocks.Afternoon, true)]
    [InlineData(Schedule.Afternoon, TimeBlocks.Morning, false)]
    [InlineData(Schedule.Evening, TimeBlocks.Evening, true)]
    [InlineData(Schedule.Always, TimeBlocks.Morning, true)]
    [InlineData(Schedule.Always, TimeBlocks.Afternoon, true)]
    [InlineData(Schedule.Always, TimeBlocks.Evening, true)]
    [InlineData(Schedule.Market_Days, TimeBlocks.Morning, true)]
    [InlineData(Schedule.Market_Days, TimeBlocks.Afternoon, true)]
    [InlineData(Schedule.Market_Days, TimeBlocks.Evening, false)]
    public void NPC_IsAvailable_Should_Return_Correct_Availability_Based_On_Schedule(
        Schedule npcSchedule, TimeBlocks currentTime, bool expectedAvailable)
    {
        // Arrange
        NPC npc = new NPC
        {
            ID = "test_npc",
            Name = "Test NPC",
            AvailabilitySchedule = npcSchedule
        };

        // Act
        bool isAvailable = npc.IsAvailable(currentTime);

        // Assert
        Assert.Equal(expectedAvailable, isAvailable);
    }

    [Fact]
    public void NPC_Should_Track_Provided_Services()
    {
        // Arrange
        NPC npc = new NPC
        {
            ID = "test_npc",
            Name = "Test NPC",
            ProvidedServices = new List<ServiceTypes> { ServiceTypes.Trade, ServiceTypes.Information }
        };

        // Act & Assert
        Assert.Contains(ServiceTypes.Trade, npc.ProvidedServices);
        Assert.Contains(ServiceTypes.Information, npc.ProvidedServices);
        Assert.DoesNotContain(ServiceTypes.Healing, npc.ProvidedServices);
    }

    [Theory]
    [InlineData(ServiceTypes.Trade, true)]
    [InlineData(ServiceTypes.Information, true)]
    [InlineData(ServiceTypes.Healing, false)]
    public void NPC_CanProvideService_Should_Return_Correct_Service_Availability(
        ServiceTypes requestedService, bool expectedCanProvide)
    {
        // Arrange
        NPC npc = new NPC
        {
            ID = "test_npc",
            Name = "Test NPC",
            ProvidedServices = new List<ServiceTypes> { ServiceTypes.Trade, ServiceTypes.Information }
        };

        // Act
        bool canProvide = npc.CanProvideService(requestedService);

        // Assert
        Assert.Equal(expectedCanProvide, canProvide);
    }

    [Theory]
    [InlineData(Social_Class.Commoner, Social_Expectation.Any, true)]
    [InlineData(Social_Class.Merchant, Social_Expectation.Any, true)]
    [InlineData(Social_Class.Commoner, Social_Expectation.Merchant_Class, false)]
    [InlineData(Social_Class.Merchant, Social_Expectation.Merchant_Class, true)]
    [InlineData(Social_Class.Craftsman, Social_Expectation.Merchant_Class, true)]
    [InlineData(Social_Class.Commoner, Social_Expectation.Professional, false)]
    [InlineData(Social_Class.Craftsman, Social_Expectation.Professional, true)]
    [InlineData(Social_Class.Merchant, Social_Expectation.Professional, false)]
    [InlineData(Social_Class.Commoner, Social_Expectation.Noble_Class, false)]
    [InlineData(Social_Class.Merchant, Social_Expectation.Noble_Class, false)]
    [InlineData(Social_Class.Minor_Noble, Social_Expectation.Noble_Class, true)]
    [InlineData(Social_Class.Major_Noble, Social_Expectation.Noble_Class, true)]
    public void NPC_MeetsLocationRequirements_Should_Check_Social_Class_Against_Expectations(
        Social_Class npcSocialClass, Social_Expectation locationExpectation, bool expectedMeetsRequirements)
    {
        // Arrange
        NPC npc = new NPC
        {
            ID = "test_npc",
            Name = "Test NPC",
            SocialClass = npcSocialClass
        };

        // Act
        bool meetsRequirements = npc.MeetsLocationRequirements(locationExpectation);

        // Assert
        Assert.Equal(expectedMeetsRequirements, meetsRequirements);
    }

    [Fact]
    public void NPC_Should_Have_Default_Relationship_As_Neutral()
    {
        // Arrange & Act
        NPC npc = new NPC
        {
            ID = "test_npc",
            Name = "Test NPC"
        };

        // Assert
        Assert.Equal(NPCRelationship.Neutral, npc.PlayerRelationship);
    }

    [Fact]
    public void NPC_Should_Provide_Readable_Description_Properties()
    {
        // Arrange
        NPC npc = new NPC
        {
            ID = "test_npc",
            Name = "Test NPC",
            Profession = Professions.Merchant,
            SocialClass = Social_Class.Minor_Noble,
            AvailabilitySchedule = Schedule.Market_Days,
            ProvidedServices = new List<ServiceTypes> { ServiceTypes.Trade, ServiceTypes.Information }
        };

        // Act & Assert
        Assert.Equal("Merchant", npc.ProfessionDescription);
        Assert.Equal("Minor Noble", npc.SocialClassDescription);
        Assert.Equal("Market Days", npc.ScheduleDescription);
        Assert.Contains("Services: Trade, Information", npc.ProvidedServicesDescription);
    }

    [Fact]
    public void NPC_Should_Show_No_Services_Available_When_Empty()
    {
        // Arrange
        NPC npc = new NPC
        {
            ID = "test_npc",
            Name = "Test NPC",
            ProvidedServices = new List<ServiceTypes>()
        };

        // Act & Assert
        Assert.Equal("No services available", npc.ProvidedServicesDescription);
    }
}