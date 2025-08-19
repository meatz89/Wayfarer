using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.GameState;
using Wayfarer.GameState.Interactions;
using Wayfarer.ViewModels;

public class ConversationScreenBase : ComponentBase
{
    [Inject] protected GameFacade GameFacade { get; set; }
    [Inject] protected NavigationManager Navigation { get; set; }

    [Parameter] public string NpcId { get; set; }
    [Parameter] public Action OnConversationEnd { get; set; }
    [Parameter] public EventCallback<CurrentViews> OnNavigate { get; set; }

    protected ConversationViewModel Model { get; set; }
    protected List<ConversationChoice> Choices { get; set; }
    protected int CurrentPatience { get; set; }
    protected int MaxPatience { get; set; }
    protected int CurrentComfort { get; set; }
    protected Dictionary<ConnectionType, int> NpcTokens { get; set; }
    protected ConversationChoice DefaultChoice => GetDefaultChoice();

    // Transparent mechanics properties
    protected NPCEmotionalState? CurrentEmotionalState { get; set; }
    protected StakeType? CurrentStakes { get; set; }
    protected TimeSpan? TimeToDeadline { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadConversation();
        RefreshAllState(); // Use single state refresh pattern
    }

    protected async Task LoadConversation()
    {
        try
        {
            // First try to get the current pending conversation
            Model = GameFacade.GetCurrentConversation();

            // If no pending conversation, start a new one
            if (Model == null && !string.IsNullOrEmpty(NpcId))
            {
                Console.WriteLine($"[ConversationScreen] No pending conversation found, starting new conversation with NPC: {NpcId}");
                Model = await GameFacade.GetConversationAsync(NpcId);
            }

            if (Model == null)
            {
                Console.WriteLine($"[ConversationScreen] ERROR: Could not load conversation model");
            }
            else
            {
                Console.WriteLine($"[ConversationScreen] Loaded conversation with NPC: {Model.NpcId}, Text: {Model.CurrentText?.Substring(0, Math.Min(50, Model.CurrentText?.Length ?? 0))}...");
                await GenerateChoicesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading conversation: {ex.Message}");
        }
    }

    protected void RefreshConversationState()
    {
        // Get NPC patience and comfort for conversation UI display from GameFacade
        ConversationManager conversationManager = GameFacade.GetCurrentConversationManager();
        ConversationState? conversationState = conversationManager?.State;
        
        Console.WriteLine($"[ConversationScreen.RefreshConversationState] ConversationManager exists: {conversationManager != null}");
        Console.WriteLine($"[ConversationScreen.RefreshConversationState] ConversationState exists: {conversationState != null}");
        
        if (conversationState != null)
        {
            // Map FocusPoints to Patience for UI display (these represent NPC patience in conversation)
            CurrentPatience = conversationState.FocusPoints;
            MaxPatience = conversationState.MaxFocusPoints;
            // Use actual comfort tracking from conversation state
            CurrentComfort = conversationState.TotalComfort;
            
            Console.WriteLine($"[ConversationScreen.RefreshConversationState] UPDATED UI STATE - Patience: {CurrentPatience}/{MaxPatience}, Comfort: {CurrentComfort}");
            Console.WriteLine($"[ConversationScreen.RefreshConversationState] Source values - FocusPoints: {conversationState.FocusPoints}, MaxFocusPoints: {conversationState.MaxFocusPoints}, TotalComfort: {conversationState.TotalComfort}");
        }
        else
        {
            // Default values if conversation state not available
            CurrentPatience = 3;
            MaxPatience = 3;
            CurrentComfort = 0;
            Console.WriteLine($"[ConversationScreen.RefreshConversationState] WARNING: No conversation state found, using defaults - Patience: {CurrentPatience}/{MaxPatience}, Comfort: {CurrentComfort}");
        }
    }

    protected async Task GenerateChoicesAsync()
    {
        // TEMPORARY: Force fresh choice generation from NPCDeck for testing categorical card coloring
        Choices = await GenerateCardBasedChoicesAsync();
        Console.WriteLine($"[ConversationScreen] TESTING: Generated {Choices.Count} fresh choices from deck");
        
        // OLD CODE (commented for testing):
        // Get choices from the current conversation state (filtered by previous selections)
        // ConversationManager? currentConversation = GameFacade.GetCurrentConversationManager();
        // if (currentConversation?.Choices != null && currentConversation.Choices.Any())
        // {
        //     // Use the filtered choices from conversation state
        //     Choices = currentConversation.Choices.Where(c => c.IsAvailable).ToList();
        //     Console.WriteLine($"[ConversationScreen] Using {Choices.Count} filtered choices from conversation state");
        // }
        // else
        // {
        //     // Fall back to generating fresh choices if conversation state has none
        //     Choices = await GenerateCardBasedChoicesAsync();
        //     Console.WriteLine($"[ConversationScreen] Generated {Choices.Count} fresh choices from deck");
        // }
    }

    protected async Task HandleChoice(ConversationChoice choice)
    {
        await SelectChoice(choice.ChoiceID);
        // SelectChoice handles ALL state updates - no redundant calls needed
    }

    protected async Task SelectChoice(string choiceId)
    {
        try
        {
            Console.WriteLine($"[ConversationScreen] SelectChoice called with: {choiceId}");

            // Process the choice through GameFacade
            ConversationViewModel updatedModel = await GameFacade.ProcessConversationChoice(choiceId);

            if (updatedModel != null)
            {
                Model = updatedModel;
                await GenerateChoicesAsync();
                RefreshAllState(); // Single atomic state refresh
                StateHasChanged(); // Single UI update
                Console.WriteLine($"[ConversationScreen] Conversation updated after choice");
            }
            else
            {
                Console.WriteLine($"[ConversationScreen] Conversation ended after choice");
                // Notify parent that conversation has ended
                OnConversationEnd?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error selecting choice: {ex.Message}");
        }
    }

    protected ConversationChoice GetDefaultChoice()
    {
        return new ConversationChoice
        {
            ChoiceID = "default",
            NarrativeText = "I understand. I'll see what I can do.",
            PatienceCost = 0,
            IsAvailable = true,
            MechanicalDescription = "→ Maintains current state",
            MechanicalEffects = new List<IMechanicalEffect>()
        };
    }

    /// <summary>
    /// Get success probability display for a choice
    /// </summary>
    protected string GetSuccessProbabilityDisplay(ConversationChoice choice)
    {
        try
        {
            Console.WriteLine($"[ConversationScreen.GetSuccessProbabilityDisplay] Called for choice: {choice.NarrativeText}, PatienceCost: {choice.PatienceCost}");
            Console.WriteLine($"[ConversationScreen.GetSuccessProbabilityDisplay] CurrentPatience: {CurrentPatience}");
            
            // Use the same calculator as the backend
            var outcomeCalculator = new Wayfarer.Game.ConversationSystem.ConversationOutcomeCalculator();
            
            // Get current conversation state
            ConversationManager? currentConversation = GameFacade.GetCurrentConversationManager();
            if (currentConversation?.Context?.TargetNPC == null)
            {
                Console.WriteLine($"[ConversationScreen.GetSuccessProbabilityDisplay] No conversation context - returning empty");
                return ""; // No probability display if no conversation
            }
                
            NPC npc = currentConversation.Context.TargetNPC;
            Player player = GameFacade.GetPlayer();
            int currentPatience = CurrentPatience;

            Console.WriteLine($"[ConversationScreen.GetSuccessProbabilityDisplay] About to call CalculateProbabilities with patience: {currentPatience}");

            // Calculate probabilities
            var probabilities = outcomeCalculator.CalculateProbabilities(choice, npc, player, currentPatience);
            
            Console.WriteLine($"[ConversationScreen.GetSuccessProbabilityDisplay] Got result: {probabilities.SuccessChance}% Success");
            
            // Return exact percentage for transparent mechanics
            return $"{probabilities.SuccessChance}% Success";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ConversationScreen.GetSuccessProbabilityDisplay] Exception: {ex.Message}");
            return "Unknown difficulty";
        }
    }

    /// <summary>
    /// Get outcome preview for Success/Neutral/Failure with individual effect coloring
    /// </summary>
    protected string GetOutcomePreview(ConversationChoice choice, Wayfarer.Game.ConversationSystem.ConversationOutcome outcome)
    {
        try
        {
            var outcomeCalculator = new Wayfarer.Game.ConversationSystem.ConversationOutcomeCalculator();
            ConversationManager? currentConversation = GameFacade.GetCurrentConversationManager();
            
            if (currentConversation?.Context?.TargetNPC == null)
                return "";
                
            NPC npc = currentConversation.Context.TargetNPC;
            Player player = GameFacade.GetPlayer();
            
            // Calculate what would happen with this outcome
            var result = outcomeCalculator.CalculateResult(choice, outcome, npc, player);
            
            List<string> effects = new List<string>();
            
            // Comfort effects with individual coloring
            if (result.ComfortGain > 0)
                effects.Add($"<span class=\"effect-positive\">+{result.ComfortGain} comfort</span>");
            else if (result.ComfortGain < 0)
                effects.Add($"<span class=\"effect-negative\">{result.ComfortGain} comfort</span>");
            
            // Token effects with individual coloring
            if (result.TokenChanges?.Any() == true)
            {
                foreach (var tokenChange in result.TokenChanges)
                {
                    string tokenName = tokenChange.Key.ToString();
                    string changeStr = tokenChange.Value > 0 ? $"+{tokenChange.Value}" : tokenChange.Value.ToString();
                    string effectClass = tokenChange.Value > 0 ? "effect-positive" : "effect-negative";
                    effects.Add($"<span class=\"{effectClass}\">{changeStr} {tokenName}</span>");
                }
            }
            
            if (effects.Any())
                return string.Join(" • ", effects);
            else
                return "<span class=\"effect-neutral\">No change</span>";
        }
        catch (Exception)
        {
            return "<span class=\"effect-neutral\">Unknown effect</span>";
        }
    }

    /// <summary>
    /// Get token relationship bonds display
    /// </summary>
    protected string GetTokenBondsDisplay()
    {
        if (NpcTokens?.Any() != true) return "";
        
        List<string> bonds = new List<string>();
        
        foreach (var tokenType in Enum.GetValues<ConnectionType>())
        {
            if (NpcTokens.TryGetValue(tokenType, out int value) && value != 0)
            {
                string icon = GetTokenIcon(tokenType);
                string dots = GetTokenDots(value);
                bonds.Add($"{icon} {dots}");
            }
        }
        
        return string.Join(" ", bonds);
    }
    
    protected string GetTokenIcon(ConnectionType tokenType)
    {
        return tokenType switch
        {
            ConnectionType.Trust => "💝",
            ConnectionType.Commerce => "🤝", 
            ConnectionType.Status => "👑",
            ConnectionType.Shadow => "🌑",
            _ => "❓"
        };
    }
    
    protected string GetTokenDots(int value)
    {
        int absValue = Math.Abs(value);
        int maxDots = 5; // Show up to 5 dots
        string dot = value >= 0 ? "●" : "●"; // Use same dot, color with CSS
        string emptyDot = "○";
        
        string dots = "";
        for (int i = 0; i < Math.Min(absValue, maxDots); i++)
        {
            dots += dot;
        }
        for (int i = absValue; i < maxDots; i++)
        {
            dots += emptyDot;
        }
        
        return dots;
    }
    
    /// <summary>
    /// Handle peripheral environmental investigation - costs attention
    /// </summary>
    protected async Task InvestigateEnvironment(string hint)
    {
        try
        {
            // This would spend attention to investigate environmental details
            // For now, just log that we're investigating
            Console.WriteLine($"[ConversationScreen] Investigating environmental hint: {hint}");
            
            // TODO: Implement attention spending and reveal environmental details
            // This might reveal new conversation options, location actions, or story information
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error investigating environment: {ex.Message}");
        }
    }

    protected async Task HandleExitConversation()
    {
        try
        {
            Console.WriteLine($"[ConversationScreen] HandleExitConversation - manually exiting conversation");

            // End the conversation through GameFacade
            await GameFacade.EndConversationAsync();

            // Notify parent that conversation has ended
            OnConversationEnd?.Invoke();

            // Also trigger navigation if we have the callback
            if (OnNavigate.HasDelegate)
            {
                await OnNavigate.InvokeAsync(CurrentViews.LocationScreen);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exiting conversation: {ex.Message}");
        }
    }

    protected void LoadTokenData()
    {
        if (!string.IsNullOrEmpty(NpcId))
        {
            NPCTokenBalance tokenBalance = GameFacade.GetTokensWithNPC(NpcId);
            NpcTokens = new Dictionary<ConnectionType, int>();

            // Convert NPCTokenBalance to Dictionary
            if (tokenBalance != null && tokenBalance.Balances != null)
            {
                foreach (TokenBalance balance in tokenBalance.Balances)
                {
                    NpcTokens[balance.TokenType] = balance.Amount;
                }
            }

            // Ensure all token types are represented
            foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
            {
                if (!NpcTokens.ContainsKey(tokenType))
                {
                    NpcTokens[tokenType] = 0;
                }
            }
        }
    }

    protected string GetCurrentConversationContext()
    {
        // Determine context from NPC emotional state - no string matching
        if (CurrentEmotionalState.HasValue)
        {
            return CurrentEmotionalState.Value switch
            {
                NPCEmotionalState.DESPERATE => "help",      // Desperate NPCs need assistance
                NPCEmotionalState.ANXIOUS => "help",        // Anxious NPCs need comfort
                NPCEmotionalState.CALCULATING => "negotiate", // Calculating NPCs prefer deals
                NPCEmotionalState.HOSTILE => "negotiate",   // Hostile NPCs require negotiation
                NPCEmotionalState.WITHDRAWN => "investigate", // Withdrawn NPCs need probing
                _ => "help"
            };
        }

        // Fallback: Default context based on categorical mechanics
        return "help"; // Default context when no emotional state available
    }

    protected string GetNpcRole()
    {
        // Determine NPC role from their ID or other context
        // This would ideally come from NPC data, but we can infer from name/ID
        if (string.IsNullOrEmpty(NpcId)) return null;

        string lowerNpcId = NpcId.ToLower();

        if (lowerNpcId.Contains("merchant") || lowerNpcId.Contains("trader") || lowerNpcId.Contains("shop"))
            return "merchant";
        if (lowerNpcId.Contains("noble") || lowerNpcId.Contains("lord") || lowerNpcId.Contains("lady"))
            return "noble";
        if (lowerNpcId.Contains("guard") || lowerNpcId.Contains("captain") || lowerNpcId.Contains("soldier"))
            return "guard";
        if (lowerNpcId.Contains("friend") || lowerNpcId == "elena" || lowerNpcId == "tam")
            return "friend";
        if (lowerNpcId.Contains("shadow") || lowerNpcId.Contains("informant"))
            return "informant";

        // Check NPC name if available
        if (Model?.NpcName != null)
        {
            string lowerName = Model.NpcName.ToLower();
            if (lowerName.Contains("merchant") || lowerName.Contains("trader"))
                return "merchant";
            if (lowerName.Contains("lord") || lowerName.Contains("lady"))
                return "noble";
            if (lowerName == "elena" || lowerName == "tam")
                return "friend";
        }

        return null; // Unknown role
    }


    /// <summary>
    /// SINGLE POINT state refresh - eliminates race conditions
    /// All state updates happen atomically in correct order
    /// </summary>
    protected void RefreshAllState()
    {
        LoadTokenData();
        RefreshConversationState();
        LoadEmotionalStateData();
        Console.WriteLine($"[ConversationScreen] State refreshed - Patience: {CurrentPatience}/{MaxPatience}, Comfort: {CurrentComfort}");
    }

    protected void LoadEmotionalStateData()
    {
        if (Model == null) return;

        // Use the emotional state from the ViewModel (calculated by NPCStateResolver)
        if (Model.EmotionalState.HasValue)
        {
            CurrentEmotionalState = Model.EmotionalState.Value;
            CurrentStakes = Model.CurrentStakes;

            if (Model.HoursToDeadline.HasValue)
            {
                TimeToDeadline = TimeSpan.FromHours(Model.HoursToDeadline.Value);
            }
        }
    }


    private StakeType DetermineStakesFromLetter(LetterViewModel letter)
    {
        // Infer stakes from letter properties
        // Since we don't have subject/content, use what we have

        // Check deadline urgency
        if (letter.DeadlineInHours < 4)
            return StakeType.SAFETY; // Very urgent = safety at stake

        // Check payment amount
        if (letter.Payment > 50)
            return StakeType.WEALTH; // High payment = financial stakes

        // Check if special letter
        if (letter.IsSpecial)
            return StakeType.SECRET; // Special letters often carry secrets

        // Default to reputation
        return StakeType.REPUTATION;
    }

    protected string GetStakesDescription()
    {
        if (CurrentStakes == null) return "No urgent letter";

        // Get the letter from the queue if it exists
        LetterQueueViewModel queue = GameFacade.GetLetterQueue();
        LetterViewModel? npcLetter = queue?.QueueSlots?.FirstOrDefault(s => s.Letter?.SenderName == NpcId)?.Letter;
        string subject = npcLetter?.SenderName ?? "matter";

        return CurrentStakes switch
        {
            StakeType.REPUTATION => $"Reputation at stake ({subject})",
            StakeType.WEALTH => $"Financial matter ({subject})",
            StakeType.SAFETY => $"Safety concerns ({subject})",
            StakeType.SECRET => $"Sensitive information ({subject})",
            _ => "Unknown stakes"
        };
    }

    protected TimeSpan? GetTimeRemaining()
    {
        return TimeToDeadline;
    }

    /// <summary>
    /// Generate enhanced conversation choices using the card deck system
    /// This integrates with the backend ConversationChoiceGenerator
    /// </summary>
    protected async Task<List<ConversationChoice>> GenerateCardBasedChoicesAsync()
    {
        try
        {
            // HIGHLANDER PRINCIPLE: Model.NpcId is the ONLY source of truth
            if (string.IsNullOrEmpty(Model?.NpcId))
            {
                throw new InvalidOperationException("Cannot generate conversation choices: Model.NpcId is required");
            }

            // Get real card-based choices from GameFacade
            List<ConversationChoice> cardChoices = await GameFacade.GetConversationChoicesFromDeckAsync(Model.NpcId);

            if (cardChoices?.Any() == true)
            {
                Console.WriteLine($"[ConversationScreen] Generated {cardChoices.Count} card-based choices for {Model.NpcId}");
                return cardChoices;
            }
            else
            {
                throw new InvalidOperationException($"NPC deck failed to generate choices for {Model.NpcId} - conversation system error");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error generating card-based choices: {ex.Message}");
            throw; // Let the exception bubble up - no fallbacks allowed
        }
    }


    // UI Helper methods for conversation choices
    protected string GetCostDisplay(int cost)
    {
        // Return just the number - CSS will handle beautiful diamond icons
        return cost.ToString();
    }

    protected List<string> ParseMechanicalDescription(string description)
    {
        // Split by pipe (|) for multiple effects and add relationship impact previews
        return description.Split('|', StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => s.Trim())
                         .Select(effect => AddRelationshipImpactPreview(effect))
                         .ToList();
    }

    /// <summary>
    /// Add relationship impact preview to mechanical effects
    /// Frontend UI translates mechanical effects to human relationship context
    /// </summary>
    protected string AddRelationshipImpactPreview(string mechanicalEffect)
    {
        string lower = mechanicalEffect.ToLowerInvariant();

        // Add human relationship context to token effects while preserving mechanical display
        if (lower.Contains("trust") && lower.Contains("+"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>({Model?.NpcName} will remember this kindness)</span>";
        }
        else if (lower.Contains("trust") && lower.Contains("-"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>({Model?.NpcName} will feel betrayed)</span>";
        }
        else if (lower.Contains("commerce") && lower.Contains("+"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>(Improved business relationship)</span>";
        }
        else if (lower.Contains("commerce") && lower.Contains("-"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>(You owe {Model?.NpcName} for this favor)</span>";
        }
        else if (lower.Contains("status") && lower.Contains("+"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>(Enhanced social standing)</span>";
        }
        else if (lower.Contains("status") && lower.Contains("-"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>(Loss of respect and standing)</span>";
        }
        else if (lower.Contains("shadow") && lower.Contains("+"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>(Deeper into secretive circles)</span>";
        }
        else if (lower.Contains("shadow") && lower.Contains("-"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>(Information networks distance themselves)</span>";
        }
        else if (lower.Contains("binding obligation"))
        {
            return $"{mechanicalEffect} <span class='relationship-preview'>(Creates ongoing commitment)</span>";
        }

        // Return original effect if no relationship context applies
        return mechanicalEffect;
    }

    protected string GetEffectIcon(string description)
    {
        // Use beautiful CSS-based icons instead of ugly Unicode symbols
        string lowerDesc = description.ToLowerInvariant();

        // Positive outcomes - return CSS class for green plus icon
        if (lowerDesc.Contains("gain") || lowerDesc.Contains("unlock") ||
            lowerDesc.Contains("open") || lowerDesc.Contains("+") ||
            lowerDesc.Contains("comfort") || lowerDesc.Contains("trust"))
            return "icon icon-positive";

        // Negative/warning outcomes - return CSS class for red minus icon
        if (lowerDesc.Contains("lose") || lowerDesc.Contains("burn") ||
            lowerDesc.Contains("cost") || lowerDesc.Contains("obligation") ||
            lowerDesc.Contains("binding") || lowerDesc.Contains("-"))
            return "icon icon-negative";

        // Arrow/progression - return CSS class for arrow
        if (lowerDesc.Contains("maintain") || lowerDesc.Contains("negotiation"))
            return "icon icon-arrow";

        // Informational/neutral - return CSS class for blue info icon
        return "icon icon-neutral";
    }

    protected string GetMechanicClass(string preview)
    {
        // Style effects based on their type - matching mockup colors
        string lowerPreview = preview.ToLowerInvariant();

        // Positive effects (green)
        if (preview.Contains("✓") || lowerPreview.Contains("gain") ||
            lowerPreview.Contains("opens") || lowerPreview.Contains("+") ||
            lowerPreview.Contains("unlock"))
            return "mechanic-positive";

        // Negative effects (red)
        if (preview.Contains("⚠") || lowerPreview.Contains("lose") ||
            lowerPreview.Contains("burn") || lowerPreview.Contains("cost") ||
            lowerPreview.Contains("-") || lowerPreview.Contains("obligation") ||
            lowerPreview.Contains("binding"))
            return "mechanic-negative";

        // Informational effects (blue)
        if (preview.Contains("ℹ") || preview.Contains("→") ||
            lowerPreview.Contains("learn") || lowerPreview.Contains("maintain") ||
            lowerPreview.Contains("time") || lowerPreview.Contains("minute") ||
            lowerPreview.Contains("investigate") || lowerPreview.Contains("discover"))
            return "mechanic-neutral";

        return "mechanic-neutral";
    }

    protected string GetChoiceClass(ConversationChoice choice)
    {
        List<string> classes = new List<string> { "choice-option" };

        if (!choice.IsAvailable || choice.PatienceCost > CurrentPatience)
        {
            classes.Add("locked");
        }

        return string.Join(" ", classes);
    }

    protected string GetPatienceOrbLabel(int index)
    {
        return index < CurrentPatience
            ? $"Patience point {index + 1} available"
            : $"Patience point {index + 1} spent";
    }

    protected string GetComfortClass()
    {
        ConversationManager conversationManager = GameFacade.GetCurrentConversationManager();
        ConversationState? state = conversationManager?.State;

        if (state == null) return "comfort-neutral";

        // Use threshold-based classes for meaningful feedback
        if (state.HasReachedPerfectThreshold())
            return "comfort-perfect";
        else if (state.HasReachedLetterThreshold())
            return "comfort-letter-ready";
        else if (state.HasReachedMaintainThreshold())
            return "comfort-maintaining";
        else if (CurrentComfort > 0)
            return "comfort-building";
        else if (CurrentComfort == 0)
            return "comfort-neutral";
        else
            return "comfort-declining";
    }

    protected string GetComfortDescription()
    {
        ConversationManager conversationManager = GameFacade.GetCurrentConversationManager();
        ConversationState? state = conversationManager?.State;

        if (state == null) return "Unknown";

        // Show threshold progress for meaningful feedback
        if (state.HasReachedPerfectThreshold())
            return $"Perfect! ({CurrentComfort})";
        else if (state.HasReachedLetterThreshold())
            return $"Trust Earned ({CurrentComfort})";
        else if (state.HasReachedMaintainThreshold())
            return $"Comfortable ({CurrentComfort})";
        else if (CurrentComfort > 0)
            return $"Building ({CurrentComfort})";
        else if (CurrentComfort == 0)
            return "Neutral (0)";
        else
            return $"Strained ({CurrentComfort})";
    }

    protected string GetComfortProgressHint()
    {
        ConversationManager conversationManager = GameFacade.GetCurrentConversationManager();
        ConversationState? state = conversationManager?.State;

        if (state == null) return "";

        int maintainThreshold = (int)(state.StartingPatience * GameRules.COMFORT_MAINTAIN_THRESHOLD);
        int letterThreshold = (int)(state.StartingPatience * GameRules.COMFORT_LETTER_THRESHOLD);
        int perfectThreshold = (int)(state.StartingPatience * GameRules.COMFORT_PERFECT_THRESHOLD);

        if (state.HasReachedPerfectThreshold())
            return "Perfect conversation achieved!";
        else if (state.HasReachedLetterThreshold())
            return $"Letter available! ({perfectThreshold - CurrentComfort} more for perfect)";
        else if (state.HasReachedMaintainThreshold())
            return $"Good relationship! ({letterThreshold - CurrentComfort} more for letter)";
        else
            return $"Need {maintainThreshold - CurrentComfort} more to maintain relationship";
    }

    protected double GetComfortProgressPercentage()
    {
        ConversationManager conversationManager = GameFacade.GetCurrentConversationManager();
        ConversationState? state = conversationManager?.State;

        if (state == null) return 0;

        int perfectThreshold = (int)(state.StartingPatience * GameRules.COMFORT_PERFECT_THRESHOLD);
        if (perfectThreshold == 0) return 0;

        return Math.Min(100, (double)CurrentComfort / perfectThreshold * 100);
    }

    protected string GetComfortThresholdClass(string thresholdType)
    {
        ConversationManager conversationManager = GameFacade.GetCurrentConversationManager();
        ConversationState? state = conversationManager?.State;

        if (state == null) return "";

        return thresholdType switch
        {
            "maintain" => state.HasReachedMaintainThreshold() ? "reached" : "",
            "letter" => state.HasReachedLetterThreshold() ? "reached" : "",
            "perfect" => state.HasReachedPerfectThreshold() ? "reached" : "",
            _ => ""
        };
    }

    /// <summary>
    /// CATEGORICAL card coloring based on ConversationChoiceType
    /// Each card type gets its appropriate color based on its categorical nature
    /// </summary>
    protected string GetChoiceColorClass(ConversationChoice choice)
    {
        // DEBUG: Log the actual ChoiceType values to understand what's happening
        Console.WriteLine($"[ConversationScreen] Choice '{choice.NarrativeText}' has ChoiceType: {choice.ChoiceType}");
        
        string colorClass = choice.ChoiceType switch
        {
            // Negative/destructive choice types - RED
            ConversationChoiceType.DeclineLetterOffer => "negative-card",
            ConversationChoiceType.PurgeLetter => "negative-card",
            ConversationChoiceType.TravelForceThrough => "negative-card",
            
            // Positive/beneficial choice types - GREEN
            ConversationChoiceType.AcceptLetterOffer => "positive-card",
            ConversationChoiceType.KeepLetter => "positive-card",
            ConversationChoiceType.Introduction => "positive-card",
            ConversationChoiceType.TravelCautious => "positive-card",
            
            // Risky/uncertain choice types - YELLOW
            ConversationChoiceType.SkipAndDeliver => "risky-card",
            ConversationChoiceType.TravelUseEquipment => "risky-card",
            ConversationChoiceType.TravelTradeHelp => "risky-card",
            ConversationChoiceType.TravelExchangeInfo => "risky-card",
            
            // Discovery/neutral choice types - BLUE
            ConversationChoiceType.DiscoverRoute => "discovery-card",
            ConversationChoiceType.RespectQueueOrder => "neutral-card",
            ConversationChoiceType.TravelSlowProgress => "neutral-card",
            
            // Default - no coloring
            ConversationChoiceType.Default => "",
            _ => ""
        };
        
        Console.WriteLine($"[ConversationScreen] Returning color class: '{colorClass}' for ChoiceType: {choice.ChoiceType}");
        return colorClass;
    }
}