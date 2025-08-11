using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;

/// <summary>
/// Effect that reveals hidden properties of a letter (true sender, actual stakes, etc.)
/// This is a core INVESTIGATE verb effect for discovering deception.
/// </summary>
public class RevealLetterPropertyEffect : IMechanicalEffect
{
    private readonly string _letterId;
    private readonly string _propertyToReveal;
    private readonly LetterQueueManager _queueManager;
    private readonly Player _player;

    public RevealLetterPropertyEffect(
        string letterId, 
        string propertyToReveal,
        LetterQueueManager queueManager,
        Player player)
    {
        _letterId = letterId;
        _propertyToReveal = propertyToReveal;
        _queueManager = queueManager;
        _player = player;
    }

    public void Apply(ConversationState state)
    {
        var position = _queueManager.GetLetterPosition(_letterId);
        if (!position.HasValue) return;
        
        var letter = _queueManager.GetLetterAt(position.Value);
        
        // Store the revealed information as a memory
        string revelation = _propertyToReveal switch
        {
            "sender" => $"The letter claiming to be from {letter.SenderName} is actually from someone else",
            "stakes" => $"The true stakes of this letter: {letter.ConsequenceIfLate}",
            "urgency" => $"This deadline is {(letter.DeadlineInHours < 6 ? "genuinely urgent" : "artificially rushed")}",
            "recipient" => $"The real recipient may not be {letter.RecipientName}",
            _ => $"Hidden truth about the letter: {_propertyToReveal}"
        };
        
        var memory = new MemoryFlag
        {
            Key = $"letter_truth_{_letterId}",
            Description = revelation,
            Importance = 8,
            ExpirationDay = -1 // Permanent knowledge
        };
        
        _player.Memories.Add(memory);
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = $"Reveal letter's {_propertyToReveal}",
                Category = EffectCategory.InformationReveal,
                LetterId = _letterId,
                IsInformationRevealed = true
            }
        };
    }
}

/// <summary>
/// Effect that predicts specific consequences if a letter fails to be delivered.
/// This is crucial for strategic planning - knowing what will happen helps prioritize.
/// </summary>
public class PredictConsequenceEffect : IMechanicalEffect
{
    private readonly string _letterId;
    private readonly LetterQueueManager _queueManager;
    private readonly ConsequenceEngine _consequenceEngine;
    private readonly Player _player;

    public PredictConsequenceEffect(
        string letterId,
        LetterQueueManager queueManager,
        ConsequenceEngine consequenceEngine,
        Player player)
    {
        _letterId = letterId;
        _queueManager = queueManager;
        _consequenceEngine = consequenceEngine;
        _player = player;
    }

    public void Apply(ConversationState state)
    {
        var position = _queueManager.GetLetterPosition(_letterId);
        if (!position.HasValue) return;
        
        var letter = _queueManager.GetLetterAt(position.Value);
        
        // Get actual consequence from the letter
        string predictedConsequence = letter.ConsequenceIfLate ?? "Unknown consequences";
        
        // Store as strategic knowledge
        var memory = new MemoryFlag
        {
            Key = $"consequence_prediction_{_letterId}",
            Description = $"If {letter.RecipientName}'s letter fails: {predictedConsequence}",
            Importance = 9,
            ExpirationDay = -1
        };
        
        _player.Memories.Add(memory);
        
        // Also add strategic weight knowledge
        if (letter.Stakes == StakeType.SAFETY)
        {
            _player.Memories.Add(new MemoryFlag
            {
                Key = $"safety_critical_{_letterId}",
                Description = "This is a matter of life and death - failure is catastrophic",
                Importance = 10
            });
        }
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = "Predict failure consequences",
                Category = EffectCategory.InformationGain,
                LetterId = _letterId,
                IsInformationRevealed = true
            }
        };
    }
}

/// <summary>
/// Effect that reveals when NPCs are available at specific locations.
/// Critical for planning delivery routes efficiently.
/// </summary>
public class LearnNPCScheduleEffect : IMechanicalEffect
{
    private readonly string _npcId;
    private readonly GameWorld _gameWorld;
    private readonly Player _player;

    public LearnNPCScheduleEffect(
        string npcId,
        GameWorld gameWorld,
        Player player)
    {
        _npcId = npcId;
        _gameWorld = gameWorld;
        _player = player;
    }

    public void Apply(ConversationState state)
    {
        var npc = _gameWorld.WorldState.NPCs.FirstOrDefault(n => n.ID == _npcId);
        if (npc == null) return;
        
        // Get NPC's schedule
        var schedule = npc.DailySchedule;
        if (schedule == null || schedule.Count() == 0) return;
        
        // Create detailed schedule knowledge
        foreach (var entry in schedule)
        {
            var memory = new MemoryFlag
            {
                Key = $"schedule_{_npcId}_{entry.TimeBlock}",
                Description = $"{npc.Name} is at {entry.LocationId} during {entry.TimeBlock}",
                Importance = 6,
                ExpirationDay = -1
            };
            _player.Memories.Add(memory);
        }
        
        // Add summary knowledge
        var summaryMemory = new MemoryFlag
        {
            Key = $"schedule_summary_{_npcId}",
            Description = $"Learned {npc.Name}'s complete daily schedule",
            Importance = 7
        };
        _player.Memories.Add(summaryMemory);
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = "Learn NPC's daily schedule",
                Category = EffectCategory.InformationGain,
                NpcId = _npcId,
                IsInformationRevealed = true
            }
        };
    }
}

/// <summary>
/// Effect that discovers connections between letters - which letters affect others.
/// Reveals the network of obligations and dependencies.
/// </summary>
public class DiscoverLetterNetworkEffect : IMechanicalEffect
{
    private readonly string _letterId;
    private readonly LetterQueueManager _queueManager;
    private readonly Player _player;

    public DiscoverLetterNetworkEffect(
        string letterId,
        LetterQueueManager queueManager,
        Player player)
    {
        _letterId = letterId;
        _queueManager = queueManager;
        _player = player;
    }

    public void Apply(ConversationState state)
    {
        var position = _queueManager.GetLetterPosition(_letterId);
        if (!position.HasValue) return;
        
        var letter = _queueManager.GetLetterAt(position.Value);
        var allLetters = _queueManager.GetActiveLetters();
        
        // Find related letters (same sender, recipient, or stakes)
        var relatedLetters = allLetters.Where(l => 
            l.Id != _letterId && 
            (l.SenderId == letter.SenderId || 
             l.RecipientId == letter.RecipientId ||
             l.Stakes == letter.Stakes)).ToList();
        
        if (relatedLetters.Any())
        {
            string connections = string.Join(", ", 
                relatedLetters.Select(l => $"{l.SenderName} to {l.RecipientName}"));
            
            var memory = new MemoryFlag
            {
                Key = $"letter_network_{_letterId}",
                Description = $"This letter is connected to: {connections}",
                Importance = 7
            };
            _player.Memories.Add(memory);
        }
        
        // Reveal if this letter affects others
        if (letter.Stakes == StakeType.REPUTATION)
        {
            _player.Memories.Add(new MemoryFlag
            {
                Key = $"reputation_chain_{_letterId}",
                Description = $"Failing this will damage {letter.RecipientName}'s reputation, affecting future letters",
                Importance = 8
            });
        }
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        return new List<MechanicalEffectDescription> {
            new MechanicalEffectDescription {
                Text = "Discover letter connections",
                Category = EffectCategory.InformationGain,
                LetterId = _letterId,
                IsInformationRevealed = true
            }
        };
    }
}

/// <summary>
/// Effect for swapping two letters' positions in the queue.
/// This is a core NEGOTIATE verb effect for queue management.
/// </summary>
public class SwapLetterPositionsEffect : IMechanicalEffect
{
    private readonly string _letterId1;
    private readonly string _letterId2;
    private readonly int _tokenCost;
    private readonly ConnectionType _tokenType;
    private readonly LetterQueueManager _queueManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly string _npcId;

    public SwapLetterPositionsEffect(
        string letterId1,
        string letterId2,
        int tokenCost,
        ConnectionType tokenType,
        LetterQueueManager queueManager,
        TokenMechanicsManager tokenManager,
        string npcId)
    {
        _letterId1 = letterId1;
        _letterId2 = letterId2;
        _tokenCost = tokenCost;
        _tokenType = tokenType;
        _queueManager = queueManager;
        _tokenManager = tokenManager;
        _npcId = npcId;
    }

    public void Apply(ConversationState state)
    {
        // Spend tokens for the swap
        if (_tokenCost > 0)
        {
            bool spent = _tokenManager.SpendTokensWithNPC(_tokenType, _tokenCost, _npcId);
            if (!spent)
            {
                throw new InvalidOperationException($"Failed to spend {_tokenCost} {_tokenType} tokens for letter swap");
            }
        }

        // Get positions of both letters
        int? pos1 = _queueManager.GetLetterPosition(_letterId1);
        int? pos2 = _queueManager.GetLetterPosition(_letterId2);
        
        if (!pos1.HasValue || !pos2.HasValue)
        {
            throw new InvalidOperationException("One or both letters not found in queue");
        }

        // Get the letters
        Letter letter1 = _queueManager.GetLetterAt(pos1.Value);
        Letter letter2 = _queueManager.GetLetterAt(pos2.Value);
        
        // Remove both from queue
        _queueManager.RemoveLetterFromQueue(Math.Max(pos1.Value, pos2.Value));
        _queueManager.RemoveLetterFromQueue(Math.Min(pos1.Value, pos2.Value));
        
        // Add them back in swapped positions
        _queueManager.MoveLetterToPosition(letter2, pos1.Value);
        _queueManager.MoveLetterToPosition(letter1, pos2.Value);
    }

    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {
        var text = _tokenCost > 0 
            ? $"Swap two letters | -{_tokenCost} {_tokenType}"
            : "Swap two letter positions";
            
        var desc = new MechanicalEffectDescription {
            Text = text,
            Category = EffectCategory.LetterSwap
        };
        
        if (_tokenCost > 0)
        {
            desc.TokenType = _tokenType;
            desc.TokenAmount = _tokenCost;
        }
        
        return new List<MechanicalEffectDescription> { desc };
    }
}