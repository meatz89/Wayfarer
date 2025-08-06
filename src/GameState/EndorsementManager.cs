using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages endorsements and their conversion to seals
/// </summary>
public class EndorsementManager
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;

    // Conversion rates for endorsements to seals
    private readonly Dictionary<SealTier, int> _endorsementsRequiredForSeal = new()
    {
        { SealTier.Apprentice, 3 },
        { SealTier.Journeyman, 5 },
        { SealTier.Master, 7 }
    };

    public EndorsementManager(GameWorld gameWorld, MessageSystem messageSystem)
    {
        _gameWorld = gameWorld;
        _messageSystem = messageSystem;
    }

    /// <summary>
    /// Track a delivered endorsement letter
    /// </summary>
    public void RecordEndorsement(Letter endorsementLetter)
    {
        if (endorsementLetter.SpecialType != LetterSpecialType.Endorsement)
        {
            throw new ArgumentException("Letter must be an endorsement type");
        }

        Player player = _gameWorld.GetPlayer();

        // Track endorsements delivered to specific guild types
        string guildType = DetermineGuildType(endorsementLetter.RecipientId);
        string endorsementKey = $"endorsements_delivered_{guildType}";

        // Get current count
        MemoryFlag memory = player.GetMemory(endorsementKey);
        int currentCount = memory?.Importance ?? 0;

        // Increment count
        player.AddMemory(
            endorsementKey,
            $"{currentCount + 1} endorsements delivered to {guildType} guilds",
            _gameWorld.CurrentDay,
            currentCount + 1
        );

        _messageSystem.AddSystemMessage(
            $"📜 Endorsement delivered! Total {guildType} endorsements: {currentCount + 1}",
            SystemMessageTypes.Success
        );

        // Check if player can now upgrade their seal
        CheckSealUpgradeEligibility(guildType, currentCount + 1);
    }

    /// <summary>
    /// Get available seal conversions at a guild
    /// </summary>
    public List<SealConversionOption> GetAvailableSealConversions(string guildLocationId)
    {
        List<SealConversionOption> options = new List<SealConversionOption>();
        Player player = _gameWorld.GetPlayer();
        SealType guildSealType = GetSealTypeForGuild(guildLocationId);

        // Get current seal tier for this type
        Seal? currentSeal = player.Seals.FirstOrDefault(s => s.Type == guildSealType);
        SealTier currentTier = currentSeal?.Tier ?? SealTier.Apprentice - 1; // -1 means no seal

        // Get endorsement count
        string endorsementKey = $"endorsements_delivered_{guildSealType}";
        MemoryFlag memory = player.GetMemory(endorsementKey);
        int endorsementCount = memory?.Importance ?? 0;

        // Check each possible upgrade
        foreach ((SealTier tier, int required) in _endorsementsRequiredForSeal)
        {
            if ((int)tier > (int)currentTier + 1) continue; // Can't skip tiers
            if ((int)tier <= (int)currentTier) continue; // Already have this tier

            options.Add(new SealConversionOption
            {
                TargetTier = tier,
                SealType = guildSealType,
                RequiredEndorsements = required,
                CurrentEndorsements = endorsementCount,
                CanConvert = endorsementCount >= required,
                GuildName = GetGuildName(guildLocationId)
            });
        }

        return options;
    }

    /// <summary>
    /// Convert endorsements to a seal
    /// </summary>
    public bool ConvertEndorsementsToSeal(string guildLocationId, SealTier targetTier)
    {
        Player player = _gameWorld.GetPlayer();
        SealType sealType = GetSealTypeForGuild(guildLocationId);

        // Validate conversion
        List<SealConversionOption> options = GetAvailableSealConversions(guildLocationId);
        SealConversionOption? option = options.FirstOrDefault(o => o.TargetTier == targetTier);

        if (option == null || !option.CanConvert)
        {
            _messageSystem.AddSystemMessage(
                "You don't meet the requirements for this seal upgrade.",
                SystemMessageTypes.Danger
            );
            return false;
        }

        // Spend endorsements
        string endorsementKey = $"endorsements_delivered_{sealType}";
        int newCount = option.CurrentEndorsements - option.RequiredEndorsements;

        player.AddMemory(
            endorsementKey,
            $"{newCount} endorsements delivered to {sealType} guilds",
            _gameWorld.CurrentDay,
            newCount
        );

        // Grant or upgrade seal
        Seal? existingSeal = player.Seals.FirstOrDefault(s => s.Type == sealType);
        if (existingSeal != null)
        {
            existingSeal.Tier = targetTier;
            existingSeal.DayIssued = _gameWorld.CurrentDay;

            _messageSystem.AddSystemMessage(
                $"🏅 Your {sealType} Seal has been upgraded to {targetTier}!",
                SystemMessageTypes.Success
            );
        }
        else
        {
            Seal newSeal = new Seal
            {
                Id = $"{sealType.ToString().ToLower()}_seal_{targetTier}",
                Name = $"{targetTier} {sealType} Seal",
                Type = sealType,
                Tier = targetTier,
                IssuingGuildId = guildLocationId,
                DayIssued = _gameWorld.CurrentDay,
                Description = GetSealDescription(sealType, targetTier),
                Material = GetSealMaterial(targetTier),
                Insignia = GetSealInsignia(sealType)
            };

            player.Seals.Add(newSeal);

            _messageSystem.AddSystemMessage(
                $"🏅 Congratulations! You have earned the {newSeal.GetFullName()}!",
                SystemMessageTypes.Success
            );
        }

        // Add memory of achievement
        player.AddMemory(
            $"seal_earned_{sealType}_{targetTier}",
            $"Earned {targetTier} {sealType} Seal from {GetGuildName(guildLocationId)}",
            _gameWorld.CurrentDay,
            5
        );

        return true;
    }

    private void CheckSealUpgradeEligibility(string guildType, int endorsementCount)
    {
        Player player = _gameWorld.GetPlayer();
        SealType sealType = Enum.Parse<SealType>(guildType, true);

        // Get current seal
        Seal? currentSeal = player.Seals.FirstOrDefault(s => s.Type == sealType);
        SealTier nextTier = currentSeal != null ? currentSeal.Tier + 1 : SealTier.Apprentice;

        if (_endorsementsRequiredForSeal.TryGetValue(nextTier, out int required) &&
            endorsementCount >= required)
        {
            _messageSystem.AddSystemMessage(
                $"✨ You have enough endorsements to earn a {nextTier} {sealType} Seal! Visit the guild to convert them.",
                SystemMessageTypes.Info
            );
        }
    }

    private string DetermineGuildType(string recipientId)
    {
        // Based on recipient location or profession, determine guild type
        WorldState worldState = _gameWorld.WorldState;
        NPC? npc = worldState.NPCs.FirstOrDefault(n => n.ID == recipientId);
        if (npc == null) return "Commerce"; // Default

        // Check NPC's primary token type
        if (npc.LetterTokenTypes.Contains(ConnectionType.Commerce))
            return "Commerce";
        if (npc.LetterTokenTypes.Contains(ConnectionType.Status))
            return "Status";
        if (npc.LetterTokenTypes.Contains(ConnectionType.Shadow))
            return "Shadow";

        return "Commerce"; // Default
    }

    public SealType GetSealTypeForGuild(string guildLocationId)
    {
        return guildLocationId switch
        {
            "merchant_guild" => SealType.Commerce,
            "messenger_guild" => SealType.Commerce, // Messengers are commerce-aligned
            "scholar_guild" => SealType.Status,
            _ => SealType.Commerce
        };
    }

    private string GetGuildName(string guildLocationId)
    {
        return guildLocationId switch
        {
            "merchant_guild" => "Merchant Guild",
            "messenger_guild" => "Courier's Guild",
            "scholar_guild" => "Scholar's Atheneum",
            _ => "Guild"
        };
    }

    private string GetSealDescription(SealType type, SealTier tier)
    {
        return (type, tier) switch
        {
            (SealType.Commerce, SealTier.Apprentice) => "Basic trading privileges and guild recognition",
            (SealType.Commerce, SealTier.Journeyman) => "Full merchant rights and trade route access",
            (SealType.Commerce, SealTier.Master) => "Elite merchant status with special privileges",
            (SealType.Status, SealTier.Apprentice) => "Recognition among the educated classes",
            (SealType.Status, SealTier.Journeyman) => "Respected scholar with library access",
            (SealType.Status, SealTier.Master) => "Distinguished academic with teaching rights",
            (SealType.Shadow, SealTier.Apprentice) => "Known to the underground networks",
            (SealType.Shadow, SealTier.Journeyman) => "Trusted operative with safe house access",
            (SealType.Shadow, SealTier.Master) => "Shadow guild leader with network control",
            _ => "Official guild recognition"
        };
    }

    private string GetSealMaterial(SealTier tier)
    {
        return tier switch
        {
            SealTier.Apprentice => "Bronze",
            SealTier.Journeyman => "Silver",
            SealTier.Master => "Gold",
            _ => "Bronze"
        };
    }

    private string GetSealInsignia(SealType type)
    {
        return type switch
        {
            SealType.Commerce => "Crossed keys over a merchant ship",
            SealType.Status => "An open book beneath a crown",
            SealType.Shadow => "A crescent moon veiled in mist",
            _ => "Guild crest"
        };
    }
}

/// <summary>
/// Represents an available seal conversion option
/// </summary>
public class SealConversionOption
{
    public SealTier TargetTier { get; set; }
    public SealType SealType { get; set; }
    public int RequiredEndorsements { get; set; }
    public int CurrentEndorsements { get; set; }
    public bool CanConvert { get; set; }
    public string GuildName { get; set; }
}