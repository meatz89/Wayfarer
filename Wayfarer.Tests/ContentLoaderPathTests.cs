using Xunit;
using System;
using System.IO;

namespace Wayfarer.Tests
{
    public class ContentLoaderPathTests
    {
        [Fact]
        public void ContentLoader_CurrentConfiguration_CorrectPath()
        {
            // The current configuration in ServiceConfiguration uses "content" 
            // but when run from src/, this resolves to src/content/
            string contentDirectory = "content";
            string expectedTemplatePath = Path.Combine(contentDirectory, "Templates");
            
            // When running from /mnt/c/git/wayfarer/src, this should look for:
            // /mnt/c/git/wayfarer/src/content/Templates/
            
            Assert.False(Directory.Exists(expectedTemplatePath), 
                $"Expected path {expectedTemplatePath} should NOT exist from src directory");
        }

        [Fact]
        public void ContentLoader_ActualFilesLocation_Verification()
        {
            // The files are actually at src/Content/Templates/
            string actualContentPath = "Content";
            string actualTemplatePath = Path.Combine(actualContentPath, "Templates");
            
            // This should exist when running from src/ directory
            Assert.True(Directory.Exists(actualTemplatePath), 
                $"Actual path {actualTemplatePath} should exist from src directory");
            
            // Verify key files exist
            string locationsFile = Path.Combine(actualTemplatePath, "locations.json");
            string gameWorldFile = Path.Combine(actualTemplatePath, "gameWorld.json");
            
            Assert.True(File.Exists(locationsFile), $"locations.json should exist at {locationsFile}");
            Assert.True(File.Exists(gameWorldFile), $"gameWorld.json should exist at {gameWorldFile}");
        }

        [Fact]
        public void GameWorldInitializer_WithCorrectPath_ShouldLoadSuccessfully()
        {
            // Test that GameWorldInitializer works with the correct path
            string correctContentDirectory = "Content"; // Capital C to match actual directory
            GameWorldInitializer contentLoader = new GameWorldInitializer(correctContentDirectory);
            
            // This should succeed
            GameWorld gameWorld = contentLoader.LoadGame();
            
            Assert.NotNull(gameWorld);
            Assert.NotNull(gameWorld.WorldState);
            Assert.NotNull(gameWorld.WorldState.locations);
            Assert.True(gameWorld.WorldState.locations.Count > 0);
        }

        [Fact]
        public void GameWorldInitializer_WithWrongPath_ShouldFail()
        {
            // Test that GameWorldInitializer fails with the current configured path
            string wrongContentDirectory = "content"; // lowercase c, doesn't exist from src/
            GameWorldInitializer contentLoader = new GameWorldInitializer(wrongContentDirectory);
            
            // This should throw because the path doesn't exist
            Exception exception = Record.Exception(() => contentLoader.LoadGame());
            
            Assert.NotNull(exception);
            Assert.True(exception is FileNotFoundException || exception is DirectoryNotFoundException);
        }

        [Fact]
        public void ServiceConfiguration_PathMismatch_Issue()
        {
            // This test documents the exact issue in ServiceConfiguration.cs line 7
            string configuredPath = "content"; // from ServiceConfiguration.cs line 7
            string actualPath = "Content";     // where files actually exist
            
            Assert.NotEqual(configuredPath, actualPath);
            
            // When running from src/, the configured path resolves to src/content/
            // But files are at src/Content/
            string resolvedConfiguredPath = Path.Combine(Directory.GetCurrentDirectory(), configuredPath);
            string resolvedActualPath = Path.Combine(Directory.GetCurrentDirectory(), actualPath);
            
            Assert.False(Directory.Exists(resolvedConfiguredPath));
            Assert.True(Directory.Exists(resolvedActualPath));
        }
    }
}