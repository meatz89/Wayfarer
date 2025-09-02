using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Parser for AccessRequirement entities from JSON.
/// </summary>
public static class AccessRequirementParser
{
    /// <summary>
    /// Convert an AccessRequirementDTO to an AccessRequirement domain model
    /// </summary>
    public static AccessRequirement ConvertDTOToAccessRequirement(AccessRequirementDTO dto)
    {
        if (dto == null)
            return null;

        AccessRequirement requirement = new AccessRequirement
        {
            Id = dto.Id ?? Guid.NewGuid().ToString(),
            Name = dto.Name ?? string.Empty,
            BlockedMessage = dto.BlockedMessage ?? "You cannot access this area.",
            HintMessage = dto.HintMessage ?? string.Empty
        };

        // Parse logic
        if (EnumParser.TryParse<RequirementLogic>(dto.Logic, out RequirementLogic logic))
        {
            requirement.Logic = logic;
        }

        // Parse equipment requirements
        foreach (string equipStr in dto.RequiredEquipment)
        {
            if (EnumParser.TryParse<ItemCategory>(equipStr, out ItemCategory category))
            {
                requirement.RequiredEquipment.Add(category);
            }
            else
            {
                Console.WriteLine($"[WARNING] Unknown equipment category: {equipStr}");
            }
        }

        // Parse item requirements
        requirement.RequiredItemIds = dto.RequiredItemIds;

        // Parse NPC token requirements
        if (dto.RequiredTokensPerNPC != null)
        {
            foreach (KeyValuePair<string, int> kvp in dto.RequiredTokensPerNPC)
            {
                requirement.RequiredTokensPerNPC.Add(new TokenRequirement
                {
                    NPCId = kvp.Key,
                    MinimumCount = kvp.Value
                });
            }
        }

        // Parse token type requirements
        if (dto.RequiredTokensPerType != null)
        {
            foreach (KeyValuePair<string, int> kvp in dto.RequiredTokensPerType)
            {
                if (EnumParser.TryParse<ConnectionType>(kvp.Key, out ConnectionType tokenType))
                {
                    requirement.RequiredTokensPerType.Add(new TokenTypeRequirement
                    {
                        TokenType = tokenType,
                        MinimumCount = kvp.Value
                    });
                }
                else
                {
                    Console.WriteLine($"[WARNING] Unknown token type: {kvp.Key}");
                }
            }
        }

        return requirement;
    }
    public static AccessRequirement ParseAccessRequirement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
            return null;

        AccessRequirementDTO? dto = JsonSerializer.Deserialize<AccessRequirementDTO>(element.GetRawText());
        if (dto == null)
            return null;

        AccessRequirement requirement = new AccessRequirement
        {
            Id = dto.Id ?? Guid.NewGuid().ToString(),
            Name = dto.Name ?? string.Empty,
            BlockedMessage = dto.BlockedMessage ?? "You cannot access this area.",
            HintMessage = dto.HintMessage ?? string.Empty
        };

        // Parse logic
        if (EnumParser.TryParse<RequirementLogic>(dto.Logic, out RequirementLogic logic))
        {
            requirement.Logic = logic;
        }

        // Parse equipment requirements
        foreach (string equipStr in dto.RequiredEquipment)
        {
            if (EnumParser.TryParse<ItemCategory>(equipStr, out ItemCategory category))
            {
                requirement.RequiredEquipment.Add(category);
            }
            else
            {
                Console.WriteLine($"[WARNING] Unknown equipment category: {equipStr}");
            }
        }

        // Parse item requirements
        requirement.RequiredItemIds = dto.RequiredItemIds;

        // Parse NPC token requirements
        if (dto.RequiredTokensPerNPC != null)
        {
            foreach (KeyValuePair<string, int> kvp in dto.RequiredTokensPerNPC)
            {
                requirement.RequiredTokensPerNPC.Add(new TokenRequirement
                {
                    NPCId = kvp.Key,
                    MinimumCount = kvp.Value
                });
            }
        }

        // Parse token type requirements
        if (dto.RequiredTokensPerType != null)
        {
            foreach (KeyValuePair<string, int> kvp in dto.RequiredTokensPerType)
            {
                if (EnumParser.TryParse<ConnectionType>(kvp.Key, out ConnectionType tokenType))
                {
                    requirement.RequiredTokensPerType.Add(new TokenTypeRequirement
                    {
                        TokenType = tokenType,
                        MinimumCount = kvp.Value
                    });
                }
                else
                {
                    Console.WriteLine($"[WARNING] Unknown token type: {kvp.Key}");
                }
            }
        }

        return requirement;
    }
}