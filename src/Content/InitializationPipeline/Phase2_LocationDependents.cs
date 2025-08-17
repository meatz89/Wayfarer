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
            Console.WriteLine("INFO: npcs.json not found, creating default NPCs");
            return;
        }

        try
        {
            List<NPCDTO> npcDTOs = context.ContentLoader.LoadValidatedContent<List<NPCDTO>>(npcsPath);

            if (npcDTOs == null || !npcDTOs.Any())
            {
                Console.WriteLine("WARNING: No NPCs found in npcs.json, creating defaults");
                return;
            }

            NPCFactory npcFactory = new NPCFactory();
            List<Location> locations = context.GameWorld.WorldState.locations;

            foreach (NPCDTO dto in npcDTOs)
            {
                // Verify location exists
                Location? location = locations.FirstOrDefault(l => l.Id == dto.LocationId);
                if (location == null)
                {
                    context.Warnings.Add($"NPC {dto.Id} references unknown location {dto.LocationId}");
                    continue;
                }

                // Parse profession
                if (!Enum.TryParse<Professions>(dto.Profession, true, out Professions profession))
                {
                    profession = Professions.Merchant;
                    context.Warnings.Add($"Invalid profession '{dto.Profession}' for {dto.Id}, defaulting to Merchant");
                }

                // Parse letter token types
                List<ConnectionType> tokenTypes = new List<ConnectionType>();
                foreach (string tokenStr in dto.LetterTokenTypes ?? new List<string> { "Common" })
                {
                    if (Enum.TryParse<ConnectionType>(tokenStr, true, out ConnectionType tokenType))
                    {
                        tokenTypes.Add(tokenType);
                    }
                }

                // Create NPC using ID-based method
                NPC npc = npcFactory.CreateNPCFromIds(
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
            }

            Console.WriteLine($"Loaded {context.GameWorld.WorldState.NPCs.Count} NPCs");
        }
        catch (ContentValidationException ex)
        {
            Console.WriteLine($"NPC validation failed with {ex.Errors.Count()} errors:");
            foreach (ValidationError error in ex.Errors)
            {
                Console.WriteLine($"  - {error.Message}");
                context.Errors.Add($"NPC validation: {error.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load NPCs: {ex.Message}");
            context.Errors.Add($"Failed to load NPCs: {ex.Message}");
        }
    }
}