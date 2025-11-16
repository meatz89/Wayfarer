/// <summary>
/// Generates narrative text for game events.
/// Contains all narrative generation logic from the original NarrativeService.
/// </summary>
public class EventNarrator
{
    private readonly NPCRepository _npcRepository;
    private readonly GameWorld _gameWorld;
    private readonly Random _random = new Random();

    public EventNarrator(NPCRepository npcRepository, GameWorld gameWorld)
    {
        _npcRepository = npcRepository;
        _gameWorld = gameWorld;
    }

    // ========== TOKEN NARRATIVES ==========

    /// <summary>
    /// Generate narrative for token gains with NPC-specific reactions
    /// </summary>
    public TokenNarrativeResult GenerateTokenGainNarrative(ConnectionType type, int count, string npcId)
    {
        if (string.IsNullOrEmpty(npcId)) return null;

        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return null;

        string[] reactions = GetTokenGainReactions(type, npc.Name);
        string reaction = reactions[_random.Next(reactions.Length)];
        string summary = $"+{count} {type} connection with {npc.Name}";

        return new TokenNarrativeResult(reaction, summary);
    }

    /// <summary>
    /// Generate narrative for relationship milestones
    /// </summary>
    public MilestoneNarrativeResult GenerateRelationshipMilestone(string npcId, int totalTokens)
    {
        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return null;

        Dictionary<int, string> milestones = new Dictionary<int, string>
    {
        { 3, $"{npc.Name} now trusts you enough to share private correspondence." },
        { 5, $"Your bond with {npc.Name} has deepened considerably." },
        { 8, $"{npc.Name} considers you among their most trusted associates." },
        { 10, $"You've earned {npc.Name}'s complete confidence." },
        { 15, $"Few people enjoy the level of trust {npc.Name} has in you." },
        { 20, $"{npc.Name} would trust you with their life." }
    };

        if (milestones.TryGetValue(totalTokens, out string message))
        {
            string additional = totalTokens == 3
                ? $"{{icon:open-book}} {npc.Name} may now offer you letter delivery opportunities!"
                : null;
            return new MilestoneNarrativeResult(message, additional);
        }

        return null;
    }

    /// <summary>
    /// Generate narrative for relationship damage
    /// </summary>
    public NarrativeResult GenerateRelationshipDamageNarrative(string npcId, ConnectionType type, int remainingTokens)
    {
        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return null;

        if (remainingTokens < 0)
        {
            return new NarrativeResult($"{npc.Name} feels you owe them for past failures.", "danger");
        }
        else if (remainingTokens == 0)
        {
            return new NarrativeResult($"Your {type} relationship with {npc.Name} has been completely severed.", "warning");
        }
        else
        {
            return new NarrativeResult($"Your relationship with {npc.Name} has been damaged.", "warning");
        }
    }

    // ========== TIME NARRATIVES ==========

    /// <summary>
    /// Generate narrative for time transitions
    /// </summary>
    public TransitionNarrativeResult GenerateTimeTransitionNarrative(TimeBlocks from, TimeBlocks to, string actionDescription)
    {
        string transition = GetTimeTransitionNarrative(from, to);
        string action = null;

        if (!string.IsNullOrEmpty(actionDescription))
        {
            action = $"You spent time {actionDescription}.";
        }

        return new TransitionNarrativeResult(transition, action);
    }

    /// <summary>
    /// Generate narrative for token spending
    /// </summary>
    public string GenerateTokenSpendingNarrative(ConnectionType type, int amount, string action)
    {
        Dictionary<string, string[]> spendingNarratives = new Dictionary<string, string[]>
    {
        { "skip", new[] {
            $"You call in a {type} favor to skip ahead.",
            $"Your {type} connections help rearrange priorities.",
            $"A {type} token smooths the way forward."
        }},
        { "purge", new[] {
            $"You burn {amount} {type} tokens to clear your obligations.",
            $"Your {type} relationships take the hit as you abandon the letter.",
            $"The {type} network won't forget this betrayal."
        }},
        { "swap", new[] {
            $"You leverage {type} influence to reorder deliveries.",
            $"A few {type} tokens convince others to wait.",
            $"Your {type} connections enable the switch."
        }}
    };

        if (spendingNarratives.TryGetValue(action.ToLower(), out string[] narratives))
        {
            return narratives[_random.Next(narratives.Length)];
        }

        return $"You spend {amount} {type} tokens.";
    }

    /// <summary>
    /// Generate narrative for obligation acceptance
    /// </summary>
    public string GenerateObligationAcceptanceNarrative(StandingObligation obligation)
    {
        Dictionary<string, string[]> acceptanceNarratives = new Dictionary<string, string[]>
    {
        { "Trade Exclusivity", new[] {
            "You sign the exclusive trade agreement, binding yourself to merchant guild rules.",
            "The guild seal marks your new allegiance. Other traders eye you with suspicion.",
            "\"Welcome to the guild,\" the merchant says. \"Remember - we take care of our own.\""
        }},
        { "Shadow Obligation", new[] {
            "The shadowy figure nods. \"You're one of us now. There's no backing out.\"",
            "A chill runs down your spine as you accept. The underworld has long memories.",
            "\"Excellent choice,\" whispers the contact. \"We'll be in touch... frequently.\""
        }},
    };

        foreach (string key in acceptanceNarratives.Keys)
        {
            if (obligation.Name.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                string[] narratives = acceptanceNarratives[key];
                return narratives[_random.Next(narratives.Length)];
            }
        }

        return $"You accept the terms of {obligation.Name}. This will reshape how you conduct business.";
    }

    /// <summary>
    /// Generate narrative for obligation conflicts
    /// </summary>
    public string GenerateObligationConflictNarrative(string newObligation, List<string> conflicts)
    {
        if (conflicts.Count == 1)
        {
            return $"The terms of {newObligation} directly contradict your existing {conflicts[0]} agreement. Choose your allegiances carefully.";
        }
        else
        {
            return $"Accepting {newObligation} would violate your commitments to: {string.Join(", ", conflicts)}. Some bridges, once burned, cannot be rebuilt.";
        }
    }

    /// <summary>
    /// Generate narrative for obligation removal
    /// </summary>
    public string GenerateObligationRemovalNarrative(StandingObligation obligation, bool isVoluntary)
    {
        if (isVoluntary)
        {
            string[] breakingNarratives = new[] {
            $"You tear up the {obligation.Name} agreement. There will be consequences.",
            $"The {obligation.Name} contract burns in the fire. Your former associates will not forget this betrayal.",
            $"With a heavy heart, you abandon {obligation.Name}. Trust, once broken, is hard to rebuild."
        };
            return breakingNarratives[_random.Next(breakingNarratives.Length)];
        }
        else
        {
            return $"The terms of {obligation.Name} have been fulfilled. You are released from your obligations.";
        }
    }

    /// <summary>
    /// Generate narrative for breaking obligations
    /// </summary>
    public string GenerateObligationBreakingNarrative(StandingObligation obligation, int tokenLoss)
    {
        string[] penaltyNarratives = new[] {
        $"Word spreads quickly of your betrayal. {tokenLoss} {obligation.RelatedTokenType} connections turn their backs on you.",
        $"Breaking {obligation.Name} costs you dearly - {tokenLoss} {obligation.RelatedTokenType} tokens vanish like smoke.",
        $"Your former associates scatter. {tokenLoss} {obligation.RelatedTokenType} relationships crumble.",
        $"\"We had a deal!\" The fury in their eyes matches the {tokenLoss} {obligation.RelatedTokenType} tokens you've lost."
    };

        return penaltyNarratives[_random.Next(penaltyNarratives.Length)];
    }

    // ========== Venue NARRATIVES ==========

    /// <summary>
    /// Generate venue arrival narrative
    /// </summary>
    public string GenerateArrivalText(Venue venue, Location entrySpot)
    {
        if (venue == null) return "You arrive at an unknown venue.";

        string spotDesc = "";
        if (entrySpot != null)
        {
            spotDesc = $" at {entrySpot.Name}";
        }

        return $"You arrive at {venue.Name}{spotDesc}.";
    }

    /// <summary>
    /// Generate venue departure narrative
    /// </summary>
    public string GenerateDepartureText(Venue venue, Location exitSpot)
    {
        if (venue == null) return "You depart from your current venue.";

        string spotDesc = "";
        if (exitSpot != null)
        {
            spotDesc = $" from {exitSpot.Name}";
        }

        return $"You leave {venue.Name}{spotDesc}.";
    }

    /// <summary>
    /// Generate movement between Locations narrative
    /// </summary>
    public string GenerateMovementText(Location fromSpot, Location toSpot)
    {
        if (fromSpot == null || toSpot == null)
            return "You move to a new area.";

        return $"You move from {fromSpot.Name} to {toSpot.Name}.";
    }

    // ========== HELPER METHODS ==========

    private string[] GetTokenGainReactions(ConnectionType type, string npcName)
    {
        return type switch
        {
            ConnectionType.Trust => new[]
            {
            $"{npcName} smiles warmly. \"I knew I could count on you.\"",
            $"{npcName} clasps your hand. \"Thank you, my friend.\"",
            $"\"You've proven yourself trustworthy,\" {npcName} says with appreciation.",
            $"{npcName} nods gratefully. \"It's good to have someone reliable.\""
        },
            ConnectionType.Diplomacy => new[]
            {
            $"{npcName} nods approvingly. \"Good business, as always.\"",
            $"\"Reliable couriers are worth their focus in gold,\" says {npcName}.",
            $"{npcName} makes a note. \"I'll remember this efficiency.\"",
            $"\"Professional work deserves professional relationships,\" {npcName} remarks."
        },
            ConnectionType.Status => new[]
            {
            $"{npcName} acknowledges you formally. \"Your service honors us both.\"",
            $"\"Discretion and duty - admirable qualities,\" {npcName} observes.",
            $"{npcName} inclines their head. \"The nobility remembers its friends.\"",
            $"\"Such dedication deserves recognition,\" {npcName} states."
        },
            ConnectionType.Shadow => new[]
            {
            $"{npcName} whispers, \"Your discretion is... appreciated.\"",
            $"A slight nod from {npcName}. \"The shadows remember their own.\"",
            $"{npcName} vanishes a coin between fingers. \"Trust is rare in our trade.\"",
            $"\"Few understand the value of silence,\" {npcName} murmurs."
        },
            _ => new[] { $"{npcName} acknowledges your service." }
        };
    }

    private string GetTimeNarrative(TimeBlocks time)
    {
        return time switch
        {
            TimeBlocks.Morning => "this morning",
            TimeBlocks.Midday => "this afternoon",
            TimeBlocks.Afternoon => "this evening",
            TimeBlocks.Evening => "late tonight",
            _ => "recently"
        };
    }

    private string[] GetApproachNarratives(string npcName, string timeNarrative)
    {
        return new[]
        {
        $"{{icon:open-book}} {npcName} approached you {timeNarrative} with a letter request.",
        $"{{icon:open-book}} {npcName} caught your attention {timeNarrative} with urgent business.",
        $"{{icon:open-book}} You encountered {npcName} {timeNarrative}, who has a delivery need.",
        $"{{icon:open-book}} {npcName} sought you out {timeNarrative} with correspondence."
    };
    }

    private string GetTimeTransitionNarrative(TimeBlocks from, TimeBlocks to)
    {
        if (from == TimeBlocks.Morning && to == TimeBlocks.Midday)
            return "The day grows warm as afternoon approaches.";
        if (from == TimeBlocks.Midday && to == TimeBlocks.Afternoon)
            return "Shadows lengthen as evening draws near.";
        if (from == TimeBlocks.Afternoon && to == TimeBlocks.Evening)
            return "Darkness falls across the land.";
        if (from == TimeBlocks.Evening && to == TimeBlocks.Morning)
            return "You sleep through the night and wake at morning.";
        return "Time passes...";
    }
}
