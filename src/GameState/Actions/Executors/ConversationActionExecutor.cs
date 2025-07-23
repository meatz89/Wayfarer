using System.Threading.Tasks;

namespace Wayfarer.GameState.Actions.Executors;

/// <summary>
/// Executor for conversation-based actions
/// </summary>
public class ConversationActionExecutor : IActionExecutor
{
    private readonly ConversationFactory _conversationFactory;
    private readonly NPCRepository _npcRepository;
    private readonly NarrativeManager _narrativeManager;
    private readonly NPCLetterOfferService _letterOfferService;
    
    public ConversationActionExecutor(
        ConversationFactory conversationFactory,
        NPCRepository npcRepository,
        NarrativeManager narrativeManager,
        NPCLetterOfferService letterOfferService)
    {
        _conversationFactory = conversationFactory;
        _npcRepository = npcRepository;
        _narrativeManager = narrativeManager;
        _letterOfferService = letterOfferService;
    }
    
    public bool CanHandle(LocationAction actionType)
    {
        return actionType == LocationAction.Converse || 
               actionType == LocationAction.Deliver ||
               actionType == LocationAction.TravelEncounter;
    }
    
    public async Task<ActionExecutionResult> Execute(ActionOption action, Player player, GameWorld world)
    {
        // Create conversation context
        var context = BuildConversationContext(action, player, world);
        
        // Create conversation
        var conversation = await _conversationFactory.CreateConversation(context, player);
        
        // Return result requiring conversation UI
        return ActionExecutionResult.RequiringConversation(conversation);
    }
    
    private ActionConversationContext BuildConversationContext(ActionOption action, Player player, GameWorld world)
    {
        var context = new ActionConversationContext
        {
            GameWorld = world,
            Player = player,
            LocationName = player.CurrentLocation?.Name ?? "",
            LocationSpotName = player.CurrentLocationSpot?.Name ?? "",
            TargetNPC = action.NPCId != null ? _npcRepository.GetNPCById(action.NPCId) : null,
            ConversationTopic = action.IsLetterOffer ? "LetterOffer" : $"Action_{action.Action}",
            SourceAction = action,
            InitialNarrative = action.InitialNarrative
        };
        
        // Check for narrative overrides
        if (_narrativeManager.HasActiveNarrative() && 
            action.Action == LocationAction.Converse && 
            context.TargetNPC != null)
        {
            var narrativeIntro = _narrativeManager.GetNarrativeIntroduction(context);
            if (!string.IsNullOrEmpty(narrativeIntro))
            {
                context.InitialNarrative = narrativeIntro;
            }
        }
        
        // Add letter offer templates if applicable
        if (action.IsLetterOffer && context.TargetNPC != null)
        {
            var offers = _letterOfferService.GenerateNPCLetterOffers(action.NPCId);
            context.AvailableTemplates = offers.Select(offer => new ChoiceTemplate
            {
                Purpose = $"Accept {offer.Category} letter",
                Description = $"{offer.Category} delivery for {offer.Payment} coins",
                FocusCost = 0,
                ChoiceType = ConversationChoiceType.AcceptLetterOffer,
                TokenType = offer.LetterType,
                Category = offer.Category
            }).ToList();
            
            // Add decline option
            context.AvailableTemplates.Add(new ChoiceTemplate
            {
                Purpose = "Decline letter offers",
                Description = "Politely refuse any letter work",
                FocusCost = 0,
                ChoiceType = ConversationChoiceType.DeclineLetterOffer
            });
        }
        
        return context;
    }
}