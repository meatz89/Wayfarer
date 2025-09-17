using System;
using System.Collections.Generic;
using System.Text.Json;
using Wayfarer.GameState.Enums;

/// <summary>
/// Parser for stranger NPC content from JSON packages
/// </summary>
public static class StrangerParser
{
    /// <summary>
    /// Parse strangers from a JSON package
    /// </summary>
    public static List<StrangerNPC> ParseStrangers(JsonElement root)
    {
        List<StrangerNPC> strangers = new List<StrangerNPC>();

        // Parse content section
        if (root.TryGetProperty("content", out JsonElement content))
        {
            // Parse strangers array
            if (content.TryGetProperty("strangers", out JsonElement strangersElement))
            {
                foreach (JsonElement strangerElement in strangersElement.EnumerateArray())
                {
                    StrangerNPCDTO? strangerDto = JsonSerializer.Deserialize<StrangerNPCDTO>(strangerElement.GetRawText());
                    if (strangerDto != null)
                    {
                        strangers.Add(ConvertDTOToStrangerNPC(strangerDto));
                    }
                }
            }
        }

        return strangers;
    }

    /// <summary>
    /// Convert StrangerNPCDTO to StrangerNPC domain model
    /// </summary>
    public static StrangerNPC ConvertDTOToStrangerNPC(StrangerNPCDTO dto)
    {
        // Parse personality type
        if (!Enum.TryParse<PersonalityType>(dto.Personality, true, out PersonalityType personalityType))
        {
            throw new ArgumentException($"Invalid personality type: {dto.Personality}");
        }

        // Parse time block
        if (!Enum.TryParse<TimeBlock>(dto.TimeBlock, true, out TimeBlock timeBlock))
        {
            throw new ArgumentException($"Invalid time block: {dto.TimeBlock}");
        }

        StrangerNPC stranger = new StrangerNPC
        {
            Id = dto.Id ?? "",
            Name = dto.Name ?? "",
            Level = dto.Level,
            Personality = personalityType,
            LocationId = dto.LocationId ?? "",
            AvailableTime = timeBlock,
            ConversationTypes = new Dictionary<string, StrangerConversation>(),
            HasBeenTalkedTo = false
        };

        // Parse conversation types
        if (dto.ConversationTypes != null)
        {
            foreach (StrangerConversationDTO conversationDto in dto.ConversationTypes)
            {
                StrangerConversation conversation = ConvertDTOToStrangerConversation(conversationDto);
                stranger.ConversationTypes[conversationDto.Type] = conversation;
            }
        }

        return stranger;
    }

    /// <summary>
    /// Convert StrangerConversationDTO to StrangerConversation domain model
    /// </summary>
    private static StrangerConversation ConvertDTOToStrangerConversation(StrangerConversationDTO dto)
    {
        StrangerConversation conversation = new StrangerConversation
        {
            Type = dto.Type ?? "",
            RapportThresholds = new List<int>(dto.RapportThresholds ?? new List<int>()),
            Rewards = new List<StrangerReward>()
        };

        // Parse rewards
        if (dto.Rewards != null)
        {
            foreach (StrangerRewardDTO rewardDto in dto.Rewards)
            {
                conversation.Rewards.Add(ConvertDTOToStrangerReward(rewardDto));
            }
        }

        return conversation;
    }

    /// <summary>
    /// Convert StrangerRewardDTO to StrangerReward domain model
    /// </summary>
    private static StrangerReward ConvertDTOToStrangerReward(StrangerRewardDTO dto)
    {
        StrangerReward reward = new StrangerReward
        {
            Coins = dto.Coins,
            Health = dto.Health,
            Food = dto.Food,
            Familiarity = dto.Familiarity,
            Item = dto.Item,
            Permit = dto.Permit,
            Observation = dto.Observation,
            Tokens = new Dictionary<string, int>(dto.Tokens ?? new Dictionary<string, int>())
        };

        return reward;
    }
}

/// <summary>
/// Domain model for stranger NPCs
/// </summary>
public class StrangerNPC
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Level { get; set; }
    public PersonalityType Personality { get; set; }
    public string LocationId { get; set; } = "";
    public TimeBlock AvailableTime { get; set; }
    public Dictionary<string, StrangerConversation> ConversationTypes { get; set; } = new();
    public bool HasBeenTalkedTo { get; set; }

    /// <summary>
    /// Get XP multiplier based on stranger level
    /// </summary>
    public int GetXPMultiplier()
    {
        return Level;
    }

    /// <summary>
    /// Check if stranger is available at the current time block
    /// </summary>
    public bool IsAvailableAtTime(TimeBlock currentTime)
    {
        return AvailableTime == currentTime;
    }

    /// <summary>
    /// Mark this stranger as talked to for current time block
    /// </summary>
    public void MarkAsTalkedTo()
    {
        HasBeenTalkedTo = true;
    }

    /// <summary>
    /// Reset availability for new time block
    /// Called when time block changes
    /// </summary>
    public void RefreshForNewTimeBlock()
    {
        HasBeenTalkedTo = false;
    }

    /// <summary>
    /// Get available conversation if stranger hasn't been talked to
    /// </summary>
    public StrangerConversation GetAvailableConversation(string conversationType)
    {
        if (HasBeenTalkedTo)
            return null;

        return ConversationTypes.GetValueOrDefault(conversationType);
    }

    /// <summary>
    /// Get all available conversation types for this stranger
    /// </summary>
    public List<string> GetAvailableConversationTypes()
    {
        if (HasBeenTalkedTo)
            return new List<string>();

        return new List<string>(ConversationTypes.Keys);
    }

    /// <summary>
    /// Get reward for reaching a specific rapport threshold
    /// </summary>
    public StrangerReward GetRewardForThreshold(string conversationType, int thresholdIndex)
    {
        StrangerConversation? conversation = ConversationTypes.GetValueOrDefault(conversationType);
        if (conversation == null || thresholdIndex >= conversation.Rewards.Count)
            return null;

        return conversation.Rewards[thresholdIndex];
    }
}

/// <summary>
/// Domain model for stranger conversation types
/// </summary>
public class StrangerConversation
{
    public string Type { get; set; } = "";
    public List<int> RapportThresholds { get; set; } = new();
    public List<StrangerReward> Rewards { get; set; } = new();
}

/// <summary>
/// Domain model for stranger rewards
/// </summary>
public class StrangerReward
{
    public int Coins { get; set; }
    public int Health { get; set; }
    public int Food { get; set; }
    public int Familiarity { get; set; }
    public string Item { get; set; }
    public string Permit { get; set; }
    public string Observation { get; set; }
    public Dictionary<string, int> Tokens { get; set; } = new();
}

/// <summary>
/// Enum for time blocks when strangers are available
/// </summary>
public enum TimeBlock
{
    Dawn,
    Morning,
    Midday,
    Afternoon,
    Evening,
    Night,
    LateNight
}