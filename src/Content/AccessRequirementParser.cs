using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
/// <summary>
/// Parser for AccessRequirement entities from JSON.
/// </summary>
public static class AccessRequirementParser
{
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
        requirement.RequiredTokensPerNPC = dto.RequiredTokensPerNPC;

        // Parse token type requirements
        foreach ((string typeStr, int count) in dto.RequiredTokensPerType)
        {
            if (EnumParser.TryParse<ConnectionType>(typeStr, out ConnectionType tokenType))
            {
                requirement.RequiredTokensPerType[tokenType] = count;
            }
            else
            {
                Console.WriteLine($"[WARNING] Unknown token type: {typeStr}");
            }
        }

        return requirement;
    }
}