using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Subsystems.ExchangeSubsystem
{
    /// <summary>
    /// Orchestrates exchange sessions and coordinates exchange operations.
    /// Internal to the Exchange subsystem - not exposed publicly.
    /// </summary>
    public class ExchangeOrchestrator
    {
        private readonly GameWorld _gameWorld;
        private readonly ExchangeValidator _validator;
        private readonly ExchangeProcessor _processor;
        private readonly MessageSystem _messageSystem;
        
        private Dictionary<string, ExchangeSession> _activeSessions;

        public ExchangeOrchestrator(
            GameWorld gameWorld,
            ExchangeValidator validator,
            ExchangeProcessor processor,
            MessageSystem messageSystem)
        {
            _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
            _activeSessions = new Dictionary<string, ExchangeSession>();
        }

        /// <summary>
        /// Create a new exchange session with an NPC
        /// </summary>
        public ExchangeSession CreateSession(NPC npc, List<ExchangeOption> availableExchanges)
        {
            // End any existing session with this NPC
            if (_activeSessions.ContainsKey(npc.ID))
            {
                EndSession(npc.ID);
            }

            // Create new session
            ExchangeSession session = new ExchangeSession
            {
                SessionId = Guid.NewGuid().ToString(),
                NPC = npc,
                AvailableExchanges = availableExchanges,
                StartTime = DateTime.Now,
                IsActive = true
            };

            _activeSessions[npc.ID] = session;

            _messageSystem.AddSystemMessage(
                $"Exchange session started with {npc.Name}",
                SystemMessageTypes.Info);

            return session;
        }

        /// <summary>
        /// End an exchange session
        /// </summary>
        public void EndSession(string npcId)
        {
            if (_activeSessions.TryGetValue(npcId, out ExchangeSession session))
            {
                session.IsActive = false;
                _activeSessions.Remove(npcId);

                _messageSystem.AddSystemMessage(
                    $"Exchange session with {session.NPC.Name} ended",
                    SystemMessageTypes.Info);
            }
        }

        /// <summary>
        /// Get an active session by NPC ID
        /// </summary>
        public ExchangeSession GetActiveSession(string npcId)
        {
            return _activeSessions.TryGetValue(npcId, out ExchangeSession session) ? session : null;
        }

        /// <summary>
        /// Check if an NPC has an active exchange session
        /// </summary>
        public bool HasActiveSession(string npcId)
        {
            return _activeSessions.ContainsKey(npcId);
        }

        /// <summary>
        /// Process a selected exchange within a session
        /// </summary>
        public async Task<ExchangeResult> ProcessSessionExchange(string npcId, string exchangeId)
        {
            ExchangeSession session = GetActiveSession(npcId);
            if (session == null || !session.IsActive)
            {
                return new ExchangeResult
                {
                    Success = false,
                    Message = "No active exchange session"
                };
            }

            // Find the exchange in the session
            ExchangeOption selectedExchange = session.AvailableExchanges
                .FirstOrDefault(e => e.ExchangeId == exchangeId);

            if (selectedExchange == null)
            {
                return new ExchangeResult
                {
                    Success = false,
                    Message = "Exchange not available in this session"
                };
            }

            // Validate the exchange is still possible
            PlayerResourceState playerResources = _gameWorld.GetPlayerResourceState();
            ExchangeValidationResult validation = _validator.ValidateExchange(
                selectedExchange.ExchangeData, 
                session.NPC, 
                playerResources,
                new List<string>());

            if (!validation.IsValid)
            {
                return new ExchangeResult
                {
                    Success = false,
                    Message = validation.ValidationMessage ?? "Exchange no longer valid"
                };
            }

            // Process the exchange
            ExchangeResult result = await _processor.ProcessExchange(
                selectedExchange.ExchangeData, 
                session.NPC);

            // Update session state if successful
            if (result.Success)
            {
                // Remove unique exchanges from available list
                if (selectedExchange.ExchangeData.IsUnique)
                {
                    session.AvailableExchanges.Remove(selectedExchange);
                }

                // Check if session should continue
                if (!session.AvailableExchanges.Any())
                {
                    EndSession(npcId);
                    result.Message += " (No more exchanges available)";
                }
            }

            return result;
        }

        // REMOVED: Exchange system should not create ConversationCards
        // The Exchange subsystem is separate from conversations
        /*
        /// <summary>
        /// Create exchange cards for a commerce conversation
        /// </summary>
        public List<ConversationCard> CreateExchangeCards(NPC npc, List<ExchangeOption> exchanges)
        {
            List<ConversationCard> cards = new List<ConversationCard>();

            foreach (ExchangeOption exchange in exchanges)
            {
                // Create accept card
                ConversationCard acceptCard = new ConversationCard
                {
                    Id = $"exchange_accept_{exchange.ExchangeId}",
                    Title = $"Accept: {exchange.Name}",
                    Description = exchange.Description,
                    CardType = CardType.Conversation,
                    Category = CardCategory.Exchange,
                    Properties = new List<CardProperty> { CardProperty.Accept },
                    Focus = 0, // Exchange cards have no focus cost
                    SuccessEffect = new CardEffect
                    {
                        Type = CardEffectType.Exchange,
                        ExchangeData = exchange.ExchangeData
                    },
                    Context = new CardContext
                    {
                        ExchangeData = exchange.ExchangeData,
                        IsAcceptCard = true
                    }
                };

                // Add cost/reward display to card
                if (!string.IsNullOrEmpty(exchange.Cost))
                {
                    acceptCard.DisplayCost = exchange.Cost;
                }
                if (!string.IsNullOrEmpty(exchange.Reward))
                {
                    acceptCard.DisplayReward = exchange.Reward;
                }

                // Disable if player can't afford
                if (!exchange.CanAfford)
                {
                    acceptCard.IsDisabled = true;
                    acceptCard.DisabledReason = "Cannot afford";
                }

                cards.Add(acceptCard);

                // Create decline card
                ConversationCard declineCard = new ConversationCard
                {
                    Id = $"exchange_decline_{exchange.ExchangeId}",
                    Title = $"Decline: {exchange.Name}",
                    Description = "Politely refuse this exchange",
                    CardType = CardType.Conversation,
                    Category = CardCategory.Exchange,
                    Properties = new List<CardProperty> { CardProperty.Decline },
                    Focus = 0,
                    Context = new CardContext
                    {
                        ExchangeData = exchange.ExchangeData,
                        IsDeclineCard = true
                    }
                };

                cards.Add(declineCard);
            }

            // Add a general "leave" card
            ConversationCard leaveCard = new ConversationCard
            {
                Id = "exchange_leave",
                Title = "End Trading",
                Description = "Thank them and leave",
                CardType = CardType.Exchange,
                Category = CardCategory.Exchange,
                Properties = new List<CardProperty> { CardProperty.Leave },
                Focus = 0,
                SuccessEffect = new CardEffect
                {
                    Type = CardEffectType.EndConversation
                }
            };

            cards.Add(leaveCard);

            return cards;
        }
        */

        /// <summary>
        /// Check if exchange should trigger special events
        /// </summary>
        public void CheckExchangeTriggers(ExchangeData exchange, NPC npc)
        {
            // Check for relationship milestones
            if (exchange.GrantsTokens)
            {
                foreach (ResourceAmount reward in exchange.Rewards)
                {
                    if (IsTokenResource(reward.Type))
                    {
                        ConnectionType tokenType = MapResourceToToken(reward.Type);
                        CheckRelationshipMilestone(npc.ID, tokenType, reward.Amount);
                    }
                }
            }

            // Check for special exchange chains
            if (!string.IsNullOrEmpty(exchange.UnlocksExchangeId))
            {
                UnlockExchange(npc.ID, exchange.UnlocksExchangeId);
            }

            // Check for story triggers
            if (!string.IsNullOrEmpty(exchange.TriggerEvent))
            {
                TriggerStoryEvent(exchange.TriggerEvent);
            }
        }

        /// <summary>
        /// Clear all active sessions (used when loading game or changing locations)
        /// </summary>
        public void ClearAllSessions()
        {
            foreach (string npcId in _activeSessions.Keys.ToList())
            {
                EndSession(npcId);
            }
        }

        // Helper methods

        private bool IsTokenResource(ResourceType type)
        {
            return type == ResourceType.TrustToken ||
                   type == ResourceType.CommerceToken ||
                   type == ResourceType.StatusToken ||
                   type == ResourceType.ShadowToken;
        }

        private ConnectionType MapResourceToToken(ResourceType type)
        {
            return type switch
            {
                ResourceType.TrustToken => ConnectionType.Trust,
                ResourceType.CommerceToken => ConnectionType.Commerce,
                ResourceType.StatusToken => ConnectionType.Status,
                ResourceType.ShadowToken => ConnectionType.Shadow,
                _ => ConnectionType.None
            };
        }

        private void CheckRelationshipMilestone(string npcId, ConnectionType tokenType, int amount)
        {
            // This would integrate with the Token subsystem to check milestones
            Console.WriteLine($"[ExchangeOrchestrator] Checking milestone for {npcId}: {tokenType} +{amount}");
        }

        private void UnlockExchange(string npcId, string exchangeId)
        {
            // This would integrate with ExchangeInventory to unlock new exchanges
            Console.WriteLine($"[ExchangeOrchestrator] Unlocking exchange {exchangeId} for {npcId}");
        }

        private void TriggerStoryEvent(string eventId)
        {
            // This would integrate with a story/event system
            Console.WriteLine($"[ExchangeOrchestrator] Triggering story event: {eventId}");
        }
    }
}