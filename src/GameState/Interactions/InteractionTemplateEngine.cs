using System.Text.RegularExpressions;

/// <summary>
/// Generates interaction content from templates to avoid content explosion
/// This is the KEY to making the system work without 400+ hours of content
/// </summary>
public class InteractionTemplateEngine
{
    private readonly Random _random = new();

    // Template patterns for different interaction types
    private readonly Dictionary<InteractionType, List<string>> _templates = new()
    {
        [InteractionType.ObserveEnvironment] = new()
        {
            "You notice {detail} near {location_feature}",
            "The {ambient_quality} reveals {detail}",
            "Something about {location_feature} catches your attention",
            "{detail} becomes apparent as you focus"
        },

        [InteractionType.ObserveNPC] = new()
        {
            "{npc_name}'s {body_part} {action_verb}, suggesting {emotion}",
            "You notice {npc_name} {subtle_action}",
            "{npc_name} seems {emotional_state}, judging by {tell}",
            "The way {npc_name} {action_verb} reveals {insight}"
        },

        [InteractionType.ConversationHelp] = new()
        {
            "Offer assistance with {problem}",
            "Help {npc_name} with their {concern}",
            "Provide aid regarding {topic}",
            "Lend support for {situation}"
        },

        [InteractionType.ConversationNegotiate] = new()
        {
            "Negotiate about {topic}",
            "Bargain regarding {issue}",
            "Discuss terms for {arrangement}",
            "Propose a deal about {matter}"
        },

        [InteractionType.ConversationInvestigate] = new()
        {
            "Ask about {topic}",
            "Investigate {mystery}",
            "Probe deeper into {subject}",
            "Inquire about {rumor}"
        }
    };

    // Context vocabularies for template filling
    private readonly Dictionary<string, List<string>> _vocabularies = new()
    {
        ["detail"] = new() { "worn scratches", "fresh mud", "a dropped kerchief", "nervous movement", "hurried whispers" },
        ["location_feature"] = new() { "the fountain", "the doorway", "the market stalls", "the cobblestones", "the shadows" },
        ["ambient_quality"] = new() { "morning light", "bustling crowd", "sudden quiet", "shifting shadows", "cool breeze" },
        ["body_part"] = new() { "hands", "shoulders", "eyes", "posture", "expression" },
        ["action_verb"] = new() { "trembles", "tightens", "shifts", "relaxes", "hardens" },
        ["emotion"] = new() { "anxiety", "determination", "exhaustion", "hope", "suspicion" },
        ["subtle_action"] = new() { "glancing at the door", "fidgeting with their purse", "watching you carefully", "avoiding eye contact" },
        ["emotional_state"] = new() { "troubled", "relieved", "calculating", "desperate", "guarded" },
        ["tell"] = new() { "their clenched jaw", "the way they hold themselves", "their rapid breathing", "their forced smile" },
        ["insight"] = new() { "their true priorities", "hidden concerns", "unspoken fears", "desperate hope" }
    };

    /// <summary>
    /// Generate an interactive choice from a template and context
    /// This is where we avoid writing 400+ hours of unique content
    /// </summary>
    public IInteractiveChoice GenerateChoice(
        InteractionType type,
        Dictionary<string, string> context,
        int attentionCost = 1,
        int timeCost = 0)
    {
        string template = SelectTemplate(type);
        string text = FillTemplate(template, context);

        return new TemplatedChoice
        {
            Id = $"{type}_{Guid.NewGuid():N}",
            DisplayText = text,
            AttentionCost = attentionCost,
            TimeCostMinutes = timeCost,
            Type = type,
            MechanicalPreviews = GeneratePreviews(type, context),
            IsAvailable = true,
            Style = DetermineStyle(type, context)
        };
    }

    /// <summary>
    /// Generate observation choices for a location
    /// This creates varied content without manual writing
    /// </summary>
    public List<IInteractiveChoice> GenerateLocationObservations(
        Location location,
        LocationSpot spot,
        List<NPC> presentNPCs,
        TimeBlocks timeBlock)
    {
        List<IInteractiveChoice> choices = new List<IInteractiveChoice>();

        // Environmental observations based on location tags
        if (spot.DomainTags?.Contains("Crowded") == true)
        {
            choices.Add(GenerateChoice(
                InteractionType.ObserveEnvironment,
                new()
                {
                    ["location_feature"] = "the crowd",
                    ["detail"] = "patterns in people's movement"
                },
                attentionCost: 1
            ));
        }

        if (spot.DomainTags?.Contains("Shadowed") == true)
        {
            choices.Add(GenerateChoice(
                InteractionType.ObserveEnvironment,
                new()
                {
                    ["location_feature"] = "the dark corners",
                    ["detail"] = "someone watching"
                },
                attentionCost: 1
            ));
        }

        // NPC observations
        foreach (NPC? npc in presentNPCs.Take(2)) // Limit to avoid choice overload
        {
            Dictionary<string, string> emotionContext = DetermineNPCContext(npc);
            choices.Add(GenerateChoice(
                InteractionType.ObserveNPC,
                emotionContext,
                attentionCost: 1
            ));
        }

        return choices;
    }

    /// <summary>
    /// Generate conversation choices with appropriate depth
    /// </summary>
    public List<IInteractiveChoice> GenerateConversationChoices(
        NPC npc,
        SceneContext context,
        AttentionManager attention)
    {
        List<IInteractiveChoice> choices = new List<IInteractiveChoice>();

        // Always include a free option
        choices.Add(new TemplatedChoice
        {
            Id = "free_acknowledge",
            DisplayText = "\"I understand.\"",
            AttentionCost = 0,
            Type = InteractionType.ConversationFree,
            IsAvailable = true,
            Style = InteractionStyle.Default
        });

        // Add verb-based choices if attention available
        if (attention.GetAvailableAttention() >= 1)
        {
            choices.Add(GenerateChoice(
                InteractionType.ConversationHelp,
                new() { ["npc_name"] = npc.Name, ["concern"] = "their immediate problem" },
                attentionCost: 1
            ));
        }

        if (attention.GetAvailableAttention() >= 2)
        {
            choices.Add(GenerateChoice(
                InteractionType.ConversationNegotiate,
                new() { ["topic"] = "letter queue positions" },
                attentionCost: 2
            ));
        }

        if (attention.GetAvailableAttention() >= 3)
        {
            choices.Add(GenerateChoice(
                InteractionType.ConversationInvestigate,
                new() { ["mystery"] = "the real situation" },
                attentionCost: 3
            ));
        }

        return choices;
    }

    private string SelectTemplate(InteractionType type)
    {
        if (!_templates.ContainsKey(type))
            return "{placeholder}";

        List<string> templates = _templates[type];
        return templates[_random.Next(templates.Count)];
    }

    private string FillTemplate(string template, Dictionary<string, string> context)
    {
        string result = template;

        // First fill from provided context
        foreach (KeyValuePair<string, string> kvp in context)
        {
            result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
        }

        // Then fill remaining placeholders from vocabularies
        MatchCollection placeholders = Regex.Matches(result, @"\{(\w+)\}");
        foreach (Match match in placeholders)
        {
            string key = match.Groups[1].Value;
            if (_vocabularies.ContainsKey(key))
            {
                List<string> options = _vocabularies[key];
                string value = options[_random.Next(options.Count)];
                result = result.Replace($"{{{key}}}", value);
            }
        }

        return result;
    }

    private List<string> GeneratePreviews(InteractionType type, Dictionary<string, string> context)
    {
        return type switch
        {
            InteractionType.ConversationHelp => new() { "✓ Builds trust", "⏱ Takes time" },
            InteractionType.ConversationNegotiate => new() { "↔ Queue management", "⚠ May cost tokens" },
            InteractionType.ConversationInvestigate => new() { "ℹ Gain information", "? Uncertain outcome" },
            InteractionType.ObserveEnvironment => new() { "👁 Notice details" },
            InteractionType.ObserveNPC => new() { "😊 Read emotions" },
            _ => new()
        };
    }

    private InteractionStyle DetermineStyle(InteractionType type, Dictionary<string, string> context)
    {
        if (context.ContainsKey("urgent"))
            return InteractionStyle.Urgent;
        if (context.ContainsKey("beneficial"))
            return InteractionStyle.Beneficial;
        if (type.ToString().Contains("Investigate") || type.ToString().Contains("Observe"))
            return InteractionStyle.Mysterious;
        return InteractionStyle.Default;
    }

    private Dictionary<string, string> DetermineNPCContext(NPC npc)
    {
        // Generate appropriate context based on NPC state
        // TODO: Get actual emotional state from NPCStateResolver
        return new()
        {
            ["npc_name"] = npc.Name,
            ["emotional_state"] = "anxious", // Default for now
            ["body_part"] = "hands",
            ["action_verb"] = "fidgets"
        };
    }
}

/// <summary>
/// Concrete implementation of a templated choice
/// </summary>
public class TemplatedChoice : IInteractiveChoice
{
    public string Id { get; set; }
    public string DisplayText { get; set; }
    public int AttentionCost { get; set; }
    public int TimeCostMinutes { get; set; }
    public InteractionType Type { get; set; }
    public List<string> MechanicalPreviews { get; set; } = new();
    public bool IsAvailable { get; set; }
    public string LockReason { get; set; }
    public InteractionStyle Style { get; set; }

    public InteractionResult Execute(GameWorld gameWorld, AttentionManager attention)
    {
        // Deduct attention cost
        if (AttentionCost > 0 && !attention.TrySpend(AttentionCost))
        {
            return new InteractionResult
            {
                Success = false,
                NarrativeText = "You're too mentally exhausted to focus on this."
            };
        }

        // Execute based on type
        InteractionResult result = Type switch
        {
            InteractionType.ObserveEnvironment => ExecuteEnvironmentObservation(gameWorld),
            InteractionType.ObserveNPC => ExecuteNPCObservation(gameWorld),
            InteractionType.ConversationHelp => ExecuteHelpAction(gameWorld),
            InteractionType.ConversationNegotiate => ExecuteNegotiateAction(gameWorld),
            InteractionType.ConversationInvestigate => ExecuteInvestigateAction(gameWorld),
            _ => new InteractionResult { Success = true, NarrativeText = "You take the action." }
        };

        return result;
    }

    private InteractionResult ExecuteEnvironmentObservation(GameWorld gameWorld)
    {
        return new InteractionResult
        {
            Success = true,
            NarrativeText = "You focus your attention and notice something interesting.",
            SystemMessages = { "Gained insight about the location." }
        };
    }

    private InteractionResult ExecuteNPCObservation(GameWorld gameWorld)
    {
        return new InteractionResult
        {
            Success = true,
            NarrativeText = "You carefully observe their behavior and understand more about their state.",
            SystemMessages = { "Learned about NPC's emotional state." }
        };
    }

    private InteractionResult ExecuteHelpAction(GameWorld gameWorld)
    {
        return new InteractionResult
        {
            Success = true,
            NarrativeText = "You offer your assistance.",
            StateChanges = { ["trust_gained"] = 1 }
        };
    }

    private InteractionResult ExecuteNegotiateAction(GameWorld gameWorld)
    {
        return new InteractionResult
        {
            Success = true,
            NarrativeText = "You negotiate terms.",
            StateChanges = { ["queue_modified"] = true }
        };
    }

    private InteractionResult ExecuteInvestigateAction(GameWorld gameWorld)
    {
        return new InteractionResult
        {
            Success = true,
            NarrativeText = "You dig deeper and uncover valuable information.",
            StateChanges = { ["information_gained"] = "secret_discovered" }
        };
    }
}