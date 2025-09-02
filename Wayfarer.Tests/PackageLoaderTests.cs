using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Wayfarer.Tests
{
    [TestFixture]
    public class PackageLoaderTests
    {
        private PackageLoader packageLoader;
        private GameWorld gameWorld;
        private string testPackagesPath;

        [SetUp]
        public void Setup()
        {
            gameWorld = new GameWorld();
            packageLoader = new PackageLoader(gameWorld);
            testPackagesPath = Path.Combine(Path.GetTempPath(), "wayfarer_test_packages");
            Directory.CreateDirectory(testPackagesPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(testPackagesPath))
            {
                Directory.Delete(testPackagesPath, true);
            }
        }

        [Test]
        public void LoadPackage_WithValidPackage_LoadsAllContent()
        {
            // Arrange
            var package = new Package
            {
                PackageId = "test_complete",
                Metadata = new PackageMetadata
                {
                    Name = "Complete Test Package",
                    Timestamp = DateTime.Now,
                    Description = "Test package with all content types"
                },
                Content = new PackageContent
                {
                    Cards = new List<ConversationCardDTO>
                    {
                        new ConversationCardDTO
                        {
                            Id = "test_card_1",
                            Type = "Single",
                            ConnectionType = "Trust",
                            Weight = 1,
                            Persistence = "Persistent",
                            BaseComfort = 1
                        }
                    },
                    Npcs = new List<NPCDTO>
                    {
                        new NPCDTO
                        {
                            Id = "test_npc_1",
                            Name = "Test NPC",
                            PersonalityType = "Devoted",
                            BasePatience = 15,
                            InitialEmotionalState = "Neutral",
                            ConversationDeckCardIds = new List<string> { "test_card_1" }
                        }
                    },
                    Spots = new List<LocationSpotDTO>
                    {
                        new LocationSpotDTO
                        {
                            Id = "test_spot_1",
                            Name = "Test Spot",
                            LocationId = "Test Location",
                            District = "Test District",
                            SpotTraits = new List<string> { "Crossroads" }
                        }
                    },
                    Routes = new List<RouteDTO>
                    {
                        new RouteDTO
                        {
                            Id = "test_route_1",
                            FromSpotId = "test_spot_1",
                            ToSpotId = "test_spot_2",
                            TravelTimeMinutes = 10
                        }
                    },
                    Observations = new List<ObservationDTO>
                    {
                        new ObservationDTO
                        {
                            Id = "test_obs_1",
                            DisplayText = "Test observation",
                            Category = "Normal",
                            Weight = 1
                        }
                    }
                }
            };

            string packagePath = Path.Combine(testPackagesPath, "test_complete.json");
            File.WriteAllText(packagePath, JsonSerializer.Serialize(package));

            // Act
            packageLoader.LoadPackage(packagePath);

            // Assert
            Assert.That(gameWorld.AllCardDefinitions.Count, Is.EqualTo(1));
            Assert.That(gameWorld.NPCs.Count, Is.EqualTo(1));
            Assert.That(gameWorld.WorldState.locationSpots.Count, Is.GreaterThan(0));
            Assert.That(gameWorld.WorldState.Routes.Count, Is.EqualTo(1));
            Assert.That(gameWorld.PlayerObservationCards.Count, Is.EqualTo(1));
        }

        [Test]
        public void LoadPackage_WithCardsOnly_LoadsOnlyCards()
        {
            // Arrange
            var package = new Package
            {
                PackageId = "test_cards_only",
                Content = new PackageContent
                {
                    Cards = new List<ConversationCardDTO>
                    {
                        new ConversationCardDTO
                        {
                            Id = "test_card_1",
                            Type = "Single",
                            ConnectionType = "Trust",
                            Weight = 1,
                            Persistence = "Persistent",
                            BaseComfort = 1
                        },
                        new ConversationCardDTO
                        {
                            Id = "test_card_2",
                            Type = "Combine",
                            ConnectionType = "Commerce",
                            Weight = 2,
                            Persistence = "Fleeting",
                            BaseComfort = 2
                        }
                    }
                }
            };

            string packagePath = Path.Combine(testPackagesPath, "test_cards_only.json");
            File.WriteAllText(packagePath, JsonSerializer.Serialize(package));

            // Act
            packageLoader.LoadPackage(packagePath);

            // Assert
            Assert.That(gameWorld.AllCardDefinitions.Count, Is.EqualTo(2));
            Assert.That(gameWorld.NPCs.Count, Is.EqualTo(0));
        }

        [Test]
        public void LoadPackage_WithStartingConditions_AppliesConditions()
        {
            // Arrange
            var package = new Package
            {
                PackageId = "test_starting",
                StartingConditions = new PackageStartingConditions
                {
                    PlayerConfig = new PlayerInitialConfig
                    {
                        Coins = 50,
                        Health = 100,
                        Food = 25,
                        StaminaPoints = 8
                    },
                    StartingSpotId = "test_spot_1",
                    StartingObligations = new List<StandingObligationDTO>
                    {
                        new StandingObligationDTO
                        {
                            Id = "test_obligation_1",
                            Type = "Letter",
                            FromNpcId = "test_npc_1",
                            ToNpcId = "test_npc_2",
                            DeadlineDay = 2,
                            Payment = 10
                        }
                    },
                    StartingTokens = new Dictionary<string, NPCTokenRelationship>
                    {
                        ["test_npc_1"] = new NPCTokenRelationship { Trust = 2, Commerce = 1 }
                    }
                }
            };

            string packagePath = Path.Combine(testPackagesPath, "test_starting.json");
            File.WriteAllText(packagePath, JsonSerializer.Serialize(package));

            // Act
            packageLoader.LoadPackage(packagePath);

            // Assert
            Assert.That(gameWorld.PlayerCoins, Is.EqualTo(50));
            Assert.That(gameWorld.GetPlayer().Health, Is.EqualTo(100));
            Assert.That(gameWorld.GetPlayer().Food, Is.EqualTo(25));
            Assert.That(gameWorld.GetPlayer().CurrentLocationSpot?.SpotID, Is.EqualTo("test_spot_1"));
            Assert.That(gameWorld.GetPlayer().ObligationQueue.Count, Is.EqualTo(1));
        }

        [Test]
        public void LoadPackage_WithMultiplePackages_AccumulatesContent()
        {
            // Arrange
            var package1 = new Package
            {
                PackageId = "test_package_1",
                Content = new PackageContent
                {
                    Cards = new List<ConversationCardDTO>
                    {
                        new ConversationCardDTO { Id = "card_1", ConnectionType = "Trust", Weight = 1 }
                    },
                    Npcs = new List<NPCDTO>
                    {
                        new NPCDTO { Id = "npc_1", Name = "NPC 1" }
                    }
                }
            };

            var package2 = new Package
            {
                PackageId = "test_package_2",
                Content = new PackageContent
                {
                    Cards = new List<ConversationCardDTO>
                    {
                        new ConversationCardDTO { Id = "card_2", ConnectionType = "Commerce", Weight = 2 }
                    },
                    Npcs = new List<NPCDTO>
                    {
                        new NPCDTO { Id = "npc_2", Name = "NPC 2" }
                    }
                }
            };

            string packagePath1 = Path.Combine(testPackagesPath, "test_package_1.json");
            string packagePath2 = Path.Combine(testPackagesPath, "test_package_2.json");
            File.WriteAllText(packagePath1, JsonSerializer.Serialize(package1));
            File.WriteAllText(packagePath2, JsonSerializer.Serialize(package2));

            // Act
            packageLoader.LoadPackage(packagePath1);
            packageLoader.LoadPackage(packagePath2);

            // Assert
            Assert.That(gameWorld.AllCardDefinitions.Count, Is.EqualTo(2));
            Assert.That(gameWorld.NPCs.Count, Is.EqualTo(2));
            Assert.That(gameWorld.AllCardDefinitions.ContainsKey("card_1"), Is.True);
            Assert.That(gameWorld.AllCardDefinitions.ContainsKey("card_2"), Is.True);
        }

        [Test]
        public void LoadPackage_WithGeneratedPackage_LoadsIdentically()
        {
            // Arrange - simulating AI-generated package
            var aiPackage = new Package
            {
                PackageId = "package_generated_001",
                Metadata = new PackageMetadata
                {
                    Name = "AI Generated Content",
                    Timestamp = DateTime.Now,
                    Description = "Generated by AI",
                    Author = "AI Generator v1.0"
                },
                Content = new PackageContent
                {
                    Cards = new List<ConversationCardDTO>
                    {
                        new ConversationCardDTO
                        {
                            Id = "gen001_card_1",
                            ConnectionType = "Shadow",
                            Weight = 3,
                            Difficulty = "Hard",
                            Persistence = "Opportunity",
                            EffectType = "ScaledComfort",
                            EffectFormula = "TokenCount:Shadow"
                        }
                    },
                    Npcs = new List<NPCDTO>
                    {
                        new NPCDTO
                        {
                            Id = "gen001_npc_silva",
                            Name = "Silva the Whisperer",
                            PersonalityType = "Cunning",
                            BasePatience = 12
                        }
                    }
                }
            };

            string packagePath = Path.Combine(testPackagesPath, "package_generated_001.json");
            File.WriteAllText(packagePath, JsonSerializer.Serialize(aiPackage));

            // Act
            packageLoader.LoadPackage(packagePath);

            // Assert
            Assert.That(gameWorld.AllCardDefinitions.ContainsKey("gen001_card_1"), Is.True);
            Assert.That(gameWorld.NPCs.Count, Is.EqualTo(1));
            Assert.That(gameWorld.NPCs[0].ID, Is.EqualTo("gen001_npc_silva"));
        }

        [Test]
        public void LoadPackage_WithMissingFile_ThrowsException()
        {
            // Arrange
            string nonExistentPath = Path.Combine(testPackagesPath, "non_existent.json");

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => packageLoader.LoadPackage(nonExistentPath));
        }

        [Test]
        public void LoadPackage_WithInvalidJson_ThrowsException()
        {
            // Arrange
            string packagePath = Path.Combine(testPackagesPath, "invalid.json");
            File.WriteAllText(packagePath, "{ this is not valid json ]");

            // Act & Assert
            Assert.Throws<JsonException>(() => packageLoader.LoadPackage(packagePath));
        }

        [Test]
        public void LoadPackage_WithNullContent_HandlesGracefully()
        {
            // Arrange
            var package = new Package
            {
                PackageId = "test_null_content",
                Content = new PackageContent()  // All lists are null
            };

            string packagePath = Path.Combine(testPackagesPath, "test_null_content.json");
            File.WriteAllText(packagePath, JsonSerializer.Serialize(package));

            // Act
            packageLoader.LoadPackage(packagePath);

            // Assert - should not throw, game world should be unchanged
            Assert.That(gameWorld.AllCardDefinitions.Count, Is.EqualTo(0));
            Assert.That(gameWorld.NPCs.Count, Is.EqualTo(0));
        }
    }
}