using System;
using System.Collections.Generic;
using System.Text.Json;

public static class NPCParser
{
    /// <summary>
    /// Convert an NPCDTO to an NPC domain model
    /// </summary>
    public static NPC ConvertDTOToNPC(NPCDTO dto, GameWorld gameWorld)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException("NPC DTO missing required 'Id' field");
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"NPC {dto.Id} missing required 'Name' field");
        if (string.IsNullOrEmpty(dto.VenueId))
            throw new InvalidOperationException($"NPC {dto.Id} missing required 'VenueId' field");
        if (string.IsNullOrEmpty(dto.LocationId))
            throw new InvalidOperationException($"NPC {dto.Id} missing required 'LocationId' field");

        NPC npc = new NPC
        {
            ID = dto.Id,
            Name = dto.Name,
            Role = !string.IsNullOrEmpty(dto.Role) ? dto.Role : dto.Name, // Use name as role if role not specified
            Description = dto.Description, // Description is optional
            Venue = dto.VenueId,
            LocationId = dto.LocationId,
            Tier = dto.Tier,
            Level = dto.Level > 0 ? dto.Level : 1, // Default to level 1 if not specified
            ConversationDifficulty = dto.ConversationDifficulty > 0 ? dto.ConversationDifficulty : 1
        };

        Console.WriteLine($"[DEBUG] NPCParser: Parsing NPC {npc.ID} with venueId: '{dto.VenueId}'");

        // Parse profession with mapping from JSON values to enum
        if (string.IsNullOrEmpty(dto.Profession))
            throw new InvalidOperationException($"NPC {dto.Id} missing required 'Profession' field");
        npc.Profession = MapProfessionFromJson(dto.Profession);

        // Parse personality - preserve authentic description and map to categorical type
        npc.PersonalityDescription = dto.Personality; // Optional field

        // Parse personalityType directly from DTO - NO FALLBACKS
        Console.WriteLine($"[NPCParser] Parsing NPC '{npc.Name}' - personalityType from DTO: '{dto.PersonalityType}'");

        if (!string.IsNullOrEmpty(dto.PersonalityType) && Enum.TryParse<PersonalityType>(dto.PersonalityType, true, out PersonalityType parsedType))
        {
            npc.PersonalityType = parsedType;
            Console.WriteLine($"[NPCParser] Successfully parsed PersonalityType: {parsedType} for {npc.Name}");

            // Initialize conversation modifier based on personality type
            npc.ConversationModifier = PersonalityModifier.CreateFromPersonalityType(parsedType);
        }
        else
        {
            // NO FALLBACKS - crash if personalityType not in DTO
            throw new InvalidOperationException($"NPC '{npc.Name}' (ID: {npc.ID}) is missing 'personalityType' in DTO or has invalid value '{dto.PersonalityType}' - fix DTO data");
        }

        // Parse services and map to ServiceTypes enum
        if (dto.Services != null)
        {
            foreach (string serviceStr in dto.Services)
            {
                ServiceTypes? mappedService = MapServiceFromJson(serviceStr);
                if (mappedService.HasValue)
                {
                    npc.ProvidedServices.Add(mappedService.Value);
                }
            }
        }

        // Set default player relationship
        npc.PlayerRelationship = NPCRelationship.Neutral;

        // Parse initial connection state and convert to flow value
        if (string.IsNullOrEmpty(dto.CurrentState))
            throw new InvalidOperationException($"NPC {dto.Id} missing required 'CurrentState' field");
        string currentStateStr = dto.CurrentState;
        if (Enum.TryParse<ConnectionState>(currentStateStr, true, out ConnectionState connectionState))
        {
            // Convert initial state to flow value (start at neutral position within state)
            npc.RelationshipFlow = connectionState switch
            {
                ConnectionState.DISCONNECTED => 2,  // Position 2 (neutral) in DISCONNECTED range
                ConnectionState.GUARDED => 7,       // Position 2 (neutral) in GUARDED range  
                ConnectionState.NEUTRAL => 12,      // Position 2 (neutral) in NEUTRAL range
                ConnectionState.RECEPTIVE => 17,    // Position 2 (neutral) in RECEPTIVE range
                ConnectionState.TRUSTING => 22,     // Position 2 (neutral) in TRUSTING range
                _ => throw new InvalidOperationException($"Unhandled ConnectionState: {connectionState}")
            };
        }
        else
        {
            throw new InvalidOperationException($"NPC {dto.Id} has invalid CurrentState value: '{currentStateStr}'");
        }

        // Observation deck will be populated from deck compositions if NPC has observation cards
        // The deck initialization happens in PackageLoader when processing deckCompositions

        // Parse initial tokens if specified
        if (dto.InitialTokens != null && dto.InitialTokens.Count > 0)
        {
            // These would be set during game initialization after player is created
            // For now, store the values to be applied later
            npc.InitialTokenValues = new Dictionary<string, int>(dto.InitialTokens);
        }

        // Parse obstacles for this NPC (Social barriers only)
        if (dto.Obstacles != null && dto.Obstacles.Count > 0)
        {
            foreach (ObstacleDTO obstacleDto in dto.Obstacles)
            {
                Obstacle obstacle = ObstacleParser.ConvertDTOToObstacle(obstacleDto, npc.ID, gameWorld);

                // Validate: NPCs can ONLY have SocialDifficulty obstacles
                if (obstacle.PhysicalDanger > 0 || obstacle.MentalComplexity > 0 ||
                    obstacle.StaminaCost > 0 || obstacle.TimeCost > 0)
                {
                    throw new InvalidOperationException(
                        $"NPC '{npc.Name}' (ID: {npc.ID}) has obstacle '{obstacle.Name}' with non-social properties. " +
                        $"NPCs can ONLY have SocialDifficulty obstacles. " +
                        $"Found: PhysicalDanger={obstacle.PhysicalDanger}, MentalComplexity={obstacle.MentalComplexity}, " +
                        $"StaminaCost={obstacle.StaminaCost}, TimeCost={obstacle.TimeCost}");
                }

                npc.Obstacles.Add(obstacle);
            }
            Console.WriteLine($"[NPCParser] Parsed {npc.Obstacles.Count} social obstacles for NPC '{npc.Name}'");
        }

        return npc;
    }

    private static Professions MapProfessionFromJson(string jsonProfession)
    {
        return jsonProfession switch
        {
            "Craftsman" => Professions.Craftsman,
            "Merchant" => Professions.Merchant,
            "Innkeeper" => Professions.Innkeeper,
            "Soldier" => Professions.Soldier,
            "Guard" => Professions.Soldier, // Map Guard to Soldier
            "Scholar" => Professions.Scholar,
            "Scribe" => Professions.Scribe,
            "Clerk" => Professions.Scribe, // Map Clerk to Scribe
            "Noble" => Professions.Noble,
            "Smuggler" => Professions.Agent,
            "Information_Broker" => Professions.Information_Broker,
            "Miller's Daughter" => Professions.Craftsman,
            "Village Elder" => Professions.Noble,
            "Farmer" => Professions.Craftsman,
            _ => throw new ArgumentException($"Unknown profession in JSON: '{jsonProfession}' - add to profession mapping")
        };
    }



    private static ServiceTypes? MapServiceFromJson(string jsonService)
    {
        return jsonService switch
        {
            "Trade" => ServiceTypes.Trade,
            "Work" => ServiceTypes.Work,
            "Information" => ServiceTypes.Information,
            "Lodging" => ServiceTypes.Rest,
            "equipment_commissioning" => ServiceTypes.EquipmentRepair,
            "workshop_contracts" => ServiceTypes.Training,
            "trade_goods" => ServiceTypes.Trade,
            "delivery_contracts" => ServiceTypes.Trading,
            "rest_services" => ServiceTypes.Rest,
            "labor_contracts" => ServiceTypes.Training,
            "lumber_sales" => ServiceTypes.Trade,
            "logging_contracts" => ServiceTypes.Training,
            "herb_sales" => ServiceTypes.Trade,
            "gathering_contracts" => ServiceTypes.Training,
            "heavy_labor" => ServiceTypes.Training,
            "equipment_repair" => ServiceTypes.EquipmentRepair,
            "fish_sales" => ServiceTypes.Trade,
            "dock_work" => ServiceTypes.Training,
            "transport_contracts" => ServiceTypes.Trading,
            "simple_labor" => ServiceTypes.Training,
            "boat_maintenance" => ServiceTypes.EquipmentRepair,
            _ => null // Unknown service
        };
    }

    private static ConnectionType? ParseConnectionType(string connectionTypeStr)
    {
        return connectionTypeStr.ToLower() switch
        {
            "trust" => ConnectionType.Trust,
            "diplomacy" => ConnectionType.Diplomacy,
            "trade" => ConnectionType.Diplomacy,
            "status" => ConnectionType.Status,
            "noble" => ConnectionType.Status,
            "common" => ConnectionType.Trust,
            "shadow" => ConnectionType.Shadow,
            _ => null
        };
    }
}