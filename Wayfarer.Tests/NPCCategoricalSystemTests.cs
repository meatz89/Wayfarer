
namespace Wayfarer.Tests;

public class NPCCategoricalSystemTests
{
    [Fact]
    public void NPC_Should_Have_Profession_Properties()
    {
        // Arrange & Act
        NPC npc = new NPC
        {
            ID = "test_npc",
            Name = "Test NPC",
            Profession = Professions.Merchant,
        };

        // Assert
        Assert.Equal(Professions.Merchant, npc.Profession);
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
            ProvidedServices = new List<ServiceTypes> { ServiceTypes.Trade, ServiceTypes.Information }
        };

        // Act & Assert
        Assert.Equal("Merchant", npc.ProfessionDescription);
        Assert.Equal("Market Hours", npc.ScheduleDescription);
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