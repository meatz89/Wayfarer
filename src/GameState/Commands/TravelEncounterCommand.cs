/// <summary>
/// Command to handle encounters during travel
/// </summary>
public class TravelEncounterCommand : BaseGameCommand
{
    private readonly RouteOption _route;
    private readonly ConversationFactory _conversationFactory;
    private readonly LocationRepository _locationRepository;
    private readonly MessageSystem _messageSystem;


    public TravelEncounterCommand(
        RouteOption route,
        ConversationFactory conversationFactory,
        LocationRepository locationRepository,
        MessageSystem messageSystem)
    {
        _route = route ?? throw new ArgumentNullException(nameof(route));
        _conversationFactory = conversationFactory ?? throw new ArgumentNullException(nameof(conversationFactory));
        _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));

        Description = $"Travel encounter on route to {route.Destination}";
    }

    public override CommandValidationResult CanExecute(GameWorld gameWorld)
    {
        // Travel encounters are always valid if we're in travel
        return CommandValidationResult.Success();
    }

    public override async Task<CommandResult> ExecuteAsync(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        Location origin = _locationRepository.GetCurrentLocation();
        Location destination = _locationRepository.GetLocation(_route.Destination);

        // Create travel conversation context
        TravelConversationContext travelContext = new TravelConversationContext
        {
            GameWorld = gameWorld,
            Player = player,
            LocationName = origin.Name,
            LocationSpotName = player.CurrentLocationSpot?.Name ?? "",
            Route = _route,
            Origin = origin,
            Destination = destination,
            ConversationTopic = $"Travel from {origin.Name} to {destination.Name}"
        };

        // Create conversation through factory
        ConversationManager conversationManager = await _conversationFactory.CreateConversation(travelContext, player);

        // Set up conversation for UI
        gameWorld.PendingConversationManager = conversationManager;
        gameWorld.ConversationPending = true;

        return CommandResult.Success(
            "Travel encounter initiated",
            new
            {
                Route = _route.Name,
                RequiresConversation = true,
                ConversationManager = conversationManager
            }
        );
    }

}