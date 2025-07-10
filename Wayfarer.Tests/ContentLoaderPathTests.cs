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
            // The current configuration in ServiceConfiguration uses "Content" 
            // which resolves correctly from the test bin directory where files are copied
            string contentDirectory = "Content";
            string expectedTemplatePath = Path.Combine(contentDirectory, "Templates");
            
            // When running tests from bin directory, this should look for:
            // bin/Debug/net8.0/Content/Templates/ (files are copied by build process)
            
            Assert.True(Directory.Exists(expectedTemplatePath), 
                $"Expected path {expectedTemplatePath} should exist from test bin directory");
        }

        [Fact]
        public void ContentLoader_ActualFilesLocation_Verification()
        {
            // The files are copied to Content/Templates/ in the test bin directory by the build process
            string actualContentPath = "Content";
            string actualTemplatePath = Path.Combine(actualContentPath, "Templates");
            
            // This should exist when running from test bin directory
            Assert.True(Directory.Exists(actualTemplatePath), 
                $"Actual path {actualTemplatePath} should exist from test bin directory");
            
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
            string correctContentDirectory = "Content"; // Correct path from test bin directory
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
            // Test that GameWorldInitializer fails with a wrong path
            string wrongContentDirectory = "wrong_path"; // Path that doesn't exist
            GameWorldInitializer contentLoader = new GameWorldInitializer(wrongContentDirectory);
            
            // This should throw because the path doesn't exist
            Exception exception = Record.Exception(() => contentLoader.LoadGame());
            
            Assert.NotNull(exception);
            Assert.True(exception is FileNotFoundException || exception is DirectoryNotFoundException);
        }

        [Fact]
        public void ServiceConfiguration_PathConfiguration_IsCorrect()
        {
            // This test validates that the path configuration works correctly
            string configuredPath = "Content"; // from ServiceConfiguration.cs
            
            // When running from test bin directory, the configured path should resolve correctly
            string resolvedConfiguredPath = Path.Combine(Directory.GetCurrentDirectory(), configuredPath);
            string templatesPath = Path.Combine(resolvedConfiguredPath, "Templates");
            
            Assert.True(Directory.Exists(resolvedConfiguredPath), $"Configured path {resolvedConfiguredPath} should exist");
            Assert.True(Directory.Exists(templatesPath), $"Templates path {templatesPath} should exist");
        }
    }
}