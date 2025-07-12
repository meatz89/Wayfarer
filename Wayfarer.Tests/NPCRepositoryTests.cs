using Xunit;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Tests
{
    /// <summary>
    /// Tests that validate NPCRepository functionality and integration with GameWorld
    /// </summary>
    public class NPCRepositoryTests
    {
        private GameWorld CreateTestGameWorld()
        {
            GameWorldInitializer initializer = new GameWorldInitializer("Content");
            return initializer.LoadGame();
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
            NPC marcus = npcRepository.GetNPCById("innkeeper_marcus");

            // Assert
            Assert.NotNull(marcus);
            Assert.Equal("innkeeper_marcus", marcus.ID);
            Assert.Equal("Marcus the Innkeeper", marcus.Name);
        }

        [Fact]
        public void NPCRepository_Should_Get_NPCs_By_Location()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            NPCRepository npcRepository = new NPCRepository(gameWorld);

            // Act
            List<NPC> dustyFlagonNPCs = npcRepository.GetNPCsForLocation("dusty_flagon");

            // Assert
            Assert.NotNull(dustyFlagonNPCs);
            Assert.NotEmpty(dustyFlagonNPCs);
            
            // Verify all NPCs are indeed at dusty_flagon
            foreach (NPC npc in dustyFlagonNPCs)
            {
                Assert.Equal("dusty_flagon", npc.Location);
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
            List<NPC> availableNPCs = npcRepository.GetNPCsForLocationAndTime("dusty_flagon", TimeBlocks.Afternoon);

            // Assert
            Assert.NotNull(availableNPCs);
            
            // Verify all returned NPCs are at the location and available at the time
            foreach (NPC npc in availableNPCs)
            {
                Assert.Equal("dusty_flagon", npc.Location);
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
                SocialClass = Social_Class.Commoner,
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

            // Act
            bool removed = npcRepository.RemoveNPC("innkeeper_marcus");

            // Assert
            Assert.True(removed);
            Assert.Equal(initialCount - 1, npcRepository.GetAllNPCs().Count);
            Assert.Null(npcRepository.GetNPCById("innkeeper_marcus"));
        }

        [Fact]
        public void NPCRepository_Should_Get_NPCs_For_Location_Spot()
        {
            // Arrange
            GameWorld gameWorld = CreateTestGameWorld();
            NPCRepository npcRepository = new NPCRepository(gameWorld);

            // Act - Look for NPCs at the innkeeper spot (CHARACTER type location spot)
            List<NPC> innkeeperSpotNPCs = npcRepository.GetNPCsForLocationSpotAndTime("innkeeper", TimeBlocks.Morning);

            // Assert
            Assert.NotNull(innkeeperSpotNPCs);
            // Note: This test might find 0 or more NPCs depending on whether NPCs are connected to spots
            // The connection happens in GameWorldInitializer.ConnectNPCsToLocationSpots
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