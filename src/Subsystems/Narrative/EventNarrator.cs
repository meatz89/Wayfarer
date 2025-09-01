using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Subsystems.NarrativeSubsystem
{
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

        // ========== OBSERVATION NARRATIVES ==========

        /// <summary>
        /// Generate narrative for taking an observation
        /// </summary>
        public string NarrateObservationTaken(Observation observation, ObservationCard card)
        {
            string[] templates = new[]
            {
                $"You notice {observation.Text} and make a mental note.",
                $"Your keen observation of {observation.Text} may prove useful.",
                $"You carefully observe {observation.Text}.",
                $"The details of {observation.Text} catch your attention."
            };
            
            return templates[_random.Next(templates.Length)];
        }

        /// <summary>
        /// Generate narrative for time block transition
        /// </summary>
        public string NarrateTimeBlockTransition()
        {
            string[] templates = new[]
            {
                "Time passes and new opportunities arise.",
                "The world shifts as the hours advance.",
                "New observations become available.",
                "Fresh details emerge as time moves forward."
            };
            
            return templates[_random.Next(templates.Length)];
        }

        // ========== LETTER NARRATIVES ==========

        /// <summary>
        /// Generate narrative for letter positioning
        /// </summary>
        public string NarrateLetterPositioning(string senderName, LetterPositioningReason reason, int position, int strength, int debt)
        {
            string reasonText = reason switch
            {
                LetterPositioningReason.HighRelationship => $"strong relationship (strength: {strength})",
                LetterPositioningReason.Debt => $"outstanding debt (debt: {debt})",
                LetterPositioningReason.Urgency => "urgent matter",
                LetterPositioningReason.StandingObligation => "standing obligation",
                _ => "standard priority"
            };

            return $"{senderName}'s letter takes position {position} due to {reasonText}.";
        }

        /// <summary>
        /// Generate narrative for special letter events
        /// </summary>
        public string NarrateSpecialLetterEvent(SpecialLetterEvent letterEvent)
        {
            return letterEvent.EventType switch
            {
                SpecialLetterEventType.UrgentRequest => $"URGENT: {letterEvent.TargetNPCId} requires immediate assistance!",
                SpecialLetterEventType.RelationshipBonus => $"Your relationship with {letterEvent.TargetNPCId} has deepened significantly.",
                SpecialLetterEventType.DeadlineApproaching => $"WARNING: {letterEvent.TargetNPCId}'s letter deadline approaches!",
                SpecialLetterEventType.LetterFailed => $"FAILURE: {letterEvent.TargetNPCId}'s letter has expired.",
                SpecialLetterEventType.LetterDelivered => $"SUCCESS: Letter delivered to {letterEvent.TargetNPCId}!",
                _ => $"Event: {letterEvent.EventType} involving {letterEvent.TargetNPCId}"
            };
        }

        // ========== TOKEN NARRATIVES ==========

        /// <summary>
        /// Generate narrative for token gains with NPC-specific reactions
        /// </summary>
        public (string reaction, string summary) GenerateTokenGainNarrative(ConnectionType type, int count, string npcId)
        {
            if (string.IsNullOrEmpty(npcId)) return (null, null);

            NPC npc = _npcRepository.GetById(npcId);
            if (npc == null) return (null, null);

            string[] reactions = GetTokenGainReactions(type, npc.Name);
            string reaction = reactions[_random.Next(reactions.Length)];
            string summary = $"+{count} {type} connection with {npc.Name}";

            return (reaction, summary);
        }

        /// <summary>
        /// Generate narrative for relationship milestones
        /// </summary>
        public (string milestone, string additional) GenerateRelationshipMilestone(string npcId, int totalTokens)
        {
            NPC npc = _npcRepository.GetById(npcId);
            if (npc == null) return (null, null);

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
                    ? $"📮 {npc.Name} may now offer you letter delivery opportunities!"
                    : null;
                return (message, additional);
            }

            return (null, null);
        }

        /// <summary>
        /// Generate narrative for relationship damage
        /// </summary>
        public (string message, string severity) GenerateRelationshipDamageNarrative(string npcId, ConnectionType type, int remainingTokens)
        {
            NPC npc = _npcRepository.GetById(npcId);
            if (npc == null) return (null, null);

            if (remainingTokens < 0)
            {
                return ($"{npc.Name} feels you owe them for past failures.", "danger");
            }
            else if (remainingTokens == 0)
            {
                return ($"Your {type} relationship with {npc.Name} has been completely severed.", "warning");
            }
            else
            {
                return ($"Your relationship with {npc.Name} has been damaged.", "warning");
            }
        }

        // ========== QUEUE NARRATIVES ==========

        /// <summary>
        /// Generate narrative for queue reorganization
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
        /// Generate narrative for queue entry
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
                return $"📨 Letter queued at position {position}.";
            }
        }

        // ========== MORNING NARRATIVES ==========

        /// <summary>
        /// Generate narrative for morning letter generation
        /// </summary>
        public (string morning, string letterCount, string severity) GenerateMorningLetterNarrative(int lettersGenerated, bool queueFull)
        {
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

            return (morning, letterCount, severity);
        }

        // ========== NPC NARRATIVES ==========

        /// <summary>
        /// Generate narrative for NPC letter offer
        /// </summary>
        public string[] GenerateNPCLetterOfferNarrative(NPC npc, DeliveryObligation offer, TimeBlocks currentTime, int npcTokens)
        {
            List<string> messages = new List<string>();
            string timeNarrative = GetTimeNarrative(currentTime);

            string[] approachNarratives = GetApproachNarratives(npc.Name, timeNarrative);
            messages.Add(approachNarratives[_random.Next(approachNarratives.Length)]);

            messages.Add($"\"{offer.Message}\"");

            if (npcTokens >= 8)
            {
                messages.Add($"({npc.Name} offers premium payment for your trusted service)");
            }
            else if (npcTokens >= 5)
            {
                messages.Add($"({npc.Name} offers fair compensation for reliable work)");
            }

            return messages.ToArray();
        }

        // ========== TIME NARRATIVES ==========

        /// <summary>
        /// Generate narrative for time transitions
        /// </summary>
        public (string transition, string action) GenerateTimeTransitionNarrative(TimeBlocks from, TimeBlocks to, string actionDescription = null)
        {
            string transition = GetTimeTransitionNarrative(from, to);
            string action = null;

            if (!string.IsNullOrEmpty(actionDescription))
            {
                action = $"You spent time {actionDescription}.";
            }

            return (transition, action);
        }

        // ========== OBLIGATION NARRATIVES ==========

        /// <summary>
        /// Generate narrative for obligation warnings
        /// </summary>
        public (string message, string severity) GenerateObligationWarning(StandingObligation obligation, int daysUntilForced)
        {
            if (daysUntilForced == 1)
            {
                return ($"⚠️ Your {obligation.Name} obligation will demand action tomorrow!", "warning");
            }
            else if (daysUntilForced == 0)
            {
                return ($"📮 Your {obligation.Name} obligation forces a letter into your queue!", "danger");
            }

            return (null, null);
        }

        /// <summary>
        /// Generate narrative for deadline warnings
        /// </summary>
        public (string message, string severity) GenerateDeadlineWarning(DeliveryObligation letter, int daysRemaining)
        {
            string urgency = daysRemaining switch
            {
                0 => "expires today",
                1 => "expires tomorrow",
                2 => "has only 2 days left",
                _ => $"has {daysRemaining} days remaining"
            };

            string severity = daysRemaining <= 1 ? "warning" : "info";
            string message = $"⏰ Letter from {letter.SenderName} {urgency}!";

            return (message, severity);
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
                { "Noble Patronage", new[] {
                    "The noble's seal on parchment makes it official. You now serve at their pleasure.",
                    "\"Splendid!\" the patron exclaims. \"I expect great things from our arrangement.\"",
                    "With a flourish of signatures, your fate is tied to noble whims."
                }}
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
        /// Generate narrative for forced letter
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

            foreach (string key in forcedNarratives.Keys)
            {
                if (obligation.Name.Contains(key, StringComparison.OrdinalIgnoreCase) ||
                    (obligation.RelatedTokenType.HasValue && obligation.RelatedTokenType.Value.ToString().Contains(key)))
                {
                    string[] narratives = forcedNarratives[key];
                    return narratives[_random.Next(narratives.Length)];
                }
            }

            return $"Your {obligation.Name} compels you to accept a letter from {letter.SenderName} to {letter.RecipientName}.";
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

        // ========== LOCATION NARRATIVES ==========

        /// <summary>
        /// Generate location arrival narrative
        /// </summary>
        public string GenerateArrivalText(Location location, LocationSpot entrySpot)
        {
            if (location == null) return "You arrive at an unknown location.";
            
            string spotDesc = "";
            if (entrySpot != null)
            {
                spotDesc = $" at {entrySpot.Name}";
            }
            
            return $"You arrive at {location.Name}{spotDesc}.";
        }

        /// <summary>
        /// Generate location departure narrative
        /// </summary>
        public string GenerateDepartureText(Location location, LocationSpot exitSpot)
        {
            if (location == null) return "You depart from your current location.";
            
            string spotDesc = "";
            if (exitSpot != null)
            {
                spotDesc = $" from {exitSpot.Name}";
            }
            
            return $"You leave {location.Name}{spotDesc}.";
        }

        /// <summary>
        /// Generate movement between spots narrative
        /// </summary>
        public string GenerateMovementText(LocationSpot fromSpot, LocationSpot toSpot)
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
                ConnectionType.Commerce => new[]
                {
                    $"{npcName} nods approvingly. \"Good business, as always.\"",
                    $"\"Reliable couriers are worth their weight in gold,\" says {npcName}.",
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
    }
}