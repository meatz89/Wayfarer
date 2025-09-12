using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// Service that generates contextual narrative text for game mechanics.
/// Works WITH MessageSystem - NarrativeService generates the narrative content,
/// MessageSystem handles the display. This separation allows for rich, varied
/// narrative while keeping MessageSystem focused on UI toast management.
/// 
/// Responsibility: Generate narrative text with variety and context
/// NOT Responsible: Display, timing, or UI concerns (that's MessageSystem's job)
/// </summary>
public class NarrativeService
{
    private readonly NPCRepository _npcRepository;
    private readonly Random _random = new Random();

    public NarrativeService(NPCRepository npcRepository)
    {
        _npcRepository = npcRepository;
    }

    /// <summary>
    /// Generate narrative for token gains with NPC-specific reactions
    /// Returns: TokenNarrativeResult or null if invalid NPC
    /// </summary>
    public TokenNarrativeResult GenerateTokenGainNarrative(ConnectionType type, int count, string npcId)
    {
        if (string.IsNullOrEmpty(npcId)) return null;

        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null) return null;

        // Get type-specific reactions
        string[] reactions = GetTokenGainReactions(type, npc.Name);
        string reaction = reactions[_random.Next(reactions.Length)];
        string summary = $"+{count} {type} connection with {npc.Name}";

        return new TokenNarrativeResult(reaction, summary);
    }

    /// <summary>
    /// Generate narrative for relationship milestones when reaching token thresholds
    /// Returns: MilestoneNarrativeResult or null if no milestone
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

        if (milestones.TryGetValue(totalTokens, out string? message))
        {
            string additional = totalTokens == 3
                ? $"📮 {npc.Name} may now offer you letter delivery opportunities!"
                : null;
            return new MilestoneNarrativeResult(message, additional);
        }

        return null;
    }

    /// <summary>
    /// Generate narrative for relationship damage when tokens are removed
    /// Returns: NarrativeResult or null if invalid NPC
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

        string morning = morningNarratives[_random.Next(morningNarratives.Length)];
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
    public TransitionNarrativeResult GenerateTimeTransitionNarrative(TimeBlocks from, TimeBlocks to, string actionDescription = null)
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
            return new NarrativeResult($"⚠️ Your {obligation.Name} obligation will demand action tomorrow!", "warning");
        }
        else if (daysUntilForced == 0)
        {
            return new NarrativeResult($"📮 Your {obligation.Name} obligation forces a letter into your queue!", "danger");
        }

        return null;
    }

    /// <summary>
    /// Generate narrative for letter deadline warnings
    /// Returns: NarrativeResult
    /// </summary>
    public NarrativeResult GenerateDeadlineWarning(DeliveryObligation letter, int daysRemaining)
    {
        string urgency = daysRemaining switch
        {
            0 => "expires today",
            1 => "expires tomorrow",
            2 => "has only 2 days left",
            _ => $"has {daysRemaining} days remaining"
        };

        string severity = daysRemaining <= 1 ? "warning" : "info";
        string message = $"⏰ DeliveryObligation from {letter.SenderName} {urgency}!";

        return new NarrativeResult(message, severity);
    }


    /// <summary>

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
            ConnectionType.Commerce => new[]
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
            TimeBlocks.Dawn => "early this morning",
            TimeBlocks.Morning => "this morning",
            TimeBlocks.Afternoon => "this afternoon",
            TimeBlocks.Evening => "this evening",
            TimeBlocks.Night => "late tonight",
            _ => "recently"
        };
    }

    private string[] GetApproachNarratives(string npcName, string timeNarrative)
    {
        return new[]
        {
                $"📮 {npcName} approached you {timeNarrative} with a letter request.",
                $"📮 {npcName} caught your attention {timeNarrative} with urgent business.",
                $"📮 You encountered {npcName} {timeNarrative}, who has a delivery need.",
                $"📮 {npcName} sought you out {timeNarrative} with correspondence."
            };
    }

    private string GetTimeTransitionNarrative(TimeBlocks from, TimeBlocks to)
    {
        return (from, to) switch
        {
            (TimeBlocks.Dawn, TimeBlocks.Morning) => "The sun climbs higher as morning arrives.",
            (TimeBlocks.Morning, TimeBlocks.Afternoon) => "The day grows warm as afternoon approaches.",
            (TimeBlocks.Afternoon, TimeBlocks.Evening) => "Shadows lengthen as evening draws near.",
            (TimeBlocks.Evening, TimeBlocks.Night) => "Darkness falls across the land.",
            (TimeBlocks.Night, TimeBlocks.Dawn) => "The first light of dawn breaks the horizon.",
            _ => "Time passes..."
        };
    }

    /// <summary>
    /// Generate queue position narrative when letters enter at different positions
    /// </summary>
    public string GenerateQueueEntryNarrative(DeliveryObligation letter, int position)
    {
        if (position <= 3)
        {
            return $"⚡ {letter.SenderName}'s urgent letter pushes to position {position}!";
        }
        else if (position <= 5)
        {
            return $"📬 New letter enters queue at position {position}.";
        }
        else
        {
            return $"📨 DeliveryObligation queued at position {position}.";
        }
    }

    /// <summary>
    /// Generate narrative for token spending (queue manipulation)
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

        if (spendingNarratives.TryGetValue(action.ToLower(), out string[]? narratives))
        {
            return narratives[_random.Next(narratives.Length)];
        }

        return $"You spend {amount} {type} tokens.";
    }

    /// <summary>
    /// Generate narrative for accepting a new standing obligation
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
                { "Noble Patronage", new[] {
                    "The noble's seal on parchment makes it official. You now serve at their pleasure.",
                    "\"Splendid!\" the patron exclaims. \"I expect great things from our arrangement.\"",
                    "With a flourish of signatures, your fate is tied to noble whims."
                }}
            };

        // Try to find specific narratives for this obligation
        foreach (string key in acceptanceNarratives.Keys)
        {
            if (obligation.Name.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                string[] narratives = acceptanceNarratives[key];
                return narratives[_random.Next(narratives.Length)];
            }
        }

        // Generic acceptance narrative
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
    /// Generate narrative for removing an obligation
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
    /// Generate narrative for forced letter generation
    /// </summary>
    public string GenerateForcedLetterNarrative(StandingObligation obligation, DeliveryObligation letter)
    {
        Dictionary<string, string[]> forcedNarratives = new Dictionary<string, string[]>
            {
                { "Shadow", new[] {
                    $"A shadowy messenger slips a letter into your queue. \"{letter.SenderName} requires your... immediate attention.\"",
                    $"You find a black-sealed letter among your papers. The {obligation.Name} demands payment.",
                    $"\"No exceptions,\" the hooded figure insists, adding {letter.SenderName}'s letter to your burden."
                }},
                { "Patron", new[] {
                    $"Your patron's herald arrives with official correspondence. \"{letter.RecipientName} expects prompt delivery.\"",
                    $"The monthly obligation from {obligation.Name} arrives right on schedule.",
                    $"\"His Lordship insists,\" the servant says, handing you {letter.SenderName}'s sealed letter."
                }},
                { "Trade", new[] {
                    $"The guild enforcer visits. \"Guild business. {letter.RecipientName} needs this immediately.\"",
                    $"Your {obligation.Name} requires this guild letter be delivered without delay.",
                    $"\"Union rules,\" the merchant explains, adding another letter to your load."
                }}
            };

        // Find matching narrative type
        foreach (string key in forcedNarratives.Keys)
        {
            if (obligation.Name.Contains(key, StringComparison.OrdinalIgnoreCase) ||
                (obligation.RelatedTokenType.HasValue && obligation.RelatedTokenType.Value.ToString().Contains(key)))
            {
                string[] narratives = forcedNarratives[key];
                return narratives[_random.Next(narratives.Length)];
            }
        }

        // Generic forced letter narrative
        return $"Your {obligation.Name} compels you to accept a letter from {letter.SenderName} to {letter.RecipientName}.";
    }

    /// <summary>
    /// Generate narrative for breaking obligation penalties
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
}
