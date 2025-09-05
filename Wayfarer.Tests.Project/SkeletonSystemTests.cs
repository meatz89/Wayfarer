using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Wayfarer.Tests
{
    [TestFixture]
    public class SkeletonSystemTests
    {
        private GameWorld gameWorld;
        private PackageLoader packageLoader;
        private string contentPath;

        [SetUp]
        public void Setup()
        {
            gameWorld = new GameWorld();
            packageLoader = new PackageLoader(gameWorld);
            contentPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "src", "Content", "TestPackages");
        }

        [Test]
        public void LoadPackageWithMissingReferences_CreatesSkeletons()
        {
            // Arrange
            string packagePath = Path.Combine(contentPath, "test_01_npcs_with_missing_refs.json");
            
            // Act
            packageLoader.LoadPackage(packagePath);

            // Assert - verify skeletons were created for missing locations
            Assert.That(gameWorld.SkeletonRegistry.Count, Is.GreaterThan(0), "Skeletons should be created");
            Assert.That(gameWorld.SkeletonRegistry.ContainsKey("mysterious_tower"), Is.True, "Should have skeleton for mysterious_tower");
            Assert.That(gameWorld.SkeletonRegistry.ContainsKey("hidden_den"), Is.True, "Should have skeleton for hidden_den");
            
            // Verify skeleton locations exist and are marked as skeletons
            var mysteriousTower = gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == "mysterious_tower");
            Assert.That(mysteriousTower, Is.Not.Null, "Skeleton location should exist");
            Assert.That(mysteriousTower.IsSkeleton, Is.True, "Location should be marked as skeleton");
            Assert.That(mysteriousTower.SkeletonSource, Contains.Substring("npc_"), "Should track what created the skeleton");
            
            // Verify NPCs were loaded successfully despite missing references
            Assert.That(gameWorld.NPCs.Count, Is.EqualTo(2), "NPCs should be loaded");
            Assert.That(gameWorld.NPCs.Any(n => n.ID == "traveling_merchant"), Is.True);
            Assert.That(gameWorld.NPCs.Any(n => n.ID == "shadow_broker"), Is.True);
        }

        [Test]
        public void LoadResolvingPackage_ReplacesSkeletons()
        {
            // Arrange - first load NPCs with missing references
            string npcPackagePath = Path.Combine(contentPath, "test_01_npcs_with_missing_refs.json");
            packageLoader.LoadPackage(npcPackagePath);
            
            int initialSkeletonCount = gameWorld.SkeletonRegistry.Count;
            Assert.That(initialSkeletonCount, Is.GreaterThan(0), "Should have skeletons initially");
            
            // Act - load package that resolves the skeletons
            string locationPackagePath = Path.Combine(contentPath, "test_02_locations_resolving_skeletons.json");
            packageLoader.LoadPackage(locationPackagePath);
            
            // Assert - skeletons should be replaced with real content
            Assert.That(gameWorld.SkeletonRegistry.Count, Is.LessThan(initialSkeletonCount), "Some skeletons should be resolved");
            Assert.That(gameWorld.SkeletonRegistry.ContainsKey("mysterious_tower"), Is.False, "mysterious_tower skeleton should be replaced");
            Assert.That(gameWorld.SkeletonRegistry.ContainsKey("hidden_den"), Is.False, "hidden_den skeleton should be replaced");
            
            // Verify real locations replaced skeletons
            var mysteriousTower = gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == "mysterious_tower");
            Assert.That(mysteriousTower, Is.Not.Null, "Real location should exist");
            Assert.That(mysteriousTower.IsSkeleton, Is.False, "Location should no longer be skeleton");
            Assert.That(mysteriousTower.Name, Is.EqualTo("The Tower of Echoes"), "Should have real name");
            
            // Verify no duplicate locations
            var towerCount = gameWorld.WorldState.locations.Count(l => l.Id == "mysterious_tower");
            Assert.That(towerCount, Is.EqualTo(1), "Should not have duplicate locations");
        }

        [Test]
        public void SkeletonGenerator_CreatesDeterministicContent()
        {
            // Arrange
            string testId = "test_location_123";
            string source = "test_source";
            
            // Act - generate same skeleton twice
            var skeleton1 = SkeletonGenerator.GenerateSkeletonLocation(testId, source);
            var skeleton2 = SkeletonGenerator.GenerateSkeletonLocation(testId, source);
            
            // Assert - should generate identical content (deterministic based on ID)
            Assert.That(skeleton1.Name, Is.EqualTo(skeleton2.Name), "Names should be identical");
            Assert.That(skeleton1.LocationType, Is.EqualTo(skeleton2.LocationType), "Types should be identical");
            Assert.That(skeleton1.Tier, Is.EqualTo(skeleton2.Tier), "Tiers should be identical");
            Assert.That(skeleton1.IsSkeleton, Is.True, "Should be marked as skeleton");
            Assert.That(skeleton1.SkeletonSource, Is.EqualTo(source), "Should track source");
        }

        [Test]
        public void SkeletonNPC_HasValidMechanicalDefaults()
        {
            // Arrange & Act
            var skeleton = SkeletonGenerator.GenerateSkeletonNPC("test_npc", "test_source");
            
            // Assert - verify all required mechanical properties are set
            Assert.That(Enum.IsDefined(typeof(PersonalityType), skeleton.PersonalityType), Is.True, "Should have valid personality");
            Assert.That(Enum.IsDefined(typeof(Professions), skeleton.Profession), Is.True, "Should have valid profession");
            Assert.That(skeleton.CurrentState, Is.EqualTo(ConnectionState.NEUTRAL), "Should have neutral state");
            Assert.That(skeleton.Tier, Is.InRange(1, 3), "Should have valid tier");
            Assert.That(skeleton.Name, Is.Not.Empty, "Should have generic name");
            Assert.That(skeleton.Description, Is.Not.Empty, "Should have generic description");
        }

        [Test]
        public void GetSkeletonReport_ListsAllSkeletons()
        {
            // Arrange
            gameWorld.SkeletonRegistry["location_1"] = "Location";
            gameWorld.SkeletonRegistry["npc_1"] = "NPC";
            gameWorld.SkeletonRegistry["spot_1"] = "LocationSpot";
            
            // Act
            var report = gameWorld.GetSkeletonReport();
            
            // Assert
            Assert.That(report.Count, Is.EqualTo(3), "Should list all skeletons");
            Assert.That(report.Any(r => r.Contains("Location: location_1")), Is.True);
            Assert.That(report.Any(r => r.Contains("NPC: npc_1")), Is.True);
            Assert.That(report.Any(r => r.Contains("LocationSpot: spot_1")), Is.True);
        }

        [Test]
        public void MultiplePackages_AccumulateContentWithSkeletons()
        {
            // Arrange - load packages in sequence
            string[] packageFiles = new[]
            {
                "test_01_npcs_with_missing_refs.json",
                "test_03_letters_with_missing_npcs.json",
                "test_02_locations_resolving_skeletons.json"
            };
            
            // Act
            foreach (var file in packageFiles)
            {
                string packagePath = Path.Combine(contentPath, file);
                if (File.Exists(packagePath))
                {
                    packageLoader.LoadPackage(packagePath);
                }
            }
            
            // Assert - verify content accumulated and some skeletons remain
            Assert.That(gameWorld.NPCs.Count, Is.GreaterThanOrEqualTo(2), "NPCs should accumulate");
            Assert.That(gameWorld.WorldState.locations.Count, Is.GreaterThanOrEqualTo(2), "Locations should accumulate");
            Assert.That(gameWorld.WorldState.LetterTemplates.Count, Is.GreaterThanOrEqualTo(2), "Letters should accumulate");
            
            // Some skeletons should remain for NPCs that were never defined
            var skeletonReport = gameWorld.GetSkeletonReport();
            Console.WriteLine($"Remaining skeletons: {string.Join(", ", skeletonReport)}");
            Assert.That(skeletonReport.Count, Is.GreaterThan(0), "Some skeletons should remain unresolved");
        }

        [Test]
        public void GameIsPlayable_WithSkeletons()
        {
            // Arrange - load package with missing references
            string packagePath = Path.Combine(contentPath, "test_01_npcs_with_missing_refs.json");
            packageLoader.LoadPackage(packagePath);
            
            // Act - verify game is still playable
            var npc = gameWorld.NPCs.FirstOrDefault();
            var location = gameWorld.WorldState.locations.FirstOrDefault();
            
            // Assert - basic game mechanics should work
            Assert.That(npc, Is.Not.Null, "Should have NPCs");
            Assert.That(location, Is.Not.Null, "Should have locations (including skeletons)");
            Assert.That(Enum.IsDefined(typeof(PersonalityType), npc.PersonalityType), Is.True, "NPC should have valid personality");
            Assert.That(Enum.IsDefined(typeof(LocationTypes), location.LocationType), Is.True, "Location should have valid type");
            
            // Skeleton locations should be navigable
            var skeletonLocation = gameWorld.WorldState.locations.FirstOrDefault(l => l.IsSkeleton);
        }
    }
}