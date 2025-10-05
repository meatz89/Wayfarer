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

        NPC stranger = new NPC
        {
            ID = dto.Id ?? "",
            Name = dto.Name ?? "",
            Location = dto.LocationId ?? "",
            PersonalityType = personalityType,
            IsStranger = true,
            AvailableTimeBlock = timeBlock,
            Level = dto.Level,
            HasBeenEncountered = false,
            Tier = dto.Level, // Use level as tier for difficulty
            Description = $"Level {dto.Level} stranger",
            Requests = new List<NPCRequest>()
        };

        // Convert stranger's single request to NPCRequest
        if (dto.Request != null)
        {
            NPCRequest request = ConvertRequestDTOToNPCRequest(dto.Request);
            stranger.Requests.Add(request);
        }

        return stranger;
    }

    /// <summary>
    /// Convert stranger request DTO to NPCRequest
    /// </summary>
    private static NPCRequest ConvertRequestDTOToNPCRequest(StrangerRequestDTO dto)
    {
        // Create request from stranger request DTO
        NPCRequest request = new NPCRequest
        {
            Id = dto.Id ?? "",
            Name = dto.Name ?? "",
            Description = dto.Description ?? "",
            SystemType = TacticalSystemType.Social,  // Strangers use Social system
            EngagementTypeId = dto.ConversationTypeId ?? "",  // Map old ConversationTypeId to new EngagementTypeId
            Status = RequestStatus.Available,
            MomentumThresholds = new List<int>(dto.MomentumThresholds ?? new List<int>()),
            Rewards = ConvertRewards(dto.Rewards)
        };

        return request;
    }

    /// <summary>
    /// Convert stranger rewards to NPCRequest rewards
    /// </summary>
    private static List<RequestReward> ConvertRewards(List<StrangerRewardDTO> rewardDtos)
    {
        List<RequestReward> rewards = new List<RequestReward>();
        if (rewardDtos != null)
        {
            foreach (StrangerRewardDTO dto in rewardDtos)
            {
                RequestReward reward = new RequestReward
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
                rewards.Add(reward);
            }
        }
        return rewards;
    }

}