using System;
using System.Collections.Generic;
using System.Linq;

public class NPCDeckFactory
{
    private readonly Random _random = new Random();
    private readonly TokenMechanicsManager _tokenManager;
    
    public NPCDeckFactory(TokenMechanicsManager tokenManager = null)
    {
        _tokenManager = tokenManager;
    }
    
    public CardDeck CreateDeckForNPC(NPC npc)
    {
        var deck = new CardDeck();
        
        // Use the proper initialization method that sets up depth levels correctly
        deck.InitializeForNPC(npc, _tokenManager);
        
        // The deck is now properly initialized with depth-based cards
        // No need to add custom cards here as InitializeForNPC handles it
        
        return deck;
    }
}