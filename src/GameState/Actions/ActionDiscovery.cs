using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Actions.Requirements;

namespace Wayfarer.GameState.Actions;

/// <summary>
/// Discovers available and locked actions, providing visibility into game possibilities
/// </summary>
public class ActionDiscovery
{
    private readonly GameWorld _gameWorld;
    private readonly NPCRepository _npcRepository;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly IGameRuleEngine _ruleEngine;
    private readonly ActionPrerequisiteFactory _prerequisiteFactory;
    
    public ActionDiscovery(
        GameWorld gameWorld,
        NPCRepository npcRepository,
        ConnectionTokenManager tokenManager,
        IGameRuleEngine ruleEngine,
        ActionPrerequisiteFactory prerequisiteFactory)
    {
        _gameWorld = gameWorld;
        _npcRepository = npcRepository;
        _tokenManager = tokenManager;
        _ruleEngine = ruleEngine;
        _prerequisiteFactory = prerequisiteFactory;
    }
    
    /// <summary>
    /// Get all potential actions at a location, both available and locked
    /// </summary>
    public ActionDiscoveryResult DiscoverActions(LocationSpot spot, Player player)
    {
        var result = new ActionDiscoveryResult();
        var currentTimeBlock = _gameWorld.TimeManager.GetCurrentTimeBlock();
        
        // Get NPCs at location
        var npcsHere = _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTimeBlock);
        
        // Discover NPC-based actions
        foreach (var npc in npcsHere)
        {
            DiscoverNPCActions(npc, player, result);
        }
        
        // Discover environmental actions
        DiscoverEnvironmentalActions(spot, player, result);
        
        // Discover patron actions if applicable
        DiscoverPatronActions(spot, player, result);
        
        return result;
    }
    
    private void DiscoverNPCActions(NPC npc, Player player, ActionDiscoveryResult result)
    {
        // Basic conversation - always discoverable
        var converseAction = CreateConverseAction(npc);
        var conversePrereqs = _prerequisiteFactory.CreatePrerequisites(converseAction);
        
        if (conversePrereqs.AllSatisfied(player, _gameWorld))
        {
            result.AvailableActions.Add(converseAction);
        }
        else
        {
            result.LockedActions.Add(new LockedAction
            {
                Action = converseAction,
                Prerequisites = conversePrereqs,
                UnlockHint = "Come back when you have more time",
                Progress = conversePrereqs.Requirements.Average(r => r.GetProgress(player, _gameWorld))
            });
        }
        
        // Socialize - requires relationship
        if (HasRelationship(npc.ID))
        {
            var socializeAction = CreateSocializeAction(npc);
            var socializePrereqs = _prerequisiteFactory.CreatePrerequisites(socializeAction);
            
            if (socializePrereqs.AllSatisfied(player, _gameWorld))
            {
                result.AvailableActions.Add(socializeAction);
            }
            else
            {
                result.LockedActions.Add(new LockedAction
                {
                    Action = socializeAction,
                    Prerequisites = socializePrereqs,
                    UnlockHint = socializePrereqs.Requirements
                        .FirstOrDefault(r => !r.IsSatisfied(player, _gameWorld))
                        ?.GetRemediationHint() ?? "Build relationship first",
                    Progress = socializePrereqs.Requirements.Average(r => r.GetProgress(player, _gameWorld))
                });
            }
        }
        else
        {
            // Show as locked potential
            result.LockedActions.Add(new LockedAction
            {
                Action = CreateSocializeAction(npc),
                Prerequisites = new ActionPrerequisites 
                { 
                    Requirements = { new RelationshipRequirement(npc.ID, 1, null, _npcRepository, _tokenManager) }
                },
                UnlockHint = $"Talk with {npc.Name} first to establish a connection",
                Progress = 0.0
            });
        }
        
        // Work actions - discoverable based on profession
        DiscoverWorkActions(npc, player, result);
        
        // Letter actions - discoverable with relationship
        DiscoverLetterActions(npc, player, result);
    }
    
    private void DiscoverWorkActions(NPC npc, Player player, ActionDiscoveryResult result)
    {
        var workActions = GetProfessionWorkActions(npc);
        
        foreach (var workAction in workActions)
        {
            var prerequisites = _prerequisiteFactory.CreatePrerequisites(workAction);
            
            // Check time-based availability
            if (!IsWorkAvailable(npc, workAction))
            {
                result.LockedActions.Add(new LockedAction
                {
                    Action = workAction,
                    Prerequisites = prerequisites,
                    UnlockHint = GetWorkTimeHint(npc, workAction),
                    Progress = 0.0
                });
                continue;
            }
            
            if (prerequisites.AllSatisfied(player, _gameWorld))
            {
                result.AvailableActions.Add(workAction);
            }
            else
            {
                var failedReq = prerequisites.Requirements
                    .FirstOrDefault(r => !r.IsSatisfied(player, _gameWorld));
                    
                result.LockedActions.Add(new LockedAction
                {
                    Action = workAction,
                    Prerequisites = prerequisites,
                    UnlockHint = failedReq?.GetRemediationHint() ?? "Meet requirements to work",
                    Progress = prerequisites.Requirements.Average(r => r.GetProgress(player, _gameWorld))
                });
            }
        }
    }
    
    private void DiscoverLetterActions(NPC npc, Player player, ActionDiscoveryResult result)
    {
        var tokens = _tokenManager.GetTokensWithNPC(npc.ID);
        var totalTokens = tokens.Values.Sum();
        
        // Show letter opportunities as locked if no relationship
        if (totalTokens == 0)
        {
            result.LockedActions.Add(new LockedAction
            {
                Action = new ActionOption
                {
                    Action = LocationAction.Converse,
                    Name = $"Ask {npc.Name} about letter work",
                    Description = "Potential letter delivery opportunities",
                    HourCost = 1,
                    NPCId = npc.ID
                },
                Prerequisites = new ActionPrerequisites 
                { 
                    Requirements = { new RelationshipRequirement(npc.ID, 1, null, _npcRepository, _tokenManager) }
                },
                UnlockHint = $"Build trust with {npc.Name} first",
                Progress = 0.0
            });
        }
        else
        {
            // Check for actual letter offers
            var letterOfferAction = new ActionOption
            {
                Action = LocationAction.Converse,
                Name = $"Ask {npc.Name} about letter work",
                Description = "Check if they need letters delivered",
                HourCost = 1,
                NPCId = npc.ID,
                IsLetterOffer = true
            };
            
            var prerequisites = _prerequisiteFactory.CreatePrerequisites(letterOfferAction);
            if (prerequisites.AllSatisfied(player, _gameWorld))
            {
                result.AvailableActions.Add(letterOfferAction);
            }
            else
            {
                result.LockedActions.Add(new LockedAction
                {
                    Action = letterOfferAction,
                    Prerequisites = prerequisites,
                    UnlockHint = "Need more time or resources",
                    Progress = prerequisites.Requirements.Average(r => r.GetProgress(player, _gameWorld))
                });
            }
        }
    }
    
    private void DiscoverEnvironmentalActions(LocationSpot spot, Player player, ActionDiscoveryResult result)
    {
        if (!spot.DomainTags.Any()) return;
        
        // Resource gathering
        if (spot.DomainTags.Contains("RESOURCES"))
        {
            var gatherBerries = new ActionOption
            {
                Action = LocationAction.GatherResources,
                Name = "Gather wild berries",
                Description = "Search the area for edible berries",
                HourCost = 1,
                StaminaCost = 1,
                Effect = "+2 food items"
            };
            
            var prerequisites = _prerequisiteFactory.CreatePrerequisites(gatherBerries);
            if (prerequisites.AllSatisfied(player, _gameWorld))
            {
                result.AvailableActions.Add(gatherBerries);
            }
            else
            {
                result.LockedActions.Add(new LockedAction
                {
                    Action = gatherBerries,
                    Prerequisites = prerequisites,
                    UnlockHint = "Need energy to gather resources",
                    Progress = prerequisites.Requirements.Average(r => r.GetProgress(player, _gameWorld))
                });
            }
        }
        
        // Market browsing
        if (spot.DomainTags.Contains("COMMERCE"))
        {
            var browseMarket = new ActionOption
            {
                Action = LocationAction.Browse,
                Name = "Browse market stalls",
                Description = "Check prices and available goods",
                HourCost = 1,
                Effect = "Learn current market prices"
            };
            
            var prerequisites = _prerequisiteFactory.CreatePrerequisites(browseMarket);
            if (prerequisites.AllSatisfied(player, _gameWorld))
            {
                result.AvailableActions.Add(browseMarket);
            }
            else
            {
                result.LockedActions.Add(new LockedAction
                {
                    Action = browseMarket,
                    Prerequisites = prerequisites,
                    UnlockHint = "Come back when you have time",
                    Progress = prerequisites.Requirements.Average(r => r.GetProgress(player, _gameWorld))
                });
            }
        }
    }
    
    private void DiscoverPatronActions(LocationSpot spot, Player player, ActionDiscoveryResult result)
    {
        // Can write to patron at desk locations
        if (spot.SpotID.Contains("room") || spot.SpotID.Contains("desk") || spot.SpotID.Contains("study"))
        {
            var requestFunds = new ActionOption
            {
                Action = LocationAction.RequestPatronFunds,
                Name = "Write to patron requesting funds",
                Description = "Receive 30 coins, -1 Patron leverage",
                HourCost = 1,
                Effect = "30 coins, -1 Noble token with patron"
            };
            
            var prerequisites = _prerequisiteFactory.CreatePrerequisites(requestFunds);
            if (prerequisites.AllSatisfied(player, _gameWorld))
            {
                result.AvailableActions.Add(requestFunds);
            }
            else
            {
                result.LockedActions.Add(new LockedAction
                {
                    Action = requestFunds,
                    Prerequisites = prerequisites,
                    UnlockHint = "Need time to write a letter",
                    Progress = prerequisites.Requirements.Average(r => r.GetProgress(player, _gameWorld))
                });
            }
        }
    }
    
    private bool HasRelationship(string npcId)
    {
        var tokens = _tokenManager.GetTokensWithNPC(npcId);
        return tokens.Values.Sum() > 0;
    }
    
    private List<ActionOption> GetProfessionWorkActions(NPC npc)
    {
        var actions = new List<ActionOption>();
        
        switch (npc.Profession)
        {
            case Professions.Merchant:
                actions.Add(new ActionOption
                {
                    Action = LocationAction.Work,
                    Name = $"Help {npc.Name} with inventory",
                    Description = "Earn coins through honest labor",
                    HourCost = 1,
                    StaminaCost = 2,
                    NPCId = npc.ID,
                    Effect = "+4 coins"
                });
                break;
                
            case Professions.TavernKeeper:
                actions.Add(new ActionOption
                {
                    Action = LocationAction.Work,
                    Name = $"Serve drinks for {npc.Name}",
                    Description = "Help with the evening rush",
                    HourCost = 1,
                    StaminaCost = 2,
                    NPCId = npc.ID,
                    Effect = "+4 coins"
                });
                break;
                
            case Professions.Scribe:
                actions.Add(new ActionOption
                {
                    Action = LocationAction.Work,
                    Name = $"Copy documents for {npc.Name}",
                    Description = "Careful work with quill and ink",
                    HourCost = 1,
                    StaminaCost = 1,
                    NPCId = npc.ID,
                    Effect = "+3 coins"
                });
                break;
        }
        
        return actions;
    }
    
    private bool IsWorkAvailable(NPC npc, ActionOption workAction)
    {
        var currentTime = _gameWorld.TimeManager.GetCurrentTimeBlock();
        
        return npc.Profession switch
        {
            Professions.Merchant => currentTime == TimeBlocks.Morning || currentTime == TimeBlocks.Afternoon,
            Professions.TavernKeeper => currentTime == TimeBlocks.Evening,
            Professions.Innkeeper => currentTime == TimeBlocks.Morning,
            _ => true
        };
    }
    
    private string GetWorkTimeHint(NPC npc, ActionOption workAction)
    {
        return npc.Profession switch
        {
            Professions.Merchant => "Merchants need help during business hours (morning/afternoon)",
            Professions.TavernKeeper => "Taverns are busiest in the evening",
            Professions.Innkeeper => "Room cleaning happens in the morning",
            _ => "Come back at the right time"
        };
    }
    
    private ActionOption CreateConverseAction(NPC npc)
    {
        return new ActionOption
        {
            Action = LocationAction.Converse,
            Name = $"Talk with {npc.Name}",
            Description = npc.Description,
            HourCost = 1,
            NPCId = npc.ID
        };
    }
    
    private ActionOption CreateSocializeAction(NPC npc)
    {
        var tokenType = npc.LetterTokenTypes.FirstOrDefault();
        return new ActionOption
        {
            Action = LocationAction.Socialize,
            Name = $"Spend time with {npc.Name}",
            Description = "Deepen your connection",
            HourCost = 1,
            NPCId = npc.ID,
            Effect = $"+1 {tokenType} token"
        };
    }
}

/// <summary>
/// Result of action discovery
/// </summary>
public class ActionDiscoveryResult
{
    public List<ActionOption> AvailableActions { get; set; } = new();
    public List<LockedAction> LockedActions { get; set; } = new();
    
    public int TotalActionCount => AvailableActions.Count + LockedActions.Count;
}

/// <summary>
/// Represents an action that is currently locked
/// </summary>
public class LockedAction
{
    public ActionOption Action { get; set; }
    public ActionPrerequisites Prerequisites { get; set; }
    public string UnlockHint { get; set; }
    public double Progress { get; set; } // 0.0 to 1.0
    
    /// <summary>
    /// Get a visual representation of progress
    /// </summary>
    public string GetProgressBar(int width = 10)
    {
        var filled = (int)(Progress * width);
        var empty = width - filled;
        return $"[{'='.Repeat(filled)}{'-'.Repeat(empty)}]";
    }
}