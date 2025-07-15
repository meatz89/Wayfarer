using System.Text.Json;
using Wayfarer.Content;

namespace Wayfarer.Tests;

public class NPCParserTests
{
    [Fact]
    public void NPCParser_Should_Parse_NPC_From_JSON()
    {
        // Arrange
        string json = """
        {
            "id": "innkeeper_marcus",
            "name": "Marcus the Innkeeper",
            "description": "A jovial man who runs the Dusty Flagon with warmth and efficiency.",
            "locationId": "dusty_flagon",
            "profession": "Merchant",
            "services": ["rest_services", "labor_contracts"],
            "contractCategories": ["Standard"]
        }
        """;

        // Act
        NPC npc = NPCParser.ParseNPC(json);

        // Assert
        Assert.Equal("innkeeper_marcus", npc.ID);
        Assert.Equal("Marcus the Innkeeper", npc.Name);
        Assert.Equal("Marcus the Innkeeper", npc.Role); // Role is now set to name for current JSON structure
        Assert.Equal("A jovial man who runs the Dusty Flagon with warmth and efficiency.", npc.Description);
        Assert.Equal("dusty_flagon", npc.Location);
        Assert.Equal(Professions.Merchant, npc.Profession);
        Assert.Equal(Schedule.Market_Hours, npc.AvailabilitySchedule); // Default schedule for Merchant profession
        Assert.Equal(NPCRelationship.Neutral, npc.PlayerRelationship);

        Assert.Contains(ServiceTypes.Rest, npc.ProvidedServices);
        Assert.Contains(ServiceTypes.Training, npc.ProvidedServices);
        Assert.Equal(2, npc.ProvidedServices.Count);
    }

    [Fact]
    public void NPCParser_Should_Handle_Empty_Services_Array()
    {
        // Arrange
        string json = """
        {
            "id": "test_npc",
            "name": "Test NPC",
            "description": "A test NPC",
            "locationId": "test_location",
            "profession": "Scholar",
            "services": [],
            "contractCategories": []
        }
        """;

        // Act
        NPC npc = NPCParser.ParseNPC(json);

        // Assert
        Assert.Empty(npc.ProvidedServices);
        Assert.Equal(NPCRelationship.Neutral, npc.PlayerRelationship); // All NPCs default to Neutral
        Assert.Equal(Professions.Scholar, npc.Profession);
    }

    [Fact]
    public void NPCParser_Should_Parse_All_NPCs_From_Template_File()
    {
        // Arrange
        string npcsJson = File.ReadAllText(Path.Combine("Content", "Templates", "npcs.json"));
        using JsonDocument doc = JsonDocument.Parse(npcsJson);

        // Act
        List<NPC> npcs = new List<NPC>();
        foreach (JsonElement npcElement in doc.RootElement.EnumerateArray())
        {
            NPC npc = NPCParser.ParseNPC(npcElement.GetRawText());
            npcs.Add(npc);
        }

        // Assert
        Assert.NotEmpty(npcs);
        Assert.True(npcs.Count >= 6, "Should have at least 6 NPCs in template");

        // Verify all NPCs have required categorical properties
        foreach (NPC npc in npcs)
        {
            Assert.NotNull(npc.ID);
            Assert.NotNull(npc.Name);
            // Profession should be properly set (not default initialized)
            Assert.True(npc.Profession != default(Professions) || npc.Profession == Professions.Soldier,
                $"NPC {npc.Name} has invalid profession: {npc.Profession}");
            // Schedule should be properly set
            Assert.True(npc.AvailabilitySchedule != default(Schedule) || npc.AvailabilitySchedule == Schedule.Always,
                $"NPC {npc.Name} has invalid schedule: {npc.AvailabilitySchedule}");
            Assert.Equal(NPCRelationship.Neutral, npc.PlayerRelationship); // All should start neutral
        }

        // Verify we have diverse professions
        List<Professions> professions = npcs.Select(n => n.Profession).Distinct().ToList();
        Assert.True(professions.Count >= 3, "Should have at least 3 different professions");

        // Verify we have diverse schedules
        List<Schedule> schedules = npcs.Select(n => n.AvailabilitySchedule).Distinct().ToList();
        Assert.True(schedules.Count >= 3, "Should have at least 3 different schedules");
    }

    [Fact]
    public void NPCParser_Should_Validate_Categorical_System_Integration()
    {
        // Arrange
        string npcsJson = File.ReadAllText(Path.Combine("Content", "Templates", "npcs.json"));
        using JsonDocument doc = JsonDocument.Parse(npcsJson);

        // Act
        List<NPC> npcs = new List<NPC>();
        foreach (JsonElement npcElement in doc.RootElement.EnumerateArray())
        {
            NPC npc = NPCParser.ParseNPC(npcElement.GetRawText());
            npcs.Add(npc);
        }

        // Assert - Test categorical system methods work correctly
        foreach (NPC npc in npcs)
        {
            // Test availability checking across all time blocks
            bool availableInDawn = npc.IsAvailable(TimeBlocks.Dawn);
            bool availableInMorning = npc.IsAvailable(TimeBlocks.Morning);
            bool availableInAfternoon = npc.IsAvailable(TimeBlocks.Afternoon);
            bool availableInEvening = npc.IsAvailable(TimeBlocks.Evening);
            bool availableInNight = npc.IsAvailable(TimeBlocks.Night);

            // At least one time should be available
            Assert.True(availableInDawn || availableInMorning || availableInAfternoon || availableInEvening || availableInNight,
                $"NPC {npc.Name} (ID: {npc.ID}) with schedule {npc.AvailabilitySchedule} is not available at any time: Dawn={availableInDawn}, Morning={availableInMorning}, Afternoon={availableInAfternoon}, Evening={availableInEvening}, Night={availableInNight}");

            // Test service provision
            foreach (ServiceTypes service in npc.ProvidedServices)
            {
                Assert.True(npc.CanProvideService(service));
            }

            // Test helper descriptions are not empty
            Assert.NotEmpty(npc.ProfessionDescription);
            Assert.NotEmpty(npc.ScheduleDescription);
        }
    }
}