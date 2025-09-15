using System;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Simple test to verify two-phase package loading is working
/// </summary>
public class TestPackageLoader
{
    public static void TestTwoPhaseLoading()
    {
        Console.WriteLine("=== Testing Two-Phase Package Loading ===");

        // Create a test game world
        GameWorld gameWorld = new GameWorld();
        PackageLoader loader = new PackageLoader(gameWorld);

        // Test parsing a simple package
        string testPackageJson = @"{
            ""packageId"": ""test_package"",
            ""metadata"": {
                ""name"": ""Test Package"",
                ""version"": ""1.0.0""
            },
            ""content"": {
                ""regions"": [
                    {
                        ""id"": ""test_region"",
                        ""name"": ""Test Region"",
                        ""description"": ""A test region""
                    }
                ],
                ""items"": [
                    {
                        ""id"": ""test_item"",
                        ""name"": ""Test Item"",
                        ""type"": ""Consumable"",
                        ""description"": ""A test item""
                    }
                ]
            }
        }";

        // Test dynamic loading
        bool success = loader.LoadDynamicPackageFromJson(testPackageJson, "test_package");
        Console.WriteLine($"Dynamic package load: {(success ? "SUCCESS" : "FAILED")}");

        // Verify entities were loaded
        Console.WriteLine($"Regions loaded: {gameWorld.WorldState.Regions.Count}");
        Console.WriteLine($"Items loaded: {gameWorld.WorldState.Items.Count}");

        // Test duplicate prevention
        bool duplicateSuccess = loader.LoadDynamicPackageFromJson(testPackageJson, "test_package");
        Console.WriteLine($"Duplicate prevention: {(!duplicateSuccess ? "SUCCESS (blocked)" : "FAILED (not blocked)")}");

        // Get loaded packages
        var loadedPackages = loader.GetLoadedPackages();
        Console.WriteLine($"Total packages loaded: {loadedPackages.Count}");
        foreach (var pkg in loadedPackages)
        {
            Console.WriteLine($"  - {pkg.PackageId}: {pkg.EntityCounts.TotalEntities} entities");
        }

        Console.WriteLine("=== Test Complete ===");
    }
}