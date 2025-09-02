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
                    Cards = new List<CardData>
                    {
                        new CardData
                        {
                            Id = "test_card_1",
                            TokenType = "Trust",
                            Weight = 1,
                            Difficulty = "Easy",
                            Persistence = "Persistent",
                            EffectType = "FixedComfort",
                            EffectValue = 1
                        }
                    },
                    Npcs = new List<NpcData>
                    {
                        new NpcData
                        {
                            Id = "test_npc_1",
                            Name = "Test NPC",
                            PersonalityType = "Devoted",
                            BasePatience = 15,
                            CurrentState = "Neutral",
                            ConversationDeckCardIds = new List<string> { "test_card_1" }
                        }
                    },
                    Spots = new List<SpotData>
                    {
                        new SpotData
                        {
                            Id = "test_spot_1",
                            Name = "Test Spot",
                            LocationName = "Test Location",
                            District = "Test District",
                            Properties = new List<string> { "Crossroads" }
                        }
                    },
                    Routes = new List<RouteData>
                    {
                        new RouteData
                        {
                            Id = "test_route_1",
                            FromSpotId = "test_spot_1",
                            ToSpotId = "test_spot_2",
                            TravelMinutes = 10
                        }
                    },
                    Observations = new List<ObservationData>
                    {
                        new ObservationData
                        {
                            Id = "test_obs_1",
                            SpotId = "test_spot_1",
                            TimePeriod = "Morning",
                            AttentionCost = 1,
                            ObservationCardId = "test_obs_card_1"
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
            Assert.That(gameWorld.Locations.Count, Is.GreaterThan(0));
            Assert.That(gameWorld.Routes.Count, Is.EqualTo(1));
            Assert.That(gameWorld.Observations.Count, Is.EqualTo(1));
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
                    Cards = new List<CardData>
                    {
                        new CardData
                        {
                            Id = "test_card_1",
                            TokenType = "Trust",
                            Weight = 1,
                            Difficulty = "Easy",
                            Persistence = "Persistent",
                            EffectType = "FixedComfort",
                            EffectValue = 1
                        },
                        new CardData
                        {
                            Id = "test_card_2",
                            TokenType = "Commerce",
                            Weight = 2,
                            Difficulty = "Medium",
                            Persistence = "Fleeting",
                            EffectType = "Draw",
                            EffectValue = 2
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
                StartingConditions = new StartingConditions
                {
                    PlayerResources = new PlayerResources
                    {
                        Coins = 50,
                        Health = 100,
                        Hunger = 25,
                        Attention = 8
                    },
                    StartingSpotId = "test_spot_1",
                    StartingQueue = new List<ObligationData>
                    {
                        new ObligationData
                        {
                            Id = "test_obligation_1",
                            Type = "Letter",
                            SenderId = "test_npc_1",
                            RecipientId = "test_npc_2",
                            DeadlineHours = 24,
                            Payment = 10
                        }
                    },
                    StartingTokens = new Dictionary<string, TokenSet>
                    {
                        ["test_npc_1"] = new TokenSet { Trust = 2, Commerce = 1 }
                    }
                }
            };

            string packagePath = Path.Combine(testPackagesPath, "test_starting.json");
            File.WriteAllText(packagePath, JsonSerializer.Serialize(package));

            // Act
            packageLoader.LoadPackage(packagePath);

            // Assert
            Assert.That(gameWorld.PlayerCoins, Is.EqualTo(50));
            Assert.That(gameWorld.Player.Health, Is.EqualTo(100));
            Assert.That(gameWorld.Player.Hunger, Is.EqualTo(25));
            Assert.That(gameWorld.WorldState.CurrentLocationSpotId, Is.EqualTo("test_spot_1"));
            Assert.That(gameWorld.ObligationQueue.Count, Is.EqualTo(1));
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
                    Cards = new List<CardData>
                    {
                        new CardData { Id = "card_1", TokenType = "Trust", Weight = 1 }
                    },
                    Npcs = new List<NpcData>
                    {
                        new NpcData { Id = "npc_1", Name = "NPC 1" }
                    }
                }
            };

            var package2 = new Package
            {
                PackageId = "test_package_2",
                Content = new PackageContent
                {
                    Cards = new List<CardData>
                    {
                        new CardData { Id = "card_2", TokenType = "Commerce", Weight = 2 }
                    },
                    Npcs = new List<NpcData>
                    {
                        new NpcData { Id = "npc_2", Name = "NPC 2" }
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
                    Cards = new List<CardData>
                    {
                        new CardData
                        {
                            Id = "gen001_card_1",
                            TokenType = "Shadow",
                            Weight = 3,
                            Difficulty = "Hard",
                            Persistence = "Opportunity",
                            EffectType = "ScaledComfort",
                            ScalingFormula = "TokenCount:Shadow"
                        }
                    },
                    Npcs = new List<NpcData>
                    {
                        new NpcData
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
            Assert.That(gameWorld.NPCs[0].Id, Is.EqualTo("gen001_npc_silva"));
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