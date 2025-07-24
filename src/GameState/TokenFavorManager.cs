using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// Manages contextual token favors - specific unlocks offered by specific NPCs for specific token types.
/// </summary>
public class TokenFavorManager
{
    private readonly GameWorld _gameWorld;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly NPCRepository _npcRepository;
    private readonly ItemRepository _itemRepository;
    private readonly RouteDiscoveryManager _routeDiscoveryManager;
    private readonly NetworkUnlockManager _networkUnlockManager;
    private readonly LocationRepository _locationRepository;
    private readonly MessageSystem _messageSystem;
    private readonly NPCLetterOfferService _letterOfferService;

    // Store available favors by NPC ID
    private readonly Dictionary<string, List<TokenFavor>> _favorsByNPC = new Dictionary<string, List<TokenFavor>>();

    public TokenFavorManager(
        GameWorld gameWorld,
        ConnectionTokenManager tokenManager,
        NPCRepository npcRepository,
        ItemRepository itemRepository,
        RouteDiscoveryManager routeDiscoveryManager,
        NetworkUnlockManager networkUnlockManager,
        LocationRepository locationRepository,
        MessageSystem messageSystem,
        NPCLetterOfferService letterOfferService)
    {
        _gameWorld = gameWorld;
        _tokenManager = tokenManager;
        _npcRepository = npcRepository;
        _itemRepository = itemRepository;
        _routeDiscoveryManager = routeDiscoveryManager;
        _networkUnlockManager = networkUnlockManager;
        _locationRepository = locationRepository;
        _messageSystem = messageSystem;
        _letterOfferService = letterOfferService;
    }

    /// <summary>
    /// Load token favors from configuration.
    /// </summary>
    public void LoadTokenFavors(List<TokenFavor> favors)
    {
        _favorsByNPC.Clear();

        foreach (TokenFavor favor in favors)
        {
            if (!_favorsByNPC.ContainsKey(favor.NPCId))
            {
                _favorsByNPC[favor.NPCId] = new List<TokenFavor>();
            }

            _favorsByNPC[favor.NPCId].Add(favor);
        }
    }

    /// <summary>
    /// Get available favors from a specific NPC.
    /// </summary>
    public List<TokenFavor> GetAvailableFavors(string npcId)
    {
        if (!_favorsByNPC.ContainsKey(npcId))
            return new List<TokenFavor>();

        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null)
            return new List<TokenFavor>();

        Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(npcId);
        int totalTokens = npcTokens.Values.Sum();

        List<TokenFavor> availableFavors = new List<TokenFavor>();

        foreach (TokenFavor favor in _favorsByNPC[npcId])
        {
            // Skip if already purchased and one-time only
            if (favor.IsPurchased && favor.IsOneTime)
                continue;

            // Check relationship level requirement
            if (totalTokens < favor.MinimumRelationshipLevel)
                continue;

            // Check if NPC has the required token type
            if (!npc.LetterTokenTypes.Contains(favor.RequiredTokenType))
                continue;

            // Check if player has enough tokens of the required type
            int tokensOfType = npcTokens.GetValueOrDefault(favor.RequiredTokenType, 0);
            if (tokensOfType >= favor.TokenCost)
            {
                availableFavors.Add(favor);
            }
        }

        return availableFavors;
    }

    /// <summary>
    /// Purchase a token favor from an NPC.
    /// </summary>
    public bool PurchaseFavor(string npcId, string favorId)
    {
        if (!_favorsByNPC.ContainsKey(npcId))
            return false;

        TokenFavor? favor = _favorsByNPC[npcId].FirstOrDefault(f => f.Id == favorId);
        if (favor == null)
            return false;

        // Validate availability
        List<TokenFavor> availableFavors = GetAvailableFavors(npcId);
        if (!availableFavors.Any(f => f.Id == favorId))
        {
            _messageSystem.AddSystemMessage(
                "You don't meet the requirements for this favor.",
                SystemMessageTypes.Warning
            );
            return false;
        }

        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null)
            return false;

        // Spend the tokens
        _tokenManager.SpendTokens(favor.RequiredTokenType, favor.TokenCost, npcId);

        // Show purchase narrative
        if (!string.IsNullOrEmpty(favor.PurchaseText))
        {
            _messageSystem.AddSystemMessage(
                $"{npc.Name}: \"{favor.PurchaseText}\"",
                SystemMessageTypes.Success
            );
        }

        // Grant the favor based on type
        bool success = GrantFavor(favor, npc);

        if (success)
        {
            favor.IsPurchased = true;

            // Save purchased favor state to player
            Player player = _gameWorld.GetPlayer();
            if (!player.PurchasedFavors.Contains(favorId))
            {
                player.PurchasedFavors.Add(favorId);
            }
        }

        return success;
    }

    /// <summary>
    /// Grant the actual reward from a favor.
    /// </summary>
    private bool GrantFavor(TokenFavor favor, NPC npc)
    {
        switch (favor.FavorType)
        {
            case TokenFavorType.RouteDiscovery:
                return GrantRouteDiscovery(favor, npc);

            case TokenFavorType.ItemPurchase:
                return GrantItem(favor, npc);

            case TokenFavorType.LocationAccess:
                return GrantLocationAccess(favor, npc);

            case TokenFavorType.NPCIntroduction:
                return GrantNPCIntroduction(favor, npc);

            case TokenFavorType.LetterOpportunity:
                return GrantLetterOpportunity(favor, npc);

            case TokenFavorType.InformationPurchase:
                return GrantInformation(favor, npc);

            case TokenFavorType.ServiceAccess:
                return GrantServiceAccess(favor, npc);

            default:
                return false;
        }
    }

    private bool GrantRouteDiscovery(TokenFavor favor, NPC npc)
    {
        string routeId = favor.GrantsId;
        if (string.IsNullOrEmpty(routeId))
            return false;

        // Use the RouteDiscoveryManager to discover the route
        bool discovered = _routeDiscoveryManager.TryDiscoverRoute(routeId);

        if (discovered)
        {
            _messageSystem.AddSystemMessage(
                $"🗺️ You've learned about a new route: {routeId}!",
                SystemMessageTypes.Success
            );
        }

        return discovered;
    }

    private bool GrantItem(TokenFavor favor, NPC npc)
    {
        string itemId = favor.GrantsId;
        if (string.IsNullOrEmpty(itemId))
            return false;

        Item item = _itemRepository.GetItemById(itemId);
        if (item == null)
        {
            _messageSystem.AddSystemMessage(
                $"Error: Item '{itemId}' not found.",
                SystemMessageTypes.Danger
            );
            return false;
        }

        Player player = _gameWorld.GetPlayer();

        // Check if player has inventory space
        if (!player.Inventory.CanAddItem(item, _itemRepository))
        {
            _messageSystem.AddSystemMessage(
                "Your inventory is full!",
                SystemMessageTypes.Warning
            );
            return false;
        }

        // Add the item
        player.Inventory.AddItem(itemId);

        string quantity = favor.AdditionalData.GetValueOrDefault("quantity", "1");
        _messageSystem.AddSystemMessage(
            $"📦 Received {quantity}x {item.Name}",
            SystemMessageTypes.Success
        );

        return true;
    }

    private bool GrantLocationAccess(TokenFavor favor, NPC npc)
    {
        string locationId = favor.GrantsId;
        if (string.IsNullOrEmpty(locationId))
            return false;

        Location location = _locationRepository.GetLocation(locationId);
        if (location == null)
            return false;

        // Add location to player's unlocked locations
        Player player = _gameWorld.GetPlayer();
        if (!player.UnlockedLocationIds.Contains(locationId))
        {
            player.UnlockedLocationIds.Add(locationId);
        }

        _messageSystem.AddSystemMessage(
            $"🏛️ You now have access to {location.Name}!",
            SystemMessageTypes.Success
        );

        return true;
    }

    private bool GrantNPCIntroduction(TokenFavor favor, NPC npc)
    {
        string targetNpcId = favor.GrantsId;
        if (string.IsNullOrEmpty(targetNpcId))
            return false;

        // Use NetworkUnlockManager for proper NPC introduction
        return _networkUnlockManager.UnlockNetworkContact(npc.ID, targetNpcId);
    }

    private bool GrantLetterOpportunity(TokenFavor favor, NPC npc)
    {
        // Check if queue has space
        if (_gameWorld.GetPlayer().LetterQueue.Count() >= 5) // TODO: Get max queue size from config
        {
            _messageSystem.AddSystemMessage(
                "Letter queue is full. Cannot accept more letters.",
                SystemMessageTypes.Danger
            );
            return false;
        }

        // Generate a special letter offer from this NPC
        List<LetterOffer> offers = _letterOfferService.GenerateNPCLetterOffers(npc.ID);
        if (offers.Any())
        {
            // Accept the first offer automatically as part of the favor
            LetterOffer offer = offers.First();
            bool success = _letterOfferService.AcceptNPCLetterOffer(npc.ID, offer.Id);

            if (success)
            {
                _messageSystem.AddSystemMessage(
                    $"💌 {npc.Name} has entrusted you with a special letter!",
                    SystemMessageTypes.Success
                );

                // Add additional favor-specific message if provided
                if (favor.AdditionalData?.ContainsKey("letter_message") == true)
                {
                    _messageSystem.AddSystemMessage(
                        $"\"{favor.AdditionalData["letter_message"]}\"",
                        SystemMessageTypes.Info
                    );
                }
            }

            return success;
        }
        else
        {
            // Fallback if no offers can be generated
            _messageSystem.AddSystemMessage(
                $"💌 {npc.Name} will have a special letter for you soon!",
                SystemMessageTypes.Success
            );
            return true;
        }
    }

    private bool GrantInformation(TokenFavor favor, NPC npc)
    {
        string info = favor.AdditionalData.GetValueOrDefault("information", "");
        if (!string.IsNullOrEmpty(info))
        {
            _messageSystem.AddSystemMessage(
                $"💡 {npc.Name} shares valuable information: \"{info}\"",
                SystemMessageTypes.Info
            );
        }

        return true;
    }

    private bool GrantServiceAccess(TokenFavor favor, NPC npc)
    {
        string serviceType = favor.GrantsId;

        // Add to player's unlocked services
        Player player = _gameWorld.GetPlayer();
        if (!player.UnlockedServices.Contains(serviceType))
        {
            player.UnlockedServices.Add(serviceType);
        }

        _messageSystem.AddSystemMessage(
            $"🛠️ {npc.Name} now offers {serviceType} services!",
            SystemMessageTypes.Success
        );

        return true;
    }

    /// <summary>
    /// Show available favors narrative when visiting an NPC.
    /// </summary>
    public void ShowAvailableFavorsNarrative(string npcId)
    {
        List<TokenFavor> favors = GetAvailableFavors(npcId);
        if (!favors.Any())
            return;

        NPC npc = _npcRepository.GetById(npcId);
        if (npc == null)
            return;

        _messageSystem.AddSystemMessage(
            $"💰 {npc.Name} has special favors available for trusted associates.",
            SystemMessageTypes.Info
        );
    }
}