using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Parser for stranger NPC content from JSON packages
/// Returns regular NPC objects with IsStranger flag set
/// </summary>
public static class StrangerParser
{

    /// <summary>
    /// Convert StrangerNPCDTO to NPC domain model with IsStranger flag
    /// </summary>
    public static NPC ConvertDTOToNPC(StrangerNPCDTO dto)
    {
        // Parse personality type
        if (!Enum.TryParse<PersonalityType>(dto.Personality, true, out PersonalityType personalityType))
        {
            throw new ArgumentException($"Invalid personality type: {dto.Personality}");
        }

        // Parse time block
        if (!Enum.TryParse<TimeBlocks>(dto.TimeBlock, true, out TimeBlocks timeBlock))
        {
            throw new ArgumentException($"Invalid time block: {dto.TimeBlock}");
        }

        // Validate required fields
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidDataException("StrangerNPC missing required field 'Id'");
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidDataException($"StrangerNPC '{dto.Id}' missing required field 'Name'");
        if (string.IsNullOrEmpty(dto.LocationId))
            throw new InvalidDataException($"StrangerNPC '{dto.Id}' missing required field 'LocationId'");

        NPC stranger = new NPC
        {
            ID = dto.Id,
            Name = dto.Name,
            LocationId = dto.LocationId,
            PersonalityType = personalityType,
            IsStranger = true,
            AvailableTimeBlock = timeBlock,
            Level = dto.Level,
            HasBeenEncountered = false,
            Tier = dto.Level, // Use level as tier for difficulty
            Description = $"Level {dto.Level} stranger"
        };

        return stranger;
    }

}