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
        if (string.IsNullOrEmpty(dto.LocationId))
            throw new InvalidOperationException($"NPC {dto.Id} missing required 'LocationId' field");

        NPC npc = new NPC
        {
            ID = dto.Id,
            Name = dto.Name,
            Role = !string.IsNullOrEmpty(dto.Role) ? dto.Role : dto.Name, // Use name as role if role not specified
            Description = dto.Description, // Description is optional
            Tier = dto.Tier,
            Level = dto.Level > 0 ? dto.Level : 1, // Default to level 1 if not specified
            ConversationDifficulty = dto.ConversationDifficulty > 0 ? dto.ConversationDifficulty : 1
        };

        // Parse profession with mapping from JSON values to enum
        if (string.IsNullOrEmpty(dto.Profession))
            throw new InvalidOperationException($"NPC {dto.Id} missing required 'Profession' field");
        npc.Profession = MapProfessionFromJson(dto.Profession);

        // Parse personality - preserve authentic description and map to categorical type
        npc.PersonalityDescription = dto.Personality; // Optional field

        // Parse personalityType directly from DTO - NO FALLBACKS
        if (!string.IsNullOrEmpty(dto.PersonalityType) && Enum.TryParse<PersonalityType>(dto.PersonalityType, true, out PersonalityType parsedType))
        {
            npc.PersonalityType = parsedType;

            // Initialize conversation modifier based on personality type
            npc.ConversationModifier = PersonalityModifier.CreateFromPersonalityType(parsedType);
        }
        else
        {
            // NO FALLBACKS - crash if personalityType not in DTO
            throw new InvalidOperationException($"NPC '{npc.Name}' (ID: {npc.ID}) is missing 'personalityType' in DTO or has invalid value '{dto.PersonalityType}' - fix DTO data");
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
            foreach (KeyValuePair<string, int> kvp in dto.InitialTokens)
            {
                npc.InitialTokenValues.Add(new InitialTokenValue
                {
                    TokenId = kvp.Key,
                    Value = kvp.Value
                });
            }
        }

        // Resolve Location object reference during parsing (HIGHLANDER: ID is parsing artifact)
        if (!string.IsNullOrEmpty(dto.LocationId))
        {
            npc.Location = gameWorld.Locations.FirstOrDefault(l => l.Id == dto.LocationId);
        }

        // NOTE: Old inline scene parsing removed - NEW Scene-Situation architecture
        // Scenes now spawn via Situation spawn rewards (SceneSpawnReward) instead of inline definitions
        // NPCs will receive Scene references through the spawning system, not direct parsing

        return npc;
    }

    private static Professions MapProfessionFromJson(string jsonProfession)
    {
        return jsonProfession switch
        {
            "Craftsman" => Professions.Craftsman,
            "Merchant" => Professions.Merchant,
            "General Merchant" => Professions.Merchant,
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
            "Warehouse Foreman" => Professions.Dock_Boss, // Manages warehouse workers and cargo
            _ => throw new ArgumentException($"Unknown profession in JSON: '{jsonProfession}' - add to profession mapping")
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