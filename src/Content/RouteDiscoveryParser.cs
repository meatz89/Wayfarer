using System;
using System.Collections.Generic;
using System.Text.Json;

public class RouteDiscoveryParser
{
    public List<RouteDiscovery> Parse(string json)
    {
        List<RouteDiscovery> discoveries = new List<RouteDiscovery>();

        try
        {
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                foreach (JsonElement element in doc.RootElement.EnumerateArray())
                {
                    RouteDiscovery discovery = new RouteDiscovery
                    {
                        RouteId = element.GetProperty("routeId").GetString() ?? "",
                        RequiredTokensWithNPC = element.GetProperty("requiredTokensWithNPC").GetInt32()
                    };

                    // Parse known by NPCs
                    if (element.TryGetProperty("knownByNPCs", out JsonElement npcArray))
                    {
                        foreach (JsonElement npcId in npcArray.EnumerateArray())
                        {
                            discovery.KnownByNPCs.Add(npcId.GetString() ?? "");
                        }
                    }

                    // Parse discovery contexts for each NPC
                    if (element.TryGetProperty("discoveryContexts", out JsonElement contextsElement))
                    {
                        foreach (JsonProperty contextProp in contextsElement.EnumerateObject())
                        {
                            string npcId = contextProp.Name;
                            JsonElement contextElement = contextProp.Value;

                            RouteDiscoveryContext context = new RouteDiscoveryContext
                            {
                                Narrative = contextElement.GetProperty("narrative").GetString() ?? ""
                            };

                            // Parse required equipment for this context
                            if (contextElement.TryGetProperty("requiredEquipment", out JsonElement equipArray))
                            {
                                foreach (JsonElement equipId in equipArray.EnumerateArray())
                                {
                                    context.RequiredEquipment.Add(equipId.GetString() ?? "");
                                }
                            }

                            discovery.DiscoveryContexts[npcId] = context;
                        }
                    }

                    discoveries.Add(discovery);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to parse route discoveries: {ex.Message}", ex);
        }

        return discoveries;
    }
}
