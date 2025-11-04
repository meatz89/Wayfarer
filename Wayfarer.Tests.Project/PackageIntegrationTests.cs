using NUnit.Framework;
using System;
using System.IO;

namespace Wayfarer.Tests
{
    [TestFixture]
    public class PackageIntegrationTests
    {
        private GameWorld gameWorld;
        private PackageLoader packageLoader;
        private string contentPath;

        [SetUp]
        public void Setup()
        {
            gameWorld = new GameWorld();
            packageLoader = new PackageLoader(gameWorld);
            contentPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "src", "Content", "Core");
        }

        [Test]
        public void LoadCoreGamePackage_LoadsSuccessfully()
        {
            // Arrange
            string packagePath = Path.Combine(contentPath, "core_game_package.json");

            // Check that the file exists
            Assert.That(File.Exists(packagePath), Is.True, $"Package file not found at {packagePath}");

            // Act
            packageLoader.LoadDynamicPackage(packagePath);

            // Assert - verify content was loaded
            Assert.That(gameWorld.SocialCards.Count, Is.GreaterThan(0), "Cards should be loaded");
            Assert.That(gameWorld.NPCs.Count, Is.GreaterThan(0), "NPCs should be loaded");
            Assert.That(gameWorld.locations.Count, Is.GreaterThan(0), "Locations should be loaded");
            Assert.That(gameWorld.locations.Count, Is.GreaterThan(0), "Location spots should be loaded");
            Assert.That(gameWorld.Routes.Count, Is.GreaterThan(0), "Routes should be loaded");
            Assert.That(gameWorld.LetterTemplates.Count, Is.GreaterThan(0), "Letter templates should be loaded");

            // Check specific content
            Assert.That(gameWorld.NPCs.Any(n => n.Name == "Elena"), Is.True, "Elena should be loaded");
            Assert.That(gameWorld.NPCs.Any(n => n.Name == "Marcus"), Is.True, "Marcus should be loaded");

            // Check starting conditions
            Assert.That(gameWorld.InitialLocationId, Is.EqualTo("corner_table"), "Should start at corner table");
        }

        [Test]
        public void LoadPackage_WithInvalidPath_ThrowsException()
        {
            // Arrange
            string invalidPath = Path.Combine(contentPath, "non_existent_package.json");

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => packageLoader.LoadDynamicPackage(invalidPath));
        }

        [Test]
        public void GameWorldInitializer_CreatesValidGameWorld()
        {
            // Act
            GameWorld initializedWorld = GameWorldInitializer.CreateGameWorld();

            // Assert
            Assert.That(initializedWorld, Is.Not.Null);
            Assert.That(initializedWorld.SocialCards.Count, Is.GreaterThan(0), "Cards should be loaded");
            Assert.That(initializedWorld.NPCs.Count, Is.EqualTo(6), "Should have 6 NPCs");
            Assert.That(initializedWorld.GameWorld.locations.Count, Is.EqualTo(5), "Should have 5 locations");
        }
    }
}