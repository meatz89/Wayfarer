using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages NPC exchange inventories and tracks exchange history.
/// Internal to the Exchange subsystem - not exposed publicly.
/// </summary>
public class ExchangeInventory
{
    private readonly GameWorld _gameWorld;

    // NPC ID -> List of available exchanges
    private Dictionary<string, List<ExchangeCard>> _npcExchanges;

    // NPC ID -> Exchange history
    private Dictionary<string, List<ExchangeHistoryEntry>> _exchangeHistory;

    // Track unique exchanges that have been used
    private HashSet<string> _usedUniqueExchanges;

    public ExchangeInventory(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _npcExchanges = new Dictionary<string, List<ExchangeCard>>();
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
            List<ExchangeCard> npcExchangeList = new List<ExchangeCard>();

            // Check if NPC has exchange deck in GameWorld
            NPCExchangeCardEntry? exchangeEntry = gameWorld.NPCExchangeCards.FindById(npc.ID.ToLower());
            if (exchangeEntry != null)
            {
                foreach (ExchangeCard card in exchangeEntry.ExchangeCards)
                {
                    npcExchangeList.Add(card);
                }
            }

            // Also check if NPC has exchange deck directly
            if (npc.ExchangeDeck != null && npc.ExchangeDeck.Any())
            {
                foreach (ExchangeCard card in npc.ExchangeDeck)
                {
                    // Avoid duplicates
                    if (!npcExchangeList.Any(e => e.Id == card.Id))
                    {
                        npcExchangeList.Add(card);
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
    public List<ExchangeCard> GetNPCExchanges(string npcId)
    {
        if (_npcExchanges.TryGetValue(npcId, out List<ExchangeCard> exchanges))
        {
            // Filter out used unique exchanges
            return exchanges.Where(e => !IsExchangeExhausted(e)).ToList();
        }
        return new List<ExchangeCard>();
    }

    /// <summary>
    /// Get a specific exchange by ID
    /// </summary>
    public ExchangeCard GetExchange(string npcId, string exchangeId)
    {
        if (_npcExchanges.TryGetValue(npcId, out List<ExchangeCard> exchanges))
        {
            ExchangeCard exchange = exchanges.FirstOrDefault(e => e.Id == exchangeId);
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
    public void AddExchange(string npcId, ExchangeCard exchange)
    {
        if (!_npcExchanges.ContainsKey(npcId))
        {
            _npcExchanges[npcId] = new List<ExchangeCard>();
        }

        // Ensure exchange has an ID
        if (string.IsNullOrEmpty(exchange.Id))
        {
            exchange.Id = Guid.NewGuid().ToString();
        }

        _npcExchanges[npcId].Add(exchange);
        Console.WriteLine($"[ExchangeInventory] Added exchange {exchange.Name} to NPC {npcId}");
    }

    /// <summary>
    /// Remove an exchange from an NPC's inventory
    /// </summary>
    public void RemoveExchange(string npcId, string exchangeId)
    {
        if (_npcExchanges.TryGetValue(npcId, out List<ExchangeCard> exchanges))
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
        ExchangeCard exchange = GetExchange(npcId, exchangeId);
        string exchangeName = exchange?.Name ?? exchangeId;

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
            // Record use (increments TimesUsed, marks IsCompleted for SingleUse)
            exchange.RecordUse();

            // Mark unique exchanges as used for fast lookup
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
    private bool IsExchangeExhausted(ExchangeCard exchange)
    {
        return exchange.IsExhausted();
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
        if (_npcExchanges.TryGetValue(npcId, out List<ExchangeCard> exchanges))
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
