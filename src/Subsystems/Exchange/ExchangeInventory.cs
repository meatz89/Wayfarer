using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Subsystems.ExchangeSubsystem
{
    /// <summary>
    /// Manages NPC exchange inventories and tracks exchange history.
    /// Internal to the Exchange subsystem - not exposed publicly.
    /// </summary>
    public class ExchangeInventory
    {
        private readonly GameWorld _gameWorld;
        
        // NPC ID -> List of available exchanges
        private Dictionary<string, List<ExchangeData>> _npcExchanges;
        
        // NPC ID -> Exchange history
        private Dictionary<string, List<ExchangeHistoryEntry>> _exchangeHistory;
        
        // Track unique exchanges that have been used
        private HashSet<string> _usedUniqueExchanges;

        public ExchangeInventory(GameWorld gameWorld)
        {
            _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
            _npcExchanges = new Dictionary<string, List<ExchangeData>>();
            _exchangeHistory = new Dictionary<string, List<ExchangeHistoryEntry>>();
            _usedUniqueExchanges = new HashSet<string>();
        }

        /// <summary>
        /// Initialize exchange inventories from GameWorld data
        /// </summary>
        public void InitializeFromGameWorld(GameWorld gameWorld)
        {
            _npcExchanges.Clear();
            
            // Load exchanges from NPC exchange decks
            foreach (NPC npc in gameWorld.NPCs)
            {
                List<ExchangeData> npcExchangeList = new List<ExchangeData>();

                // Check if NPC has exchange deck in GameWorld
                if (gameWorld.NPCExchangeCards.TryGetValue(npc.ID.ToLower(), out List<ExchangeCard> exchangeCards))
                {
                    foreach (ExchangeCard card in exchangeCards)
                    {
                        // Convert ExchangeCard to ExchangeData for compatibility
                        ExchangeData exchangeData = ConvertExchangeCardToData(card);
                        if (exchangeData != null)
                        {
                            npcExchangeList.Add(exchangeData);
                        }
                    }
                }

                // Also check if NPC has exchange deck directly
                if (npc.ExchangeDeck != null && npc.ExchangeDeck.Any())
                {
                    foreach (ExchangeCard card in npc.ExchangeDeck)
                    {
                        ExchangeData exchangeData = ConvertExchangeCardToData(card);
                        if (exchangeData != null)
                        {
                            // Avoid duplicates
                            if (!npcExchangeList.Any(e => e.Id == exchangeData.Id))
                            {
                                npcExchangeList.Add(exchangeData);
                            }
                        }
                    }
                }

                if (npcExchangeList.Any())
                {
                    _npcExchanges[npc.ID] = npcExchangeList;
                    Console.WriteLine($"[ExchangeInventory] Loaded {npcExchangeList.Count} exchanges for {npc.Name}");
                }
            }
        }

        /// <summary>
        /// Get all exchanges for an NPC
        /// </summary>
        public List<ExchangeData> GetNPCExchanges(string npcId)
        {
            if (_npcExchanges.TryGetValue(npcId, out List<ExchangeData> exchanges))
            {
                // Filter out used unique exchanges
                return exchanges.Where(e => !IsExchangeExhausted(e)).ToList();
            }
            return new List<ExchangeData>();
        }

        /// <summary>
        /// Get a specific exchange by ID
        /// </summary>
        public ExchangeData GetExchange(string npcId, string exchangeId)
        {
            if (_npcExchanges.TryGetValue(npcId, out List<ExchangeData> exchanges))
            {
                ExchangeData exchange = exchanges.FirstOrDefault(e => e.Id == exchangeId);
                if (exchange != null && !IsExchangeExhausted(exchange))
                {
                    return exchange;
                }
            }
            return null;
        }

        /// <summary>
        /// Add an exchange to an NPC's inventory
        /// </summary>
        public void AddExchange(string npcId, ExchangeData exchange)
        {
            if (!_npcExchanges.ContainsKey(npcId))
            {
                _npcExchanges[npcId] = new List<ExchangeData>();
            }

            // Ensure exchange has an ID
            if (string.IsNullOrEmpty(exchange.Id))
            {
                exchange.Id = Guid.NewGuid().ToString();
            }

            _npcExchanges[npcId].Add(exchange);
            Console.WriteLine($"[ExchangeInventory] Added exchange {exchange.ExchangeName} to NPC {npcId}");
        }

        /// <summary>
        /// Remove an exchange from an NPC's inventory
        /// </summary>
        public void RemoveExchange(string npcId, string exchangeId)
        {
            if (_npcExchanges.TryGetValue(npcId, out List<ExchangeData> exchanges))
            {
                exchanges.RemoveAll(e => e.Id == exchangeId);
                
                // Mark as used if unique
                _usedUniqueExchanges.Add(exchangeId);
                
                Console.WriteLine($"[ExchangeInventory] Removed exchange {exchangeId} from NPC {npcId}");
            }
        }

        /// <summary>
        /// Record that an exchange has been completed
        /// </summary>
        public void RecordExchange(string npcId, string exchangeId)
        {
            // Initialize history for NPC if needed
            if (!_exchangeHistory.ContainsKey(npcId))
            {
                _exchangeHistory[npcId] = new List<ExchangeHistoryEntry>();
            }

            // Get exchange details
            ExchangeData exchange = GetExchange(npcId, exchangeId);
            string exchangeName = exchange?.ExchangeName ?? exchangeId;

            // Create history entry
            ExchangeHistoryEntry entry = new ExchangeHistoryEntry
            {
                ExchangeId = exchangeId,
                ExchangeName = exchangeName,
                Timestamp = DateTime.Now,
                Day = _gameWorld.CurrentDay,
                TimeBlock = _gameWorld.CurrentTimeBlock,
                WasSuccessful = true
            };

            _exchangeHistory[npcId].Add(entry);

            // Update exchange usage count
            if (exchange != null)
            {
                // Exchange completed - mark as used if single use
                if (exchange.SingleUse)
                {
                    exchange.IsAvailable = false;
                };
                
                // Mark unique exchanges as used
                if (exchange.SingleUse)
                {
                    _usedUniqueExchanges.Add(exchangeId);
                }
            }

            Console.WriteLine($"[ExchangeInventory] Recorded exchange {exchangeName} with NPC {npcId}");
        }

        /// <summary>
        /// Get exchange history for an NPC
        /// </summary>
        public List<ExchangeHistoryEntry> GetExchangeHistory(string npcId)
        {
            if (_exchangeHistory.TryGetValue(npcId, out List<ExchangeHistoryEntry> history))
            {
                return new List<ExchangeHistoryEntry>(history);
            }
            return new List<ExchangeHistoryEntry>();
        }

        /// <summary>
        /// Check if an exchange has been exhausted
        /// </summary>
        private bool IsExchangeExhausted(ExchangeData exchange)
        {
            // Check if unique exchange has been used
            if (exchange.SingleUse && _usedUniqueExchanges.Contains(exchange.Id))
            {
                return true;
            }

            // Check if exchange is still available
            if (!exchange.IsAvailable)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Convert ExchangeCard to ExchangeData for compatibility
        /// </summary>
        private ExchangeData ConvertExchangeCardToData(ExchangeCard card)
        {
            if (card == null) return null;

            ExchangeData data = new ExchangeData
            {
                Id = card.Id,
                ExchangeName = card.Name,
                Name = card.Name,
                Description = card.Description,
                TemplateId = card.Id,
                Costs = new List<ResourceAmount>(),
                Rewards = new List<ResourceAmount>(),
                SingleUse = card.SingleUse,
                IsAvailable = !card.IsCompleted
            };

            // Convert cost structure - copy resource list
            if (card.Cost?.Resources != null)
            {
                data.Costs = new List<ResourceAmount>(card.Cost.Resources);
            }

            // Convert reward structure - copy resource list
            if (card.Reward?.Resources != null)
            {
                data.Rewards = new List<ResourceAmount>(card.Reward.Resources);
            }

            return data;
        }


        /// <summary>
        /// Get statistics about exchanges
        /// </summary>
        public ExchangeStatistics GetStatistics()
        {
            return new ExchangeStatistics
            {
                TotalNPCsWithExchanges = _npcExchanges.Count,
                TotalExchangesAvailable = _npcExchanges.Values.Sum(list => list.Count(e => !IsExchangeExhausted(e))),
                TotalExchangesCompleted = _exchangeHistory.Values.Sum(list => list.Count),
                UniqueExchangesUsed = _usedUniqueExchanges.Count
            };
        }

        /// <summary>
        /// Clear all exchange data (for new game or reset)
        /// </summary>
        public void Clear()
        {
            _npcExchanges.Clear();
            _exchangeHistory.Clear();
            _usedUniqueExchanges.Clear();
        }

        /// <summary>
        /// Unlock a specific exchange for an NPC
        /// </summary>
        public void UnlockExchange(string npcId, string exchangeId)
        {
            // This would load the exchange from a template/database
            // For now, log the unlock
            Console.WriteLine($"[ExchangeInventory] Unlocking exchange {exchangeId} for NPC {npcId}");
            
            // In a full implementation, this would:
            // 1. Load exchange template from GameWorld or content files
            // 2. Add it to the NPC's available exchanges
            // 3. Possibly trigger a notification
        }

        /// <summary>
        /// Check if an NPC has a specific exchange available
        /// </summary>
        public bool HasExchange(string npcId, string exchangeId)
        {
            if (_npcExchanges.TryGetValue(npcId, out List<ExchangeData> exchanges))
            {
                return exchanges.Any(e => e.Id == exchangeId && !IsExchangeExhausted(e));
            }
            return false;
        }

        /// <summary>
        /// Get all NPCs that have exchanges available
        /// </summary>
        public List<string> GetNPCsWithExchanges()
        {
            return _npcExchanges
                .Where(kvp => kvp.Value.Any(e => !IsExchangeExhausted(e)))
                .Select(kvp => kvp.Key)
                .ToList();
        }
    }

    /// <summary>
    /// Statistics about exchange system usage
    /// </summary>
    public class ExchangeStatistics
    {
        public int TotalNPCsWithExchanges { get; set; }
        public int TotalExchangesAvailable { get; set; }
        public int TotalExchangesCompleted { get; set; }
        public int UniqueExchangesUsed { get; set; }
    }
}