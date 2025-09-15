using NUnit.Framework;
using System.Collections.Generic;

[TestFixture]
public class GameRulesDrawCountTest
{
    [Test]
    public void TestListenDrawCountsLoaded()
    {
        // Arrange - Load game rules from JSON
        string jsonContent = @"{
            ""listenDrawCounts"": {
                ""Disconnected"": 3,
                ""Guarded"": 4,
                ""Neutral"": 4,
                ""Receptive"": 5,
                ""Trusting"": 5
            }
        }";

        var rules = new GameRules();

        // Act - Parse and apply rules
        GameRulesParser.ParseAndApplyRules(jsonContent, rules);

        // Assert - Verify all values are loaded correctly
        Assert.AreEqual(3, rules.ListenDrawCounts[ConnectionState.DISCONNECTED], "DISCONNECTED should draw 3 cards");
        Assert.AreEqual(4, rules.ListenDrawCounts[ConnectionState.GUARDED], "GUARDED should draw 4 cards");
        Assert.AreEqual(4, rules.ListenDrawCounts[ConnectionState.NEUTRAL], "NEUTRAL should draw 4 cards");
        Assert.AreEqual(5, rules.ListenDrawCounts[ConnectionState.RECEPTIVE], "RECEPTIVE should draw 5 cards");
        Assert.AreEqual(5, rules.ListenDrawCounts[ConnectionState.TRUSTING], "TRUSTING should draw 5 cards");
    }

    [Test]
    public void TestConversationSessionUsesConfiguredDrawCounts()
    {
        // Arrange - Set up game rules with new draw counts
        GameRules.StandardRuleset.ListenDrawCounts = new Dictionary<ConnectionState, int>
        {
            { ConnectionState.DISCONNECTED, 3 },
            { ConnectionState.GUARDED, 4 },
            { ConnectionState.NEUTRAL, 4 },
            { ConnectionState.RECEPTIVE, 5 },
            { ConnectionState.TRUSTING, 5 }
        };

        // Create a conversation session
        var session = new ConversationSession
        {
            CurrentState = ConnectionState.NEUTRAL,
            CurrentAtmosphere = AtmosphereType.None
        };

        // Act - Get draw count
        int drawCount = session.GetDrawCount();

        // Assert - Should use configured value for NEUTRAL (4 cards)
        Assert.AreEqual(4, drawCount, "NEUTRAL state should draw 4 cards based on configuration");

        // Test other states
        session.CurrentState = ConnectionState.DISCONNECTED;
        Assert.AreEqual(3, session.GetDrawCount(), "DISCONNECTED should draw 3 cards");

        session.CurrentState = ConnectionState.RECEPTIVE;
        Assert.AreEqual(5, session.GetDrawCount(), "RECEPTIVE should draw 5 cards");

        session.CurrentState = ConnectionState.TRUSTING;
        Assert.AreEqual(5, session.GetDrawCount(), "TRUSTING should draw 5 cards");
    }

    [Test]
    public void TestAtmosphereModifiersStillWork()
    {
        // Arrange - Set up game rules
        GameRules.StandardRuleset.ListenDrawCounts = new Dictionary<ConnectionState, int>
        {
            { ConnectionState.NEUTRAL, 4 }
        };

        var session = new ConversationSession
        {
            CurrentState = ConnectionState.NEUTRAL
        };

        // Test with Receptive atmosphere (+1 card)
        session.CurrentAtmosphere = AtmosphereType.Receptive;
        Assert.AreEqual(5, session.GetDrawCount(), "Receptive atmosphere should add 1 to draw count");

        // Test with Pressured atmosphere (-1 card)
        session.CurrentAtmosphere = AtmosphereType.Pressured;
        Assert.AreEqual(3, session.GetDrawCount(), "Pressured atmosphere should subtract 1 from draw count");
    }
}