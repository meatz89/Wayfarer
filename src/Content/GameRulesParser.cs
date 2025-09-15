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
            var drawCounts = new List<ListenDrawCountEntry>();

            if (listenDrawCounts.TryGetProperty("Disconnected", out JsonElement disconnected))
                drawCounts.Add(new ListenDrawCountEntry { State = ConnectionState.DISCONNECTED, DrawCount = disconnected.GetInt32() });

            if (listenDrawCounts.TryGetProperty("Guarded", out JsonElement guarded))
                drawCounts.Add(new ListenDrawCountEntry { State = ConnectionState.GUARDED, DrawCount = guarded.GetInt32() });

            if (listenDrawCounts.TryGetProperty("Neutral", out JsonElement neutral))
                drawCounts.Add(new ListenDrawCountEntry { State = ConnectionState.NEUTRAL, DrawCount = neutral.GetInt32() });

            if (listenDrawCounts.TryGetProperty("Receptive", out JsonElement receptive))
                drawCounts.Add(new ListenDrawCountEntry { State = ConnectionState.RECEPTIVE, DrawCount = receptive.GetInt32() });

            if (listenDrawCounts.TryGetProperty("Trusting", out JsonElement trusting))
                drawCounts.Add(new ListenDrawCountEntry { State = ConnectionState.TRUSTING, DrawCount = trusting.GetInt32() });

            targetRules.ListenDrawCounts = drawCounts;
        }
    }
}