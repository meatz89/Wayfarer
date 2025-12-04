/// <summary>
/// Service that generates contextual narrative text for game mechanics.
/// Works WITH MessageSystem - NarrativeService generates the narrative content,
/// MessageSystem handles the display. This separation allows for rich, varied
/// narrative while keeping MessageSystem focused on UI toast management.
///
/// Responsibility: Generate narrative text with variety and context
/// NOT Responsible: Display, timing, or UI concerns (that's MessageSystem's job)
/// DDR-007: All narrative selection is deterministic based on game state
/// </summary>
public class NarrativeService
{
    private readonly NPCRepository _npcRepository;

    public NarrativeService(NPCRepository npcRepository)
    {
        _npcRepository = npcRepository;
    }

    /// <summary>
    /// Generate narrative for queue reorganization after letter removal
    /// Returns: array of narrative messages
    /// </summary>
    public string[] GenerateQueueReorganizationNarrative(int removedPosition, int lettersShifted)
    {
        List<string> messages = new List<string>();

        if (lettersShifted > 0)
        {
            messages.Add($"Letters moved up in queue after position {removedPosition} was cleared.");

            if (lettersShifted > 3)
            {
                messages.Add("The queue shifts dramatically as letters cascade upward.");
            }
        }

        return messages.ToArray();
    }

    /// <summary>
    /// Generate narrative for morning letter generation
    /// Returns: MorningNarrativeResult
    /// DDR-007: Deterministic selection based on letters generated count
    /// </summary>
    public MorningNarrativeResult GenerateMorningLetterNarrative(int lettersGenerated, bool queueFull)
    {
        // Morning arrival narrative
        string[] morningNarratives = new[]
            {
            "The morning brings new correspondence to the posting board.",
            "Dawn's light reveals fresh letters awaiting carriers.",
            "The night courier has left new deliveries on the board.",
            "Morning mist clears to show new letters have arrived."
        };

        // DDR-007: Deterministic selection based on letters count (predictable variety)
        int narrativeIndex = lettersGenerated % morningNarratives.Length;
        string morning = morningNarratives[narrativeIndex];
        string letterCount;
        string severity = "info";

        // DeliveryObligation count narrative
        if (lettersGenerated > 0)
        {
            letterCount = $"{lettersGenerated} new letter{(lettersGenerated > 1 ? "s have" : " has")} arrived at the posting board.";
        }
        else if (queueFull)
        {
            letterCount = "New letters arrived, but your queue is already full.";
            severity = "warning";
        }
        else
        {
            letterCount = "No new letters have arrived this morning.";
        }

        return new MorningNarrativeResult(morning, letterCount, severity);
    }

    /// <summary>
    /// Generate narrative for time block transitions
    /// Returns: TransitionNarrativeResult
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
    /// Generate narrative for standing obligation warnings
    /// Returns: NarrativeResult or null if no warning needed
    /// </summary>
    public NarrativeResult GenerateObligationWarning(StandingObligation obligation, int daysUntilForced)
    {
        if (daysUntilForced == 1)
        {
            return new NarrativeResult($"Your {obligation.Name} obligation will demand action tomorrow!", "warning");
        }
        else if (daysUntilForced == 0)
        {
            return new NarrativeResult($"Your {obligation.Name} obligation forces a letter into your queue!", "danger");
        }

        return null;
    }

    // Helper methods

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
            $"{npcName} approached you {timeNarrative} with a letter request.",
            $"{npcName} caught your attention {timeNarrative} with urgent business.",
            $"You encountered {npcName} {timeNarrative}, who has a delivery need.",
            $"{npcName} sought you out {timeNarrative} with correspondence."
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

    /// <summary>
    /// Generate narrative for token spending (queue manipulation)
    /// DDR-007: Deterministic selection based on amount spent
    /// DOMAIN COLLECTION PRINCIPLE: Switch-based lookup instead of Dictionary
    /// </summary>
    public string GenerateTokenSpendingNarrative(ConnectionType type, int amount, string action)
    {
        string[] narratives = GetSpendingNarratives(action.ToLower(), type, amount);
        if (narratives.Length > 0)
        {
            int narrativeIndex = amount % narratives.Length;
            return narratives[narrativeIndex];
        }
        return $"You spend {amount} {type} tokens.";
    }

    private string[] GetSpendingNarratives(string action, ConnectionType type, int amount)
    {
        return action switch
        {
            "skip" => new[]
            {
                $"You call in a {type} favor to skip ahead.",
                $"Your {type} connections help rearrange priorities.",
                $"A {type} token smooths the way forward."
            },
            "purge" => new[]
            {
                $"You burn {amount} {type} tokens to clear your obligations.",
                $"Your {type} relationships take the hit as you abandon the letter.",
                $"The {type} network won't forget this betrayal."
            },
            "swap" => new[]
            {
                $"You leverage {type} influence to reorder deliveries.",
                $"A few {type} tokens convince others to wait.",
                $"Your {type} connections enable the switch."
            },
            _ => Array.Empty<string>()
        };
    }

    /// <summary>
    /// Generate narrative for accepting a new standing obligation
    /// DDR-007: Categorical selection based on obligation name
    /// DOMAIN COLLECTION PRINCIPLE: Switch-based lookup instead of Dictionary
    /// </summary>
    public string GenerateObligationAcceptanceNarrative(StandingObligation obligation)
    {
        string narrative = GetObligationAcceptanceNarrative(obligation.Name);
        if (!string.IsNullOrEmpty(narrative))
        {
            return narrative;
        }
        return $"You accept the terms of {obligation.Name}. This will reshape how you conduct business.";
    }

    private string GetObligationAcceptanceNarrative(string obligationName)
    {
        if (obligationName.Contains("Trade Exclusivity", StringComparison.OrdinalIgnoreCase))
        {
            return "You sign the exclusive trade agreement, binding yourself to merchant guild rules.";
        }
        if (obligationName.Contains("Shadow Obligation", StringComparison.OrdinalIgnoreCase))
        {
            return "The shadowy figure nods. \"You're one of us now. There's no backing out.\"";
        }
        return null;
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
    /// Generate narrative for removing an obligation
    /// DDR-007: Deterministic selection based on obligation name hash
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
            // DDR-007: Use first narrative (categorical selection, no hash-based pseudo-randomness)
            return breakingNarratives[0];
        }
        else
        {
            return $"The terms of {obligation.Name} have been fulfilled. You are released from your obligations.";
        }
    }

    /// <summary>
    /// Generate narrative for breaking obligation penalties
    /// DDR-007: Deterministic selection based on token loss amount
    /// </summary>
    public string GenerateObligationBreakingNarrative(StandingObligation obligation, int tokenLoss)
    {
        string[] penaltyNarratives = new[] {
            $"Word spreads quickly of your betrayal. {tokenLoss} {obligation.RelatedTokenType} connections turn their backs on you.",
            $"Breaking {obligation.Name} costs you dearly - {tokenLoss} {obligation.RelatedTokenType} tokens vanish like smoke.",
            $"Your former associates scatter. {tokenLoss} {obligation.RelatedTokenType} relationships crumble.",
            $"\"We had a deal!\" The fury in their eyes matches the {tokenLoss} {obligation.RelatedTokenType} tokens you've lost."
        };

        // DDR-007: Deterministic selection based on token loss
        int narrativeIndex = tokenLoss % penaltyNarratives.Length;
        return penaltyNarratives[narrativeIndex];
    }
}
