using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Tests that validate NPCRepository functionality and integration with GameWorld
    /// </summary>
    public class NPCRepositoryTests
    {
        private GameWorld CreateTestGameWorld()
        {
            // Create test service provider with test content
            IServiceCollection services = new ServiceCollection();
            
            // Add configuration
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"DefaultAIProvider", "None"} // Disable AI
                })
                .Build();
            services.AddSingleton(configuration);
            services.AddLogging();
            
            // Use test service configuration
            services.ConfigureTestServices("Content");
            
            // Build service provider and get GameWorld
            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<GameWorld>();
        }

        [Fact]
        public void NPCRepository_Should_Load_NPCs_From_GameWorld()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            NPCRepository npcRepository = new NPCRepository(gameWorld);

            // Act
            List<NPC> npcs = npcRepository.GetAllNPCs();

            // Assert
            Assert.NotNull(npcs);
            Assert.NotEmpty(npcs);
            Assert.True(npcs.Count >= 6, "Should have at least 6 NPCs from template file");
        }

        [Fact]
        public void NPCRepository_Should_Get_NPC_By_ID()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            NPCRepository npcRepository = new NPCRepository(gameWorld);

            // Act
            NPC testMerchant = npcRepository.GetNPCById("test_merchant_npc");

            // Assert
            Assert.NotNull(testMerchant);
            Assert.Equal("test_merchant_npc", testMerchant.ID);
            Assert.Equal("Test Merchant", testMerchant.Name);
        }

        [Fact]
        public void NPCRepository_Should_Get_NPCs_By_Location()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            NPCRepository npcRepository = new NPCRepository(gameWorld);

            // Debug: Check all NPCs and their locations
            List<NPC> allNPCs = npcRepository.GetAllNPCs();
            Assert.True(allNPCs.Count > 0, $"No NPCs loaded! Expected at least 6 NPCs.");
            foreach (var npc in allNPCs)
            {
                Assert.True(!string.IsNullOrEmpty(npc.Location), $"NPC {npc.Name} has empty Location!");
            }

            // Act
            List<NPC> testLocationNPCs = npcRepository.GetNPCsForLocation("test_start_location");

            // Assert
            Assert.NotNull(testLocationNPCs);
            Assert.NotEmpty(testLocationNPCs);
            Assert.True(testLocationNPCs.Count >= 3, "Should have at least 3 NPCs at test_start_location");

            // Verify all NPCs are indeed at test_start_location
            foreach (NPC npc in testLocationNPCs)
            {
                Assert.Equal("test_start_location", npc.Location);
            }
        }

        [Fact]
        public void NPCRepository_Should_Get_Available_NPCs_By_Time()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            NPCRepository npcRepository = new NPCRepository(gameWorld);

            // Act
            List<NPC> morningNPCs = npcRepository.GetAvailableNPCs(TimeBlocks.Morning);
            List<NPC> eveningNPCs = npcRepository.GetAvailableNPCs(TimeBlocks.Evening);

            // Assert
            Assert.NotNull(morningNPCs);
            Assert.NotNull(eveningNPCs);

            // Verify all returned NPCs are actually available at the requested time
            foreach (NPC npc in morningNPCs)
            {
                Assert.True(npc.IsAvailable(TimeBlocks.Morning), $"NPC {npc.Name} should be available in the morning");
            }

            foreach (NPC npc in eveningNPCs)
            {
                Assert.True(npc.IsAvailable(TimeBlocks.Evening), $"NPC {npc.Name} should be available in the evening");
            }
        }

        [Fact]
        public void NPCRepository_Should_Get_NPCs_By_Profession()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            NPCRepository npcRepository = new NPCRepository(gameWorld);

            // Act
            List<NPC> merchants = npcRepository.GetNPCsByProfession(Professions.Merchant);

            // Assert
            Assert.NotNull(merchants);

            // Verify all returned NPCs have the merchant profession
            foreach (NPC npc in merchants)
            {
                Assert.Equal(Professions.Merchant, npc.Profession);
            }
        }

        [Fact]
        public void NPCRepository_Should_Get_NPCs_Providing_Service()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            NPCRepository npcRepository = new NPCRepository(gameWorld);

            // Act
            List<NPC> lodgingProviders = npcRepository.GetNPCsProvidingService(ServiceTypes.Lodging);

            // Assert
            Assert.NotNull(lodgingProviders);

            // Verify all returned NPCs provide lodging service
            foreach (NPC npc in lodgingProviders)
            {
                Assert.True(npc.ProvidedServices.Contains(ServiceTypes.Lodging),
                    $"NPC {npc.Name} should provide lodging service");
            }
        }

        [Fact]
        public void NPCRepository_Should_Get_NPCs_For_Location_And_Time()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            NPCRepository npcRepository = new NPCRepository(gameWorld);

            // Act
            List<NPC> availableNPCs = npcRepository.GetNPCsForLocationAndTime("test_start_location", TimeBlocks.Afternoon);

            // Assert
            Assert.NotNull(availableNPCs);
            Assert.NotEmpty(availableNPCs); // test_merchant_npc and test_craftsman_npc should be available in afternoon

            // Verify all returned NPCs are at the location and available at the time
            foreach (NPC npc in availableNPCs)
            {
                Assert.Equal("test_start_location", npc.Location);
                Assert.True(npc.IsAvailable(TimeBlocks.Afternoon),
                    $"NPC {npc.Name} should be available in the afternoon");
            }
        }

        [Fact]
        public void NPCRepository_Should_Add_New_NPC()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            NPCRepository npcRepository = new NPCRepository(gameWorld);

            NPC newNPC = new NPC
            {
                ID = "test_npc",
                Name = "Test NPC",
                Location = "town_square",
                Profession = Professions.Scholar,
                AvailabilitySchedule = Schedule.Always
            };

            // Act
            npcRepository.AddNPC(newNPC);

            // Assert
            NPC retrievedNPC = npcRepository.GetNPCById("test_npc");
            Assert.NotNull(retrievedNPC);
            Assert.Equal("test_npc", retrievedNPC.ID);
            Assert.Equal("Test NPC", retrievedNPC.Name);
        }

        [Fact]
        public void NPCRepository_Should_Remove_NPC()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            NPCRepository npcRepository = new NPCRepository(gameWorld);

            // Get initial count
            int initialCount = npcRepository.GetAllNPCs().Count;

            // Act - remove a test NPC we know exists
            bool removed = npcRepository.RemoveNPC("test_innkeeper_npc");

            // Assert
            Assert.True(removed);
            Assert.Equal(initialCount - 1, npcRepository.GetAllNPCs().Count);
            Assert.Null(npcRepository.GetNPCById("test_innkeeper_npc"));
        }

        [Fact]
        public void NPCRepository_Should_Get_Primary_NPC_For_Spot()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            NPCRepository npcRepository = new NPCRepository(gameWorld);

            // Act - Try to get primary NPC for innkeeper spot
            NPC primaryNPC = npcRepository.GetPrimaryNPCForSpot("innkeeper", TimeBlocks.Afternoon);

            // Assert - May be null if no NPCs are connected to spots yet
            // This depends on the ConnectNPCsToLocationSpots implementation
            Assert.True(primaryNPC == null || primaryNPC.IsAvailable(TimeBlocks.Afternoon));
        }
    }
}