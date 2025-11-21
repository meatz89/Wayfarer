/// <summary>
/// Parser for converting EmergencySituationDTO to EmergencySituation domain model
/// </summary>
public static class EmergencyParser
{
    /// <summary>
    /// Convert EmergencySituationDTO to EmergencySituation entity
    /// </summary>
    public static EmergencySituation Parse(EmergencySituationDTO dto, GameWorld gameWorld)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException("EmergencySituation missing required 'Id' field");

        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidOperationException($"EmergencySituation '{dto.Id}' missing required 'Name' field");

        if (dto.Responses == null || dto.Responses.Count == 0)
            throw new InvalidOperationException($"EmergencySituation '{dto.Id}' must have at least one response option");

        if (dto.ResponseWindowSegments < 0)
            throw new InvalidOperationException($"EmergencySituation '{dto.Id}' has invalid ResponseWindowSegments '{dto.ResponseWindowSegments}'. Must be >= 0.");

        // Verify trigger locations exist if specified
        // NOTE: Location.Id removed per ADR-007 (categorical architecture)
        // Validation removed - locations matched categorically at runtime
        if (dto.TriggerLocationIds != null && dto.TriggerLocationIds.Count > 0)
        {
            // TriggerLocationIds will be validated at runtime via categorical matching
            // Cannot validate here without Location.Id
        }

        EmergencySituation emergency = new EmergencySituation
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description ?? "",
            TriggerDay = dto.TriggerDay,
            TriggerSegment = dto.TriggerSegment,
            ResponseWindowSegments = dto.ResponseWindowSegments,
            IsTriggered = dto.IsTriggered,
            IsResolved = dto.IsResolved,
            TriggeredAtSegment = dto.TriggeredAtSegment
        };

        // Parse response options
        foreach (EmergencyResponseDTO responseDto in dto.Responses)
        {
            EmergencyResponse response = ParseResponse(responseDto, dto.Id, gameWorld);
            emergency.Responses.Add(response);
        }

        // Parse ignore outcome if present
        if (dto.IgnoreOutcome != null)
        {
            emergency.IgnoreOutcome = ParseOutcome(dto.IgnoreOutcome, dto.Id, gameWorld);
        }
        else
        {
            // Create default ignore outcome with penalty
            emergency.IgnoreOutcome = new EmergencyOutcome
            {
                RelationshipDelta = -10,
                NarrativeResult = "You chose not to respond. The situation resolved without your involvement."
            };
        }

        return emergency;
    }

    /// <summary>
    /// Parse an emergency response option from DTO
    /// </summary>
    private static EmergencyResponse ParseResponse(EmergencyResponseDTO dto, string emergencyId, GameWorld gameWorld)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException($"EmergencyResponse in emergency '{emergencyId}' missing required 'Id' field");

        if (string.IsNullOrEmpty(dto.ResponseText))
            throw new InvalidOperationException($"EmergencyResponse '{dto.Id}' in emergency '{emergencyId}' missing required 'ResponseText' field");

        // Validate costs are non-negative
        if (dto.StaminaCost < 0)
            throw new InvalidOperationException($"EmergencyResponse '{dto.Id}' has invalid StaminaCost '{dto.StaminaCost}'. Must be >= 0.");

        if (dto.HealthCost < 0)
            throw new InvalidOperationException($"EmergencyResponse '{dto.Id}' has invalid HealthCost '{dto.HealthCost}'. Must be >= 0.");

        if (dto.CoinCost < 0)
            throw new InvalidOperationException($"EmergencyResponse '{dto.Id}' has invalid CoinCost '{dto.CoinCost}'. Must be >= 0.");

        if (dto.TimeCost < 0)
            throw new InvalidOperationException($"EmergencyResponse '{dto.Id}' has invalid TimeCost '{dto.TimeCost}'. Must be >= 0.");

        EmergencyResponse response = new EmergencyResponse
        {
            Id = dto.Id,
            ResponseText = dto.ResponseText,
            Description = dto.Description ?? "",
            StaminaCost = dto.StaminaCost,
            HealthCost = dto.HealthCost,
            CoinCost = dto.CoinCost,
            TimeCost = dto.TimeCost
        };

        // Parse outcome if present
        if (dto.Outcome != null)
        {
            response.Outcome = ParseOutcome(dto.Outcome, emergencyId, gameWorld);
        }
        else
        {
            // Create empty outcome
            response.Outcome = new EmergencyOutcome();
        }

        return response;
    }

    /// <summary>
    /// Parse an emergency outcome from DTO
    /// </summary>
    private static EmergencyOutcome ParseOutcome(EmergencyOutcomeDTO dto, string emergencyId, GameWorld gameWorld)
    {
        if (dto == null)
            return new EmergencyOutcome();

        EmergencyOutcome outcome = new EmergencyOutcome
        {
            RelationshipDelta = dto.RelationshipDelta,
            NPCRelationshipDeltas = ParseNPCRelationshipDeltas(dto.NPCRelationshipDeltas, gameWorld),
            GrantedKnowledge = dto.GrantedKnowledge ?? new List<string>(),
            CoinReward = dto.CoinReward,
            NarrativeResult = dto.NarrativeResult ?? ""
        };

        return outcome;
    }

    /// <summary>
    /// Parse NPC relationship deltas from string dictionary to NPC object dictionary
    /// </summary>
    private static Dictionary<NPC, int> ParseNPCRelationshipDeltas(Dictionary<string, int> npcDeltas, GameWorld gameWorld)
    {
        if (npcDeltas == null || !npcDeltas.Any())
            return new Dictionary<NPC, int>();

        Dictionary<NPC, int> result = new Dictionary<NPC, int>();
        foreach (KeyValuePair<string, int> entry in npcDeltas)
        {
            NPC npc = gameWorld.NPCs.FirstOrDefault(n => n.Name == entry.Key);
            if (npc == null)
            {
                Console.WriteLine($"[EmergencyParser.ParseNPCRelationshipDeltas] WARNING: NPC '{entry.Key}' not found");
                continue; // Skip invalid NPC
            }

            result[npc] = entry.Value; // Object reference key, not string
        }

        return result;
    }
}
