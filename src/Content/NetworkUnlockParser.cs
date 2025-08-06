using System.Collections.Generic;
using System.Text.Json;

public static class NetworkUnlockParser
{
    public static NetworkUnlock ParseNetworkUnlock(string json)
    {
        JsonDocumentOptions options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        using (JsonDocument doc = JsonDocument.Parse(json, options))
        {
            JsonElement root = doc.RootElement;

            NetworkUnlock unlock = new NetworkUnlock
            {
                Id = GetStringProperty(root, "id", ""),
                Type = GetStringProperty(root, "type", "npc_network"),
                UnlockerNpcId = GetStringProperty(root, "unlockerNpcId", ""),
                TokensRequired = GetIntProperty(root, "tokensRequired", 5),
                UnlockDescription = GetStringProperty(root, "unlockDescription", "")
            };

            // Parse unlock targets
            if (root.TryGetProperty("unlocks", out JsonElement unlocksElement) &&
                unlocksElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement targetElement in unlocksElement.EnumerateArray())
                {
                    NetworkUnlockTarget target = new NetworkUnlockTarget
                    {
                        NpcId = GetStringProperty(targetElement, "npcId", ""),
                        IntroductionText = GetStringProperty(targetElement, "introductionText", "")
                    };
                    unlock.Unlocks.Add(target);
                }
            }

            return unlock;
        }
    }

    private static string GetStringProperty(JsonElement element, string propertyName, string defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            string value = property.GetString() ?? defaultValue;
            return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
        }
        return defaultValue;
    }

    private static int GetIntProperty(JsonElement element, string propertyName, int defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.Number)
        {
            return property.GetInt32();
        }
        return defaultValue;
    }
}
