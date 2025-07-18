using System.Collections.Generic;
using System.Linq;
    /// <summary>
    /// Repository for accessing token favor data.
    /// </summary>
    public class TokenFavorRepository
    {
        private readonly GameWorld _gameWorld;
        
        public TokenFavorRepository(GameWorld gameWorld)
        {
            _gameWorld = gameWorld;
        }
        
        /// <summary>
        /// Get all token favors in the game.
        /// </summary>
        public List<TokenFavor> GetAllTokenFavors()
        {
            return _gameWorld.WorldState.TokenFavors ?? new List<TokenFavor>();
        }
        
        /// <summary>
        /// Get token favors offered by a specific NPC.
        /// </summary>
        public List<TokenFavor> GetTokenFavorsByNPC(string npcId)
        {
            return GetAllTokenFavors()
                .Where(f => f.NPCId == npcId)
                .ToList();
        }
        
        /// <summary>
        /// Get a specific token favor by ID.
        /// </summary>
        public TokenFavor GetTokenFavorById(string favorId)
        {
            return GetAllTokenFavors()
                .FirstOrDefault(f => f.Id == favorId);
        }
        
        /// <summary>
        /// Get token favors of a specific type.
        /// </summary>
        public List<TokenFavor> GetTokenFavorsByType(TokenFavorType type)
        {
            return GetAllTokenFavors()
                .Where(f => f.FavorType == type)
                .ToList();
        }
    }