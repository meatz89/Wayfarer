using System;
using System.Collections.Generic;
using System.Text.Json;

public class GameRulesParser
{
    public static void ParseAndApplyRules(string jsonContent, GameRules targetRules)
    {
        using JsonDocument document = JsonDocument.Parse(jsonContent);
        JsonElement root = document.RootElement;

        // Parse listenDrawCounts
        if (root.TryGetProperty("listenDrawCounts", out JsonElement listenDrawCounts))
        {
            var drawCounts = new Dictionary<ConnectionState, int>();

            if (listenDrawCounts.TryGetProperty("Disconnected", out JsonElement disconnected))
                drawCounts[ConnectionState.DISCONNECTED] = disconnected.GetInt32();

            if (listenDrawCounts.TryGetProperty("Guarded", out JsonElement guarded))
                drawCounts[ConnectionState.GUARDED] = guarded.GetInt32();

            if (listenDrawCounts.TryGetProperty("Neutral", out JsonElement neutral))
                drawCounts[ConnectionState.NEUTRAL] = neutral.GetInt32();

            if (listenDrawCounts.TryGetProperty("Receptive", out JsonElement receptive))
                drawCounts[ConnectionState.RECEPTIVE] = receptive.GetInt32();

            if (listenDrawCounts.TryGetProperty("Trusting", out JsonElement trusting))
                drawCounts[ConnectionState.TRUSTING] = trusting.GetInt32();

            targetRules.ListenDrawCounts = drawCounts;
        }
    }
}