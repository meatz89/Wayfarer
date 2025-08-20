using System;
using Wayfarer.GameState;
using Wayfarer.GameState.Constants;

namespace Wayfarer.GameState.Operations
{
    /// <summary>
    /// Operation to spend tokens from the player's token pool
    /// </summary>
public class SpendTokensOperation : IGameOperation
{
    private readonly ConnectionType _tokenType;
    private readonly int _amount;
    private readonly string _npcId;
    private readonly TokenMechanicsManager _tokenManager;

    public SpendTokensOperation(ConnectionType tokenType, int amount, string npcId, TokenMechanicsManager tokenManager)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        _tokenType = tokenType;
        _amount = amount;
        _npcId = npcId;
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
    }

    public string Description => string.IsNullOrEmpty(_npcId)
        ? $"Spend {_amount} {_tokenType} tokens"
        : $"Spend {_amount} {_tokenType} tokens with NPC {_npcId}";

    public bool CanExecute(GameWorld gameWorld)
    {
        return _tokenManager.HasTokens(_tokenType, _amount);
    }

    public void Execute(GameWorld gameWorld)
    {
        bool tokensSpent;
        if (string.IsNullOrEmpty(_npcId))
        {
            tokensSpent = _tokenManager.SpendTokens(_tokenType, _amount);
        }
        else
        {
            tokensSpent = _tokenManager.SpendTokensWithNPC(_tokenType, _amount, _npcId);
        }

        if (!tokensSpent)
        {
            throw new InvalidOperationException($"Failed to spend {_amount} {_tokenType} tokens");
        }
    }
}
}