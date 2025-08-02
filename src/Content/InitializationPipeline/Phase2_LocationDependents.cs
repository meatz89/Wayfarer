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
        var spotsPath = Path.Combine(context.ContentPath, "location_spots.json");
        
        if (!File.Exists(spotsPath))
        {
            context.Errors.Add("location_spots.json not found - this is required for player initialization");
            return;
        }
        
        try
        {
            var spotDTOs = context.ContentLoader.LoadValidatedContent<List<LocationSpotDTO>>(spotsPath);
            
            if (spotDTOs == null || !spotDTOs.Any())
            {
                context.Errors.Add("No location spots found in location_spots.json");
                return;
            }
            
            var spotFactory = new LocationSpotFactory();
            var locations = context.GameWorld.WorldState.locations;
            
            foreach (var dto in spotDTOs)
            {
                try
                {
                    // Verify location exists
                    var location = locations.FirstOrDefault(l => l.Id == dto.LocationId);
                    if (location == null)
                    {
                        context.Warnings.Add($"LocationSpot {dto.Id} references unknown location {dto.LocationId}");
                        continue;
                    }
                    
                    // Parse spot type
                    if (!Enum.TryParse<LocationSpotTypes>(dto.Type, true, out var spotType))
                    {
                        context.Warnings.Add($"Invalid spot type '{dto.Type}' for {dto.Id}, skipping");
                        continue;
                    }
                    
                    // Parse time blocks
                    var timeBlocks = new List<TimeBlocks>();
                    foreach (var timeStr in dto.CurrentTimeBlocks ?? new List<string>())
                    {
                        if (Enum.TryParse<TimeBlocks>(timeStr, true, out var timeBlock))
                        {
                            timeBlocks.Add(timeBlock);
                        }
                        else
                        {
                            context.Warnings.Add($"Invalid time block '{timeStr}' for spot {dto.Id}");
                        }
                    }
                    
                    // Create spot using the ID-based method
                    var spot = spotFactory.CreateLocationSpotFromIds(
                        dto.Id,
                        dto.Name,
                        dto.LocationId,
                        spotType,
                        locations, // Pass available locations
                        dto.Description,
                        dto.InitialState,
                        timeBlocks,
                        dto.DomainTags?.ToList() ?? new List<string>()
                    );
                    
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
            foreach (var error in ex.Errors)
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
        var npcsPath = Path.Combine(context.ContentPath, "npcs.json");
        
        if (!File.Exists(npcsPath))
        {
            Console.WriteLine("INFO: npcs.json not found, creating default NPCs");
            return;
        }
        
            var npcDTOs = context.ContentLoader.LoadValidatedContent<List<NPCDTO>>(npcsPath);
            
            if (npcDTOs == null || !npcDTOs.Any())
            {
                Console.WriteLine("WARNING: No NPCs found in npcs.json, creating defaults");
                return;
            }
            
            var npcFactory = new NPCFactory();
            var locations = context.GameWorld.WorldState.locations;

        foreach (var dto in npcDTOs)
        {
            // Verify location exists
            var location = locations.FirstOrDefault(l => l.Id == dto.LocationId);
            if (location == null)
            {
                context.Warnings.Add($"NPC {dto.Id} references unknown location {dto.LocationId}");
                continue;
            }

            // Parse profession
            if (!Enum.TryParse<Professions>(dto.Profession, true, out var profession))
            {
                profession = Professions.Merchant;
                context.Warnings.Add($"Invalid profession '{dto.Profession}' for {dto.Id}, defaulting to Merchant");
            }

            // Parse letter token types
            var tokenTypes = new List<ConnectionType>();
            foreach (var tokenStr in dto.LetterTokenTypes ?? new List<string> { "Common" })
            {
                if (Enum.TryParse<ConnectionType>(tokenStr, true, out var tokenType))
                {
                    tokenTypes.Add(tokenType);
                }
            }

            // Create NPC using ID-based method
            var npc = npcFactory.CreateNPCFromIds(
                dto.Id,
                dto.Name,
                dto.LocationId,
                locations,
                profession,
                dto.SpotId,
                dto.Role ?? profession.ToString(),
                dto.Description ?? $"A {profession} in {location.Name}",
                new List<ServiceTypes>(), // Services can be added later
                tokenTypes,
                dto.Tier
            );

            // Token types already set during creation

            context.GameWorld.WorldState.NPCs.Add(npc);
            Console.WriteLine($"  Loaded NPC: {npc.Name} ({npc.ID}) at {npc.Location}");

            Console.WriteLine($"Loaded {context.GameWorld.WorldState.NPCs.Count} NPCs");
        }
    }
}