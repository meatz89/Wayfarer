using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Phase 2: Load entities that depend on locations being already loaded.
/// This includes: LocationSpots, NPCs (which reference locations)
/// </summary>
public class Phase2_LocationDependents : IInitializationPhase
{
    public int PhaseNumber => 2;
    public string Name => "Location-Dependent Entities";
    public bool IsCritical => true;

    public void Execute(InitializationContext context)
    {
        // 1. Load LocationSpots (depends on Locations)
        LoadLocationSpots(context);

        // 2. Load NPCs (depends on Locations and optionally Spots)
        LoadNPCs(context);
    }

    private void LoadLocationSpots(InitializationContext context)
    {
        string spotsPath = Path.Combine(context.ContentPath, "location_spots.json");

        if (!File.Exists(spotsPath))
        {
            context.Errors.Add("location_spots.json not found - this is required for player initialization");
            return;
        }

        try
        {
            List<LocationSpotDTO> spotDTOs = context.ContentLoader.LoadValidatedContent<List<LocationSpotDTO>>(spotsPath);

            if (spotDTOs == null || !spotDTOs.Any())
            {
                context.Errors.Add("No location spots found in location_spots.json");
                return;
            }

            LocationSpotFactory spotFactory = new LocationSpotFactory();
            List<Location> locations = context.GameWorld.WorldState.locations;

            foreach (LocationSpotDTO dto in spotDTOs)
            {
                try
                {
                    // Verify location exists
                    Location? location = locations.FirstOrDefault(l => l.Id == dto.LocationId);
                    if (location == null)
                    {
                        context.Warnings.Add($"LocationSpot {dto.Id} references unknown location {dto.LocationId}");
                        continue;
                    }

                    // Parse time blocks
                    List<TimeBlocks> timeBlocks = new List<TimeBlocks>();
                    foreach (string timeStr in dto.CurrentTimeBlocks ?? new List<string>())
                    {
                        if (Enum.TryParse<TimeBlocks>(timeStr, true, out TimeBlocks timeBlock))
                        {
                            timeBlocks.Add(timeBlock);
                        }
                        else
                        {
                            context.Warnings.Add($"Invalid time block '{timeStr}' for spot {dto.Id}");
                        }
                    }

                    // Create spot using the factory (type parameter removed)
                    LocationSpot spot = spotFactory.CreateLocationSpotFromIds(
                        dto.Id,
                        dto.Name,
                        dto.LocationId,
                        locations, // Pass available locations
                        dto.InitialState,
                        timeBlocks,
                        dto.DomainTags?.ToList() ?? new List<string>()
                    );

                    // Parse and add spot properties from DTO
                    if (dto.SpotProperties != null && dto.SpotProperties.Any())
                    {
                        foreach (string propStr in dto.SpotProperties)
                        {
                            if (Enum.TryParse<SpotPropertyType>(propStr, true, out SpotPropertyType prop))
                            {
                                spot.SpotProperties.Add(prop);
                            }
                            else
                            {
                                context.Warnings.Add($"Invalid spot property '{propStr}' for spot {dto.Id}");
                            }
                        }
                    }

                    // Parse and add time-specific properties from DTO
                    if (dto.TimeSpecificProperties != null && dto.TimeSpecificProperties.Any())
                    {
                        foreach (KeyValuePair<string, List<string>> kvp in dto.TimeSpecificProperties)
                        {
                            if (Enum.TryParse<TimeBlocks>(kvp.Key, true, out TimeBlocks timeBlock))
                            {
                                List<SpotPropertyType> properties = new List<SpotPropertyType>();
                                foreach (string propStr in kvp.Value)
                                {
                                    if (Enum.TryParse<SpotPropertyType>(propStr, true, out SpotPropertyType prop))
                                    {
                                        properties.Add(prop);
                                    }
                                    else
                                    {
                                        context.Warnings.Add($"Invalid time-specific property '{propStr}' for spot {dto.Id} at {kvp.Key}");
                                    }
                                }
                                if (properties.Any())
                                {
                                    spot.TimeSpecificProperties[timeBlock] = properties;
                                }
                            }
                            else
                            {
                                context.Warnings.Add($"Invalid time block '{kvp.Key}' for time-specific properties in spot {dto.Id}");
                            }
                        }
                    }

                    context.GameWorld.WorldState.locationSpots.Add(spot);

                    // Also add spot to the location's AvailableSpots list
                    location.AvailableSpots.Add(spot);

                    Console.WriteLine($"  Loaded spot: {spot.SpotID} at {spot.LocationId}");
                }
                catch (Exception ex)
                {
                    context.Warnings.Add($"Failed to create location spot {dto.Id}: {ex.Message}");
                }
            }

            Console.WriteLine($"Loaded {context.GameWorld.WorldState.locationSpots.Count} location spots");
        }
        catch (ContentValidationException ex)
        {
            foreach (ValidationError error in ex.Errors)
            {
                context.Errors.Add($"LocationSpot validation: {error.Message}");
            }
        }
        catch (Exception ex)
        {
            context.Errors.Add($"Failed to load location spots: {ex.Message}");
        }
    }

    private void LoadNPCs(InitializationContext context)
    {
        string npcsPath = Path.Combine(context.ContentPath, "npcs.json");

        if (!File.Exists(npcsPath))
        {
            // NO FALLBACKS - crash if NPCs file is missing
            throw new FileNotFoundException($"npcs.json is required but not found at {npcsPath}");
        }

        try
        {
            List<NPCDTO> npcDTOs = context.ContentLoader.LoadValidatedContent<List<NPCDTO>>(npcsPath);

            if (npcDTOs == null || !npcDTOs.Any())
            {
                // NO FALLBACKS - crash if no NPCs found
                throw new InvalidOperationException("npcs.json exists but contains no NPCs - NPCs are required for game initialization");
            }

            List<Location> locations = context.GameWorld.WorldState.locations;

            foreach (NPCDTO dto in npcDTOs)
            {
                try
                {
                    // Verify location exists - CRASH if invalid
                    Location? location = locations.FirstOrDefault(l => l.Id == dto.LocationId);
                    if (location == null)
                    {
                        throw new InvalidOperationException($"NPC {dto.Id} references unknown location {dto.LocationId} - fix npcs.json");
                    }

                    // Convert DTO to JSON string for NPCParser
                    string npcJson = System.Text.Json.JsonSerializer.Serialize(dto, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    });

                    Console.WriteLine($"[LoadNPCs] Parsing NPC {dto.Id} with JSON: {npcJson}");

                    // Use NPCParser to create NPC - will crash if PersonalityType missing
                    NPC npc = NPCParser.ParseNPC(npcJson);

                    // Validate parsed NPC has valid location reference
                    if (npc.Location != dto.LocationId)
                    {
                        throw new InvalidOperationException($"NPCParser returned NPC with mismatched location: expected {dto.LocationId}, got {npc.Location}");
                    }

                    // Add to both collections
                    context.GameWorld.WorldState.NPCs.Add(npc);
                    context.GameWorld.NPCs.Add(npc); // Also add to GameWorld.NPCs for conversation system

                    Console.WriteLine($"  Loaded NPC: {npc.Name} ({npc.ID}) at {npc.Location} with PersonalityType: {npc.PersonalityType}");
                }
                catch (Exception ex)
                {
                    // DON'T CATCH - let individual NPC failures crash the entire loading
                    throw new InvalidOperationException($"Failed to load NPC {dto.Id}: {ex.Message}", ex);
                }
            }

            Console.WriteLine($"Successfully loaded {context.GameWorld.WorldState.NPCs.Count} NPCs using NPCParser");
        }
        catch (ContentValidationException ex)
        {
            Console.WriteLine($"NPC validation failed with {ex.Errors.Count()} errors:");
            foreach (ValidationError error in ex.Errors)
            {
                Console.WriteLine($"  - {error.Message}");
            }
            // DON'T ADD TO CONTEXT.ERRORS - let it crash
            throw new InvalidOperationException($"NPC validation failed - fix npcs.json", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load NPCs: {ex.Message}");
            // DON'T ADD TO CONTEXT.ERRORS - let it crash  
            throw;
        }
    }
}