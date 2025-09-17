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
            List<ListenDrawCountEntry> drawCounts = new List<ListenDrawCountEntry>();

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

        // Parse cardProgression
        if (root.TryGetProperty("cardProgression", out JsonElement cardProgression))
        {
            CardProgression progression = new CardProgression();

            // Parse XP thresholds
            if (cardProgression.TryGetProperty("xpThresholds", out JsonElement xpThresholds))
            {
                List<int> thresholds = new List<int>();
                foreach (JsonElement threshold in xpThresholds.EnumerateArray())
                {
                    thresholds.Add(threshold.GetInt32());
                }
                progression.XpThresholds = thresholds;
            }

            // Parse level bonuses
            if (cardProgression.TryGetProperty("levelBonuses", out JsonElement levelBonuses))
            {
                List<LevelBonus> bonuses = new List<LevelBonus>();
                foreach (JsonElement bonus in levelBonuses.EnumerateArray())
                {
                    LevelBonus levelBonus = new LevelBonus
                    {
                        Level = bonus.GetProperty("level").GetInt32(),
                        SuccessBonus = bonus.GetProperty("successBonus").GetInt32()
                    };

                    // Parse effects
                    if (bonus.TryGetProperty("effects", out JsonElement effects))
                    {
                        List<string> effectsList = new List<string>();
                        foreach (JsonElement effect in effects.EnumerateArray())
                        {
                            effectsList.Add(effect.GetString());
                        }
                        levelBonus.Effects = effectsList;
                    }

                    bonuses.Add(levelBonus);
                }
                progression.LevelBonuses = bonuses;
            }

            targetRules.CardProgression = progression;
        }
    }
}