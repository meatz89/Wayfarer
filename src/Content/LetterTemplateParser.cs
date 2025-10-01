using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public static class LetterTemplateParser
{
    /// <summary>
    /// Convert a LetterTemplateDTO to a LetterTemplate domain model
    /// </summary>
    public static LetterTemplate ConvertDTOToLetterTemplate(LetterTemplateDTO dto)
    {
        LetterTemplate template = new LetterTemplate
        {
            Id = dto.Id ?? "",
            Description = dto.Description ?? "",
            MinDeadlineInSegments = dto.MinDeadlineInSegments,
            MaxDeadlineInSegments = dto.MaxDeadlineInSegments,
            MinPayment = dto.MinPayment,
            MaxPayment = dto.MaxPayment,
            MinTokensRequired = dto.MinTokensRequired ?? 1
        };

        // Parse token type
        if (!string.IsNullOrEmpty(dto.TokenType))
        {
            template.TokenType = ParseConnectionType(dto.TokenType);
        }

        // Parse optional arrays
        template.PossibleSenders = dto.PossibleSenders?.ToArray() ?? new string[0];
        template.PossibleRecipients = dto.PossibleRecipients?.ToArray() ?? new string[0];

        // Parse special letter properties
        template.SpecialType = ParseSpecialType(dto.SpecialType ?? "None");
        template.SpecialTargetId = dto.SpecialTargetId ?? "";

        // Parse human context and consequences
        template.ConsequenceIfLate = dto.ConsequenceIfLate ?? "";
        template.ConsequenceIfDelivered = dto.ConsequenceIfDelivered ?? "";

        // Parse emotional focus
        template.EmotionalFocus = ParseEmotionalFocus(dto.EmotionalFocus ?? "MEDIUM");

        // Parse stakes
        template.Stakes = ParseStakeType(dto.Stakes ?? "REPUTATION");

        // Parse category
        if (!string.IsNullOrEmpty(dto.Category))
        {
            if (Enum.TryParse<LetterCategory>(dto.Category, out LetterCategory category))
            {
                template.Category = category;
            }
        }

        // Parse tier level
        if (!string.IsNullOrEmpty(dto.TierLevel))
        {
            if (Enum.TryParse<TierLevel>(dto.TierLevel, out TierLevel tier))
            {
                template.TierLevel = tier;
            }
        }

        // Parse size
        if (!string.IsNullOrEmpty(dto.Size))
        {
            if (Enum.TryParse<SizeCategory>(dto.Size, out SizeCategory size))
            {
                template.Size = size;
            }
        }

        return template;
    }

    private static ConnectionType ParseConnectionType(string connectionTypeStr)
    {
        return connectionTypeStr switch
        {
            "Trust" => ConnectionType.Trust,
            "Diplomacy" => ConnectionType.Diplomacy,
            "Status" => ConnectionType.Status,
            "Shadow" => ConnectionType.Shadow,
            _ => throw new ArgumentException($"Unknown connection type in JSON: '{connectionTypeStr}' - add to connection type mapping")
        };
    }

    private static LetterSpecialType ParseSpecialType(string specialTypeStr)
    {
        return specialTypeStr switch
        {
            "Introduction" => LetterSpecialType.Introduction,
            "AccessPermit" => LetterSpecialType.AccessPermit,
            "None" => LetterSpecialType.None,
            _ => throw new ArgumentException($"Unknown special type in JSON: '{specialTypeStr}' - add to special type mapping")
        };
    }

    private static EmotionalFocus ParseEmotionalFocus(string focusStr)
    {
        return focusStr?.ToUpper() switch
        {
            "LOW" => EmotionalFocus.LOW,
            "MEDIUM" => EmotionalFocus.MEDIUM,
            "HIGH" => EmotionalFocus.HIGH,
            "CRITICAL" => EmotionalFocus.CRITICAL,
            _ => throw new ArgumentException($"Unknown emotional focus in JSON: '{focusStr}' - add to emotional focus mapping")
        };
    }

    private static StakeType ParseStakeType(string stakeStr)
    {
        return stakeStr?.ToUpper() switch
        {
            "REPUTATION" => StakeType.REPUTATION,
            "WEALTH" => StakeType.WEALTH,
            "SAFETY" => StakeType.SAFETY,
            "SECRET" => StakeType.SECRET,
            _ => throw new ArgumentException($"Unknown stake type in JSON: '{stakeStr}' - add to stake type mapping")
        };
    }

}