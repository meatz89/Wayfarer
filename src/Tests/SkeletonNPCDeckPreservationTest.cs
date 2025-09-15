using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Test to verify that skeleton NPC deck preservation works correctly during skeleton replacement
/// </summary>
public static class SkeletonNPCDeckPreservationTest
{
    public static void RunTest()
    {
        Console.WriteLine("=== Testing Skeleton NPC Deck Preservation ===");

        // Create a test game world
        GameWorld gameWorld = new GameWorld();
        PackageLoader loader = new PackageLoader(gameWorld);

        // First, create a skeleton NPC (simulating AI referencing a non-existent NPC)
        NPC skeletonNpc = SkeletonGenerator.GenerateSkeletonNPC("test_npc", "test_source");

        // Verify the skeleton has the basic structure
        Console.WriteLine($"Created skeleton NPC: {skeletonNpc.Name} (ID: {skeletonNpc.ID})");
        Console.WriteLine($"IsSkeleton: {skeletonNpc.IsSkeleton}");
        Console.WriteLine($"SkeletonSource: {skeletonNpc.SkeletonSource}");

        // Add some test cards to the skeleton's persistent decks (simulating gameplay progress)
        var testObservationCard = new ConversationCard
        {
            Id = "test_observation",
            CardType = CardType.Observation,
            Text = "Test observation card"
        };
        var testBurdenCard = new ConversationCard
        {
            Id = "test_burden",
            CardType = CardType.BurdenGoal,
            Text = "Test burden card"
        };
        var testExchangeCard = new ExchangeCard
        {
            Id = "test_exchange",
            Name = "Test Exchange",
            Description = "Test exchange card"
        };

        skeletonNpc.ObservationDeck.AddCard(testObservationCard);
        skeletonNpc.BurdenDeck.AddCard(testBurdenCard);
        skeletonNpc.ExchangeDeck.Add(testExchangeCard);

        Console.WriteLine($"Added test cards to skeleton:");
        Console.WriteLine($"  - Observation deck: {skeletonNpc.ObservationDeck.GetAllCards().Count()} cards");
        Console.WriteLine($"  - Burden deck: {skeletonNpc.BurdenDeck.GetAllCards().Count()} cards");
        Console.WriteLine($"  - Exchange deck: {skeletonNpc.ExchangeDeck.Count} cards");

        // Add skeleton to game world
        gameWorld.NPCs.Add(skeletonNpc);
        gameWorld.WorldState.NPCs.Add(skeletonNpc);
        gameWorld.SkeletonRegistry[skeletonNpc.ID] = "NPC";

        // Create a package with real NPC content that should replace the skeleton
        string realNpcPackageJson = @"{
            ""packageId"": ""real_npc_package"",
            ""metadata"": {
                ""name"": ""Real NPC Package"",
                ""version"": ""1.0.0""
            },
            ""content"": {
                ""npcs"": [
                    {
                        ""id"": ""test_npc"",
                        ""name"": ""Real Test NPC"",
                        ""description"": ""A real NPC that replaces the skeleton"",
                        ""role"": ""Merchant"",
                        ""personalityType"": ""MERCANTILE"",
                        ""profession"": ""Merchant"",
                        ""tier"": 2,
                        ""locationId"": ""test_location"",
                        ""spotId"": ""test_spot""
                    }
                ]
            }
        }";

        // Load the package which should replace the skeleton with real content
        bool success = loader.LoadDynamicPackageFromJson(realNpcPackageJson, "real_npc_package");
        Console.WriteLine($"Real NPC package load: {(success ? "SUCCESS" : "FAILED")}");

        // Verify skeleton was replaced
        var replacedNpc = gameWorld.NPCs.FirstOrDefault(n => n.ID == "test_npc");
        if (replacedNpc != null)
        {
            Console.WriteLine($"Found replaced NPC: {replacedNpc.Name} (ID: {replacedNpc.ID})");
            Console.WriteLine($"IsSkeleton: {replacedNpc.IsSkeleton}");

            // Check if cards were preserved
            var preservedObservationCards = replacedNpc.ObservationDeck.GetAllCards().Count();
            var preservedBurdenCards = replacedNpc.BurdenDeck.GetAllCards().Count();
            var preservedExchangeCards = replacedNpc.ExchangeDeck.Count;

            Console.WriteLine($"Preserved cards after replacement:");
            Console.WriteLine($"  - Observation deck: {preservedObservationCards} cards");
            Console.WriteLine($"  - Burden deck: {preservedBurdenCards} cards");
            Console.WriteLine($"  - Exchange deck: {preservedExchangeCards} cards");

            // Verify specific cards were preserved
            bool observationPreserved = replacedNpc.ObservationDeck.GetAllCards().Any(c => c.Id == "test_observation");
            bool burdenPreserved = replacedNpc.BurdenDeck.GetAllCards().Any(c => c.Id == "test_burden");
            bool exchangePreserved = replacedNpc.ExchangeDeck.Any(c => c.Id == "test_exchange");

            Console.WriteLine($"Specific card preservation:");
            Console.WriteLine($"  - Test observation card: {(observationPreserved ? "PRESERVED" : "LOST")}");
            Console.WriteLine($"  - Test burden card: {(burdenPreserved ? "PRESERVED" : "LOST")}");
            Console.WriteLine($"  - Test exchange card: {(exchangePreserved ? "PRESERVED" : "LOST")}");

            // Check that skeleton was removed from registry
            bool skeletonRemoved = !gameWorld.SkeletonRegistry.ContainsKey("test_npc");
            Console.WriteLine($"Skeleton registry cleanup: {(skeletonRemoved ? "SUCCESS" : "FAILED")}");

            // Final test result
            bool testPassed = !replacedNpc.IsSkeleton &&
                             observationPreserved &&
                             burdenPreserved &&
                             exchangePreserved &&
                             skeletonRemoved;

            Console.WriteLine($"Overall test result: {(testPassed ? "PASSED" : "FAILED")}");
        }
        else
        {
            Console.WriteLine("FAILED: No NPC found with test_npc ID after replacement");
        }

        Console.WriteLine("=== Test Complete ===");
    }
}