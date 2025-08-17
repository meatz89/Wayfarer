using System;
using Wayfarer.GameState;

/// <summary>
/// Factory for creating NPCDeck instances with proper DI injection
/// </summary>
public class NPCDeckFactory
{
    private readonly TokenMechanicsManager _tokenManager;
    private readonly GameWorld _gameWorld;

    public NPCDeckFactory(TokenMechanicsManager tokenManager, GameWorld gameWorld)
    {
        _tokenManager = tokenManager;
        _gameWorld = gameWorld;
    }

    public NPCDeck CreateDeck(string npcId, PersonalityType personalityType)
    {
        return new NPCDeck(npcId, personalityType, _tokenManager, _gameWorld);
    }
}